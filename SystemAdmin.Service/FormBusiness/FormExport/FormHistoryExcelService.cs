using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using SystemAdmin.Common.Excel;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.FormOperate.Dto;
using SystemAdmin.Model.FormBusiness.FormOperate.Queries;
using SystemAdmin.Repository.FormBusiness.FormOperate;
using System.Data;

namespace SystemAdmin.Service.FormBusiness.FormExport
{
    /// <summary>
    /// 申请/审批历史记录导出Excel
    /// </summary>
    public class FormHistoryExcelService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<FormHistoryExcelService> _logger;
        private readonly ApplyHistoryRepository _applyHistoryRepo;
        private readonly ReviewHistoryRepository _reviewHistoryRepo;
        private readonly LocalizationService _localization;
        private readonly string _this = "FormBusiness.FormOperate.FormPending";

        public FormHistoryExcelService(CurrentUser loginuser, ILogger<FormHistoryExcelService> logger, ApplyHistoryRepository applyHistoryRepo, ReviewHistoryRepository reviewHistoryRepo, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _applyHistoryRepo = applyHistoryRepo;
            _reviewHistoryRepo = reviewHistoryRepo;
            _localization = localization;
        }

        /// <summary>
        /// 导出申请历史记录Excel
        /// </summary>
        public async Task<Result<FormPdfDto>> ExportApplyHistoryExcel(GetFormHistoryPage getpage)
        {
            try
            {
                var dt = await _applyHistoryRepo.GetApplyHistoryExcel(getpage, _loginuser.UserId);
                var title = Msg("ExcelApplyHistoryTitle");
                var excel = new FormPdfDto
                {
                    FileName = $"{title}_{DateTime.Now:yyyyMMddHHmmss}.xlsx",
                    FileStream = BuildFormHistoryExcel(dt, title)
                };
                return Result<FormPdfDto>.Ok(excel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<FormPdfDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 导出审批历史记录Excel
        /// </summary>
        public async Task<Result<FormPdfDto>> ExportReviewHistoryExcel(GetFormHistoryPage getpage)
        {
            try
            {
                var dt = await _reviewHistoryRepo.GetReviewHistoryExcel(getpage, _loginuser.UserId);
                var title = Msg("ExcelReviewHistoryTitle");
                var excel = new FormPdfDto
                {
                    FileName = $"{title}_{DateTime.Now:yyyyMMddHHmmss}.xlsx",
                    FileStream = BuildFormHistoryExcel(dt, title)
                };
                return Result<FormPdfDto>.Ok(excel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<FormPdfDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 组装历史记录Excel（申请/审批共用列，字段参考 FormHistoryDto）
        /// </summary>
        private MemoryStream BuildFormHistoryExcel(DataTable dt, string sheetName)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Your Name");

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add(sheetName);

            var columnConfigs = new Dictionary<string, ExcelColumnConfig>
            {
                { "FormNo", ExcelColumnConfig.Text },// 表单单号，防前导零消失
                { "ApplicantDate", ExcelColumnConfig.Date },// 申请日期 yyyy/MM/dd
            };
            var headers = new Dictionary<string, string>
            {
                { "FormTypeName", Msg("ExcelFormTypeName") },
                { "FormNo", Msg("ExcelFormNo") },
                { "ApplicantDate", Msg("ExcelApplicantDate") },
                { "FormStatusName", Msg("ExcelFormStatusName") },
                { "ApplyUserName", Msg("ExcelApplyUserName") },
                { "ApplyUserDeptName", Msg("ExcelApplyUserDeptName") },
            };

            ExcelStyleHelper.ApplyStandardStyle(ws, dt, headers, false, columnConfigs);
            package.Workbook.CalcMode = ExcelCalcMode.Manual;

            var stream = new MemoryStream(package.GetAsByteArray());
            stream.Position = 0;
            return stream;
        }

        private string Msg(string key)
        {
            return _localization.ReturnMsg($"{_this}{key}");
        }
    }
}
