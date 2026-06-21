using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.FormBusiness.FormOperate.Dto
{
    public class InvalidFormDto
    {
        /// <summary>
        /// 表单类别Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long FormTypeId { get; set; }

        /// <summary>
        /// 表单类型
        /// </summary>
        public string FormTypeName { get; set; } = string.Empty;

        /// <summary>
        /// 表单Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long FormId { get; set; }

        /// <summary>
        /// 表单单号
        /// </summary>
        public string FormNo { get; set; } = string.Empty;

        /// <summary>
        /// 申请日期
        /// </summary>
        public DateOnly ApplicantDate { get; set; }

        /// <summary>
        /// 表单状态
        /// </summary>
        public string FormStatus { get; set; } = string.Empty;

        /// <summary>
        /// 表单状态名称
        /// </summary>
        public string FormStatusName { get; set; } = string.Empty;

        /// <summary>
        /// 作废日期
        /// </summary>
        public DateTime VoidedDate { get; set; }

        /// <summary>
        /// 表单视图路径
        /// </summary>
        public string ViewPath { get; set; } = string.Empty;
    }
}
