using Microsoft.Extensions.Logging;
using SqlSugar;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.Forms.DocumentCirculate.Commands;
using SystemAdmin.Model.FormBusiness.Forms.DocumentCirculate.Dto;
using SystemAdmin.Model.FormBusiness.Forms.DocumentCirculate.Entity;
using SystemAdmin.Repository.FormBusiness.Forms;
using SystemAdmin.Repository.FormBusiness.Workflow;

namespace SystemAdmin.Service.FormBusiness.Forms
{
    public class DocumentCirculateService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<DocumentCirculateService> _logger;
        private readonly SqlSugarScope _db;
        private readonly FormPermissionChecker _formChecker;
        private readonly DocumentCirculateRepository _documentCirculate;
        private readonly FormManager _formmanger;
        private readonly LocalizationService _localization;
        private readonly string _form = "FormBusiness.Forms.";

        public DocumentCirculateService(CurrentUser loginuser, ILogger<DocumentCirculateService> logger, SqlSugarScope db, FormPermissionChecker formchecker, DocumentCirculateRepository documentCirculate, FormManager formmanger, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _formChecker = formchecker;
            _documentCirculate = documentCirculate;
            _formmanger = formmanger;
            _localization = localization;
        }

        /// <summary>
        /// 初始化传签单
        /// </summary>
        /// <param name="formTypeId"></param>
        /// <returns></returns>
        public async Task<Result<DocumentCirculateDto>> InitDocumentCirculate(string formTypeId)
        {
            try
            {
                await _db.BeginTranAsync();
                var formId = await _formmanger.InitFormInstance(long.Parse(formTypeId));

                var documentCirculate = new DocumentCirculateEntity()
                {
                    FormId = long.Parse(formId),
                    IssueDept = null,
                    CirculationPurpose = null,
                    ContentSummary = null,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = DateTime.Now
                };

                await _documentCirculate.InitDocumentCirculate(documentCirculate);
                await _formmanger.MatchWorkflowRule(long.Parse(formId));
                await _db.CommitTranAsync();

                var documentCirculateDto = await _documentCirculate.GetDocumentCirculate(long.Parse(formId));
                documentCirculateDto.Attachment = await _formmanger.GetAttachmentList(long.Parse(formId));
                documentCirculateDto.AddReview = await _formmanger.GetAddReviewList(long.Parse(formId));
                documentCirculateDto.ReviewRecord = await _formmanger.GetReviewRecordList(long.Parse(formId));
                documentCirculateDto.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(long.Parse(formId), _loginuser.UserId);
                return Result<DocumentCirculateDto>.Ok(documentCirculateDto);
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<DocumentCirculateDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询传签单明细
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<Result<DocumentCirculateDto>> GetDocumentCirculate(string formId, string type)
        {
            try
            {
                var isCan = await _formChecker.CanView(long.Parse(formId), type);
                if (!isCan)
                {
                    return Result<DocumentCirculateDto>.Failure(400, _localization.ReturnMsg($"{_form}NotCanView"));
                }

                var form = await _documentCirculate.GetDocumentCirculate(long.Parse(formId));
                form.Attachment = await _formmanger.GetAttachmentList(long.Parse(formId));
                form.AddReview = await _formmanger.GetAddReviewList(long.Parse(formId));
                form.ReviewRecord = await _formmanger.GetReviewRecordList(long.Parse(formId));
                form.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(form.FormId, _loginuser.UserId);
                return Result<DocumentCirculateDto>.Ok(form);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<DocumentCirculateDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 保存传签单
        /// </summary>
        /// <param name="save"></param>
        /// <returns></returns>
        public async Task<Result<int>> SaveDocumentCirculate(DocumentCirculateSave save)
        {
            try
            {
                var entity = new DocumentCirculateEntity()
                {
                    FormId = long.Parse(save.FormId),
                    IssueDept = save.IssueDept,
                    CirculationPurpose = save.CirculationPurpose,
                    ContentSummary = save.ContentSummary,
                    ModifiedBy = _loginuser.UserId,
                    ModifiedDate = DateTime.Now
                };
                await _db.BeginTranAsync();
                var count = await _documentCirculate.SaveDocumentCirculate(entity);
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
