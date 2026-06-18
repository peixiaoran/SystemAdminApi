using SqlSugar;
using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Options;
using SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Dto;
using SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Entity;
using SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Queries;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.HR.BasicInfo.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;

namespace SystemAdmin.Repository.FormBusiness.Forms
{
    public class LeaveFormRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        public LeaveFormRepository(SqlSugarScope db, Language lang)
        {
            _db = db;
            _lang = lang;
        }

        /// <summary>
        /// 请假类别下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<LeaveTypeDropDto>> GetLeaveTypeDrop()
        {
            return await _db.Queryable<DictionaryInfoEntity>()
                            .Where(dic => dic.DicType == "LeaveType")
                            .Select(dic => new LeaveTypeDropDto()
                            {
                                LeaveType = dic.DicCode,
                                LeaveTypeName = _lang.Locale == "zh-CN"
                                                ? dic.DicNameCn
                                                : dic.DicNameEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 部门树下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<DepartmentDropDto>> GetDepartmentDrop()
        {
            return await _db.Queryable<DepartmentInfoEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<DepartmentLevelEntity>((dept, deptlevel) => dept.DepartmentLevelId == deptlevel.DepartmentLevelId)
                            .OrderBy((dept, deptlevel) => deptlevel.SortOrder)
                            .Select((dept, deptlevel) => new DepartmentDropDto
                            {
                                DepartmentId = dept.DepartmentId,
                                DepartmentName = _lang.Locale == "zh-CN"
                                                 ? dept.DepartmentNameCn
                                                 : dept.DepartmentNameEn,
                                ParentId = dept.ParentId,
                            }).ToTreeAsync(menu => menu.DepartmentChildList, menu => menu.ParentId, 0);
        }

        /// <summary>
        /// 查询可代理用户
        /// </summary>
        /// <param name="getPage"></param>
        /// <param name="loginUserId"></param>
        /// <returns></returns>
        public async Task<ResultPaged<AgentUserInfoDto>> GetUserInfoAgentView(GetAgentUserPage getPage, long loginUserId)
        {
            RefAsync<int> totalCount = 0;
            var query = _db.Queryable<UserInfoEntity>()
                           .With(SqlWith.NoLock)
                           .InnerJoin<DepartmentInfoEntity>((user, dept) => user.DepartmentId == dept.DepartmentId)
                           .InnerJoin<PositionInfoEntity>((user, dept, position) => user.PositionId == position.PositionId)
                           .InnerJoin<UserLaborEntity>((user, dept, position, labor) => user.LaborId == labor.LaborId)
                           .InnerJoin<NationalityInfoEntity>((user, dept, position, labor, nation) => user.Nationality == nation.NationId)
                           .Where((user, dept, position, labor, nation) => user.IsEmployed == 1 && user.IsAgent == 0 && user.IsFreeze == 0 && user.UserId != loginUserId);

            // 用户工号
            if (!string.IsNullOrEmpty(getPage.UserNo))
            {
                query = query.Where((user, dept, position, labor, nation) =>
                    user.UserNo.Contains(getPage.UserNo));
            }
            // 用户姓名
            if (!string.IsNullOrEmpty(getPage.UserName))
            {
                query = query.Where((user, dept, position, labor, nation) =>
                    user.UserNameCn.Contains(getPage.UserName) ||
                    user.UserNameEn.Contains(getPage.UserName));
            }
            // 部门Id - 仅当工号和姓名都为空时才应用
            if (string.IsNullOrEmpty(getPage.UserNo) &&
                string.IsNullOrEmpty(getPage.UserName) &&
                !string.IsNullOrEmpty(getPage.DepartmentId) &&
                long.Parse(getPage.DepartmentId) > 0)
            {
                query = query.Where((user, dept, position, labor, nation) =>
                    user.DepartmentId == long.Parse(getPage.DepartmentId));
            }

            // 排序
            query = query.OrderBy((user, dept, position, labor, nation) => user.UserId);

            var page = await query.Select((user, dept, position, labor, nation) => new AgentUserInfoDto
            {
                UserId = user.UserId,
                UserNo = user.UserNo,
                UserName = _lang.Locale == "zh-CN"
                           ? user.UserNameCn
                           : user.UserNameEn,
                DepartmentName = _lang.Locale == "zh-CN"
                           ? dept.DepartmentNameCn
                           : dept.DepartmentNameEn
            }).ToPageListAsync(getPage.PageIndex, getPage.PageSize, totalCount);

            return ResultPaged<AgentUserInfoDto>.Ok(page, totalCount, "");
        }

        /// <summary>
        /// 查询假期基础余额
        /// </summary>
        public async Task<List<UserLeaveBalanceEntity>> GetApplicantLeaveBalances(long formId, List<int> yearList)
        {
            var balanceList = await _db.Queryable<UserLeaveBalanceEntity>()
                                       .With(SqlWith.NoLock)
                                       .InnerJoin<FormInstanceEntity>((balance, instance) => balance.UserId == instance.ApplicantUserId)
                                       .Where((balance, instance) => instance.FormId == formId)
                                       .WhereIF(yearList.Count > 0, (balance, instance) => yearList.Contains(balance.Year))
                                       .Select((balance, instance) => balance)
                                       .ToListAsync();
            return balanceList;
        }

        /// <summary>
        /// 查询审批中的请假单
        /// </summary>
        public async Task<List<LeaveFormEntity>> GetApplicantPendingLeaves(long formId)
        {
            var list = await _db.Queryable<LeaveFormEntity>()
                                .With(SqlWith.NoLock)
                                .InnerJoin<FormInstanceEntity>((leave, instance) => leave.FormId == instance.FormId)
                                .InnerJoin<FormInstanceEntity>((leave, instance, applicant) => applicant.FormId == formId)
                                .Where((leave, instance, applicant) => instance.ApplicantUserId == applicant.ApplicantUserId
                                    && instance.FormStatus == FormStatus.UnderReview.ToEnumString())
                                .Select((leave, instance, applicant) => leave)
                                .ToListAsync();
            return list;
        }

        /// <summary>
        /// 查询当前表单的请假信息
        /// </summary>
        public async Task<LeaveFormEntity>  GetCurrentLeaveForm(long formId)
        {
            return await _db.Queryable<LeaveFormEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<FormInstanceEntity>((leave, instance) => leave.FormId == instance.FormId)
                            .Where((leave, instance) => instance.FormId == formId)
                            .Select((leave, instance) => leave)
                            .FirstAsync();
        }

        /// <summary>
        /// 查询假别字典
        /// </summary>
        public async Task<List<DictionaryInfoEntity>> GetLeaveTypeDictionary()
        {
            var list = await _db.Queryable<DictionaryInfoEntity>()
                                .With(SqlWith.NoLock)
                                .Where(dic => dic.DicType == "LeaveType")
                                .ToListAsync();
            return list;
        }

        /// <summary>
        /// 初始化请假表单
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> InitLeaveForm(LeaveFormEntity entity)
        {
            return await _db.Insertable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 保存请假表单
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> SaveLeaveForm(LeaveFormEntity entity)
        {
            return await _db.Updateable(entity)
                            .IgnoreColumns(leave => new
                            {
                                leave.FormId,
                                leave.CreatedBy,
                                leave.CreatedDate,
                            }).Where(leave => leave.FormId == entity.FormId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 查询请假单明细
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<LeaveFormDto> GetLeaveForm(long formId)
        {
            return await _db.Queryable<FormInstanceEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<LeaveFormEntity>((form, leave) => form.FormId == leave.FormId)
                            .InnerJoin<UserInfoEntity>((form, leave, user) => form.ApplicantUserId == user.UserId)
                            .InnerJoin<DepartmentInfoEntity>((form, leave, user, dept) => user.DepartmentId == dept.DepartmentId)
                            .InnerJoin<DictionaryInfoEntity>((form, leave, user, dept, dic) => dic.DicType == "FormStatus" && form.FormStatus == dic.DicCode)
                            .Where((form, leave, user, dept, dic) => form.FormId == formId)
                            .Select((form, leave, user, dept, dic) => new LeaveFormDto()
                            {
                                FormTypeId = form.FormTypeId,
                                RuleId = form.RuleId,
                                CurrentStepId = form.CurrentStepId,
                                FormStatus = form.FormStatus,
                                FormStatusName = _lang.Locale == "zh-CN"
                                                 ? dic.DicNameCn
                                                 : dic.DicNameEn,
                                FormId = form.FormId,
                                FormNo = form.FormNo,
                                ApplicantUserNo = user.UserNo,
                                ApplicantUserName = _lang.Locale == "zh-CN"
                                                 ? user.UserNameCn
                                                 : user.UserNameEn,
                                ApplicantDeptName = _lang.Locale == "zh-CN"
                                                 ? dept.DepartmentNameCn
                                                 : dept.DepartmentNameEn,
                                ApplicantDate = form.ApplicantDate,
                                LeaveType = leave.LeaveType,
                                LeaveReason = leave.LeaveReason,
                                StartDateTime = leave.StartDateTime,
                                EndDateTime = leave.EndDateTime,
                                LeaveHours = leave.LeaveHours,
                                AgentUserId = leave.AgentUserId,
                                AgentUserName = leave.AgentUserName,
                            }).FirstAsync();
        }
    }
}
