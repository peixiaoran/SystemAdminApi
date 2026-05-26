namespace SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Dto
{
    /// <summary>
    /// 表单用户基本信息
    /// </summary>
    public class UserBasicInfoDto
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 用户工号
        /// </summary>
        public string UserNo { get; set; } = string.Empty;

        /// <summary>
        /// 用户姓名
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 部门Id
        /// </summary>
        public long DetpId { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string DetpName { get; set; } = string.Empty;

        /// <summary>
        /// 职级编码
        /// </summary>
        public string PositionNo { get; set; } = string.Empty;

        /// <summary>
        /// 职级名称
        /// </summary>
        public string PositionName { get; set; } = string.Empty;

        /// <summary>
        /// 电话号码
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
