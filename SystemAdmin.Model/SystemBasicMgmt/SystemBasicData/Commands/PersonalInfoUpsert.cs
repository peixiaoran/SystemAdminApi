using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Commands
{
    /// <summary>
    /// 个人信息修改类
    /// </summary>
    public class PersonalInfoUpsert
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long UserId { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// 登录密码
        /// </summary>
        public string PassWord { get; set; } = string.Empty;

        /// <summary>
        /// 是否实时邮件通知
        /// </summary>
        public int IsRealtimeNotification { get; set; }

        /// <summary>
        /// 是否定时邮件通知
        /// </summary>
        public int IsScheduledNotification { get; set; }

        /// <summary>
        /// 头像图片地址
        /// </summary>
        public string AvatarAddress { get; set; } = string.Empty;

        /// <summary>
        /// 邮件通知语言
        /// </summary>
        public string NoticeLanguage { get; set; } = string.Empty;
    }
}
