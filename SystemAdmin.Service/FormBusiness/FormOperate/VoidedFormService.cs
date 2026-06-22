using Microsoft.Extensions.Logging;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.FormOperate.Dto;
using SystemAdmin.Model.FormBusiness.FormOperate.Queries;
using SystemAdmin.Repository.FormBusiness.FormOperate;

namespace SystemAdmin.Service.FormBusiness.FormOperate
{
    public class VoidedFormService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<VoidedFormService> _logger;
        private readonly VoidedFormRepository _voidedFormRepo;

        public VoidedFormService(CurrentUser loginuser, ILogger<VoidedFormService> logger, VoidedFormRepository voidedFormRepo)
        {
            _loginuser = loginuser;
            _logger = logger;
            _voidedFormRepo = voidedFormRepo;
        }

        /// <summary>
        /// 表单组别下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<FormGroupDropDto>>> GetFormGroupDrop()
        {
            try
            {
                var drop = await _voidedFormRepo.GetFormGroupDrop();
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
                var drop = await _voidedFormRepo.GetFormTypeDrop(long.Parse(formGroupId));
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
                var drop = await _voidedFormRepo.GetFormStatusDrop();
                return Result<List<FormStatusDropDto>>.Ok(drop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<FormStatusDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询已作废分页
        /// </summary>
        /// <returns></returns>
        public async Task<ResultPaged<VoidedFormDto>> GetVoidedFormsPage(GetVoidedFormPage getpage)
        {
            try
            {
                return await _voidedFormRepo.GetVoidedFormsPage(getpage, _loginuser.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<VoidedFormDto>.Failure(500, ex.Message);
            }
        }
    }
}
