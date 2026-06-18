using Microsoft.Extensions.Logging;
using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.FormOperate.Dto;
using SystemAdmin.Model.FormBusiness.FormOperate.Queries;
using SystemAdmin.Repository.FormBusiness.FormOperate;
using SystemAdmin.Repository.FormBusiness.Workflow;

namespace SystemAdmin.Service.FormBusiness.FormOperate
{
    public class FormPendingService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<FormPendingService> _logger;
        private readonly SqlSugarScope _db;
        private readonly FormPermissionChecker _formChecker;
        private readonly FormPendingRepository _formPendingRepo;
        private readonly LocalizationService _localization;
        private readonly string _this = "FormBusiness.FormOperate.FormPending";

        public FormPendingService(CurrentUser loginuser, ILogger<FormPendingService> logger, SqlSugarScope db, FormPermissionChecker formChecker,  FormPendingRepository formPendingRepo, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _formChecker = formChecker;
            _formPendingRepo = formPendingRepo;
            _localization = localization;
        }

        /// <summary>
        /// 表单组别下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<FormGroupDropDto>>> GetFormGroupDrop()
        {
            try
            {
                var drop = await _formPendingRepo.GetFormGroupDrop();
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
                var drop = await _formPendingRepo.GetFormTypeDrop(long.Parse(formGroupId));
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
                var drop = await _formPendingRepo.GetFormStatusDrop();
                return Result<List<FormStatusDropDto>>.Ok(drop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<FormStatusDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询待送审分页
        /// </summary>
        /// <returns></returns>
        public async Task<ResultPaged<FormPendingDto>> GetPendingSubmitPage(GetFormPendingPage getpage)
        {
            try
            {
                return await _formPendingRepo.GetPendingSubmitPage(getpage, _loginuser.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<FormPendingDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询待审批分页
        /// </summary>
        /// <returns></returns>
        public async Task<ResultPaged<FormPendingDto>> GetPendingReviewPage(GetFormPendingPage getpage)
        {
            try
            {
                return await _formPendingRepo.GetPendingReviewPage(getpage, _loginuser.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<FormPendingDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询表单待审批人列表
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<Result<List<FormPendingUserDto>>> GetFormPendingUsers(string formId)
        {
            try
            {
                var list = await _formPendingRepo.GetFormPendingUsers(long.Parse(formId));
                return Result<List<FormPendingUserDto>>.Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<FormPendingUserDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 表单作废
        /// </summary>
        /// <returns></returns>
        public async Task<Result<int>> VoidedForm(string formId)
        {
            try
            {
                var isCan = await _formChecker.CanVoided(long.Parse(formId));
                if (!isCan)
                {
                    return Result<int>.Ok(500, _localization.ReturnMsg($"{_this}NotCanVoided"));
                }

                await _db.BeginTranAsync();
                var count = await _formPendingRepo.VoidedForm(long.Parse(formId), _loginuser.UserId);
                await _db.CommitTranAsync();

                return count >= 1
                        ? Result<int>.Ok(count, _localization.ReturnMsg($"{_this}VoidedSuccess"))
                        : Result<int>.Failure(500, _localization.ReturnMsg($"{_this}VoidedFailed"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }
    }
}
