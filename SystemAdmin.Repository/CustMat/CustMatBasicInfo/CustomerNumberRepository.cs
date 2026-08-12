using Mapster;
using SqlSugar;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Dto;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Entity;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Queries;

namespace SystemAdmin.Repository.CustMat.CustMatBasicInfo
{
    public class CustomerNumberRepository
    {
        private readonly SqlSugarScope _db;

        public CustomerNumberRepository(SqlSugarScope db)
        {
            _db = db;
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
            var customerNumberEntity = await _db.Queryable<CustomerNumberEntity>()
                                            .With(SqlWith.NoLock)
                                            .Where(customerNumber => customerNumber.PartNumberId == partNumberId)
                                            .FirstAsync();
            return customerNumberEntity.Adapt<CustomerNumberDto>();
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
    }
}
