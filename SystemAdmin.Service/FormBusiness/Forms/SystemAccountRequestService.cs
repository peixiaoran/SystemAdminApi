using Microsoft.Extensions.Logging;
using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Repository.SystemBasicMgmt.SystemAuth;
using SystemAdmin.Service.SystemBasicMgmt.SystemAuth;
using SystemAdmin.Model.FormBusiness.Forms.OverseasTripApp.Dto;
using SystemAdmin.Model.FormBusiness.Forms.SystemAccountRequest.Commands;
using SystemAdmin.Model.FormBusiness.Forms.SystemAccountRequest.Dto;
using SystemAdmin.Model.FormBusiness.Forms.SystemAccountRequest.Entity;
using DepartmentDropDto = SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto.DepartmentDropDto;
using NationalityDropDto = SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto.NationalityDropDto;
using SystemAdmin.Repository.FormBusiness.Forms;
using SystemAdmin.Repository.FormBusiness.Workflow;

namespace SystemAdmin.Service.FormBusiness.Forms
{
    public class SystemAccountRequestService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<SystemAccountRequestService> _logger;
        private readonly SqlSugarScope _db;
        private readonly FormPermissionChecker _formChecker;
        private readonly SystemAccountRequestRepository _systemAccountRequest;
        private readonly FormManager _formmanger;
        private readonly LocalizationService _localization;
        private readonly PasskeyRepository _passkeyRepo;
        private readonly PasskeyBindMailService _passkeyBindMail;
        private readonly string _form = "FormBusiness.Forms.";

        public SystemAccountRequestService(CurrentUser loginuser, ILogger<SystemAccountRequestService> logger, SqlSugarScope db, FormPermissionChecker formchecker, SystemAccountRequestRepository systemAccountRequest, FormManager formmanger, LocalizationService localization, PasskeyRepository passkeyRepo, PasskeyBindMailService passkeyBindMail)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _formChecker = formchecker;
            _systemAccountRequest = systemAccountRequest;
            _formmanger = formmanger;
            _localization = localization;
            _passkeyRepo = passkeyRepo;
            _passkeyBindMail = passkeyBindMail;
        }

        /// <summary>
        /// 厂区下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<SiteDropDto>>> GetSiteDrop()
        {
            try
            {
                var list = await _systemAccountRequest.GetSiteDrop();
                return Result<List<SiteDropDto>>.Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<SiteDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 部门下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<DepartmentDropDto>>> GetDepartmentDrop()
        {
            try
            {
                var drop = await _systemAccountRequest.GetDepartmentDrop();
                return Result<List<DepartmentDropDto>>.Ok(drop, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<DepartmentDropDto>>.Failure(500, ex.Message.ToString());
            }
        }

        /// <summary>
        /// 国籍下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<NationalityDropDto>>> GetNationalityDrop()
        {
            try
            {
                var drop = await _systemAccountRequest.GetNationalityDrop();
                return Result<List<NationalityDropDto>>.Ok(drop, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<NationalityDropDto>>.Failure(500, ex.Message.ToString());
            }
        }

        /// <summary>
        /// 系统下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<SystemDropDto>>> GetSystemDrop()
        {
            try
            {
                var list = await _systemAccountRequest.GetSystemDrop();
                return Result<List<SystemDropDto>>.Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<SystemDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 按系统和厂区查询权限下拉
        /// </summary>
        /// <param name="system"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public async Task<Result<List<PermissionsDropDto>>> GetPermissionsDrop(string system, string site)
        {
            try
            {
                var list = await _systemAccountRequest.GetPermissionsDrop(system, site);
                return Result<List<PermissionsDropDto>>.Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<PermissionsDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 初始化系统账号申请单
        /// </summary>
        /// <param name="formTypeId"></param>
        /// <returns></returns>
        public async Task<Result<SystemAccountRequestDto>> InitSystemAccountRequest(string formTypeId)
        {
            try
            {
                await _db.BeginTranAsync();
                var formId = await _formmanger.InitFormInstance(long.Parse(formTypeId));

                var header = new SystemAccountRequestEntity()
                {
                    FormId = long.Parse(formId),
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = DateTime.Now
                };

                await _systemAccountRequest.InitSystemAccountRequest(header);
                await _formmanger.MatchWorkflowRule(long.Parse(formId));
                await _db.CommitTranAsync();

                var systemAccountRequestDto = await _systemAccountRequest.GetSystemAccountRequest(long.Parse(formId));
                systemAccountRequestDto.Attachment = await _formmanger.GetAttachmentList(long.Parse(formId));
                systemAccountRequestDto.AddReview = await _formmanger.GetAddReviewList(long.Parse(formId));
                systemAccountRequestDto.ReviewRecord = await _formmanger.GetReviewRecordList(long.Parse(formId));
                systemAccountRequestDto.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(long.Parse(formId), _loginuser.UserId, "Review");
                return Result<SystemAccountRequestDto>.Ok(systemAccountRequestDto);
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<SystemAccountRequestDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询系统账号申请单明细
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<Result<SystemAccountRequestDto>> GetSystemAccountRequest(string formId, string type)
        {
            try
            {
                var isCan = await _formChecker.CanView(long.Parse(formId), type);
                if (!isCan)
                {
                    return Result<SystemAccountRequestDto>.Failure(400, _localization.ReturnMsg($"{_form}NotCanView"));
                }

                var form = await _systemAccountRequest.GetSystemAccountRequest(long.Parse(formId));
                form.Attachment = await _formmanger.GetAttachmentList(long.Parse(formId));
                form.AddReview = await _formmanger.GetAddReviewList(long.Parse(formId));
                form.ReviewRecord = await _formmanger.GetReviewRecordList(long.Parse(formId));
                form.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(form.FormId, _loginuser.UserId, type);
                return Result<SystemAccountRequestDto>.Ok(form);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<SystemAccountRequestDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 保存系统账号申请单
        /// </summary>
        /// <param name="save"></param>
        /// <returns></returns>
        public async Task<Result<int>> SaveSystemAccountRequest(SystemAccountRequestSave save)
        {
            try
            {
                var formId = long.Parse(save.FormId);
                var now = DateTime.Now;

                var entities = save.Systems
                                   .Where(item => !string.IsNullOrWhiteSpace(item.System))
                                   .GroupBy(item => item.System)
                                   .Select(group => group.First())
                                   .Select(item => new SystemAccountRequestDetailsEntity()
                                   {
                                       FormId = formId,
                                       System = item.System,
                                       Sites = JoinMulti(item.Sites),
                                       Permissions = JoinMulti(item.Permissions),
                                       Rating = item.Rating,
                                       CreatedBy = _loginuser.UserId,
                                       CreatedDate = now,
                                       ModifiedBy = _loginuser.UserId,
                                       ModifiedDate = now
                                   }).ToList();

                var header = new SystemAccountRequestEntity()
                {
                    FormId = formId,
                    RequesterUserDept = save.RequesterUserDept,
                    RequesterUserNo = save.RequesterUserNo,
                    RequesterUserNameCn = save.RequesterUserNameCn,
                    RequesterUserNameEn = save.RequesterUserNameEn,
                    RequesterGender = save.RequesterGender,
                    NationalityId = save.NationalityId,
                    RequesterEmail = save.RequesterEmail,
                    Reason = save.Reason,
                    ModifiedBy = _loginuser.UserId,
                    ModifiedDate = now
                };

                await _db.BeginTranAsync();
                var count = await _systemAccountRequest.SaveSystemAccountRequest(header, entities);
                await _formmanger.SaveFormInstance(formId);
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

        /// <summary>
        /// 给用户发送 Passkey 绑定链接邮件：按工号定位用户，新链接签发后旧的未使用链接作废
        /// </summary>
        /// <param name="send"></param>
        /// <returns></returns>
        public async Task<Result<int>> SendPasskeyBindMail(PasskeyBindMailSend send)
        {
            try
            {
                var userNo = send.UserNo?.Trim() ?? string.Empty;

                var user = userNo.Length == 0 ? null : await _passkeyRepo.GetEmployedUserByUserNo(userNo);
                if (user == null)
                {
                    return Result<int>.Failure(404, _localization.ReturnMsg($"{_form}PasskeyBindUserNotFound", (object)userNo));
                }

                return await _passkeyBindMail.SendBindMail(user, send.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 多选值去空去重后以逗号拼接
        /// </summary>
        private static string JoinMulti(List<string>? values)
        {
            return string.Join(",", (values ?? new List<string>())
                                    .Where(value => !string.IsNullOrWhiteSpace(value))
                                    .Select(value => value.Trim())
                                    .Distinct());
        }
    }
}
