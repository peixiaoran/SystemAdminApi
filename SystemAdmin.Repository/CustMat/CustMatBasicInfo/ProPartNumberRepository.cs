using Mapster;
using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Dto;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Entity;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Queries;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;

namespace SystemAdmin.Repository.CustMat.CustMatBasicInfo
{
    public class ProPartNumberRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        public ProPartNumberRepository(SqlSugarScope db, Language lang)
        {
            _db = db;
            _lang = lang;
        }

        /// <summary>
        /// 新增料号信息
        /// </summary>
        /// <param name="proPartNumberEntity"></param>
        /// <returns></returns>
        public async Task<int> InsertProPartNumber(ProPartNumberEntity proPartNumberEntity)
        {
            return await _db.Insertable(proPartNumberEntity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 删除料号信息
        /// </summary>
        /// <param name="proPartNumberId"></param>
        /// <returns></returns>
        public async Task<int> DeleteProPartNumber(long proPartNumberId)
        {
            return await _db.Deleteable<ProPartNumberEntity>()
                            .Where(proPartNumber => proPartNumber.PartNumberId == proPartNumberId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 修改料号信息
        /// </summary>
        /// <param name="proPartNumberEntity"></param>
        /// <returns></returns>
        public async Task<int> UpdateProPartNumber(ProPartNumberEntity proPartNumberEntity)
        {
            return await _db.Updateable(proPartNumberEntity)
                            .IgnoreColumns(proPartNumber => new
                            {
                                proPartNumber.PartNumberId,
                                proPartNumber.CreatedBy,
                                proPartNumber.CreatedDate,
                            }).Where(proPartNumber => proPartNumber.PartNumberId == proPartNumberEntity.PartNumberId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 查询料号实体
        /// </summary>
        /// <param name="proPartNumberId"></param>
        /// <returns></returns>
        public async Task<ProPartNumberDto> GetProPartNumberEntity(long proPartNumberId)
        {
            var proPartNumberEntity = await _db.Queryable<ProPartNumberEntity>()
                                            .With(SqlWith.NoLock)
                                            .Where(proPartNumber => proPartNumber.PartNumberId == proPartNumberId)
                                            .FirstAsync();
            return proPartNumberEntity.Adapt<ProPartNumberDto>();
        }

        /// <summary>
        /// 按查询条件构建料号查询（分页查询与导出共用）
        /// </summary>
        /// <param name="getProPartNumberPage"></param>
        /// <returns></returns>
        private ISugarQueryable<ProPartNumberEntity> BuildProPartNumberQuery(GetProPartNumberPage getProPartNumberPage)
        {
            var query = _db.Queryable<ProPartNumberEntity>()
                           .With(SqlWith.NoLock);

            // 料号
            if (!string.IsNullOrEmpty(getProPartNumberPage.PartNumber))
            {
                query = query.Where(proPartNumber => proPartNumber.PartNumber.Contains(getProPartNumberPage.PartNumber));
            }

            // 品名（中英文品名均模糊查询）
            if (!string.IsNullOrEmpty(getProPartNumberPage.PartNameCn))
            {
                query = query.Where(proPartNumber =>
                    proPartNumber.PartNameCn.Contains(getProPartNumberPage.PartNameCn) ||
                    proPartNumber.PartNameEn.Contains(getProPartNumberPage.PartNameCn));
            }

            // 料号类型
            if (!string.IsNullOrEmpty(getProPartNumberPage.PartType))
            {
                query = query.Where(proPartNumber => proPartNumber.PartType == getProPartNumberPage.PartType);
            }

            // 物料分类
            if (!string.IsNullOrEmpty(getProPartNumberPage.Category))
            {
                query = query.Where(proPartNumber => proPartNumber.Category == getProPartNumberPage.Category);
            }

            // 启用状态
            if (getProPartNumberPage.Status.HasValue)
            {
                var status = getProPartNumberPage.Status.Value == 1;
                query = query.Where(proPartNumber => proPartNumber.Status == status);
            }

            return query.OrderBy(proPartNumber => proPartNumber.CreatedDate);
        }

        /// <summary>
        /// 查询料号分页
        /// </summary>
        /// <param name="getProPartNumberPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<ProPartNumberDto>> GetProPartNumberPage(GetProPartNumberPage getProPartNumberPage)
        {
            RefAsync<int> totalCount = 0;
            var query = BuildProPartNumberQuery(getProPartNumberPage);

            var proPartNumberPage = await query.ToPageListAsync(getProPartNumberPage.PageIndex, getProPartNumberPage.PageSize, totalCount);
            return ResultPaged<ProPartNumberDto>.Ok(proPartNumberPage.Adapt<List<ProPartNumberDto>>(), totalCount, "");
        }

        /// <summary>
        /// 按查询条件查询料号信息列表
        /// </summary>
        /// <param name="getProPartNumberPage"></param>
        /// <returns></returns>
        public async Task<List<ProPartNumberEntity>> GetProPartNumberList(GetProPartNumberPage getProPartNumberPage)
        {
            return await BuildProPartNumberQuery(getProPartNumberPage).ToListAsync();
        }

        /// <summary>
        /// 批量新增料号信息列表
        /// </summary>
        /// <param name="proPartNumberInfoList"></param>
        /// <returns></returns>
        public async Task<int> InsertProPartNumberList(List<ProPartNumberEntity> proPartNumberInfoList)
        {
            return await _db.Insertable(proPartNumberInfoList).ExecuteCommandAsync();
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
            return await _db.Queryable<ProPartNumberEntity>()
                            .With(SqlWith.NoLock)
                            .Where(proPartNumber => proPartNumber.PartNumber == partNumber)
                            .AnyAsync();
        }

        /// <summary>
        /// 一次性查询给定料号列表中，数据库已存在的料号（用于导入校验，避免逐行查询）
        /// </summary>
        /// <param name="partNumbers"></param>
        /// <returns></returns>
        public async Task<List<string>> GetExistingPartNumbers(List<string> partNumbers)
        {
            return await _db.Queryable<ProPartNumberEntity>()
                            .With(SqlWith.NoLock)
                            .Where(proPartNumber => partNumbers.Contains(proPartNumber.PartNumber))
                            .Select(proPartNumber => proPartNumber.PartNumber)
                            .ToListAsync();
        }
    }
}
