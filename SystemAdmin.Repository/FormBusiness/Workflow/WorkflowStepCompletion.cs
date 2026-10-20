using SqlSugar;
using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.Forms.InformationRequest.Entity;
using SystemAdmin.Model.FormBusiness.Forms.LeaveCancell.Entity;
using SystemAdmin.Model.FormBusiness.Forms.LeaveRequest.Entity;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.FormBusiness.Forms.SystemAccountRequest.Entity;
using SystemAdmin.Model.HR.BasicInfo.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Entity;

namespace SystemAdmin.Repository.FormBusiness.Workflow
{
    /// <summary>
    /// 步骤完成后置处理
    /// </summary>
    public class WorkflowStepCompletion
    {
        private readonly SqlSugarScope _db;
        private readonly LocalizationService _localization;
        private readonly Language _lang;
        private readonly CurrentUser _loginuser;
        private readonly Dictionary<string, Func<long, Task<Result<bool>>>> _registry;
        private readonly string _this = "FormBusiness.Workflow";

        public WorkflowStepCompletion(SqlSugarScope db, Language lang, LocalizationService localization, CurrentUser loginuser)
        {
            _db = db;
            _localization = localization;
            _lang = lang;
            _loginuser = loginuser;
            _registry = new Dictionary<string, Func<long, Task<Result<bool>>>>(StringComparer.OrdinalIgnoreCase)
            {
                // 请假单
                [nameof(ProcessLeaveRequest)] = ProcessLeaveRequest,
                // 销假单
                [nameof(ProcessLeaveCancell)] = ProcessLeaveCancell,
                // 资讯需求单
                [nameof(InforRequestApply)] = InforRequestApply,
                [nameof(InforRequestEstimated)] = InforRequestEstimated,
                [nameof(InforRequestProcessStart)] = InforRequestProcessStart,
                [nameof(InforRequestProcessEnd)] = InforRequestProcessEnd,
                [nameof(InforRequestRating)] = InforRequestRating,
                // 系统账号申请单
                [nameof(SystemAccountApply)] = SystemAccountApply,
                [nameof(SystemAccountProcessStart)] = SystemAccountProcessStart,
                [nameof(SystemAccountProcessEnd)] = SystemAccountProcessEnd,
                [nameof(SystemAccountRating)] = SystemAccountRating,
            };
        }

        public async Task<Result<bool>> Resolve(string guidance, long formId)
        {
            // 未配置 Guidance 时直接放行
            if (string.IsNullOrWhiteSpace(guidance))
            {
                return Result<bool>.Ok(true);
            }

            if (!_registry.TryGetValue(guidance, out var handler))
            {
                return Result<bool>.Failure(500, _localization.ReturnMsg($"{_this}.GuidanceHandlerNotFound", _lang.Locale, guidance));
            }

            return await handler(formId);
        }

        #region 请假单

        /// <summary>
        /// 请假单审批完成后处理
        /// </summary>
        public async Task<Result<bool>> ProcessLeaveRequest(long formId)
        {
            var leaveRequest = await _db.Queryable<FormInstanceEntity>()
                                        .With(SqlWith.NoLock)
                                        .InnerJoin<LeaveRequestEntity>((instance, leave) => instance.FormId == leave.FormId)
                                        .Where((instance, leave) => instance.FormId == formId)
                                        .Select((instance, leave) => leave)
                                        .FirstAsync();

            var formInstance = await _db.Queryable<FormInstanceEntity>()
                                        .FirstAsync(instance => instance.FormId == formId);

            // 仅 Annual、Sick 占用余额
            var needBalance = leaveRequest.LeaveType == LeaveType.Annual.ToEnumString() || leaveRequest.LeaveType == LeaveType.Sick.ToEnumString();

            var updateBlance = 0;
            if (needBalance)
            {
                var startTime = leaveRequest.StartDateTime!.Value;
                var endTime = leaveRequest.EndDateTime!.Value;

                // 按年度拆分请假工时
                var leaveHoursByYear = CalcHoursByYear(startTime, endTime);

                var userInfo = await _db.Queryable<UserInfoEntity>()
                                        .FirstAsync(user => user.UserId == formInstance.ApplicantUserId);

                var isChinese = _lang.Locale == "zh-CN";
                var userName = isChinese ? userInfo.UserNameCn : userInfo.UserNameEn;

                var leaveInfo = await _db.Queryable<DictionaryInfoEntity>()
                                         .Where(dic => dic.DicType == "LeaveType" && dic.DicCode == leaveRequest.LeaveType)
                                         .FirstAsync();
                var leaveName = isChinese ? leaveInfo.DicNameCn : leaveInfo.DicNameEn;

                foreach (var (year, hours) in leaveHoursByYear)
                {
                    var days = Math.Ceiling(hours / 8);
                    var leaveAnnual = await _db.Queryable<UserLeaveBalanceEntity>()
                                               .FirstAsync(annual => annual.UserId == formInstance.ApplicantUserId && annual.Year == year && annual.LeaveType == leaveRequest.LeaveType);

                    if (leaveAnnual == null)
                    {
                        return Result<bool>.Failure(400, _localization.ReturnMsg($"{_this}.LeaveAnnualNotFound", args: new object[]
                        {
                            userName ?? formInstance.ApplicantUserId.ToString(),
                            year,
                            leaveName
                        }));
                    }

                    var newRemainingDays = leaveAnnual.RemainingDays - (decimal)days;
                    if (newRemainingDays < 0)
                    {
                        return Result<bool>.Failure(400, _localization.ReturnMsg($"{_this}.InsufficientLeaveBalance", args: new object[]
                        {
                            userName ?? formInstance.ApplicantUserId.ToString(),
                            year,
                            leaveName,
                            leaveAnnual.RemainingDays,
                            days
                        }));
                    }

                    updateBlance = await _db.Updateable<UserLeaveBalanceEntity>()
                                            .SetColumns(annual => new UserLeaveBalanceEntity
                                            {
                                                RemainingDays = newRemainingDays,
                                                ModifiedBy = formInstance.CreatedBy,
                                                ModifiedDate = DateTime.Now
                                            }).Where(annual => annual.UserId == formInstance.ApplicantUserId && annual.Year == year && annual.LeaveType == leaveRequest.LeaveType)
                                            .ExecuteCommandAsync();
                }
            }

            // SubstituteUserId：被代理人（申请人），AgentUserId：代理人
            var userAgent = new UserAgentEntity
            {
                SubstituteUserId = formInstance.ApplicantUserId,
                AgentUserId = leaveRequest.AgentUserId!.Value,
                StartTime = leaveRequest.StartDateTime!.Value,
                EndTime = leaveRequest.EndDateTime!.Value,
                CreatedBy = formInstance.CreatedBy,
                CreatedDate = DateTime.Now
            };

            var insertAgentCount = await _db.Insertable(userAgent).ExecuteCommandAsync();

            var updateAgentCount = await _db.Updateable<UserInfoEntity>()
                                            .SetColumns(user => new UserInfoEntity
                                            {
                                                IsAgent = 1
                                            }).Where(user => user.UserId == leaveRequest.AgentUserId)
                                            .ExecuteCommandAsync();

            return Result<bool>.Ok((!needBalance || updateBlance >= 1) && insertAgentCount >= 1 && updateAgentCount >= 1);
        }

        #endregion

        #region 销假单

        /// <summary>
        /// 销假单审批完成后处理：将销假时数按年度加回对应假别的剩余额度
        /// </summary>
        public async Task<Result<bool>> ProcessLeaveCancell(long formId)
        {
            var cancell = await _db.Queryable<LeaveCancellEntity>()
                                   .With(SqlWith.NoLock)
                                   .Where(item => item.FormId == formId)
                                   .FirstAsync();

            // 假别取自绑定的请假单
            var leaveType = await _db.Queryable<LeaveRequestEntity>()
                                     .With(SqlWith.NoLock)
                                     .Where(leave => leave.FormId == cancell.LeaveRequestId)
                                     .Select(leave => leave.LeaveType)
                                     .FirstAsync();

            // 仅 Annual、Sick 占用余额，其余假别无需加回
            if (leaveType != LeaveType.Annual.ToEnumString() && leaveType != LeaveType.Sick.ToEnumString())
            {
                return Result<bool>.Ok(true);
            }

            var startTime = cancell.StartDateTime!.Value;
            var endTime = cancell.EndDateTime!.Value;

            // 按年度拆分销假工时
            var cancellHoursByYear = CalcHoursByYear(startTime, endTime);

            var formInstance = await _db.Queryable<FormInstanceEntity>()
                                        .FirstAsync(instance => instance.FormId == formId);

            var userInfo = await _db.Queryable<UserInfoEntity>()
                                    .FirstAsync(user => user.UserId == formInstance.ApplicantUserId);

            var isChinese = _lang.Locale == "zh-CN";
            var userName = isChinese ? userInfo.UserNameCn : userInfo.UserNameEn;

            var leaveInfo = await _db.Queryable<DictionaryInfoEntity>()
                                     .Where(dic => dic.DicType == "LeaveType" && dic.DicCode == leaveType)
                                     .FirstAsync();
            var leaveName = isChinese ? leaveInfo.DicNameCn : leaveInfo.DicNameEn;

            var updateBalance = 0;
            foreach (var (year, hours) in cancellHoursByYear)
            {
                // 按天数加回，保留两位小数
                var days = Math.Round((decimal)hours / 8, 2, MidpointRounding.AwayFromZero);
                var leaveAnnual = await _db.Queryable<UserLeaveBalanceEntity>()
                                           .FirstAsync(annual => annual.UserId == formInstance.ApplicantUserId && annual.Year == year && annual.LeaveType == leaveType);

                if (leaveAnnual == null)
                {
                    return Result<bool>.Failure(400, _localization.ReturnMsg($"{_this}.LeaveAnnualNotFound", args: new object[]
                    {
                        userName ?? formInstance.ApplicantUserId.ToString(),
                        year,
                        leaveName
                    }));
                }

                // 加回额度，不超过给予天数
                var restoredDays = Math.Min(leaveAnnual.RemainingDays + days, leaveAnnual.RenderDays);
                updateBalance = await _db.Updateable<UserLeaveBalanceEntity>()
                                         .SetColumns(annual => new UserLeaveBalanceEntity
                                         {
                                             RemainingDays = restoredDays,
                                             ModifiedBy = formInstance.CreatedBy,
                                             ModifiedDate = DateTime.Now
                                         }).Where(annual => annual.UserId == formInstance.ApplicantUserId && annual.Year == year && annual.LeaveType == leaveType)
                                         .ExecuteCommandAsync();
            }

            return Result<bool>.Ok(updateBalance >= 1);
        }

        #endregion

        #region 资讯需求单

        /// <summary>
        /// 提交：写入申请人；重新提交时清空后续阶段字段，避免残留上一轮数据
        /// </summary>
        public async Task<Result<bool>> InforRequestApply(long formId)
        {
            var formInstance = await _db.Queryable<FormInstanceEntity>()
                                        .FirstAsync(instance => instance.FormId == formId);

            var affected = await UpsertPersonInChargeResult(
                formId,
                () => new PersonInChargeResultEntity
                {
                    FormId = formId,
                    ApplicantUserId = formInstance.ApplicantUserId,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = DateTime.Now
                },
                () => _db.Updateable<PersonInChargeResultEntity>()
                        .SetColumns(result => new PersonInChargeResultEntity
                        {
                            ApplicantUserId = formInstance.ApplicantUserId,
                            EstimatedDays = null,
                            ProcessStartTime = null,
                            ProcessEndTime = null,
                            HandlerId = null,
                            Rating = null,
                            ModifiedBy = _loginuser.UserId,
                            ModifiedDate = DateTime.Now
                        }).Where(result => result.FormId == formId)
                        .ExecuteCommandAsync());

            return Result<bool>.Ok(affected >= 1);
        }

        /// <summary>
        /// 预估处理时间：写入预计天数；重新执行时清空后续阶段字段
        /// </summary>
        public async Task<Result<bool>> InforRequestEstimated(long formId)
        {
            var estimatedDays = await _db.Queryable<InformationRequestEntity>()
                                         .Where(info => info.FormId == formId)
                                         .Select(info => info.EstimatedDays)
                                         .FirstAsync();

            var affected = await UpsertPersonInChargeResult(
                formId,
                () => new PersonInChargeResultEntity
                {
                    FormId = formId,
                    EstimatedDays = estimatedDays,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = DateTime.Now
                },
                () => _db.Updateable<PersonInChargeResultEntity>()
                        .SetColumns(result => new PersonInChargeResultEntity
                        {
                            EstimatedDays = estimatedDays,
                            ProcessStartTime = null,
                            ProcessEndTime = null,
                            HandlerId = null,
                            Rating = null,
                            ModifiedBy = _loginuser.UserId,
                            ModifiedDate = DateTime.Now
                        }).Where(result => result.FormId == formId)
                        .ExecuteCommandAsync());

            return Result<bool>.Ok(affected >= 1);
        }

        /// <summary>
        /// 处理开始：写入开始时间；重新开始时清空后续阶段字段，避免结束时间早于开始时间
        /// </summary>
        public async Task<Result<bool>> InforRequestProcessStart(long formId)
        {
            var now = DateTime.Now;

            var affected = await UpsertPersonInChargeResult(
                formId,
                () => new PersonInChargeResultEntity
                {
                    FormId = formId,
                    ProcessStartTime = now,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = now
                },
                () => _db.Updateable<PersonInChargeResultEntity>()
                        .SetColumns(result => new PersonInChargeResultEntity
                        {
                            ProcessStartTime = now,
                            ProcessEndTime = null,
                            HandlerId = null,
                            Rating = null,
                            ModifiedBy = _loginuser.UserId,
                            ModifiedDate = now
                        }).Where(result => result.FormId == formId)
                        .ExecuteCommandAsync());

            return Result<bool>.Ok(affected >= 1);
        }

        /// <summary>
        /// 处理完成：匹配登录人（或其代理人）配置的负责类别，写入处理人与结束时间；重新完成时清空评分
        /// </summary>
        public async Task<Result<bool>> InforRequestProcessEnd(long formId)
        {
            var now = DateTime.Now;

            var isHandler = await _db.Queryable<InformationRequestEntity>()
                                     .With(SqlWith.NoLock)
                                     .InnerJoin<PersonInChargeConfigEntity>((info, config) => info.Category == config.System && config.Type == "InforRequest")
                                     .LeftJoin<UserAgentEntity>((info, config, agent) =>
                                         config.PersonChargeId == agent.SubstituteUserId &&
                                         agent.StartTime <= now &&
                                         agent.EndTime >= now)
                                     .Where((info, config, agent) =>
                                         info.FormId == formId &&
                                         (config.PersonChargeId == _loginuser.UserId || agent.AgentUserId == _loginuser.UserId))
                                     .AnyAsync();

            if (!isHandler)
            {
                return Result<bool>.Ok(false);
            }

            var affected = await UpsertPersonInChargeResult(
                formId,
                () => new PersonInChargeResultEntity
                {
                    FormId = formId,
                    ProcessEndTime = now,
                    HandlerId = _loginuser.UserId,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = now
                },
                () => _db.Updateable<PersonInChargeResultEntity>()
                        .SetColumns(result => new PersonInChargeResultEntity
                        {
                            ProcessEndTime = now,
                            HandlerId = _loginuser.UserId,
                            Rating = null,
                            ModifiedBy = _loginuser.UserId,
                            ModifiedDate = now
                        }).Where(result => result.FormId == formId)
                        .ExecuteCommandAsync());

            return Result<bool>.Ok(affected >= 1);
        }

        /// <summary>
        /// 评分：写入表单评分（已有数据则更新）
        /// </summary>
        public async Task<Result<bool>> InforRequestRating(long formId)
        {
            var rating = await _db.Queryable<InformationRequestEntity>()
                                  .Where(info => info.FormId == formId)
                                  .Select(info => info.Rating)
                                  .FirstAsync();

            var affected = await UpsertPersonInChargeResult(
                formId,
                () => new PersonInChargeResultEntity
                {
                    FormId = formId,
                    Rating = rating,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = DateTime.Now
                },
                () => _db.Updateable<PersonInChargeResultEntity>()
                        .SetColumns(result => new PersonInChargeResultEntity
                        {
                            Rating = rating,
                            ModifiedBy = _loginuser.UserId,
                            ModifiedDate = DateTime.Now
                        }).Where(result => result.FormId == formId)
                        .ExecuteCommandAsync());

            return Result<bool>.Ok(affected >= 1);
        }

        /// <summary>
        /// PersonInHargeResult 按 FormId 新增或修改
        /// </summary>
        private async Task<int> UpsertPersonInChargeResult(long formId, Func<PersonInChargeResultEntity> buildInsertEntity, Func<Task<int>> update)
        {
            var exists = await _db.Queryable<PersonInChargeResultEntity>()
                                  .With(SqlWith.NoLock)
                                  .AnyAsync(result => result.FormId == formId);

            if (exists)
            {
                return await update();
            }

            return await _db.Insertable(buildInsertEntity()).ExecuteCommandAsync();
        }

        #endregion

        #region 系统账号申请单

        /// <summary>
        /// 提交：只新增一笔未指定处理人的结果（处理人随时可能变动，由处理完成、评分再同步）；重新提交时先清掉旧结果
        /// </summary>
        public async Task<Result<bool>> SystemAccountApply(long formId)
        {
            var formInstance = await _db.Queryable<FormInstanceEntity>()
                                        .FirstAsync(instance => instance.FormId == formId);

            await _db.Deleteable<PersonInChargeResultEntity>()
                     .Where(result => result.FormId == formId)
                     .ExecuteCommandAsync();

            var affected = await _db.Insertable(new PersonInChargeResultEntity
            {
                FormId = formId,
                ApplicantUserId = formInstance.ApplicantUserId,
                EstimatedDays = 0,
                CreatedBy = _loginuser.UserId,
                CreatedDate = DateTime.Now
            }).ExecuteCommandAsync();

            return Result<bool>.Ok(affected >= 1);
        }

        /// <summary>
        /// 处理开始：写入开始时间；重新开始时重置为一笔未指定处理人的结果
        /// </summary>
        public async Task<Result<bool>> SystemAccountProcessStart(long formId)
        {
            var now = DateTime.Now;

            var existing = await GetSystemAccountResults(formId);
            long? applicantUserId = existing.FirstOrDefault()?.ApplicantUserId;
            if (applicantUserId == null)
            {
                applicantUserId = await _db.Queryable<FormInstanceEntity>()
                                           .Where(instance => instance.FormId == formId)
                                           .Select(instance => instance.ApplicantUserId)
                                           .FirstAsync();
            }

            var affected = await ResetSystemAccountResult(formId, existing, applicantUserId, existing.FirstOrDefault()?.EstimatedDays, now);

            return Result<bool>.Ok(affected >= 1);
        }

        /// <summary>
        /// 处理完成：先同步负责人变动，再只更新登录人（或其代理的负责人）那一笔，清空评分
        /// </summary>
        public async Task<Result<bool>> SystemAccountProcessEnd(long formId)
        {
            var now = DateTime.Now;

            // 登录人本人或其代理的负责人
            var ownerIds = await _db.Queryable<SystemAccountRequestDetailsEntity>()
                                    .With(SqlWith.NoLock)
                                    .InnerJoin<PersonInChargeConfigEntity>((detail, config) => detail.System == config.System && config.Type == "SystemAccount")
                                    .LeftJoin<UserAgentEntity>((detail, config, agent) =>
                                        config.PersonChargeId == agent.SubstituteUserId &&
                                        agent.StartTime <= now &&
                                        agent.EndTime >= now)
                                    .Where((detail, config, agent) =>
                                        detail.FormId == formId &&
                                        (config.PersonChargeId == _loginuser.UserId || agent.AgentUserId == _loginuser.UserId))
                                    .Select((detail, config, agent) => config.PersonChargeId)
                                    .ToListAsync();

            if (ownerIds.Count == 0)
            {
                return Result<bool>.Ok(false);
            }

            var (affected, rowOfOwner) = await SyncSystemAccountResults(formId);

            // 登录人为代理人时处理人记为登录人，且只占一笔
            var targetHandlers = ownerIds.Distinct()
                                         .Where(owner => rowOfOwner.ContainsKey(owner))
                                         .Select(owner => rowOfOwner[owner])
                                         .Distinct()
                                         .ToList();

            var results = await GetSystemAccountResults(formId);
            var loginClaimed = results.Any(result => result.HandlerId == _loginuser.UserId);

            foreach (var handlerId in targetHandlers)
            {
                var claim = !loginClaimed && handlerId != _loginuser.UserId;
                if (claim)
                {
                    loginClaimed = true;
                    affected += await _db.Updateable<PersonInChargeResultEntity>()
                                         .SetColumns(result => new PersonInChargeResultEntity
                                         {
                                             HandlerId = _loginuser.UserId,
                                             ProcessEndTime = now,
                                             Rating = null,
                                             ModifiedBy = _loginuser.UserId,
                                             ModifiedDate = now
                                         }).Where(SystemAccountResultOf(formId, handlerId))
                                         .ExecuteCommandAsync();
                }
                else
                {
                    affected += await _db.Updateable<PersonInChargeResultEntity>()
                                         .SetColumns(result => new PersonInChargeResultEntity
                                         {
                                             ProcessEndTime = now,
                                             Rating = null,
                                             ModifiedBy = _loginuser.UserId,
                                             ModifiedDate = now
                                         }).Where(SystemAccountResultOf(formId, handlerId))
                                         .ExecuteCommandAsync();
                }
            }

            return Result<bool>.Ok(affected >= 1);
        }

        /// <summary>
        /// 查询表单下全部负责人结果，按创建时间排序
        /// </summary>
        private async Task<List<PersonInChargeResultEntity>> GetSystemAccountResults(long formId)
        {
            return await _db.Queryable<PersonInChargeResultEntity>()
                            .Where(result => result.FormId == formId)
                            .OrderBy(result => result.CreatedDate)
                            .ToListAsync();
        }

        /// <summary>
        /// 重置为一笔未指定处理人的结果：保留第一笔并清空阶段字段，其余删除
        /// </summary>
        private async Task<int> ResetSystemAccountResult(long formId, List<PersonInChargeResultEntity> existing, long? applicantUserId, decimal? estimatedDays, DateTime? processStartTime)
        {
            var now = DateTime.Now;

            if (existing.Count == 0)
            {
                return await _db.Insertable(new PersonInChargeResultEntity
                {
                    FormId = formId,
                    ApplicantUserId = applicantUserId,
                    EstimatedDays = estimatedDays,
                    ProcessStartTime = processStartTime,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = now
                }).ExecuteCommandAsync();
            }

            foreach (var other in existing.Skip(1))
            {
                await _db.Deleteable<PersonInChargeResultEntity>()
                         .Where(SystemAccountResultOf(formId, other.HandlerId))
                         .ExecuteCommandAsync();
            }

            return await _db.Updateable<PersonInChargeResultEntity>()
                            .SetColumns(result => new PersonInChargeResultEntity
                            {
                                ApplicantUserId = applicantUserId,
                                EstimatedDays = estimatedDays,
                                ProcessStartTime = processStartTime,
                                ProcessEndTime = null,
                                HandlerId = null,
                                Rating = null,
                                ModifiedBy = _loginuser.UserId,
                                ModifiedDate = now
                            }).Where(SystemAccountResultOf(formId, existing[0].HandlerId))
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 评分：不调整负责人；每笔结果取其负责系统的平均分，负责人已换人、找不到对应系统的取全单平均分
        /// </summary>
        public async Task<Result<bool>> SystemAccountRating(long formId)
        {
            var now = DateTime.Now;

            var allRatings = await _db.Queryable<SystemAccountRequestDetailsEntity>()
                                      .With(SqlWith.NoLock)
                                      .Where(detail => detail.FormId == formId)
                                      .Select(detail => detail.Rating)
                                      .ToListAsync();
            var formAverage = AverageRating(allRatings);

            // 当前各负责人名下系统的评分
            var ownerRatings = await _db.Queryable<SystemAccountRequestDetailsEntity>()
                                        .With(SqlWith.NoLock)
                                        .InnerJoin<PersonInChargeConfigEntity>((d, config) => d.System == config.System && config.Type == "SystemAccount")
                                        .Where((d, config) => d.FormId == formId)
                                        .Select((d, config) => new { config.PersonChargeId, d.Rating })
                                        .ToListAsync();

            var ownerIds = ownerRatings.Select(item => item.PersonChargeId).Distinct().ToList();
            var agents = await _db.Queryable<UserAgentEntity>()
                                  .With(SqlWith.NoLock)
                                  .Where(agent => ownerIds.Contains(agent.SubstituteUserId))
                                  .ToListAsync();

            var affected = 0;
            foreach (var row in await GetSystemAccountResults(formId))
            {
                // 处理人本人或处理时的代理人所对应的负责人
                var handledTime = row.ProcessEndTime ?? now;
                var owners = ownerIds.Where(owner => row.HandlerId == owner ||
                                                     agents.Any(agent => agent.SubstituteUserId == owner &&
                                                                         agent.AgentUserId == row.HandlerId &&
                                                                         agent.StartTime <= handledTime &&
                                                                         agent.EndTime >= handledTime))
                                     .ToList();

                var rowRating = owners.Count > 0
                    ? AverageRating(ownerRatings.Where(item => owners.Contains(item.PersonChargeId)).Select(item => item.Rating))
                    : formAverage;

                affected += await _db.Updateable<PersonInChargeResultEntity>()
                                     .SetColumns(result => new PersonInChargeResultEntity
                                     {
                                         Rating = rowRating,
                                         ModifiedBy = _loginuser.UserId,
                                         ModifiedDate = now
                                     }).Where(SystemAccountResultOf(formId, row.HandlerId))
                                     .ExecuteCommandAsync();
            }

            return Result<bool>.Ok(affected >= 1);
        }

        /// <summary>
        /// 按当前系统负责人同步每人一笔的结果，返回 负责人 -> 结果的处理人Id
        /// 仍负责的保留，不再负责的改给新负责人或删除，新增的负责人新增一笔
        /// </summary>
        private async Task<(int Affected, Dictionary<long, long?> RowOfOwner)> SyncSystemAccountResults(long formId)
        {
            var now = DateTime.Now;
            var rowOfOwner = new Dictionary<long, long?>();

            var details = await _db.Queryable<SystemAccountRequestDetailsEntity>()
                                   .With(SqlWith.NoLock)
                                   .InnerJoin<PersonInChargeConfigEntity>((d, config) => d.System == config.System && config.Type == "SystemAccount")
                                   .Where((d, config) => d.FormId == formId)
                                   .Select((d, config) => config.PersonChargeId)
                                   .ToListAsync();

            if (details.Count == 0)
            {
                return (0, rowOfOwner);
            }

            var existing = await GetSystemAccountResults(formId);

            var ownerIds = details.Distinct().ToList();
            var agents = await _db.Queryable<UserAgentEntity>()
                                  .With(SqlWith.NoLock)
                                  .Where(agent => ownerIds.Contains(agent.SubstituteUserId))
                                  .ToListAsync();

            var affected = 0;
            var pendingOwners = new HashSet<long>(ownerIds);
            var pendingRows = new List<PersonInChargeResultEntity>(existing);

            // 仍属于当前负责人的结果（处理人为本人或代理人）
            foreach (var row in existing)
            {
                var owner = FindRatingOwner(row, pendingOwners, agents, now);
                if (owner == null)
                {
                    continue;
                }

                pendingOwners.Remove(owner.Value);
                pendingRows.Remove(row);
                rowOfOwner[owner.Value] = row.HandlerId;
            }

            // 负责人变更：旧结果改给新负责人，多余的删除
            var newOwners = pendingOwners.ToList();
            var reassignCount = Math.Min(pendingRows.Count, newOwners.Count);
            for (var i = 0; i < reassignCount; i++)
            {
                var newOwner = newOwners[i];
                rowOfOwner[newOwner] = newOwner;
                affected += await _db.Updateable<PersonInChargeResultEntity>()
                                     .SetColumns(result => new PersonInChargeResultEntity
                                     {
                                         HandlerId = newOwner,
                                         ModifiedBy = _loginuser.UserId,
                                         ModifiedDate = now
                                     }).Where(SystemAccountResultOf(formId, pendingRows[i].HandlerId))
                                     .ExecuteCommandAsync();
            }

            foreach (var row in pendingRows.Skip(reassignCount))
            {
                affected += await _db.Deleteable<PersonInChargeResultEntity>()
                                     .Where(SystemAccountResultOf(formId, row.HandlerId))
                                     .ExecuteCommandAsync();
            }

            // 新增负责人：沿用表单级字段
            var template = existing.FirstOrDefault();
            foreach (var newOwner in newOwners.Skip(reassignCount))
            {
                rowOfOwner[newOwner] = newOwner;
                affected += await _db.Insertable(new PersonInChargeResultEntity
                {
                    FormId = formId,
                    ApplicantUserId = template?.ApplicantUserId,
                    HandlerId = newOwner,
                    EstimatedDays = template?.EstimatedDays,
                    ProcessStartTime = template?.ProcessStartTime,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = now
                }).ExecuteCommandAsync();
            }

            return (affected, rowOfOwner);
        }

        /// <summary>
        /// 结果对应的当前负责人：处理人本人，或处理时的代理人；无则返回 null
        /// </summary>
        private static long? FindRatingOwner(PersonInChargeResultEntity row, HashSet<long> pendingOwners, List<UserAgentEntity> agents, DateTime now)
        {
            if (!row.HandlerId.HasValue)
            {
                return null;
            }

            if (pendingOwners.Contains(row.HandlerId.Value))
            {
                return row.HandlerId.Value;
            }

            var handledTime = row.ProcessEndTime ?? now;
            return agents.Where(agent => agent.AgentUserId == row.HandlerId.Value &&
                                         pendingOwners.Contains(agent.SubstituteUserId) &&
                                         agent.StartTime <= handledTime &&
                                         agent.EndTime >= handledTime)
                         .Select(agent => (long?)agent.SubstituteUserId)
                         .FirstOrDefault();
        }

        /// <summary>
        /// 按处理人定位同表单的结果，处理人为空时按 IS NULL
        /// </summary>
        private static System.Linq.Expressions.Expression<Func<PersonInChargeResultEntity, bool>> SystemAccountResultOf(long formId, long? handlerId)
        {
            var id = handlerId ?? 0;
            return handlerId.HasValue
                ? result => result.FormId == formId && result.HandlerId == id
                : result => result.FormId == formId && result.HandlerId == null;
        }

        /// <summary>
        /// 平均分：忽略未评分，四舍五入；全部未评分返回 null
        /// </summary>
        private static int? AverageRating(IEnumerable<int?> ratings)
        {
            var validRatings = ratings.Where(rating => rating.HasValue).Select(rating => rating!.Value).ToList();
            return validRatings.Count > 0
                ? (int)Math.Round(validRatings.Average(), MidpointRounding.AwayFromZero)
                : null;
        }

        #endregion

        /// <summary>
        /// 按自然年拆分时间段并累计工时（8-12、13-17，午休不计）
        /// </summary>
        private static Dictionary<int, double> CalcHoursByYear(DateTime startTime, DateTime endTime)
        {
            var hoursByYear = new Dictionary<int, double>();

            var currentDate = startTime.Date;
            var endDate = endTime.Date;

            while (currentDate <= endDate)
            {
                var dayStart = currentDate;
                var dayEnd = currentDate.AddDays(1).AddTicks(-1);
                var effectiveStart = dayStart < startTime ? startTime : dayStart;
                var effectiveEnd = dayEnd > endTime ? endTime : dayEnd;

                if (effectiveStart <= effectiveEnd)
                {
                    double dayHours = 0;

                    var morningStart = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 8, 0, 0);
                    var morningEnd = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 12, 0, 0);
                    var morningCalculated = (morningStart > effectiveStart ? morningStart : effectiveStart);
                    var morningEnd2 = (morningEnd < effectiveEnd ? morningEnd : effectiveEnd);
                    if (morningCalculated < morningEnd2)
                    {
                        dayHours += (morningEnd2 - morningCalculated).TotalHours;
                    }

                    var afternoonStart = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 13, 0, 0);
                    var afternoonEnd = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 17, 0, 0);
                    var afternoonCalculated = (afternoonStart > effectiveStart ? afternoonStart : effectiveStart);
                    var afternoonEnd2 = (afternoonEnd < effectiveEnd ? afternoonEnd : effectiveEnd);
                    if (afternoonCalculated < afternoonEnd2)
                    {
                        dayHours += (afternoonEnd2 - afternoonCalculated).TotalHours;
                    }

                    if (dayHours > 0)
                    {
                        var year = currentDate.Year;
                        if (hoursByYear.ContainsKey(year))
                        {
                            hoursByYear[year] += dayHours;
                        }
                        else
                        {
                            hoursByYear[year] = dayHours;
                        }
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            return hoursByYear;
        }
    }
}
