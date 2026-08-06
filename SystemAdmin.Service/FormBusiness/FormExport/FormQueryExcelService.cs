using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using SystemAdmin.Common.Excel;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.CompreQuery.Queries;
using SystemAdmin.Model.FormBusiness.FormOperate.Dto;
using SystemAdmin.Repository.FormBusiness.CompreQuery;
using System.Data;

namespace SystemAdmin.Service.FormBusiness.FormExport
{
    /// <summary>
    /// 全部表单查询导出Excel（与 FormHistoryExcelService 区分：独立的数据来源、文案与列配置）
    /// </summary>
    public class FormQueryExcelService
    {
        private readonly ILogger<FormQueryExcelService> _logger;
        private readonly BasicQueryRepository _basicQueryRepo;
        private readonly LocalizationService _localization;
        private readonly string _this = "FormBusiness.CompreQuery.FormQueryExcel_";

        public FormQueryExcelService(ILogger<FormQueryExcelService> logger, BasicQueryRepository basicQueryRepo, LocalizationService localization)
        {
            _logger = logger;
            _basicQueryRepo = basicQueryRepo;
            _localization = localization;
        }

        /// <summary>
        /// 导出全部表单查询Excel
        /// </summary>
        public async Task<Result<FormPdfDto>> ExportFormQueryExcel(GetFormQueryPage getpage)
        {
            try
            {
                var dt = await _basicQueryRepo.GetFormQueryExcel(getpage);
                var title = Msg("Title");
                var excel = new FormPdfDto
                {
                    FileName = $"{title}_{DateTime.Now:yyyyMMddHHmmss}.xlsx",
                    FileStream = BuildFormQueryExcel(dt, title)
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
        /// 组装全部表单查询Excel（字段参考 FormQueryDto）
        /// </summary>
        private MemoryStream BuildFormQueryExcel(DataTable dt, string sheetName)
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
                { "FormTypeName", Msg("FormTypeName") },
                { "FormNo", Msg("FormNo") },
                { "ApplicantDate", Msg("ApplicantDate") },
                { "FormStatusName", Msg("FormStatusName") },
                { "ApplyUserName", Msg("ApplyUserName") },
                { "ApplyUserDeptName", Msg("ApplyUserDeptName") },
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
