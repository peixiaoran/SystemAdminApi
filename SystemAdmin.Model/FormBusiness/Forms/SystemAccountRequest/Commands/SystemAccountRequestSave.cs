using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.FormBusiness.Forms.SystemAccountRequest.Commands
{
    /// <summary>
    /// 系统账号申请单保存类
    /// </summary>
    public class SystemAccountRequestSave
    {
        /// <summary>
        /// 系统账号申请单Id
        /// </summary>
        public string FormId { get; set; } = string.Empty;

        /// <summary>
        /// 需求人部门
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long? RequesterUserDept { get; set; }

        /// <summary>
        /// 需求人工号
        /// </summary>
        public string? RequesterUserNo { get; set; }

        /// <summary>
        /// 需求人姓名（中文）
        /// </summary>
        public string? RequesterUserNameCn { get; set; }

        /// <summary>
        /// 需求人姓名（英文）
        /// </summary>
        public string? RequesterUserNameEn { get; set; }

        /// <summary>
        /// 需求人性别
        /// </summary>
        public int? RequesterGender { get; set; }

        /// <summary>
        /// 需求人国籍
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long? NationalityId { get; set; }

        /// <summary>
        /// 需求人邮件
        /// </summary>
        public string? RequesterEmail { get; set; }

        /// <summary>
        /// 申请原因
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// 系统明细列表
        /// </summary>
        public List<SystemAccountSystemSave> Systems { get; set; } = new List<SystemAccountSystemSave>();
    }

    /// <summary>
    /// 系统账号申请单系统明细保存类
    /// </summary>
    public class SystemAccountSystemSave
    {
        /// <summary>
        /// 系统
        /// </summary>
        public string System { get; set; } = string.Empty;

        /// <summary>
        /// 厂区
        /// </summary>
        public List<string> Sites { get; set; } = new List<string>();

        /// <summary>
        /// 权限
        /// </summary>
        public List<string> Permissions { get; set; } = new List<string>();

        /// <summary>
        /// 评分
        /// </summary>
        public int? Rating { get; set; }
    }
}
