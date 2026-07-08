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
        /// 申请时间
        /// </summary>
        public DateTime ApplicantDate { get; set; }
    }
}
