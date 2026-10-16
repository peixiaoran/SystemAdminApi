using System.Text.Json.Serialization;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Dto;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.FormBusiness.Forms.SystemAccountRequest.Dto
{
    /// <summary>
    /// 系统账号申请单基础信息Dto
    /// </summary>
    public class SystemAccountRequestDto
    {
        /// <summary>
        /// 系统账号申请单Id
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
        /// 系统账号申请单号
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
        /// 需求人部门
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long? RequesterUserDept { get; set; }

        /// <summary>
        /// 需求人工号
        /// </summary>
        public string? RequesterUserNo { get; set; }

        /// <summary>
        /// 需求人姓名（中文）
        /// </summary>
        public string? RequesterUserNameCn { get; set; }

        /// <summary>
        /// 需求人姓名（英文）
        /// </summary>
        public string? RequesterUserNameEn { get; set; }

        /// <summary>
        /// 需求人性别
        /// </summary>
        public int? RequesterGender { get; set; }

        /// <summary>
        /// 需求人国籍
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long? NationalityId { get; set; }

        /// <summary>
        /// 需求人邮件
        /// </summary>
        public string? RequesterEmail { get; set; }

        /// <summary>
        /// 申请原因
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// 系统明细列表
        /// </summary>
        public List<SystemAccountSystemDto> Systems { get; set; } = new List<SystemAccountSystemDto>();

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

    /// <summary>
    /// 系统账号申请单系统明细Dto
    /// </summary>
    public class SystemAccountSystemDto
    {
        /// <summary>
        /// 系统
        /// </summary>
        public string System { get; set; } = string.Empty;

        /// <summary>
        /// 厂区
        /// </summary>
        public List<string> Sites { get; set; } = new List<string>();

        /// <summary>
        /// 权限
        /// </summary>
        public List<string> Permissions { get; set; } = new List<string>();

        /// <summary>
        /// 评分
        /// </summary>
        public int? Rating { get; set; }
    }
}
