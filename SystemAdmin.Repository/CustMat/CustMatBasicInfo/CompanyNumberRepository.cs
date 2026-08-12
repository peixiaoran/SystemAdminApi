using Mapster;
using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Dto;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Entity;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Queries;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;

namespace SystemAdmin.Repository.CustMat.CustMatBasicInfo
{
    public class CompanyNumberRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        public CompanyNumberRepository(SqlSugarScope db, Language lang)
        {
            _db = db;
            _lang = lang;
        }

        /// <summary>
        /// 新增料号信息
        /// </summary>
        /// <param name="companyNumberEntity"></param>
        /// <returns></returns>
        public async Task<int> InsertCompanyNumber(CompanyNumberEntity companyNumberEntity)
        {
            return await _db.Insertable(companyNumberEntity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 删除料号信息
        /// </summary>
        /// <param name="partNumberId"></param>
        /// <returns></returns>
        public async Task<int> DeleteCompanyNumber(long partNumberId)
        {
            return await _db.Deleteable<CompanyNumberEntity>()
                            .Where(companyNumber => companyNumber.PartNumberId == partNumberId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 修改料号信息
        /// </summary>
        /// <param name="companyNumberEntity"></param>
        /// <returns></returns>
        public async Task<int> UpdateCompanyNumber(CompanyNumberEntity companyNumberEntity)
        {
            return await _db.Updateable(companyNumberEntity)
                            .IgnoreColumns(companyNumber => new
                            {
                                companyNumber.PartNumberId,
                                companyNumber.CreatedBy,
                                companyNumber.CreatedDate,
                            }).Where(companyNumber => companyNumber.PartNumberId == companyNumberEntity.PartNumberId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 查询料号实体
        /// </summary>
        /// <param name="partNumberId"></param>
        /// <returns></returns>
        public async Task<CompanyNumberDto> GetCompanyNumberEntity(long partNumberId)
        {
            var companyNumberEntity = await _db.Queryable<CompanyNumberEntity>()
                                            .With(SqlWith.NoLock)
                                            .Where(companyNumber => companyNumber.PartNumberId == partNumberId)
                                            .FirstAsync();
            return companyNumberEntity.Adapt<CompanyNumberDto>();
        }

        /// <summary>
        /// 查询料号分页
        /// </summary>
        /// <param name="getCompanyNumberPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<CompanyNumberDto>> GetCompanyNumberPage(GetCompanyNumberPage getCompanyNumberPage)
        {
            var query = _db.Queryable<CompanyNumberEntity>()
                           .With(SqlWith.NoLock)
                           .InnerJoin<DictionaryInfoEntity>((companyNumber, typeDic) => typeDic.DicType == "PartType" && companyNumber.PartType == typeDic.DicCode)
                           .InnerJoin<DictionaryInfoEntity>((companyNumber, typeDic, categoryDic) => categoryDic.DicType == "Category" && companyNumber.Category == categoryDic.DicCode)
                           .InnerJoin<DictionaryInfoEntity>((companyNumber, typeDic, categoryDic, sourceDic) => sourceDic.DicType == "SourceType" && companyNumber.SourceType == sourceDic.DicCode);

            // 料号
            if (!string.IsNullOrEmpty(getCompanyNumberPage.PartNumber))
            {
                query = query.Where((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.PartNumber.Contains(getCompanyNumberPage.PartNumber));
            }

            // 品名（中英文模糊匹配）
            if (!string.IsNullOrEmpty(getCompanyNumberPage.PartName))
            {
                query = query.Where((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.PartNameCn.Contains(getCompanyNumberPage.PartName) || companyNumber.PartNameEn.Contains(getCompanyNumberPage.PartName));
            }

            // 规格
            if (!string.IsNullOrEmpty(getCompanyNumberPage.Specification))
            {
                query = query.Where((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.Specification.Contains(getCompanyNumberPage.Specification));
            }

            // 料号类型（全值匹配）
            if (!string.IsNullOrEmpty(getCompanyNumberPage.PartType))
            {
                query = query.Where((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.PartType == getCompanyNumberPage.PartType);
            }

            // 物料分类（全值匹配）
            if (!string.IsNullOrEmpty(getCompanyNumberPage.Category))
            {
                query = query.Where((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.Category == getCompanyNumberPage.Category);
            }

            // 来源类型（全值匹配）
            if (!string.IsNullOrEmpty(getCompanyNumberPage.SourceType))
            {
                query = query.Where((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.SourceType == getCompanyNumberPage.SourceType);
            }

            // 型号
            if (!string.IsNullOrEmpty(getCompanyNumberPage.Model))
            {
                query = query.Where((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.Model.Contains(getCompanyNumberPage.Model));
            }

            // 图号
            if (!string.IsNullOrEmpty(getCompanyNumberPage.DrawingNumber))
            {
                query = query.Where((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.DrawingNumber.Contains(getCompanyNumberPage.DrawingNumber));
            }

            // 版本
            if (!string.IsNullOrEmpty(getCompanyNumberPage.Version))
            {
                query = query.Where((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.Version.Contains(getCompanyNumberPage.Version));
            }

            // 启用状态
            if (getCompanyNumberPage.Status.HasValue)
            {
                var status = getCompanyNumberPage.Status.Value == 1;
                query = query.Where((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.Status == status);
            }

            RefAsync<int> totalCount = 0;
            var companyNumberPage = await query.OrderBy((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.CreatedDate)
                                            .Select((companyNumber, typeDic, categoryDic, sourceDic) => new CompanyNumberDto
                                            {
                                                PartNumberId = companyNumber.PartNumberId,
                                                PartNumber = companyNumber.PartNumber,
                                                PartNameCn = companyNumber.PartNameCn,
                                                PartNameEn = companyNumber.PartNameEn,
                                                Specification = companyNumber.Specification,
                                                PartType = companyNumber.PartType,
                                                PartTypeName = _lang.Locale == "zh-CN" ? typeDic.DicNameCn : typeDic.DicNameEn,
                                                Category = companyNumber.Category,
                                                CategoryName = _lang.Locale == "zh-CN" ? categoryDic.DicNameCn : categoryDic.DicNameEn,
                                                Model = companyNumber.Model,
                                                DrawingNumber = companyNumber.DrawingNumber,
                                                Version = companyNumber.Version,
                                                Unit = companyNumber.Unit,
                                                SourceType = companyNumber.SourceType,
                                                SourceTypeName = _lang.Locale == "zh-CN" ? sourceDic.DicNameCn : sourceDic.DicNameEn,
                                                Manufacturer = companyNumber.Manufacturer,
                                                ManufacturerPartNumber = companyNumber.ManufacturerPartNumber,
                                                LotControl = companyNumber.LotControl,
                                                Status = companyNumber.Status,
                                                Remark = companyNumber.Remark,
                                            }).ToPageListAsync(getCompanyNumberPage.PageIndex, getCompanyNumberPage.PageSize, totalCount);
            return ResultPaged<CompanyNumberDto>.Ok(companyNumberPage, totalCount, "");
        }

        /// <summary>
        /// 按查询条件查询料号信息列表
        /// </summary>
        /// <param name="getCompanyNumberPage"></param>
        /// <returns></returns>
        public async Task<List<CompanyNumberDto>> GetCompanyNumberList(GetCompanyNumberPage getCompanyNumberPage)
        {
            var query = _db.Queryable<CompanyNumberEntity>()
                           .With(SqlWith.NoLock)
                           .InnerJoin<DictionaryInfoEntity>((companyNumber, typeDic) => typeDic.DicType == "PartType" && companyNumber.PartType == typeDic.DicCode)
                           .InnerJoin<DictionaryInfoEntity>((companyNumber, typeDic, categoryDic) => categoryDic.DicType == "Category" && companyNumber.Category == categoryDic.DicCode)
                           .InnerJoin<DictionaryInfoEntity>((companyNumber, typeDic, categoryDic, sourceDic) => sourceDic.DicType == "SourceType" && companyNumber.SourceType == sourceDic.DicCode);

            // 料号
            if (!string.IsNullOrEmpty(getCompanyNumberPage.PartNumber))
            {
                query = query.Where((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.PartNumber.Contains(getCompanyNumberPage.PartNumber));
            }

            // 品名（中英文模糊匹配）
            if (!string.IsNullOrEmpty(getCompanyNumberPage.PartName))
            {
                query = query.Where((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.PartNameCn.Contains(getCompanyNumberPage.PartName) || companyNumber.PartNameEn.Contains(getCompanyNumberPage.PartName));
            }

            // 规格
            if (!string.IsNullOrEmpty(getCompanyNumberPage.Specification))
            {
                query = query.Where((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.Specification.Contains(getCompanyNumberPage.Specification));
            }

            // 料号类型（全值匹配）
            if (!string.IsNullOrEmpty(getCompanyNumberPage.PartType))
            {
                query = query.Where((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.PartType == getCompanyNumberPage.PartType);
            }

            // 物料分类（全值匹配）
            if (!string.IsNullOrEmpty(getCompanyNumberPage.Category))
            {
                query = query.Where((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.Category == getCompanyNumberPage.Category);
            }

            // 型号
            if (!string.IsNullOrEmpty(getCompanyNumberPage.Model))
            {
                query = query.Where((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.Model.Contains(getCompanyNumberPage.Model));
            }

            // 图号
            if (!string.IsNullOrEmpty(getCompanyNumberPage.DrawingNumber))
            {
                query = query.Where((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.DrawingNumber.Contains(getCompanyNumberPage.DrawingNumber));
            }

            // 版本
            if (!string.IsNullOrEmpty(getCompanyNumberPage.Version))
            {
                query = query.Where((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.Version.Contains(getCompanyNumberPage.Version));
            }

            // 来源类型（全值匹配）
            if (!string.IsNullOrEmpty(getCompanyNumberPage.SourceType))
            {
                query = query.Where((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.SourceType == getCompanyNumberPage.SourceType);
            }

            // 启用状态
            if (getCompanyNumberPage.Status.HasValue)
            {
                var status = getCompanyNumberPage.Status.Value == 1;
                query = query.Where((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.Status == status);
            }

            return await query.OrderBy((companyNumber, typeDic, categoryDic, sourceDic) => companyNumber.CreatedDate)
                              .Select((companyNumber, typeDic, categoryDic, sourceDic) => new CompanyNumberDto
                              {
                                  PartNumberId = companyNumber.PartNumberId,
                                  PartNumber = companyNumber.PartNumber,
                                  PartNameCn = companyNumber.PartNameCn,
                                  PartNameEn = companyNumber.PartNameEn,
                                  Specification = companyNumber.Specification,
                                  PartType = companyNumber.PartType,
                                  PartTypeName = _lang.Locale == "zh-CN" ? typeDic.DicNameCn : typeDic.DicNameEn,
                                  Category = companyNumber.Category,
                                  CategoryName = _lang.Locale == "zh-CN" ? categoryDic.DicNameCn : categoryDic.DicNameEn,
                                  Model = companyNumber.Model,
                                  DrawingNumber = companyNumber.DrawingNumber,
                                  Version = companyNumber.Version,
                                  Unit = companyNumber.Unit,
                                  SourceType = companyNumber.SourceType,
                                  SourceTypeName = _lang.Locale == "zh-CN" ? sourceDic.DicNameCn : sourceDic.DicNameEn,
                                  Manufacturer = companyNumber.Manufacturer,
                                  ManufacturerPartNumber = companyNumber.ManufacturerPartNumber,
                                  LotControl = companyNumber.LotControl,
                                  Status = companyNumber.Status,
                                  Remark = companyNumber.Remark,
                              }).ToListAsync();
        }

        /// <summary>
        /// 批量新增料号信息列表
        /// </summary>
        /// <param name="companyNumberList"></param>
        /// <returns></returns>
        public async Task<int> InsertCompanyNumberList(List<CompanyNumberEntity> companyNumberList)
        {
            return await _db.Insertable(companyNumberList).ExecuteCommandAsync();
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
        public async Task<bool> ExistsCompanyNumber(string partNumber)
        {
            return await _db.Queryable<CompanyNumberEntity>()
                            .With(SqlWith.NoLock)
                            .Where(entity => entity.PartNumber == partNumber)
                            .AnyAsync();
        }

        /// <summary>
        /// 一次性查询给定料号列表中，数据库已存在的料号（用于导入校验，避免逐行查询）
        /// </summary>
        /// <param name="partNumbers"></param>
        /// <returns></returns>
        public async Task<List<string>> GetExistingCompanyNumbers(List<string> partNumbers)
        {
            return await _db.Queryable<CompanyNumberEntity>()
                            .With(SqlWith.NoLock)
                            .Where(companyNumber => partNumbers.Contains(companyNumber.PartNumber))
                            .Select(companyNumber => companyNumber.PartNumber)
                            .ToListAsync();
        }
    }
}
