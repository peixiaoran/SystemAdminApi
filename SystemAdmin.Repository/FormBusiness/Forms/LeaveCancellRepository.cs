using SqlSugar;
using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Options;
using SystemAdmin.Model.FormBusiness.Forms.LeaveCancell.Dto;
using SystemAdmin.Model.FormBusiness.Forms.LeaveCancell.Entity;
using SystemAdmin.Model.FormBusiness.Forms.LeaveCancell.Queries;
using SystemAdmin.Model.FormBusiness.Forms.LeaveRequest.Entity;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;

namespace SystemAdmin.Repository.FormBusiness.Forms
{
    public class LeaveCancellRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        public LeaveCancellRepository(SqlSugarScope db, Language lang)
        {
            _db = db;
            _lang = lang;
        }

        /// <summary>
        /// 初始化销假表单
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> InitLeaveCancell(LeaveCancellEntity entity)
        {
            return await _db.Insertable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 保存销假表单
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> SaveLeaveCancell(LeaveCancellEntity entity)
        {
            return await _db.Updateable(entity)
                            .IgnoreColumns(cancell => new
                            {
                                cancell.FormId,
                                cancell.CreatedBy,
                                cancell.CreatedDate,
                            }).Where(cancell => cancell.FormId == entity.FormId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 查询销假单明细
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<LeaveCancellDto> GetLeaveCancell(long formId)
        {
            return await _db.Queryable<FormInstanceEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<LeaveCancellEntity>((form, cancell) => form.FormId == cancell.FormId)
                            .InnerJoin<UserInfoEntity>((form, cancell, user) => form.ApplicantUserId == user.UserId)
                            .InnerJoin<DepartmentInfoEntity>((form, cancell, user, dept) => user.DepartmentId == dept.DepartmentId)
                            .LeftJoin<LeaveRequestEntity>((form, cancell, user, dept, leave) => cancell.LeaveRequestId == leave.FormId)
                            .Where((form, cancell, user, dept, leave) => form.FormId == formId)
                            .Select((form, cancell, user, dept, leave) => new LeaveCancellDto()
                            {
                                FormStatus = form.FormStatus,
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
                                LeaveRequestId = cancell.LeaveRequestId,
                                LeaveRequestNo = cancell.LeaveRequestNo,
                                LeaveStartDateTime = leave.StartDateTime,
                                LeaveEndDateTime = leave.EndDateTime,
                                LeaveHours = leave.LeaveHours,
                                StartDateTime = cancell.StartDateTime,
                                EndDateTime = cancell.EndDateTime,
                                CancellHours = cancell.CancellHours,
                            }).FirstAsync();
        }

        /// <summary>
        /// 查询登录用户已批准的请假单（今年之前结束的不能销假）
        /// </summary>
        /// <param name="getPage"></param>
        /// <param name="userId"></param>
        /// <param name="yearStart"></param>
        /// <returns></returns>
        public async Task<List<LeaveRequestDto>> GetUserApprovedLeaveRequests(GetLeaveRequestPage getPage, long userId, DateTime yearStart)
        {
            var query = _db.Queryable<FormInstanceEntity>()
                           .With(SqlWith.NoLock)
                           .InnerJoin<LeaveRequestEntity>((form, leave) => form.FormId == leave.FormId)
                           .Where((form, leave) => form.ApplicantUserId == userId
                                                && form.FormStatus == FormStatus.Approved.ToEnumString()
                                                && leave.EndDateTime >= yearStart);

            // 请假单号
            if (!string.IsNullOrEmpty(getPage.LeaveRequestNo))
            {
                query = query.Where((form, leave) => form.FormNo.Contains(getPage.LeaveRequestNo));
            }
            // 开始时间
            if (getPage.StartDate != default)
            {
                query = query.Where((form, leave) => leave.StartDateTime >= getPage.StartDate);
            }
            // 结束时间
            if (getPage.EndDate != default)
            {
                query = query.Where((form, leave) => leave.EndDateTime <= getPage.EndDate);
            }

            return await query.OrderByDescending((form, leave) => leave.StartDateTime)
                              .Select((form, leave) => new LeaveRequestDto()
                              {
                                  LeaveRequestId = form.FormId,
                                  LeaveRequestNo = form.FormNo,
                                  LeaveType = leave.LeaveType,
                                  StartDateTime = leave.StartDateTime,
                                  EndDateTime = leave.EndDateTime,
                                  LeaveHours = leave.LeaveHours,
                                  ApplicantDate = form.ApplicantDate,
                              }).ToListAsync();
        }

        /// <summary>
        /// 查询请假单已绑定的销假单（审批中、已批准）
        /// </summary>
        /// <param name="leaveRequestIds"></param>
        /// <returns></returns>
        public async Task<List<LeaveCancellEntity>> GetBoundLeaveCancells(List<long> leaveRequestIds)
        {
            return await _db.Queryable<LeaveCancellEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<FormInstanceEntity>((cancell, instance) => cancell.FormId == instance.FormId)
                            .Where((cancell, instance) => leaveRequestIds.Contains(cancell.LeaveRequestId)
                                                       && (instance.FormStatus == FormStatus.UnderReview.ToEnumString()
                                                        || instance.FormStatus == FormStatus.Approved.ToEnumString()))
                            .Select((cancell, instance) => cancell)
                            .ToListAsync();
        }

        /// <summary>
        /// 查询请假单号
        /// </summary>
        /// <param name="leaveRequestId"></param>
        /// <returns></returns>
        public async Task<string> GetLeaveRequestNo(long leaveRequestId)
        {
            return await _db.Queryable<FormInstanceEntity>()
                            .With(SqlWith.NoLock)
                            .Where(instance => instance.FormId == leaveRequestId)
                            .Select(instance => instance.FormNo)
                            .FirstAsync();
        }
    }
}
