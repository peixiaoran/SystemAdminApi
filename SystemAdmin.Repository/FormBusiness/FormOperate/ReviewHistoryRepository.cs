using SqlSugar;
using SystemAdmin.CommonSetup.Options;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Entity;
using SystemAdmin.Model.FormBusiness.FormOperate.Dto;
using SystemAdmin.Model.FormBusiness.FormOperate.Entity;
using SystemAdmin.Model.FormBusiness.FormOperate.Queries;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.FormBusiness.FormWorkflow.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Entity;

namespace SystemAdmin.Repository.FormBusiness.FormOperate
{
    public class ReviewHistoryRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        public ReviewHistoryRepository(SqlSugarScope db, Language lang)
        {
            _db = db;
            _lang = lang;
        }

        /// <summary>
        /// 表单组别下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<FormGroupDropDto>> GetFormGroupDrop()
        {
            return await _db.Queryable<FormGroupEntity>()
                            .With(SqlWith.NoLock)
                            .OrderBy(formgroup => formgroup.SortOrder)
                            .Select(formgroup => new FormGroupDropDto
                            {
                                FormGroupId = formgroup.FormGroupId,
                                FormGroupName = _lang.Locale == "zh-CN"
                                                ? formgroup.FormGroupNameCn
                                                : formgroup.FormGroupNameEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 表单类别下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<FormTypeDropDto>> GetFormTypeDrop(long formGroupId)
        {
            return await _db.Queryable<FormTypeEntity>()
                            .With(SqlWith.NoLock)
                            .Where(formgroup => formgroup.FormGroupId == formGroupId)
                            .OrderBy(formgroup => formgroup.SortOrder)
                            .Select(formgroup => new FormTypeDropDto
                            {
                                FormTypeId = formgroup.FormTypeId,
                                FormTypeName = _lang.Locale == "zh-CN"
                                               ? formgroup.FormTypeNameCn
                                               : formgroup.FormTypeNameEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 表单状态下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<FormStatusDropDto>> GetFormStatusDrop()
        {
            return await _db.Queryable<DictionaryInfoEntity>()
                            .With(SqlWith.NoLock)
                            .Where(dic => dic.DicType == "FormStatus")
                            .OrderBy(dic => dic.SortOrder)
                            .Select(dic => new FormStatusDropDto
                            {
                                FormStatusCode = dic.DicCode,
                                FormStatusName = _lang.Locale == "zh-CN"
                                                 ? dic.DicNameCn
                                                 : dic.DicNameEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 查询审批记录分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<FormHistoryDto>> GetReviewHistoryPage(GetFormHistoryPage getPage, long loginUserId)
        {
            RefAsync<int> totalCount = 0;
            var query = _db.Queryable<FormInstanceEntity>()
                           .With(SqlWith.NoLock)
                           .InnerJoin<DictionaryInfoEntity>((instance, dic) => dic.DicType == "FormStatus" && instance.FormStatus == dic.DicCode)
                           .InnerJoin<FormTypeEntity>((instance, dic, formtype) => instance.FormTypeId == formtype.FormTypeId)
                           .InnerJoin<UserInfoEntity>((instance, dic, formtype, applyuser) => instance.ApplicantUserId == applyuser.UserId)
                           .InnerJoin<DepartmentInfoEntity>((instance, dic, formtype, applyuser, applydept) => applyuser.DepartmentId == applydept.DepartmentId)
                           .Where((instance, dic, formtype, applyuser, applydept) =>
                               SqlFunc.Subqueryable<FormReviewRecordEntity>()
                                      .InnerJoin<WorkflowStepEntity>((record, step) => record.StepId == step.StepId)
                                      .Where((record, step) => record.FormId == instance.FormId && (record.OriginalUserId == loginUserId || record.OperationUserId == loginUserId) && step.IsStartStep != 1)
                           .Any());

            // 表单组别Id
            if (!string.IsNullOrEmpty(getPage.FormGroupId) && long.Parse(getPage.FormGroupId) > 0)
            {
                query = query.Where((instance, dic, formtype, applyuser, applydept) =>
                    formtype.FormGroupId == long.Parse(getPage.FormGroupId));
            }
            // 表单类别Id
            if (!string.IsNullOrEmpty(getPage.FormTypeId) && long.Parse(getPage.FormTypeId) > 0)
            {
                query = query.Where((instance, dic, formtype, applyuser, applydept) =>
                    formtype.FormTypeId == long.Parse(getPage.FormTypeId));
            }

            // 排序：按当前用户最新审批时间倒序
            query = query.OrderBy((instance, dic, formtype, applyuser, applydept) => 
                        SqlFunc.Subqueryable<FormReviewRecordEntity>()
                       .Where(record => record.FormId == instance.FormId && (record.OriginalUserId == loginUserId || record.OperationUserId == loginUserId))
                       .Max(record => record.ReviewDateTime), OrderByType.Desc);

            var page = await query.Select((instance, dic, formtype, applyuser, applydept) => new FormHistoryDto
            {
                FormId = instance.FormId,
                FormNo = instance.FormNo,
                FormTypeId = formtype.FormTypeId,
                FormTypeName = _lang.Locale == "zh-CN"
                               ? formtype.FormTypeNameCn
                               : formtype.FormTypeNameEn,
                FormStatus = instance.FormStatus,
                FormStatusName = _lang.Locale == "zh-CN"
                               ? dic.DicNameCn
                               : dic.DicNameEn,
                ApplyUserName = _lang.Locale == "zh-CN"
                               ? applyuser.UserNameCn
                               : applyuser.UserNameEn,
                ApplyUserDeptName = _lang.Locale == "zh-CN"
                               ? applydept.DepartmentNameCn
                               : applydept.DepartmentNameEn,
                ViewPath = formtype.ViewPath,
                ApplicantDate = instance.ApplicantDate
            }).ToPageListAsync(getPage.PageIndex, getPage.PageSize, totalCount);
            return ResultPaged<FormHistoryDto>.Ok(page, totalCount, "");
        }

        /// <summary>
        /// 查询待审批用户
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<List<FormPendingUserDto>> GetFormPendingUsers(long formId)
        {
            return await _db.Queryable<PendingReviewEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<WorkflowStepEntity>((pending, step) => pending.StepId == step.StepId)
                            .InnerJoin<DictionaryInfoEntity>((pending, step, dic) => dic.DicType == "AppointmentType" && pending.AppointmentType == dic.DicCode)
                            .InnerJoin<UserInfoEntity>((pending, step, dic, user) => pending.ReviewUserId == user.UserId)
                            .LeftJoin<UserAgentEntity>((pending, step, dic, user, useragent) => user.UserId == useragent.SubstituteUserId && useragent.StartTime <= DateTime.Now && useragent.EndTime >= DateTime.Now)
                            .LeftJoin<UserInfoEntity>((pending, step, dic, user, useragent, agentuser) => useragent.AgentUserId == agentuser.UserId)
                            .Where((pending, step, dic, user, useragent, agentuser) => pending.FormId == formId)
                            .Select((pending, step, dic, user, useragent, agentuser) => new FormPendingUserDto
                            {
                                StepName = _lang.Locale == "zh-CN"
                                           ? step.StepNameCn
                                           : step.StepNameEn,
                                AppointmentType = pending.AppointmentType,
                                AppointmentTypeName = _lang.Locale == "zh-CN"
                                           ? dic.DicNameCn
                                           : dic.DicNameEn,
                                ReviewUserName = _lang.Locale == "zh-CN"
                                           ? user.UserNameCn
                                           : user.UserNameEn,
                                AgentUserName = _lang.Locale == "zh-CN"
                                           ? agentuser.UserNameCn
                                           : agentuser.UserNameEn,
                            }).ToListAsync();
        }
    }
}
