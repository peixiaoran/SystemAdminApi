using Microsoft.Extensions.Logging;
using SystemAdmin.Model.FormBusiness.CompreQuery.Dto;
using SystemAdmin.Model.FormBusiness.CompreQuery.Queries;
using SystemAdmin.Repository.FormBusiness.CompreQuery;

namespace SystemAdmin.Service.FormBusiness.CompreQuery
{
    public class BasicQueryService
    {
        private readonly ILogger<BasicQueryService> _logger;
        private readonly BasicQueryRepository _basicFormQueryRepo;

        public BasicQueryService(ILogger<BasicQueryService> logger, BasicQueryRepository basicFormQueryRepo)
        {
            _logger = logger;
            _basicFormQueryRepo = basicFormQueryRepo;
        }

        /// <summary>
        /// 表单组别下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<FormGroupDropDto>>> GetFormGroupDrop()
        {
            try
            {
                var drop = await _basicFormQueryRepo.GetFormGroupDrop();
                return Result<List<FormGroupDropDto>>.Ok(drop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<FormGroupDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 表单类别下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<FormTypeDropDto>>> GetFormTypeDrop(string formGroupId)
        {
            try
            {
                var drop = await _basicFormQueryRepo.GetFormTypeDrop(long.Parse(formGroupId));
                return Result<List<FormTypeDropDto>>.Ok(drop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<FormTypeDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 表单状态下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<FormStatusDropDto>>> GetFormStatusDrop()
        {
            try
            {
                var drop = await _basicFormQueryRepo.GetFormStatusDrop();
                return Result<List<FormStatusDropDto>>.Ok(drop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<FormStatusDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询表单分页
        /// </summary>
        /// <returns></returns>
        public async Task<ResultPaged<FormQueryDto>> GetFormQueryPage(GetFormQueryPage getpage)
        {
            try
            {
                return await _basicFormQueryRepo.GetFormQueryPage(getpage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<FormQueryDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询待审批人用户
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<Result<List<FormPendingUserDto>>> GetFormPendingUsers(string formId)
        {
            try
            {
                var list = await _basicFormQueryRepo.GetFormPendingUsers(long.Parse(formId));
                return Result<List<FormPendingUserDto>>.Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<FormPendingUserDto>>.Failure(500, ex.Message);
            }
        }
    }
}
