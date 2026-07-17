using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.FormBusiness.Forms.LeaveCancell.Dto
{
    /// <summary>
    /// 请假单Dto
    /// </summary>
    public class LeaveRequestDto
    {
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
        /// 假别编码
        /// </summary>
        public string? LeaveType { get; set; }

        /// <summary>
        /// 假别名称
        /// </summary>
        public string? LeaveTypeName { get; set; }

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
        /// 可销假时数（系统计算）
        /// </summary>
        public decimal? CancellableHours { get; set; }

        /// <summary>
        /// 申请时间
        /// </summary>
        public DateOnly ApplicantDate { get; set; }
    }
}
