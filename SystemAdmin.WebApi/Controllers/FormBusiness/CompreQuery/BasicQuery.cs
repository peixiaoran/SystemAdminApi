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
    public class BasicQuery : ControllerBase
    {
        private readonly BasicQueryService _basicFormQueryService;
        private readonly FormPrintService _formPrintService;
        private readonly FormQueryExcelService _formQueryExcelService;
        public BasicQuery(BasicQueryService basicFormQueryService, FormPrintService formPrintPdfService, FormQueryExcelService formQueryExcelService)
        {
            _basicFormQueryService = basicFormQueryService;
            _formPrintService = formPrintPdfService;
            _formQueryExcelService = formQueryExcelService;
        }

        [HttpPost]
        [Tags("表单业务管理-综合表单查询")]
        [EndpointSummary("[全部表单查询] 表单组别下拉")]
        public async Task<Result<List<FormGroupDropDto>>> GetFormGroupDrop()
        {
            return await _basicFormQueryService.GetFormGroupDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-综合表单查询")]
        [EndpointSummary("[全部表单查询] 表单类别下拉")]
        public async Task<Result<List<FormTypeDropDto>>> GetFormTypeDrop([FromForm] string formGroupId)
        {
            return await _basicFormQueryService.GetFormTypeDrop(formGroupId);
        }

        [HttpPost]
        [Tags("表单业务管理-综合表单查询")]
        [EndpointSummary("[全部表单查询] 表单状态下拉")]
        public async Task<Result<List<FormStatusDropDto>>> GetFormStatusDrop()
        {
            return await _basicFormQueryService.GetFormStatusDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-综合表单查询")]
        [EndpointSummary("[全部表单查询] 查询表单分页")]
        public async Task<ResultPaged<FormQueryDto>> GetFormQueryPage([FromBody] GetFormQueryPage getpage)
        {
            return await _basicFormQueryService.GetFormQueryPage(getpage);
        }

        [HttpPost]
        [Tags("表单业务管理-综合表单查询")]
        [EndpointSummary("[全部表单查询] 查询待审批人")]
        public async Task<Result<List<FormPendingUserDto>>> GetFormPendingUsers([FromForm] string formId)
        {
            return await _basicFormQueryService.GetFormPendingUsers(formId);
        }

        [HttpPost]
        [Tags("表单业务管理-综合表单查询")]
        [EndpointSummary("[全部表单查询] 打印PDF")]
        public async Task<IActionResult> PrintFormPdf([FromForm] string formId)
        {
            var result = await _formPrintService.PrintFormPdf(formId);
            if (result.Code != 200)
            {
                return StatusCode(result.Code, result);
            }
            return File(result.Data!.FileStream, "application/pdf", result.Data.FileName);
        }

        [HttpPost]
        [Tags("表单业务管理-综合表单查询")]
        [EndpointSummary("[全部表单查询] 批量打印PDF")]
        public async Task<IActionResult> PrintFormPdfBatch([FromBody] List<string> formIds)
        {
            var result = await _formPrintService.PrintFormPdfBatch(formIds);
            if (result.Code != 200)
            {
                return StatusCode(result.Code, result);
            }
            return File(result.Data!.FileStream, "application/zip", result.Data.FileName);
        }

        [HttpPost]
        [Tags("表单业务管理-综合表单查询")]
        [EndpointSummary("[全部表单查询] 导出Excel")]
        public async Task<IActionResult> ExportFormQueryExcel([FromBody] GetFormQueryPage getpage)
        {
            var result = await _formQueryExcelService.ExportFormQueryExcel(getpage);
            if (result.Code != 200)
            {
                return StatusCode(result.Code, result);
            }
            return File(result.Data!.FileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.Data.FileName);
        }
    }
}
