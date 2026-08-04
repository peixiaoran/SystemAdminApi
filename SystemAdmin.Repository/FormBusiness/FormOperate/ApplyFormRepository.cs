using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Entity;
using SystemAdmin.Model.FormBusiness.FormOperate.Dto;
using SystemAdmin.Model.FormBusiness.FormOperate.Queries;

namespace SystemAdmin.Repository.FormBusiness.FormOperate
{
    public class ApplyFormRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        public ApplyFormRepository(SqlSugarScope db, Language lang)
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
        /// 查询申请表单分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<ApplyFormInfoDto>> GetApplyFormPage(getPage getPage)
        {
            RefAsync<int> totalCount = 0;
            var page = await _db.Queryable<FormTypeEntity>()
                                .With(SqlWith.NoLock)
                                .Where(formtype => formtype.FormGroupId == long.Parse(getPage.FormGroupId))
                                .OrderBy(formtype => formtype.SortOrder)
                                .Select(formtype => new ApplyFormInfoDto()
                                {
                                    FormTypeId = formtype.FormTypeId,
                                    FormTypeName = _lang.Locale == "zh-CN"
                                                   ? formtype.FormTypeNameCn
                                                   : formtype.FormTypeNameEn,
                                    ReviewPath = formtype.ReviewPath,
                                    ViewPath = formtype.ViewPath,
                                    Description = _lang.Locale == "zh-CN"
                                                   ? formtype.DescriptionCn
                                                   : formtype.DescriptionEn,
                                }).ToPageListAsync(getPage.PageIndex, getPage.PageSize, totalCount);
            return ResultPaged<ApplyFormInfoDto>.Ok(page, totalCount);
        }
    }
}
