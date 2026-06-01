using Microsoft.Extensions.Logging;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.FormOperate.Dto;
using SystemAdmin.Model.FormBusiness.FormOperate.Queries;
using SystemAdmin.Repository.FormBusiness.FormOperate;

namespace SystemAdmin.Service.FormBusiness.FormOperate
{
    public class FormHistoryService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<FormPendingService> _logger;
        private readonly FormHistoryRepository _formHistoryService;

        public FormHistoryService(CurrentUser loginuser, ILogger<FormPendingService> logger, FormHistoryRepository formHistoryService)
        {
            _loginuser = loginuser;
            _logger = logger;
            _formHistoryService = formHistoryService;
        }

        /// <summary>
        /// 表单组别下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<FormGroupDropDto>>> GetFormGroupDrop()
        {
            try
            {
                var drop = await _formHistoryService.GetFormGroupDrop();
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
                var drop = await _formHistoryService.GetFormTypeDrop(long.Parse(formGroupId));
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
                var drop = await _formHistoryService.GetFormStatusDrop();
                return Result<List<FormStatusDropDto>>.Ok(drop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<FormStatusDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询申请记录分页
        /// </summary>
        /// <returns></returns>
        public async Task<ResultPaged<FormHistoryDto>> GetApplyHistoryPage(GetFormHistoryPage getpage)
        {
            try
            {
                return await _formHistoryService.GetApplyHistoryPage(getpage, _loginuser.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<FormHistoryDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询审批记录分页
        /// </summary>
        /// <returns></returns>
        public async Task<ResultPaged<FormHistoryDto>> GetReviewHistoryPage(GetFormHistoryPage getpage)
        {
            try
            {
                return await _formHistoryService.GetReviewHistoryPage(getpage, _loginuser.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<FormHistoryDto>.Failure(500, ex.Message);
            }
        }
    }
}
