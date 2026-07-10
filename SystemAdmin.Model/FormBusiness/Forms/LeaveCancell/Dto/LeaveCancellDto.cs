using System.Text.Json.Serialization;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Dto;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.FormBusiness.Forms.LeaveCancell.Dto
{
    /// <summary>
    /// 销假单基础信息Dto
    /// </summary>
    public class LeaveCancellDto
    {
        /// <summary>
        /// 销假单Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long FormId { get; set; }

        /// <summary>
        /// 销假单号
        /// </summary>
        public string FormNo { get; set; } = string.Empty;

        /// <summary>
        /// 表单状态
        /// </summary>
        public string FormStatus { get; set; } = string.Empty;

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
        /// 请假单Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long LeaveRequestId { get; set; }

        /// <summary>
        /// 请假单号
        /// </summary>
        public string LeaveRequestNo { get; set; } = string.Empty;

        /// <summary>
        /// 请假单开始时间
        /// </summary>
        public DateTime? LeaveStartDateTime { get; set; }

        /// <summary>
        /// 请假单结束时间
        /// </summary>
        public DateTime? LeaveEndDateTime { get; set; }

        /// <summary>
        /// 请假总时长（系统计算）
        /// </summary>
        public decimal? LeaveHours { get; set; }

        /// <summary>
        /// 销假开始时间
        /// </summary>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// 销假结束时间
        /// </summary>
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// 销假时数（系统计算）
        /// </summary>
        public decimal? CancellHours { get; set; }

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
