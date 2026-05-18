namespace SystemAdmin.Model.FormBusiness.Workflow.FormReviewAction.Dto
{
    public class FormNoticeApprovedDto
    {
        /// <summary>
        /// 表单 Id
        /// </summary>
        public long FormId { get; set; }

        /// <summary>
        /// 表单编号
        /// </summary>
        public string FormNo { get; set; } = string.Empty;

        /// <summary>
        /// 表单类型中文名
        /// </summary>
        public string FormTypeNameCn { get; set; } = string.Empty;

        /// <summary>
        /// 表单类型英文名
        /// </summary>
        public string FormTypeNameEn { get; set; } = string.Empty;

        /// <summary>
        /// 审批页面路径
        /// </summary>
        public string ReviewPath { get; set; } = string.Empty;

        /// <summary>
        /// 申请人 UserId
        /// </summary>
        public long ApplicantUserId { get; set; }

        /// <summary>
        /// 申请人中文名
        /// </summary>
        public string ApplicantUserNameCn { get; set; } = string.Empty;

        /// <summary>
        /// 申请人英文名
        /// </summary>
        public string ApplicantUserNameEn { get; set; } = string.Empty;

        /// <summary>
        /// 申请人性别
        /// </summary>
        public int ApplicantGender { get; set; }

        /// <summary>
        /// 申请人邮箱
        /// </summary>
        public string ApplicantEmail { get; set; } = string.Empty;

        /// <summary>
        /// 申请人是否订阅实时通知（1=是，0=否）
        /// </summary>
        public int ApplicantIsRealtimeNotification { get; set; }
    }
}
