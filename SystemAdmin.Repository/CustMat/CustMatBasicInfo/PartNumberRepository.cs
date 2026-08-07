using Mapster;
using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Dto;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Entity;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Queries;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;

namespace SystemAdmin.Repository.CustMat.CustMatBasicInfo
{
    public class PartNumberRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        public PartNumberRepository(SqlSugarScope db, Language lang)
        {
            _db = db;
            _lang = lang;
        }

        /// <summary>
        /// 新增料号信息
        /// </summary>
        /// <param name="partNumberEntity"></param>
        /// <returns></returns>
        public async Task<int> InsertPartNumber(PartNumberEntity partNumberEntity)
        {
            return await _db.Insertable(partNumberEntity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 删除料号信息
        /// </summary>
        /// <param name="partNumberId"></param>
        /// <returns></returns>
        public async Task<int> DeletePartNumber(long partNumberId)
        {
            return await _db.Deleteable<PartNumberEntity>()
                            .Where(partNumber => partNumber.PartNumberId == partNumberId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 修改料号信息
        /// </summary>
        /// <param name="partNumberEntity"></param>
        /// <returns></returns>
        public async Task<int> UpdatePartNumber(PartNumberEntity partNumberEntity)
        {
            return await _db.Updateable(partNumberEntity)
                            .IgnoreColumns(partNumber => new
                            {
                                partNumber.PartNumberId,
                                partNumber.CreatedBy,
                                partNumber.CreatedDate,
                            }).Where(partNumber => partNumber.PartNumberId == partNumberEntity.PartNumberId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 查询料号实体
        /// </summary>
        /// <param name="partNumberId"></param>
        /// <returns></returns>
        public async Task<PartNumberDto> GetPartNumberEntity(long partNumberId)
        {
            var partNumberEntity = await _db.Queryable<PartNumberEntity>()
                                            .With(SqlWith.NoLock)
                                            .Where(partNumber => partNumber.PartNumberId == partNumberId)
                                            .FirstAsync();
            return partNumberEntity.Adapt<PartNumberDto>();
        }

        /// <summary>
        /// 按查询条件构建料号查询（分页查询与导出共用）
        /// </summary>
        /// <param name="getPartNumberPage"></param>
        /// <returns></returns>
        private ISugarQueryable<PartNumberEntity> BuildPartNumberQuery(GetPartNumberPage getPartNumberPage)
        {
            var query = _db.Queryable<PartNumberEntity>()
                           .With(SqlWith.NoLock);

            // 料号
            if (!string.IsNullOrEmpty(getPartNumberPage.PartNumber))
            {
                query = query.Where(partNumber => partNumber.PartNumber.Contains(getPartNumberPage.PartNumber));
            }

            // 品名（中英文品名均模糊查询）
            if (!string.IsNullOrEmpty(getPartNumberPage.PartNameCn))
            {
                query = query.Where(partNumber =>
                    partNumber.PartNameCn.Contains(getPartNumberPage.PartNameCn) ||
                    partNumber.PartNameEn.Contains(getPartNumberPage.PartNameCn));
            }

            // 料号类型
            if (!string.IsNullOrEmpty(getPartNumberPage.PartType))
            {
                query = query.Where(partNumber => partNumber.PartType == getPartNumberPage.PartType);
            }

            // 物料分类
            if (!string.IsNullOrEmpty(getPartNumberPage.Category))
            {
                query = query.Where(partNumber => partNumber.Category == getPartNumberPage.Category);
            }

            // 启用状态
            if (getPartNumberPage.Status.HasValue)
            {
                var status = getPartNumberPage.Status.Value == 1;
                query = query.Where(partNumber => partNumber.Status == status);
            }

            return query.OrderBy(partNumber => partNumber.CreatedDate);
        }

        /// <summary>
        /// 查询料号分页
        /// </summary>
        /// <param name="getPartNumberPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<PartNumberDto>> GetPartNumberPage(GetPartNumberPage getPartNumberPage)
        {
            RefAsync<int> totalCount = 0;
            var query = BuildPartNumberQuery(getPartNumberPage);

            var partNumberPage = await query.ToPageListAsync(getPartNumberPage.PageIndex, getPartNumberPage.PageSize, totalCount);
            return ResultPaged<PartNumberDto>.Ok(partNumberPage.Adapt<List<PartNumberDto>>(), totalCount, "");
        }

        /// <summary>
        /// 按查询条件查询料号信息列表
        /// </summary>
        /// <param name="getPartNumberPage"></param>
        /// <returns></returns>
        public async Task<List<PartNumberEntity>> GetPartNumberList(GetPartNumberPage getPartNumberPage)
        {
            return await BuildPartNumberQuery(getPartNumberPage).ToListAsync();
        }

        /// <summary>
        /// 批量新增料号信息列表
        /// </summary>
        /// <param name="partNumberInfoList"></param>
        /// <returns></returns>
        public async Task<int> InsertPartNumberList(List<PartNumberEntity> partNumberInfoList)
        {
            return await _db.Insertable(partNumberInfoList).ExecuteCommandAsync();
        }

        /// <summary>
        /// 料号类型下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<PartTypeDropDto>> GetPartTypeDrop()
        {
            return await _db.Queryable<DictionaryInfoEntity>()
                            .With(SqlWith.NoLock)
                            .Where(dic => dic.DicType == "PartType")
                            .OrderBy(dic => dic.SortOrder)
                            .Select(dic => new PartTypeDropDto
                            {
                                PartType = dic.DicCode,
                                PartTypeName = _lang.Locale == "zh-CN"
                                                ? dic.DicNameCn
                                                : dic.DicNameEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 物料分类下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<PartCategoryDropDto>> GetCategoryDrop()
        {
            return await _db.Queryable<DictionaryInfoEntity>()
                            .With(SqlWith.NoLock)
                            .Where(dic => dic.DicType == "Category")
                            .OrderBy(dic => dic.SortOrder)
                            .Select(dic => new PartCategoryDropDto
                            {
                                Category = dic.DicCode,
                                CategoryName = _lang.Locale == "zh-CN"
                                                ? dic.DicNameCn
                                                : dic.DicNameEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 来源类型下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<PartSourceTypeDropDto>> GetSourceTypeDrop()
        {
            return await _db.Queryable<DictionaryInfoEntity>()
                            .With(SqlWith.NoLock)
                            .Where(dic => dic.DicType == "SourceType")
                            .OrderBy(dic => dic.SortOrder)
                            .Select(dic => new PartSourceTypeDropDto
                            {
                                SourceType = dic.DicCode,
                                SourceTypeName = _lang.Locale == "zh-CN"
                                                ? dic.DicNameCn
                                                : dic.DicNameEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 按字典类型查询字典原始数据（用于导入校验，需同时比对中英文名称与编码）
        /// </summary>
        /// <param name="dicType"></param>
        /// <returns></returns>
        public async Task<List<DictionaryInfoEntity>> GetDictionaryByType(string dicType)
        {
            return await _db.Queryable<DictionaryInfoEntity>()
                            .With(SqlWith.NoLock)
                            .Where(dic => dic.DicType == dicType)
                            .ToListAsync();
        }

        /// <summary>
        /// 一次性按多个字典类型查询字典原始数据（用于导入校验，避免多次往返查询）
        /// </summary>
        /// <param name="dicTypes"></param>
        /// <returns></returns>
        public async Task<List<DictionaryInfoEntity>> GetDictionaryByTypes(List<string> dicTypes)
        {
            return await _db.Queryable<DictionaryInfoEntity>()
                            .With(SqlWith.NoLock)
                            .Where(dic => dicTypes.Contains(dic.DicType))
                            .ToListAsync();
        }

        /// <summary>
        /// 料号是否已存在
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        public async Task<bool> ExistsPartNumber(string partNumber)
        {
            return await _db.Queryable<PartNumberEntity>()
                            .With(SqlWith.NoLock)
                            .Where(entity => entity.PartNumber == partNumber)
                            .AnyAsync();
        }

        /// <summary>
        /// 一次性查询给定料号列表中，数据库已存在的料号（用于导入校验，避免逐行查询）
        /// </summary>
        /// <param name="partNumbers"></param>
        /// <returns></returns>
        public async Task<List<string>> GetExistingPartNumbers(List<string> partNumbers)
        {
            return await _db.Queryable<PartNumberEntity>()
                            .With(SqlWith.NoLock)
                            .Where(partNumber => partNumbers.Contains(partNumber.PartNumber))
                            .Select(partNumber => partNumber.PartNumber)
                            .ToListAsync();
        }
    }
}
