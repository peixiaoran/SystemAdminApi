using Microsoft.Extensions.Logging;
using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.Forms.InformationRequest.Commands;
using SystemAdmin.Model.FormBusiness.Forms.InformationRequest.Dto;
using SystemAdmin.Model.FormBusiness.Forms.InformationRequest.Entity;
using SystemAdmin.Repository.FormBusiness.Forms;
using SystemAdmin.Repository.FormBusiness.Workflow;

namespace SystemAdmin.Service.FormBusiness.Forms
{
    public class InformationRequestService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<InformationRequestService> _logger;
        private readonly SqlSugarScope _db;
        private readonly FormPermissionChecker _formChecker;
        private readonly InformationRequestRepository _informationRequest;
        private readonly FormManager _formmanger;
        private readonly LocalizationService _localization;
        private readonly string _form = "FormBusiness.Forms.";

        public InformationRequestService(CurrentUser loginuser, ILogger<InformationRequestService> logger, SqlSugarScope db, FormPermissionChecker formchecker, InformationRequestRepository informationRequest, FormManager formmanger, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _formChecker = formchecker;
            _informationRequest = informationRequest;
            _formmanger = formmanger;
            _localization = localization;
        }

        /// <summary>
        /// 需求类别下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<ITCategoryDropDto>>> GetITCategoryDrop()
        {
            try
            {
                var list = await _informationRequest.GetITCategoryDrop();
                return Result<List<ITCategoryDropDto>>.Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<ITCategoryDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 初始化资讯需求单
        /// </summary>
        /// <param name="formTypeId"></param>
        /// <returns></returns>
        public async Task<Result<InformationRequestDto>> InitInformationRequest(string formTypeId)
        {
            try
            {
                await _db.BeginTranAsync();
                var formId = await _formmanger.InitFormInstance(long.Parse(formTypeId));

                var informationRequest = new InformationRequestEntity()
                {
                    FormId = long.Parse(formId),
                    Category = null,
                    CurrentSituation = null,
                    Expectations = null,
                    EstimatedDays = null,
                    Rating = null,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = DateTime.Now
                };

                await _informationRequest.InitInformationRequest(informationRequest);
                await _formmanger.MatchWorkflowRule(long.Parse(formId));
                await _db.CommitTranAsync();

                var informationRequestDto = await _informationRequest.GetInformationRequest(long.Parse(formId));
                informationRequestDto.Attachment = await _formmanger.GetAttachmentList(long.Parse(formId));
                informationRequestDto.AddReview = await _formmanger.GetAddReviewList(long.Parse(formId));
                informationRequestDto.ReviewRecord = await _formmanger.GetReviewRecordList(long.Parse(formId));
                informationRequestDto.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(long.Parse(formId), _loginuser.UserId);
                return Result<InformationRequestDto>.Ok(informationRequestDto);
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<InformationRequestDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询资讯需求单明细
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<Result<InformationRequestDto>> GetInformationRequest(string formId, string type)
        {
            try
            {
                var isCan = await _formChecker.CanView(long.Parse(formId), type);
                if (!isCan)
                {
                    return Result<InformationRequestDto>.Failure(400, _localization.ReturnMsg($"{_form}NotCanView"));
                }

                var form = await _informationRequest.GetInformationRequest(long.Parse(formId));
                form.Attachment = await _formmanger.GetAttachmentList(long.Parse(formId));
                form.AddReview = await _formmanger.GetAddReviewList(long.Parse(formId));
                form.ReviewRecord = await _formmanger.GetReviewRecordList(long.Parse(formId));
                form.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(form.FormId, _loginuser.UserId, type == "Verification");
                return Result<InformationRequestDto>.Ok(form);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<InformationRequestDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 保存资讯需求单
        /// </summary>
        /// <param name="save"></param>
        /// <returns></returns>
        public async Task<Result<int>> SaveInformationRequest(InformationRequestSave save)
        {
            try
            {
                var entity = new InformationRequestEntity()
                {
                    FormId = long.Parse(save.FormId),
                    Category = save.Category,
                    CurrentSituation = save.CurrentSituation,
                    Expectations = save.Expectations,
                    EstimatedDays = save.EstimatedDays,
                    Rating = save.Rating,
                    ModifiedBy = _loginuser.UserId,
                    ModifiedDate = DateTime.Now
                };
                await _db.BeginTranAsync();
                var count = await _informationRequest.SaveInformationRequest(entity);
                await _formmanger.SaveFormInstance(long.Parse(save.FormId));
                await _db.CommitTranAsync();

                return count >= 1
                        ? Result<int>.Ok(count, _localization.ReturnMsg($"{_form}SaveSuccess"))
                        : Result<int>.Failure(500, _localization.ReturnMsg($"{_form}SaveFailed"));
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
