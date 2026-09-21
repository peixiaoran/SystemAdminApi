using System.Text.Json.Serialization;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Dto;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.FormBusiness.Forms.OverseasTripApp.Dto
{
    /// <summary>
    /// 出差单基础信息Dto
    /// </summary>
    public class OverseasTripAppDto
    {
        /// <summary>
        /// 出差单Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long FormId { get; set; }

        /// <summary>
        /// 表单类别Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long FormTypeId { get; set; }

        /// <summary>
        /// 所属规则Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long? RuleId { get; set; }

        /// <summary>
        /// 当前步骤Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long? CurrentStepId { get; set; }

        /// <summary>
        /// 出差单号
        /// </summary>
        public string FormNo { get; set; } = string.Empty;

        /// <summary>
        /// 表单状态
        /// </summary>
        public string FormStatus { get; set; } = string.Empty;

        /// <summary>
        /// 表单状态名称
        /// </summary>
        public string FormStatusName { get; set; } = string.Empty;

        /// <summary>
        /// 申请人工号
        /// </summary>
        public string ApplicantUserNo { get; set; } = string.Empty;

        /// <summary>
        /// 申请人姓名
        /// </summary>
        public string ApplicantUserName { get; set; } = string.Empty;

        /// <summary>
        /// 申请人部门名称
        /// </summary>
        public string ApplicantDeptName { get; set; } = string.Empty;

        /// <summary>
        /// 申请日期
        /// </summary>
        public DateOnly ApplicantDate { get; set; }

        /// <summary>
        /// 出发厂区
        /// </summary>
        public string? DepartureSite { get; set; } = string.Empty;

        /// <summary>
        /// 出发厂区名称
        /// </summary>
        public string? DepartureSiteName { get; set; } = string.Empty;

        /// <summary>
        /// 目的厂区
        /// </summary>
        public string? DestinationSite { get; set; } = string.Empty;

        /// <summary>
        /// 目的厂区名称
        /// </summary>
        public string? DestinationSiteName { get; set; } = string.Empty;

        /// <summary>
        /// 出差事由
        /// </summary>
        public string? TripReason { get; set; } = string.Empty;

        /// <summary>
        /// 出差开始日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 出差结束日期
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 出差天数
        /// </summary>
        public decimal? Days { get; set; }

        /// <summary>
        /// 去程交通方式
        /// </summary>
        public string? OutboundTravel { get; set; } = string.Empty;

        /// <summary>
        /// 去程交通方式名称
        /// </summary>
        public string? OutboundTravelName { get; set; } = string.Empty;

        /// <summary>
        /// 返程交通方式
        /// </summary>
        public string? ReturnTravel { get; set; } = string.Empty;

        /// <summary>
        /// 返程交通方式名称
        /// </summary>
        public string? ReturnTravelName { get; set; } = string.Empty;

        /// <summary>
        /// 工作内容
        /// </summary>
        public string? JobDescription { get; set; } = string.Empty;

        /// <summary>
        /// 附件列表
        /// </summary>
        public List<FormAttachmentDto> Attachment { get; set; } = new List<FormAttachmentDto>();

        /// <summary>
        /// 加审人列表
        /// </summary>
        public List<FormAddReviewDto> AddReview { get; set; } = new List<FormAddReviewDto>();

        /// <summary>
        /// 审批记录
        /// </summary>
        public List<FormReviewRecordDto> ReviewRecord { get; set; } = new List<FormReviewRecordDto>();

        /// <summary>
        /// 栏位权限
        /// </summary>
        public List<StepFieldPermissionDto> StepFieldPermission { get; set; } = new List<StepFieldPermissionDto>();
    }
}
