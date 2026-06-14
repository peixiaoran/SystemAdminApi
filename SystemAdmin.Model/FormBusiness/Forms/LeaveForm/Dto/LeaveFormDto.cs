using System.Text.Json.Serialization;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Dto;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Dto
{
    /// <summary>
    /// 请假单基础信息Dto
    /// </summary>
    public class LeaveFormDto
    {
        /// <summary>
        /// 请假单Id
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
        public long RuleId { get; set; }

        /// <summary>
        /// 当前步骤Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long? CurrentStepId { get; set; }

        /// <summary>
        /// 请假单号
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
        /// 假别编码
        /// </summary>
        public string? LeaveType { get; set; }

        /// <summary>
        /// 请假事由
        /// </summary>
        public string? LeaveReason { get; set; } = string.Empty;

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// 请假小时（系统计算）
        /// </summary>
        public decimal? LeaveHours { get; set; }

        /// <summary>
        /// 代理人Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long? AgentUserId { get; set; }

        /// <summary>
        /// 代理人姓名
        /// </summary>
        public string? AgentUserName { get; set; } = string.Empty;

        /// <summary>
        /// 附件列表
        /// </summary>
        public List<FormAttachmentDto> Attachment { get; set; } = new List<FormAttachmentDto>();

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
