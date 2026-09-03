using Microsoft.Extensions.Logging;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.SalesMgmt.Dto;
using SystemAdmin.Model.CustMat.SalesMgmt.Queries;
using SystemAdmin.Repository.CustMat.SalesMgmt;

namespace SystemAdmin.Service.CustMat.SalesMgmt
{
    public class SalesNumberService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<SalesNumberService> _logger;
        private readonly SalesNumberRepository _salesNumberRepository;

        public SalesNumberService(CurrentUser loginuser, ILogger<SalesNumberService> logger, SalesNumberRepository salesNumberRepository)
        {
            _loginuser = loginuser;
            _logger = logger;
            _salesNumberRepository = salesNumberRepository;
        }

        /// <summary>
        /// 查询业务负责料号分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<SalesNumberDto>> GetSalesNumberPage(GetSalesNumberPage getPage)
        {
            try
            {
                return await _salesNumberRepository.GetSalesNumberPage(getPage, _loginuser.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<SalesNumberDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询负责客户占比
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<SalesCustomerDistributionDto>>> GetSalesCustomerDistribution()
        {
            try
            {
                var rows = await _salesNumberRepository.GetSalesCustomerPartNumbers(_loginuser.UserId);

                var totalCount = rows.Select(row => row.CustomerPartNumber).Distinct().Count();

                var distribution = rows.GroupBy(row => new { row.CustomerId, row.CustomerCode, row.CustomerName })
                                        .Select(group =>
                                        {
                                            var partNumberCount = group.Select(row => row.CustomerPartNumber).Distinct().Count();
                                            return new SalesCustomerDistributionDto
                                            {
                                                CustomerId = group.Key.CustomerId,
                                                CustomerCode = group.Key.CustomerCode,
                                                CustomerName = group.Key.CustomerName,
                                                CustomerPartNumberCount = partNumberCount,
                                                Percentage = totalCount > 0 ? Math.Round(partNumberCount * 100m / totalCount, 2) : 0,
                                            };
                                        })
                                        .OrderByDescending(item => item.Percentage)
                                        .ToList();

                return Result<List<SalesCustomerDistributionDto>>.Ok(distribution, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<SalesCustomerDistributionDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 根据公司料号查询详情
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        public async Task<Result<CompanyNumberDetailDto>> GetPartNumberDetail(string partNumber)
        {
            try
            {
                var entity = await _salesNumberRepository.GetPartNumberDetail(partNumber);
                return Result<CompanyNumberDetailDto>.Ok(entity, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<CompanyNumberDetailDto>.Failure(500, ex.Message);
            }
        }
    }
}
