using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.FormBusiness.Forms.LeaveCancell.Dto
{
    /// <summary>
    /// 请假单明细Dto（销假单绑定的请假单信息）
    /// </summary>
    public class LeaveRequestDetailDto
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
        /// 请假时数
        /// </summary>
        public decimal? LeaveHours { get; set; }
    }
}
