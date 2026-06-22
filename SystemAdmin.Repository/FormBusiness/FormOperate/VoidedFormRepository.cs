using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;
using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Options;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Entity;
using SystemAdmin.Model.FormBusiness.FormOperate.Dto;
using SystemAdmin.Model.FormBusiness.FormOperate.Entity;
using SystemAdmin.Model.FormBusiness.FormOperate.Queries;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Entity;

namespace SystemAdmin.Repository.FormBusiness.FormOperate
{
    public class VoidedFormRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        public VoidedFormRepository(SqlSugarScope db, Language lang)
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
        /// 查询待送审分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<VoidedFormDto>> GetVoidedFormsPage(GetVoidedFormPage getPage, long loginUserId)
        {
            RefAsync<int> totalCount = 0;
            var query = _db.Queryable<FormInstanceEntity>()
                           .InnerJoin<DictionaryInfoEntity>((instance, dic) => dic.DicType == "FormStatus" && dic.DicCode == instance.FormStatus && instance.FormStatus == FormStatus.Voided.ToEnumString())
                           .InnerJoin<FormTypeEntity>((instance, dic, formtype) => instance.FormTypeId == formtype.FormTypeId)
                           .LeftJoin<UserInfoEntity>((instance, dic, formtype, applyuser) => instance.ApplicantUserId == applyuser.UserId)
                           .LeftJoin<DepartmentInfoEntity>((instance, dic, formtype, applyuser, applyuserdept) => applyuser.DepartmentId == applyuserdept.DepartmentId)
                           .LeftJoin<UserAgentEntity>((instance, dic, formtype, applyuser, applyuserdept, useragent) => applyuser.UserId == useragent.SubstituteUserId && useragent.StartTime <= DateTime.Now && useragent.EndTime >= DateTime.Now)
                           .Where((instance, dic, formtype, applyuser, applyuserdept, useragent) => instance.ApplicantUserId == loginUserId || useragent.AgentUserId == loginUserId);

            // 表单组别Id
            if (!string.IsNullOrEmpty(getPage.FormGroupId) && long.Parse(getPage.FormGroupId) > 0)
            {
                query = query.Where((instance, dic, formtype, applyuser, applyuserdept, useragent) =>
                    formtype.FormGroupId == long.Parse(getPage.FormGroupId));
            }
            // 表单类别Id
            if (!string.IsNullOrEmpty(getPage.FormTypeId) && long.Parse(getPage.FormTypeId) > 0)
            {
                query = query.Where((instance, dic, formtype, applyuser, applyuserdept, useragent) =>
                    formtype.FormTypeId == long.Parse(getPage.FormTypeId));
            }

            // 排序
            query = query.OrderBy((instance, dic, formtype, applyuser, applyuserdept, useragent) => instance.CreatedDate);

            var page = await query.Select((instance, dic, formtype, applyuser, applyuserdept, useragent) => new VoidedFormDto
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
                ApplicantDate = instance.ApplicantDate,
                VoidedDate = instance.ModifiedDate!.Value,
                ViewPath = formtype.ViewPath
            }).ToPageListAsync(getPage.PageIndex, getPage.PageSize, totalCount);
            return ResultPaged<VoidedFormDto>.Ok(page, totalCount, "");
        }
    }
}
