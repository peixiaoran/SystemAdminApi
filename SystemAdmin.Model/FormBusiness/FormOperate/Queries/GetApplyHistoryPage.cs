using SqlSugar;

namespace SystemAdmin.Model.FormBusiness.FormOperate.Queries
{
    public class GetApplyHistoryPage : PageModel
    {
        /// <summary>
        /// 表单组别Id
        /// </summary>
        public string FormGroupId { get; set; } = string.Empty;

        /// <summary>
        /// 表单类别Id
        /// </summary>
        public string FormTypeId { get; set; } = string.Empty;

        /// <summary>
        /// 表单单号
        /// </summary>
        public string FormNo { get; set; } = string.Empty;

        /// <summary>
        /// 表单状态
        /// </summary>
        public string FormStatus { get; set; } = string.Empty;

        /// <summary>
        /// 申请开始日期
        /// </summary>
        public DateOnly? StartDate { get; set; }

        /// <summary>
        /// 申请结束日期
        /// </summary>
        public DateOnly? EndDate { get; set; }

        /// <summary>
        /// 关键字
        /// </summary>
        public string Keyword { get; set; } = string.Empty;
    }
}
