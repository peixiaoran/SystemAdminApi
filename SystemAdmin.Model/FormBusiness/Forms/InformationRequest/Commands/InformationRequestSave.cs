namespace SystemAdmin.Model.FormBusiness.Forms.InformationRequest.Commands
{
    /// <summary>
    /// 资讯需求单保存类
    /// </summary>
    public class InformationRequestSave
    {
        /// <summary>
        /// 资讯需求单Id
        /// </summary>
        public string FormId { get; set; } = string.Empty;

        /// <summary>
        /// 需求类别
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// 现状
        /// </summary>
        public string? CurrentSituation { get; set; }

        /// <summary>
        /// 期望
        /// </summary>
        public string? Expectations { get; set; }

        /// <summary>
        /// 预计处理天数
        /// </summary>
        public decimal? EstimatedDays { get; set; }

        /// <summary>
        /// 评分
        /// </summary>
        public int? Rating { get; set; }
    }
}
