using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.FormBusiness.CompreQuery.Dto;
using SystemAdmin.Model.FormBusiness.CompreQuery.Queries;
using SystemAdmin.Service.FormBusiness.CompreQuery;
using SystemAdmin.Service.FormBusiness.FormExport;
using SystemAdmin.WebApi.Attributes;

namespace SystemAdmin.WebApi.Controllers.FormBusiness.CompreQuery
{
    [JwtAuthorize]
    [RoutingAuthorize]
    [Route("api/FormBusiness/CompreQuery/[controller]/[action]")]
    [ApiController]
    public class GeneralQuery : ControllerBase
    {
        private readonly GeneralQueryService _generalQueryService;
        private readonly FormPrintService _formPrintService;
        private readonly FormQueryExcelService _formQueryExcelService;
        public GeneralQuery(GeneralQueryService generalQueryService, FormPrintService formPrintService, FormQueryExcelService formQueryExcelService)
        {
            _generalQueryService = generalQueryService;
            _formPrintService = formPrintService;
            _formQueryExcelService = formQueryExcelService;
        }

        [HttpPost]
        [Tags("表单业务管理-综合表单查询")]
        [EndpointSummary("[综合查询] 表单组别下拉")]
        public async Task<Result<List<FormGroupDropDto>>> GetFormGroupDrop()
        {
            return await _generalQueryService.GetFormGroupDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-综合表单查询")]
        [EndpointSummary("[综合查询] 表单类别下拉")]
        public async Task<Result<List<FormTypeDropDto>>> GetFormTypeDrop([FromForm] string formGroupId)
        {
            return await _generalQueryService.GetFormTypeDrop(formGroupId);
        }

        [HttpPost]
        [Tags("表单业务管理-综合表单查询")]
        [EndpointSummary("[综合查询] 表单状态下拉")]
        public async Task<Result<List<FormStatusDropDto>>> GetFormStatusDrop()
        {
            return await _generalQueryService.GetFormStatusDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-综合表单查询")]
        [EndpointSummary("[综合查询] 查询表单分页")]
        public async Task<ResultPaged<FormQueryDto>> GetGeneralQueryPage([FromBody] GetGeneralQueryPage getpage)
        {
            return await _generalQueryService.GetGeneralQueryPage(getpage);
        }

        [HttpPost]
        [Tags("表单业务管理-综合表单查询")]
        [EndpointSummary("[综合查询] 查询待审批人")]
        public async Task<Result<List<FormPendingUserDto>>> GetFormPendingUsers([FromForm] string formId)
        {
            return await _generalQueryService.GetFormPendingUsers(formId);
        }

        // 综合查询下的打印/批量打印不判断 CanView 及 StepFieldPermission 控件权限
        [HttpPost]
        [Tags("表单业务管理-综合表单查询")]
        [EndpointSummary("[综合查询] 打印PDF")]
        public async Task<IActionResult> PrintFormPdf([FromForm] string formId)
        {
            var result = await _formPrintService.PrintFormPdf(formId, checkPermission: false);
            if (result.Code != 200)
            {
                return StatusCode(result.Code, result);
            }
            return File(result.Data!.FileStream, "application/pdf", result.Data.FileName);
        }

        [HttpPost]
        [Tags("表单业务管理-综合表单查询")]
        [EndpointSummary("[综合查询] 批量打印PDF")]
        public async Task<IActionResult> PrintFormPdfBatch([FromBody] List<string> formIds)
        {
            var result = await _formPrintService.PrintFormPdfBatch(formIds, checkPermission: false);
            if (result.Code != 200)
            {
                return StatusCode(result.Code, result);
            }
            return File(result.Data!.FileStream, "application/zip", result.Data.FileName);
        }

        [HttpPost]
        [Tags("表单业务管理-综合表单查询")]
        [EndpointSummary("[综合查询] 导出Excel")]
        public async Task<IActionResult> ExportGeneralQueryExcel([FromBody] GetGeneralQueryPage getpage)
        {
            var result = await _formQueryExcelService.ExportGeneralQueryExcel(getpage);
            if (result.Code != 200)
            {
                return StatusCode(result.Code, result);
            }
            return File(result.Data!.FileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.Data.FileName);
        }
    }
}
