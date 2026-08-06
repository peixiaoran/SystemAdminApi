using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.CompreQuery.Dto;
using SystemAdmin.Model.FormBusiness.CompreQuery.Queries;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Entity;
using SystemAdmin.Model.FormBusiness.FormOperate.Entity;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.FormBusiness.FormWorkflow.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Entity;
using System.Data;

namespace SystemAdmin.Repository.FormBusiness.CompreQuery
{
    public class BasicQueryRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        public BasicQueryRepository(SqlSugarScope db, Language lang)
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
                                FormStatus = dic.DicCode,
                                FormStatusName = _lang.Locale == "zh-CN"
                                                ? dic.DicNameCn
                                                : dic.DicNameEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 查询表单分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<FormQueryDto>> GetFormQueryPage(GetFormQueryPage getPage)
        {
            RefAsync<int> totalCount = 0;
            var query = _db.Queryable<FormInstanceEntity>()
                           .With(SqlWith.NoLock)
                           .InnerJoin<DictionaryInfoEntity>((instance, dic) => dic.DicType == "FormStatus" && instance.FormStatus == dic.DicCode)
                           .InnerJoin<FormTypeEntity>((instance, dic, formtype) => instance.FormTypeId == formtype.FormTypeId)
                           .InnerJoin<UserInfoEntity>((instance, dic, formtype, applyuser) => instance.ApplicantUserId == applyuser.UserId)
                           .InnerJoin<DepartmentInfoEntity>((instance, dic, formtype, applyuser, applydept) => applyuser.DepartmentId == applydept.DepartmentId);

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
            // 表单单号
            if (!string.IsNullOrEmpty(getPage.FormNo))
            {
                query = query.Where((instance, dic, formtype, applyuser, applydept) =>
                    instance.FormNo.Contains(getPage.FormNo));
            }
            // 表单状态
            if (!string.IsNullOrEmpty(getPage.FormStatus))
            {
                query = query.Where((instance, dic, formtype, applyuser, applydept) =>
                    instance.FormStatus == getPage.FormStatus);
            }
            // 申请日期范围
            if (getPage.StartDate.HasValue)
            {
                query = query.Where((instance, dic, formtype, applyuser, applydept) =>
                    instance.ApplicantDate >= getPage.StartDate.Value);
            }
            if (getPage.EndDate.HasValue)
            {
                query = query.Where((instance, dic, formtype, applyuser, applydept) =>
                    instance.ApplicantDate <= getPage.EndDate.Value);
            }

            // 排序：按申请日期倒序
            query = query.OrderBy((instance, dic, formtype, applyuser, applydept) => instance.ApplicantDate, OrderByType.Desc);

            var page = await query.Select((instance, dic, formtype, applyuser, applydept) => new FormQueryDto
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
            return ResultPaged<FormQueryDto>.Ok(page, totalCount, "");
        }

        /// <summary>
        /// 导出全部表单查询Excel（字段同 GetFormQueryPage，不分页）
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<DataTable> GetFormQueryExcel(GetFormQueryPage getPage)
        {
            var query = _db.Queryable<FormInstanceEntity>()
                           .With(SqlWith.NoLock)
                           .InnerJoin<DictionaryInfoEntity>((instance, dic) => dic.DicType == "FormStatus" && instance.FormStatus == dic.DicCode)
                           .InnerJoin<FormTypeEntity>((instance, dic, formtype) => instance.FormTypeId == formtype.FormTypeId)
                           .InnerJoin<UserInfoEntity>((instance, dic, formtype, applyuser) => instance.ApplicantUserId == applyuser.UserId)
                           .InnerJoin<DepartmentInfoEntity>((instance, dic, formtype, applyuser, applydept) => applyuser.DepartmentId == applydept.DepartmentId);

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
            // 表单单号
            if (!string.IsNullOrEmpty(getPage.FormNo))
            {
                query = query.Where((instance, dic, formtype, applyuser, applydept) =>
                    instance.FormNo.Contains(getPage.FormNo));
            }
            // 表单状态
            if (!string.IsNullOrEmpty(getPage.FormStatus))
            {
                query = query.Where((instance, dic, formtype, applyuser, applydept) =>
                    instance.FormStatus == getPage.FormStatus);
            }
            // 申请日期范围
            if (getPage.StartDate.HasValue)
            {
                query = query.Where((instance, dic, formtype, applyuser, applydept) =>
                    instance.ApplicantDate >= getPage.StartDate.Value);
            }
            if (getPage.EndDate.HasValue)
            {
                query = query.Where((instance, dic, formtype, applyuser, applydept) =>
                    instance.ApplicantDate <= getPage.EndDate.Value);
            }

            // 排序：按申请日期倒序
            query = query.OrderBy((instance, dic, formtype, applyuser, applydept) => instance.ApplicantDate, OrderByType.Desc);

            return await query.Select((instance, dic, formtype, applyuser, applydept) => new FormQueryDto
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
            }).ToDataTableAsync();
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
