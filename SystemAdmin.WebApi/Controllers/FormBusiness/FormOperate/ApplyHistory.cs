using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.FormBusiness.FormOperate.Dto;
using SystemAdmin.Model.FormBusiness.FormOperate.Queries;
using SystemAdmin.Service.FormBusiness.FormExport;
using SystemAdmin.Service.FormBusiness.FormOperate;
using SystemAdmin.WebApi.Attributes;

namespace SystemAdmin.WebApi.Controllers.FormBusiness.FormOperate
{
    [JwtAuthorize]
    [RoutingAuthorize]
    [Route("api/FormBusiness/FormOperate/[controller]/[action]")]
    [ApiController]
    public class ApplyHistory : ControllerBase
    {
        private readonly ApplyHistoryService _appHistoryService;
        private readonly FormPrintService _formPrintService;
        private readonly FormHistoryExcelService _formHistoryExcelService;
        public ApplyHistory(ApplyHistoryService appHistoryService, FormPrintService formPrintService, FormHistoryExcelService formExcelService)
        {
            _appHistoryService = appHistoryService;
            _formPrintService = formPrintService;
            _formHistoryExcelService = formExcelService;
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[申请历史记录] 表单组别下拉")]
        public async Task<Result<List<FormGroupDropDto>>> GetFormGroupDrop()
        {
            return await _appHistoryService.GetFormGroupDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[申请历史记录] 表单类别下拉")]
        public async Task<Result<List<FormTypeDropDto>>> GetFormTypeDrop([FromForm] string formGroupId)
        {
            return await _appHistoryService.GetFormTypeDrop(formGroupId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[申请历史记录] 查询申请记录分页")]
        public async Task<ResultPaged<FormHistoryDto>> GetApplyHistoryPage([FromBody] GetFormHistoryPage getpage)
        {
            return await _appHistoryService.GetApplyHistoryPage(getpage);
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[申请历史记录] 查询待审批人")]
        public async Task<Result<List<FormPendingUserDto>>> GetFormPendingUsers([FromForm] string formId)
        {
            return await _appHistoryService.GetFormPendingUsers(formId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[申请历史记录] 表单撤回")]
        public async Task<Result<int>> WithdrawForm([FromForm] string formId)
        {
            return await _appHistoryService.WithdrawForm(formId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[申请历史记录] 表单作废")]
        public async Task<Result<int>> VoidedForm([FromForm] string formId)
        {
            return await _appHistoryService.VoidedForm(formId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[审批历史记录] 打印PDF")]
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
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[申请历史记录] 批量打印PDF")]
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
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[申请历史记录] 导出Excel")]
        public async Task<IActionResult> ExportApplyHistoryExcel([FromBody] GetFormHistoryPage getpage)
        {
            var result = await _formHistoryExcelService.ExportApplyHistoryExcel(getpage);
            if (result.Code != 200)
            {
                return StatusCode(result.Code, result);
            }
            return File(result.Data!.FileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.Data.FileName);
        }
    }
}
