using SqlSugar;

namespace SystemAdmin.Model.FormBusiness.Forms.LeaveCancell.Queries
{
    /// <summary>
    /// 查询请假单分页请求参数
    /// </summary>
    public class GetLeaveRequestPage : PageModel
    {
        /// <summary>
        /// 请假单号
        /// </summary>
        public string LeaveRequestNo { get; set; } = string.Empty;

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndDate { get; set; }
    }
}
