using Mapster;
using SqlSugar;
using SystemAdmin.CommonSetup.Options;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Dto;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Entity;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Queries;

namespace SystemAdmin.Repository.FormBusiness.FormBasicInfo
{
    public class FormTypeFieldRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        public FormTypeFieldRepository(SqlSugarScope db, Language lang)
        {
            _db = db;
            _lang = lang;
        }

        /// <summary>
        /// 查询表单组别下拉
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
                                                : formgroup.FormGroupNameEn
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
        /// 新增表单栏位
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> InsertFormTypeField(FormTypeFieldEntity entity)
        {
            return await _db.Insertable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 删除表单栏位
        /// </summary>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        public async Task<int> DeleteFormTypeField(long fieldId)
        {
            return await _db.Deleteable<FormTypeFieldEntity>()
                            .Where(formfield => formfield.FieldId == fieldId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 修改表单栏位
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> UpdateFormTypeField(FormTypeFieldEntity entity)
        {
            return await _db.Updateable(entity)
                            .IgnoreColumns(formfield => new
                            {
                                formfield.FormTypeId,
                                formfield.FieldId,
                                formfield.CreatedBy,
                                formfield.CreatedDate
                            }).Where(formfield => formfield.FieldId == entity.FieldId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 查询表单栏位实体
        /// </summary>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        public async Task<FormTypeFieldDto> GetFormTypeFieldEntity(long fieldId)
        {
            var entity = await _db.Queryable<FormTypeFieldEntity>()
                                  .With(SqlWith.NoLock)
                                  .Where(formfield => formfield.FieldId == fieldId)
                                  .FirstAsync();
            return entity.Adapt<FormTypeFieldDto>();
        }

        /// <summary>
        /// 查询表单栏位分页
        /// </summary>
        /// <returns></returns>
        public async Task<ResultPaged<FormTypeFieldDto>> GetFormTypeFieldPage(GetFormTypeFieldPage getPage)
        {
            RefAsync<int> totalCount = 0;
            var query = _db.Queryable<FormTypeEntity>()
                           .With(SqlWith.NoLock)
                           .InnerJoin<FormTypeFieldEntity>((formtype, formfield) => formtype.FormTypeId == formfield.FormTypeId);

            // 表单类别Id
            if (!string.IsNullOrEmpty(getPage.FormTypeId))
            {
                query = query.Where((formtype, formfield) => formfield.FormTypeId == long.Parse(getPage.FormTypeId));
            }

            // 排序
            query = query.OrderBy((formtype, formfield) => formfield.SortOrder);

            var page = await query.Select((formtype, formfield) => new FormTypeFieldDto
            {
                FieldId = formfield.FieldId,
                FieldKey = formfield.FieldKey,
                FieldNameCn = formfield.FieldNameCn,
                FieldNameEn = formfield.FieldNameEn,
                SortOrder = formfield.SortOrder,
            }).ToPageListAsync(getPage.PageIndex, getPage.PageSize, totalCount);
            return ResultPaged<FormTypeFieldDto>.Ok(page, totalCount, "");
        }
    }
}
