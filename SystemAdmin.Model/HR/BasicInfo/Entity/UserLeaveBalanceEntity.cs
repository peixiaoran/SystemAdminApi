using SqlSugar;

namespace SystemAdmin.Model.HR.BasicInfo.Entity
{
    [SugarTable("[Hr].[UserLeaveBalance]")]
    public class UserLeaveBalanceEntity
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 年度
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 假别
        /// </summary>
        public string LeaveType { get; set; } = string.Empty;

        /// <summary>
        /// 给予时数
        /// </summary>
        public decimal RenderHours { get; set; }

        /// <summary>
        /// 剩余时数
        /// </summary>
        public decimal RemainingHours { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long CreatedBy { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public long? ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? ModifiedDate { get; set; }
    }
}
