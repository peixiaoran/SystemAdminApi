using Mapster;
using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Dto;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Entity;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Queries;

namespace SystemAdmin.Repository.CustMat.CustMatBasicInfo
{
    public class CustomerNumberRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        public CustomerNumberRepository(SqlSugarScope db, Language lang)
        {
            _db = db;
            _lang = lang;
        }

        /// <summary>
        /// 新增客户料号信息
        /// </summary>
        /// <param name="customerNumberEntity"></param>
        /// <returns></returns>
        public async Task<int> InsertCustomerNumber(CustomerNumberEntity customerNumberEntity)
        {
            return await _db.Insertable(customerNumberEntity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 删除客户料号信息
        /// </summary>
        /// <param name="partNumberId"></param>
        /// <returns></returns>
        public async Task<int> DeleteCustomerNumber(long partNumberId)
        {
            return await _db.Deleteable<CustomerNumberEntity>()
                            .Where(customerNumber => customerNumber.PartNumberId == partNumberId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 修改客户料号信息
        /// </summary>
        /// <param name="customerNumberEntity"></param>
        /// <returns></returns>
        public async Task<int> UpdateCustomerNumber(CustomerNumberEntity customerNumberEntity)
        {
            return await _db.Updateable(customerNumberEntity)
                            .IgnoreColumns(customerNumber => new
                            {
                                customerNumber.PartNumberId,
                                customerNumber.CreatedBy,
                                customerNumber.CreatedDate,
                            }).Where(customerNumber => customerNumber.PartNumberId == customerNumberEntity.PartNumberId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 查询客户料号实体
        /// </summary>
        /// <param name="partNumberId"></param>
        /// <returns></returns>
        public async Task<CustomerNumberDto> GetCustomerNumberEntity(long partNumberId)
        {
            var entity = await _db.Queryable<CustomerNumberEntity>()
                                  .With(SqlWith.NoLock)
                                  .Where(customerNumber => customerNumber.PartNumberId == partNumberId)
                                  .FirstAsync();
            return entity.Adapt<CustomerNumberDto>();
        }

        /// <summary>
        /// 查询客户料号分页
        /// </summary>
        /// <param name="getCustomerNumberPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<CustomerNumberDto>> GetCustomerNumberPage(GetCustomerNumberPage getCustomerNumberPage)
        {
            var query = _db.Queryable<CustomerNumberEntity>()
                           .With(SqlWith.NoLock);

            // 客户料号
            if (!string.IsNullOrEmpty(getCustomerNumberPage.PartNumber))
            {
                query = query.Where(customerNumber => customerNumber.PartNumber.Contains(getCustomerNumberPage.PartNumber));
            }

            // 客户代码
            if (!string.IsNullOrEmpty(getCustomerNumberPage.CustomerCode))
            {
                query = query.Where(customerNumber => customerNumber.CustomerCode.Contains(getCustomerNumberPage.CustomerCode));
            }

            // 品名（中英文模糊匹配）
            if (!string.IsNullOrEmpty(getCustomerNumberPage.PartName))
            {
                query = query.Where(customerNumber => customerNumber.PartNameCn.Contains(getCustomerNumberPage.PartName) || customerNumber.PartNameEn.Contains(getCustomerNumberPage.PartName));
            }

            // 规格型号
            if (!string.IsNullOrEmpty(getCustomerNumberPage.Specification))
            {
                query = query.Where(customerNumber => customerNumber.Specification.Contains(getCustomerNumberPage.Specification));
            }

            // 启用状态
            if (getCustomerNumberPage.Status.HasValue)
            {
                query = query.Where(customerNumber => customerNumber.Status == getCustomerNumberPage.Status.Value);
            }

            RefAsync<int> totalCount = 0;
            var customerNumberPage = await query.OrderBy(customerNumber => customerNumber.CreatedDate)
                                            .Select(customerNumber => new CustomerNumberDto
                                            {
                                                PartNumberId = customerNumber.PartNumberId,
                                                PartNumber = customerNumber.PartNumber,
                                                CustomerCode = customerNumber.CustomerCode,
                                                PartNameCn = customerNumber.PartNameCn,
                                                PartNameEn = customerNumber.PartNameEn,
                                                Specification = customerNumber.Specification,
                                                Unit = customerNumber.Unit,
                                                Status = customerNumber.Status,
                                            }).ToPageListAsync(getCustomerNumberPage.PageIndex, getCustomerNumberPage.PageSize, totalCount);
            return ResultPaged<CustomerNumberDto>.Ok(customerNumberPage, totalCount, "");
        }

        /// <summary>
        /// 按查询条件查询客户料号信息列表
        /// </summary>
        /// <param name="getCustomerNumberPage"></param>
        /// <returns></returns>
        public async Task<List<CustomerNumberDto>> GetCustomerNumberList(GetCustomerNumberPage getCustomerNumberPage)
        {
            var query = _db.Queryable<CustomerNumberEntity>()
                           .With(SqlWith.NoLock);

            // 客户料号
            if (!string.IsNullOrEmpty(getCustomerNumberPage.PartNumber))
            {
                query = query.Where(customerNumber => customerNumber.PartNumber.Contains(getCustomerNumberPage.PartNumber));
            }

            // 客户代码
            if (!string.IsNullOrEmpty(getCustomerNumberPage.CustomerCode))
            {
                query = query.Where(customerNumber => customerNumber.CustomerCode.Contains(getCustomerNumberPage.CustomerCode));
            }

            // 品名（中英文模糊匹配）
            if (!string.IsNullOrEmpty(getCustomerNumberPage.PartName))
            {
                query = query.Where(customerNumber => customerNumber.PartNameCn.Contains(getCustomerNumberPage.PartName) || customerNumber.PartNameEn.Contains(getCustomerNumberPage.PartName));
            }

            // 规格型号
            if (!string.IsNullOrEmpty(getCustomerNumberPage.Specification))
            {
                query = query.Where(customerNumber => customerNumber.Specification.Contains(getCustomerNumberPage.Specification));
            }

            // 启用状态
            if (getCustomerNumberPage.Status.HasValue)
            {
                query = query.Where(customerNumber => customerNumber.Status == getCustomerNumberPage.Status.Value);
            }

            return await query.OrderBy(customerNumber => customerNumber.CreatedDate)
                              .Select(customerNumber => new CustomerNumberDto
                              {
                                  PartNumberId = customerNumber.PartNumberId,
                                  PartNumber = customerNumber.PartNumber,
                                  CustomerCode = customerNumber.CustomerCode,
                                  PartNameCn = customerNumber.PartNameCn,
                                  PartNameEn = customerNumber.PartNameEn,
                                  Specification = customerNumber.Specification,
                                  Unit = customerNumber.Unit,
                                  Status = customerNumber.Status,
                              }).ToListAsync();
        }

        /// <summary>
        /// 批量新增客户料号信息列表
        /// </summary>
        /// <param name="customerNumberList"></param>
        /// <returns></returns>
        public async Task<int> InsertCustomerNumberList(List<CustomerNumberEntity> customerNumberList)
        {
            return await _db.Insertable(customerNumberList).ExecuteCommandAsync();
        }

        /// <summary>
        /// 客户料号是否已存在
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        public async Task<bool> ExistsCustomerNumber(string partNumber)
        {
            return await _db.Queryable<CustomerNumberEntity>()
                            .With(SqlWith.NoLock)
                            .Where(entity => entity.PartNumber == partNumber)
                            .AnyAsync();
        }

        /// <summary>
        /// 一次性查询给定客户料号列表中，数据库已存在的客户料号（用于导入校验，避免逐行查询）
        /// </summary>
        /// <param name="partNumbers"></param>
        /// <returns></returns>
        public async Task<List<string>> GetExistingCustomerNumbers(List<string> partNumbers)
        {
            return await _db.Queryable<CustomerNumberEntity>()
                            .With(SqlWith.NoLock)
                            .Where(customerNumber => partNumbers.Contains(customerNumber.PartNumber))
                            .Select(customerNumber => customerNumber.PartNumber)
                            .ToListAsync();
        }

        /// <summary>
        /// 查询指定客户编码下的全部客户料号（客户被删除时，用于联动清理其下客户料号及料号对照）
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public async Task<List<string>> GetPartNumbersByCustomerCode(string customerCode)
        {
            return await _db.Queryable<CustomerNumberEntity>()
                            .With(SqlWith.NoLock)
                            .Where(customerNumber => customerNumber.CustomerCode == customerCode)
                            .Select(customerNumber => customerNumber.PartNumber)
                            .ToListAsync();
        }

        /// <summary>
        /// 删除指定客户编码下的全部客户料号（客户被删除时联动清理）
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public async Task<int> DeleteCustomerNumbersByCustomerCode(string customerCode)
        {
            return await _db.Deleteable<CustomerNumberEntity>()
                            .Where(customerNumber => customerNumber.CustomerCode == customerCode)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 客户下拉（配合客户料号选择所属客户使用）
        /// </summary>
        /// <returns></returns>
        public async Task<List<CustomerDropDto>> GetCustomerDrop()
        {
            return await _db.Queryable<CustomerInfoEntity>()
                            .With(SqlWith.NoLock)
                            .OrderBy(customer => customer.CustomerCode)
                            .Select(customer => new CustomerDropDto
                            {
                                CustomerCode = customer.CustomerCode,
                                CustomerName = _lang.Locale == "zh-CN"
                                               ? customer.CustomerNameCn
                                               : customer.CustomerNameEn,
                            }).ToListAsync();
        }
    }
}
