using System.Text.Json.Serialization;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Dto;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.FormBusiness.Forms.InformationRequest.Dto
{
    /// <summary>
    /// 资讯需求单基础信息Dto
    /// </summary>
    public class InformationRequestDto
    {
        /// <summary>
        /// 资讯需求单Id
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
        /// 资讯需求单号
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
        /// 需求类别
        /// </summary>
        public string? Category { get; set; } = string.Empty;

        /// <summary>
        /// 需求类别名称
        /// </summary>
        public string? CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// 现状
        /// </summary>
        public string? CurrentSituation { get; set; } = string.Empty;

        /// <summary>
        /// 期望
        /// </summary>
        public string? Expectations { get; set; } = string.Empty;

        /// <summary>
        /// 预计处理天数
        /// </summary>
        public decimal? EstimatedDays { get; set; }

        /// <summary>
        /// 评分
        /// </summary>
        public int? Rating { get; set; }

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
