using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.Forms.DocumentCirculate.Dto;
using SystemAdmin.Model.FormBusiness.Forms.DocumentCirculate.Entity;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;

namespace SystemAdmin.Repository.FormBusiness.Forms
{
    public class DocumentCirculateRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        public DocumentCirculateRepository(SqlSugarScope db, Language lang)
        {
            _db = db;
            _lang = lang;
        }

        /// <summary>
        /// 初始化传签表单
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> InitDocumentCirculate(DocumentCirculateEntity entity)
        {
            return await _db.Insertable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 保存传签表单
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> SaveDocumentCirculate(DocumentCirculateEntity entity)
        {
            return await _db.Updateable(entity)
                            .IgnoreColumns(circulate => new
                            {
                                circulate.FormId,
                                circulate.CreatedBy,
                                circulate.CreatedDate,
                            }).Where(circulate => circulate.FormId == entity.FormId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 查询传签单明细
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<DocumentCirculateDto> GetDocumentCirculate(long formId)
        {
            return await _db.Queryable<FormInstanceEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<DocumentCirculateEntity>((form, circulate) => form.FormId == circulate.FormId)
                            .InnerJoin<UserInfoEntity>((form, circulate, user) => form.ApplicantUserId == user.UserId)
                            .InnerJoin<DepartmentInfoEntity>((form, circulate, user, dept) => user.DepartmentId == dept.DepartmentId)
                            .InnerJoin<DictionaryInfoEntity>((form, circulate, user, dept, dic) => dic.DicType == "FormStatus" && form.FormStatus == dic.DicCode)
                            .Where((form, circulate, user, dept, dic) => form.FormId == formId)
                            .Select((form, circulate, user, dept, dic) => new DocumentCirculateDto()
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
                                IssueDept = circulate.IssueDept,
                                CirculationPurpose = circulate.CirculationPurpose,
                                ContentSummary = circulate.ContentSummary,
                            }).FirstAsync();
        }
    }
}
