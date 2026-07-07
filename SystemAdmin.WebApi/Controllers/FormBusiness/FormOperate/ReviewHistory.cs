using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.FormBusiness.FormOperate.Dto;
using SystemAdmin.Model.FormBusiness.FormOperate.Queries;
using SystemAdmin.Service.FormBusiness.FormOperate;
using SystemAdmin.WebApi.Attributes;

namespace SystemAdmin.WebApi.Controllers.FormBusiness.FormOperate
{
    [JwtAuthorize]
    [RoutingAuthorize]
    [Route("api/FormBusiness/FormOperate/[controller]/[action]")]
    [ApiController]
    public class ReviewHistory : ControllerBase
    {
        private readonly ReviewHistoryService _reviewHistoryService;
        private readonly FormPrintPdfService _formPrintPdfService;
        public ReviewHistory(ReviewHistoryService reviewHistoryService, FormPrintPdfService formPrintPdfService)
        {
            _reviewHistoryService = reviewHistoryService;
            _formPrintPdfService = formPrintPdfService;
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[审批历史记录] 表单组别下拉")]
        public async Task<Result<List<FormGroupDropDto>>> GetFormGroupDrop()
        {
            return await _reviewHistoryService.GetFormGroupDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[审批历史记录] 表单类别下拉")]
        public async Task<Result<List<FormTypeDropDto>>> GetFormTypeDrop([FromForm] string formGroupId)
        {
            return await _reviewHistoryService.GetFormTypeDrop(formGroupId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[审批历史记录] 表单状态下拉")]
        public async Task<Result<List<FormStatusDropDto>>> GetFormStatusDrop()
        {
            return await _reviewHistoryService.GetFormStatusDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[审批历史记录] 查询申请记录分页")]
        public async Task<ResultPaged<FormHistoryDto>> GetReviewHistoryPage([FromBody] GetFormHistoryPage getpage)
        {
            return await _reviewHistoryService.GetReviewHistoryPage(getpage);
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[审批历史记录] 查询待审批人")]
        public async Task<Result<List<FormPendingUserDto>>> GetFormPendingUsers([FromForm] string formId)
        {
            return await _reviewHistoryService.GetFormPendingUsers(formId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[审批历史记录] 打印PDF")]
        public async Task<IActionResult> PrintFormPdf([FromForm] string formId, [FromForm] string prefix)
        {
            var result = await _formPrintPdfService.PrintFormPdf(formId, prefix);
            if (result.Code != 200)
            {
                return Ok(result);
            }
            return File(result.Data!.FileBytes, "application/pdf", result.Data.FileName);
        }
    }
}
