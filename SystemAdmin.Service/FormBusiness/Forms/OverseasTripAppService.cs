using Microsoft.Extensions.Logging;
using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.Forms.OverseasTripApp.Commands;
using SystemAdmin.Model.FormBusiness.Forms.OverseasTripApp.Dto;
using SystemAdmin.Model.FormBusiness.Forms.OverseasTripApp.Entity;
using SystemAdmin.Repository.FormBusiness.Forms;
using SystemAdmin.Repository.FormBusiness.Workflow;

namespace SystemAdmin.Service.FormBusiness.Forms
{
    public class OverseasTripAppService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<OverseasTripAppService> _logger;
        private readonly SqlSugarScope _db;
        private readonly FormPermissionChecker _formChecker;
        private readonly OverseasTripAppRepository _overseasTripApp;
        private readonly FormManager _formmanger;
        private readonly LocalizationService _localization;
        private readonly string _form = "FormBusiness.Forms.";

        public OverseasTripAppService(CurrentUser loginuser, ILogger<OverseasTripAppService> logger, SqlSugarScope db, FormPermissionChecker formchecker, OverseasTripAppRepository overseasTripApp, FormManager formmanger, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _formChecker = formchecker;
            _overseasTripApp = overseasTripApp;
            _formmanger = formmanger;
            _localization = localization;
        }

        /// <summary>
        /// 交通方式下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<TravelModeDropDto>>> GetTravelModeDrop()
        {
            try
            {
                var list = await _overseasTripApp.GetTravelModeDrop();
                return Result<List<TravelModeDropDto>>.Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<TravelModeDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 厂区下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<SiteDropDto>>> GetSiteDrop()
        {
            try
            {
                var list = await _overseasTripApp.GetSiteDrop();
                return Result<List<SiteDropDto>>.Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<SiteDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 初始化出差单
        /// </summary>
        /// <param name="formTypeId"></param>
        /// <returns></returns>
        public async Task<Result<OverseasTripAppDto>> InitOverseasTripApp(string formTypeId)
        {
            try
            {
                await _db.BeginTranAsync();
                var formId = await _formmanger.InitFormInstance(long.Parse(formTypeId));
                var departureSite = await _overseasTripApp.GetApplicantSite(long.Parse(formId));

                var overseasTripApp = new OverseasTripAppEntity()
                {
                    FormId = long.Parse(formId),
                    DepartureSite = departureSite,
                    DestinationSite = null,
                    TripReason = null,
                    StartDate = null,
                    EndDate = null,
                    Days = null,
                    OutboundTravel = null,
                    ReturnTravel = null,
                    JobDescription = null,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = DateTime.Now
                };

                await _overseasTripApp.InitOverseasTripApp(overseasTripApp);
                await _formmanger.MatchWorkflowRule(long.Parse(formId));
                await _db.CommitTranAsync();

                var overseasTripAppDto = await _overseasTripApp.GetOverseasTripApp(long.Parse(formId));
                overseasTripAppDto.Attachment = await _formmanger.GetAttachmentList(long.Parse(formId));
                overseasTripAppDto.AddReview = await _formmanger.GetAddReviewList(long.Parse(formId));
                overseasTripAppDto.ReviewRecord = await _formmanger.GetReviewRecordList(long.Parse(formId));
                overseasTripAppDto.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(long.Parse(formId), _loginuser.UserId, "Review");
                return Result<OverseasTripAppDto>.Ok(overseasTripAppDto);
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<OverseasTripAppDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询出差单明细
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<Result<OverseasTripAppDto>> GetOverseasTripApp(string formId, string type)
        {
            try
            {
                var isCan = await _formChecker.CanView(long.Parse(formId), type);
                if (!isCan)
                {
                    return Result<OverseasTripAppDto>.Failure(400, _localization.ReturnMsg($"{_form}NotCanView"));
                }

                var form = await _overseasTripApp.GetOverseasTripApp(long.Parse(formId));
                form.Attachment = await _formmanger.GetAttachmentList(long.Parse(formId));
                form.AddReview = await _formmanger.GetAddReviewList(long.Parse(formId));
                form.ReviewRecord = await _formmanger.GetReviewRecordList(long.Parse(formId));
                form.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(form.FormId, _loginuser.UserId, type);
                return Result<OverseasTripAppDto>.Ok(form);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<OverseasTripAppDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 出差单送审校验：申请人在本次出差期间是否已有时间冲突的出差单
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<Result<bool>> ValidateOverseasTripApp(string formId)
        {
            try
            {
                // 查询当前表单出差信息
                var currentTrip = await _overseasTripApp.GetCurrentOverseasTripApp(long.Parse(formId));
                var currentStart = currentTrip.StartDate!.Value;
                var currentEnd = currentTrip.EndDate!.Value;

                var applicantUserId = await _overseasTripApp.GetApplicantUserId(long.Parse(formId));

                // 查询申请人名下与本次出差时间重叠的其他出差单（排除已作废）
                var conflicts = await _overseasTripApp.GetOverlappingTripConflicts(applicantUserId, long.Parse(formId), currentStart, currentEnd);
                var conflict = conflicts.FirstOrDefault();

                if (conflict != null)
                {
                    return Result<bool>.Failure(400, _localization.ReturnMsg(
                        $"{_form}TripDateConflict",
                        args: new object[]
                        {
                            conflict.FormNo,
                            conflict.StartDate.ToString("yyyy-MM-dd"),
                            conflict.EndDate.ToString("yyyy-MM-dd")
                        }
                    ));
                }

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<bool>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 保存出差单
        /// </summary>
        /// <param name="save"></param>
        /// <returns></returns>
        public async Task<Result<int>> SaveOverseasTripApp(OverseasTripAppSave save)
        {
            try
            {
                var departureSite = await _overseasTripApp.GetApplicantSite(long.Parse(save.FormId));

                var entity = new OverseasTripAppEntity()
                {
                    FormId = long.Parse(save.FormId),
                    DepartureSite = departureSite,
                    DestinationSite = save.DestinationSite,
                    TripReason = save.TripReason,
                    StartDate = save.StartDate,
                    EndDate = save.EndDate,
                    Days = save.Days,
                    OutboundTravel = save.OutboundTravel,
                    ReturnTravel = save.ReturnTravel,
                    JobDescription = save.JobDescription,
                    ModifiedBy = _loginuser.UserId,
                    ModifiedDate = DateTime.Now
                };
                await _db.BeginTranAsync();
                var count = await _overseasTripApp.SaveOverseasTripApp(entity);
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
