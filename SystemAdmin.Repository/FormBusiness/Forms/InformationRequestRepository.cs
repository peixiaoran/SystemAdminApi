using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.Forms.InformationRequest.Dto;
using SystemAdmin.Model.FormBusiness.Forms.InformationRequest.Entity;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;
using SystemAdmin.Repository.FormBusiness.Workflow;

namespace SystemAdmin.Repository.FormBusiness.Forms
{
    public class InformationRequestRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;
        private readonly FormManager _formManager;

        public InformationRequestRepository(SqlSugarScope db, Language lang, FormManager formManager)
        {
            _db = db;
            _lang = lang;
            _formManager = formManager;
        }

        /// <summary>
        /// 需求类别下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<ITCategoryDropDto>> GetITCategoryDrop()
        {
            return await _db.Queryable<DictionaryInfoEntity>()
                            .With(SqlWith.NoLock)
                            .Where(dic => dic.DicType == "ITCategory")
                            .OrderBy(dic => dic.SortOrder)
                            .Select(dic => new ITCategoryDropDto()
                            {
                                Category = dic.DicCode,
                                CategoryName = _lang.Locale == "zh-CN"
                                               ? dic.DicNameCn
                                               : dic.DicNameEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 初始化资讯需求单
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> InitInformationRequest(InformationRequestEntity entity)
        {
            return await _db.Insertable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 保存资讯需求单
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> SaveInformationRequest(InformationRequestEntity entity)
        {
            var count = await _db.Updateable(entity)
                                 .IgnoreColumns(info => new
                                 {
                                     info.FormId,
                                     info.CreatedBy,
                                     info.CreatedDate,
                                 }).Where(info => info.FormId == entity.FormId)
                                 .ExecuteCommandAsync();

            await _formManager.SaveFormSearch(entity.FormId,
                                              [entity.CurrentSituation, entity.Expectations],
                                              [("ITCategory", entity.Category)]);

            return count;
        }

        /// <summary>
        /// 查询资讯需求单明细
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<InformationRequestDto> GetInformationRequest(long formId)
        {
            return await _db.Queryable<FormInstanceEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<InformationRequestEntity>((form, info) => form.FormId == info.FormId)
                            .InnerJoin<UserInfoEntity>((form, info, user) => form.ApplicantUserId == user.UserId)
                            .InnerJoin<DepartmentInfoEntity>((form, info, user, dept) => user.DepartmentId == dept.DepartmentId)
                            .InnerJoin<DictionaryInfoEntity>((form, info, user, dept, dic) => dic.DicType == "FormStatus" && form.FormStatus == dic.DicCode)
                            .LeftJoin<DictionaryInfoEntity>((form, info, user, dept, dic, categoryDic) => categoryDic.DicType == "ITCategory" && info.Category == categoryDic.DicCode)
                            .Where((form, info, user, dept, dic, categoryDic) => form.FormId == formId)
                            .Select((form, info, user, dept, dic, categoryDic) => new InformationRequestDto()
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
                                Category = info.Category,
                                CategoryName = _lang.Locale == "zh-CN"
                                                 ? categoryDic.DicNameCn
                                                 : categoryDic.DicNameEn,
                                CurrentSituation = info.CurrentSituation,
                                Expectations = info.Expectations,
                                EstimatedDays = info.EstimatedDays,
                                Rating = info.Rating,
                            }).FirstAsync();
        }
    }
}
