using Microsoft.Extensions.Logging;
using SqlSugar;
using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.FormOperate.Dto;
using SystemAdmin.Model.FormBusiness.FormOperate.Queries;
using SystemAdmin.Repository.FormBusiness.FormOperate;
using SystemAdmin.Repository.FormBusiness.Workflow;

namespace SystemAdmin.Service.FormBusiness.FormOperate
{
    public class FormHistoryService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<FormPendingService> _logger;
        private readonly SqlSugarScope _db;
        private readonly FormPermissionChecker _formChecker;
        private readonly FormReviewAction _formReviewAction;
        private readonly FormHistoryRepository _formHistoryRepo;
        private readonly LocalizationService _localization;
        private readonly string _this = "FormBusiness.FormOperate.FormPending";

        public FormHistoryService(CurrentUser loginuser, ILogger<FormPendingService> logger, SqlSugarScope db, FormPermissionChecker formChecker, FormReviewAction formReviewAction, FormHistoryRepository formHistoryRepo, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _formChecker = formChecker;
            _formReviewAction = formReviewAction;
            _formHistoryRepo = formHistoryRepo;
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
                var drop = await _formHistoryRepo.GetFormGroupDrop();
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
                var drop = await _formHistoryRepo.GetFormTypeDrop(long.Parse(formGroupId));
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
                var drop = await _formHistoryRepo.GetFormStatusDrop();
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
                return await _formHistoryRepo.GetApplyHistoryPage(getpage, _loginuser.UserId);
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
                return await _formHistoryRepo.GetReviewHistoryPage(getpage, _loginuser.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<FormHistoryDto>.Failure(500, ex.Message);
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
                var list = await _formHistoryRepo.GetFormPendingUsers(long.Parse(formId));
                return Result<List<FormPendingUserDto>>.Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<FormPendingUserDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 表单撤回
        /// </summary>
        /// <returns></returns>
        public async Task<Result<int>> WithdrawForm(string formId)
        {
            try
            {
                var isCan = await _formChecker.CanWithdraw(long.Parse(formId));
                if (!isCan)
                {
                    return Result<int>.Ok(402, _localization.ReturnMsg($"{_this}NotCanWithdraw"));
                }

                await _db.BeginTranAsync();

                var stepInfo = await _formHistoryRepo.GetStartStepInfo(long.Parse(formId));
                var pendingCount = await _formHistoryRepo.WithdrawPendingSubmit(long.Parse(formId), _loginuser.UserId);
                var withCount = await _formHistoryRepo.WithdrawForm(long.Parse(formId), stepInfo.StepId, _loginuser.UserId);

                var selfAppointments = await _formReviewAction.GetStepReviewUser(long.Parse(formId), stepInfo, _loginuser.UserId);
                var reacordsCount = await _formHistoryRepo.InsertReviewRecords(long.Parse(formId), stepInfo.StepId, ReviewResult.Withdraw, null, selfAppointments, "", ReviewType.Manual, _loginuser.UserId);
                await _db.CommitTranAsync();

                return withCount >= 1
                        ? Result<int>.Ok(withCount, _localization.ReturnMsg($"{_this}WithdrawSuccess"))
                        : Result<int>.Failure(500, _localization.ReturnMsg($"{_this}WithdrawFailed"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
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
                var count = await _formHistoryRepo.VoidedForm(long.Parse(formId), _loginuser.UserId);
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
