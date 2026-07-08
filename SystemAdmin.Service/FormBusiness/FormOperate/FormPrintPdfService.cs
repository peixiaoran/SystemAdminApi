using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Options;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.FormOperate.Dto;
using SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Dto;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Dto;
using SystemAdmin.Repository.FormBusiness.Forms;
using SystemAdmin.Repository.FormBusiness.Workflow;

namespace SystemAdmin.Service.FormBusiness.FormOperate
{
    /// <summary>
    /// 表单打印PDF服务（QuestPDF）
    /// 根据表单前缀分发到对应的打印方法，新表单在 PrintFormPdf 的 switch 中登记，
    /// 并复用「PDF通用组件」region 内的页面设置、标题、栏位行、附件/审批记录表格、印章与单元格样式
    /// </summary>
    public class FormPrintPdfService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<FormPrintPdfService> _logger;
        private readonly Language _lang;
        private readonly FormPermissionChecker _formChecker;
        private readonly LeaveFormRepository _leaveFormRepo;
        private readonly FormManager _formmanger;
        private readonly LocalizationService _localization;
        private readonly string _this = "FormBusiness.FormOperate.FormPending";
        private readonly string _forms = "FormBusiness.Forms.";

        static FormPrintPdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
        }

        public FormPrintPdfService(CurrentUser loginuser, ILogger<FormPrintPdfService> logger, Language lang, FormPermissionChecker formChecker, LeaveFormRepository leaveFormRepo, FormManager formmanger, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _lang = lang;
            _formChecker = formChecker;
            _leaveFormRepo = leaveFormRepo;
            _formmanger = formmanger;
            _localization = localization;
        }

        /// <summary>
        /// 打印PDF（根据表单前缀分发，LVR=请假单）
        /// </summary>
        public async Task<Result<FormPdfDto>> PrintFormPdf(string formId, string prefix)
        {
            try
            {
                return prefix switch
                {
                    "LVR" => await PrintLeaveFormPdf(long.Parse(formId)),
                    _ => Result<FormPdfDto>.Failure(400, _localization.ReturnMsg($"{_this}PrintNotSupport"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<FormPdfDto>.Failure(500, ex.Message);
            }
        }

        #region 请假单PDF

        /// <summary>
        /// 生成请假单PDF（权限校验+取数）
        /// </summary>
        private async Task<Result<FormPdfDto>> PrintLeaveFormPdf(long formId)
        {
            var isCan = await _formChecker.CanView(formId, "View");
            if (!isCan)
            {
                return Result<FormPdfDto>.Failure(400, _localization.ReturnMsg($"{_forms}NotCanView"));
            }

            // 表头、附件、审批记录、栏位权限（按当前登录用户，仅判断是否显示）
            var form = await _leaveFormRepo.GetLeaveForm(formId);
            form.Attachment = await _formmanger.GetAttachmentList(formId);
            form.ReviewRecord = await _formmanger.GetReviewRecordList(formId);
            form.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(formId, _loginuser.UserId);

            // 假别名称（按当前语言取字典）
            var leaveTypeDics = await _leaveFormRepo.GetLeaveTypeDictionary();
            var leaveTypeDic = leaveTypeDics.FirstOrDefault(dic => dic.DicCode == form.LeaveType);
            var leaveTypeName = _lang.Locale == "zh-CN" ? leaveTypeDic?.DicNameCn : leaveTypeDic?.DicNameEn;

            var pdf = new FormPdfDto
            {
                FileName = $"{Msg("PdfLeaveTitle")}_{form.FormNo}.pdf",
                FileStream = BuildLeaveFormPdf(form, leaveTypeName)
            };
            return Result<FormPdfDto>.Ok(pdf);
        }

        /// <summary>
        /// 组装请假单PDF文档（仅负责请假单版面，通用部分调用「PDF通用组件」）
        /// </summary>
        private MemoryStream BuildLeaveFormPdf(LeaveFormDto form, string? leaveTypeName)
        {
            var show = BuildFieldVisibility(form.StepFieldPermission);

            var leavePeriod = form.StartDateTime.HasValue && form.EndDateTime.HasValue
                ? $"{form.StartDateTime:yyyy-MM-dd HH:mm:ss}  -  {form.EndDateTime:yyyy-MM-dd HH:mm:ss}"
                : string.Empty;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurePage(page);
                    ComposeApprovalStamp(page, form.FormStatus, form.ReviewRecord);

                    page.Content().Column(column =>
                    {
                        ComposeTitle(column, Msg("PdfLeaveTitle"));

                        // 表单编号 / 申请日期
                        var row1 = new List<PdfField>();
                        if (show("FormNo")) row1.Add(new PdfField(Msg("PdfFormNo"), form.FormNo));
                        if (show("ApplyDate")) row1.Add(new PdfField(Msg("PdfApplicantDate"), form.ApplicantDate.ToString("yyyy-MM-dd")));
                        ComposeFieldRow(column, row1);

                        // 用户工号 / 用户姓名 / 用户部门
                        var row2 = new List<PdfField>();
                        if (show("UserNo")) row2.Add(new PdfField(Msg("PdfUserNo"), form.ApplicantUserNo));
                        if (show("UserName")) row2.Add(new PdfField(Msg("PdfUserName"), form.ApplicantUserName));
                        if (show("Department")) row2.Add(new PdfField(Msg("PdfDepartment"), form.ApplicantDeptName));
                        ComposeFieldRow(column, row2);

                        // 请假类型 / 请假时间
                        var row3 = new List<PdfField>();
                        if (show("LeaveType")) row3.Add(new PdfField(Msg("PdfLeaveType"), leaveTypeName ?? string.Empty));
                        if (show("LeavePeriod")) row3.Add(new PdfField(Msg("PdfLeavePeriod"), leavePeriod));
                        ComposeFieldRow(column, row3);

                        // 代理人 / 请假总时数
                        var row4 = new List<PdfField>();
                        if (show("SelectAgent")) row4.Add(new PdfField(Msg("PdfAgentUser"), form.AgentUserName ?? string.Empty));
                        if (show("LeaveHours")) row4.Add(new PdfField(Msg("PdfLeaveHours"), (form.LeaveHours ?? 0).ToString("0.00"), Emphasized: true));
                        ComposeFieldRow(column, row4);

                        // 请假事由
                        if (show("LeaveReason"))
                        {
                            ComposeFieldRow(column, new List<PdfField>
                            {
                                new PdfField(Msg("PdfLeaveReason"), form.LeaveReason ?? string.Empty, MinHeight: 44f)
                            });
                        }

                        // 附件
                        if (show("Upload"))
                        {
                            ComposeAttachmentTable(column, form.Attachment);
                        }

                        // 审批记录
                        ComposeReviewRecordTable(column, form.ReviewRecord);
                    });
                });
            });

            return GeneratePdfStream(document);
        }

        #endregion

        #region PDF通用组件（多表单共用：样式、页面、标题、栏位行、表格、印章）

        // 通用样式
        private const string FontFamilyName = "Microsoft YaHei";
        private const string BorderColor = "#DCDFE6";
        private const string LabelBgColor = "#F5F7FA";
        private const string LabelTextColor = "#606266";
        private const string BodyTextColor = "#303133";
        private const string MutedTextColor = "#909399";
        private const string EmphasizedColor = "#F56C6C";
        private const string StampColor = "#1F9254";

        // 标签格宽度 / 每行第一个值格的固定宽度（保证各行第二个栏位起始位置对齐）
        private const float LabelCellWidth = 66f;
        private const float FirstValueCellWidth = 136f;

        /// <summary>
        /// 表单栏位（标签+值）
        /// </summary>
        private sealed record PdfField(string Label, string Value, float Weight = 1f, bool Emphasized = false, float MinHeight = 0f);

        /// <summary>
        /// 读取表单多语言文案
        /// </summary>
        private string Msg(string key)
        {
            return _localization.ReturnMsg($"{_forms}{key}");
        }

        /// <summary>
        /// 页面通用设置（A4、边距、默认字体）
        /// </summary>
        private static void ConfigurePage(PageDescriptor page)
        {
            page.Size(PageSizes.A4);
            page.Margin(28);
            page.DefaultTextStyle(style => style.FontSize(9).FontFamily(FontFamilyName).FontColor(BodyTextColor));
        }

        /// <summary>
        /// 表单标题（居中标题+分隔线）
        /// </summary>
        private static void ComposeTitle(ColumnDescriptor column, string title)
        {
            column.Item().AlignCenter().Text(title).FontSize(14).SemiBold();
            column.Item().PaddingTop(10).LineHorizontal(0.75f).LineColor(BorderColor);
            column.Item().PaddingTop(14);
        }

        /// <summary>
        /// 构建栏位可见性判断（按当前登录用户的栏位权限，未配置的栏位默认显示）
        /// </summary>
        private static Func<string, bool> BuildFieldVisibility(List<StepFieldPermissionDto> permissions)
        {
            var fieldVisible = permissions
                .GroupBy(permission => permission.FieldKey)
                .ToDictionary(group => group.Key, group => group.Any(permission => permission.IsVisible == 1));

            return fieldKey => !fieldVisible.TryGetValue(fieldKey, out var isVisible) || isVisible;
        }

        /// <summary>
        /// 组装一行表单栏位（不可见的栏位不传入即可）
        /// </summary>
        private static void ComposeFieldRow(ColumnDescriptor column, List<PdfField> fields)
        {
            if (fields.Count == 0)
            {
                return;
            }

            column.Item().PaddingBottom(10).Row(row =>
            {
                for (var index = 0; index < fields.Count; index++)
                {
                    var field = fields[index];
                    row.ConstantItem(LabelCellWidth).Element(LabelCell).Text(field.Label).FontColor(LabelTextColor);

                    // 多栏位行的第一个值格固定宽度，使每行第二个栏位从相同位置开始
                    var valueItem = index == 0 && fields.Count > 1
                        ? row.ConstantItem(FirstValueCellWidth)
                        : row.RelativeItem(field.Weight);

                    var text = valueItem.Element(container => ValueCell(container, field.MinHeight))
                                        .Text(field.Value);
                    if (field.Emphasized)
                    {
                        text.FontColor(EmphasizedColor).SemiBold();
                    }
                }
            });
        }

        /// <summary>
        /// 组装附件表格（左侧标签+右侧表格）
        /// </summary>
        private void ComposeAttachmentTable(ColumnDescriptor column, List<FormAttachmentDto> attachments)
        {
            column.Item().PaddingBottom(10).Row(row =>
            {
                row.ConstantItem(LabelCellWidth).Element(LabelCell).Text(Msg("PdfAttachment")).FontColor(LabelTextColor);
                row.RelativeItem().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);
                        columns.RelativeColumn();
                        columns.ConstantColumn(90);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("#").FontColor(LabelTextColor);
                        header.Cell().Element(HeaderCell).Text(Msg("PdfAttachmentName")).FontColor(LabelTextColor);
                        header.Cell().Element(HeaderCell).AlignCenter().Text(Msg("PdfAttachmentSize")).FontColor(LabelTextColor);
                    });

                    if (attachments.Count == 0)
                    {
                        table.Cell().ColumnSpan(3).Element(BodyCell).AlignCenter().Text(Msg("PdfNoData")).FontColor(MutedTextColor);
                        return;
                    }

                    var index = 1;
                    foreach (var attachment in attachments)
                    {
                        table.Cell().Element(BodyCell).Text(index++.ToString());
                        table.Cell().Element(BodyCell).Text(attachment.AttachmentName);
                        table.Cell().Element(BodyCell).AlignRight().Text($"{attachment.AttachmentSize} KB");
                    }
                });
            });
        }

        /// <summary>
        /// 组装审批记录表格（小标题+表格）
        /// </summary>
        private void ComposeReviewRecordTable(ColumnDescriptor column, List<FormReviewRecordDto> reviewRecords)
        {
            column.Item().PaddingTop(6).Text(Msg("PdfReviewRecord")).FontSize(11).SemiBold();

            column.Item().PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);
                    columns.RelativeColumn(2f);    // 审批步骤
                    columns.RelativeColumn(2f);    // 审批人
                    columns.RelativeColumn(1.2f);  // 审批结果
                    columns.RelativeColumn(4f);    // 意见
                    columns.RelativeColumn(2.4f);  // 审批时间
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("#").FontColor(LabelTextColor);
                    header.Cell().Element(HeaderCell).Text(Msg("PdfReviewStep")).FontColor(LabelTextColor);
                    header.Cell().Element(HeaderCell).Text(Msg("PdfReviewUser")).FontColor(LabelTextColor);
                    header.Cell().Element(HeaderCell).Text(Msg("PdfReviewResult")).FontColor(LabelTextColor);
                    header.Cell().Element(HeaderCell).Text(Msg("PdfComment")).FontColor(LabelTextColor);
                    header.Cell().Element(HeaderCell).AlignCenter().Text(Msg("PdfReviewTime")).FontColor(LabelTextColor);
                });

                if (reviewRecords.Count == 0)
                {
                    table.Cell().ColumnSpan(6).Element(BodyCell).AlignCenter().Text(Msg("PdfNoData")).FontColor(MutedTextColor);
                    return;
                }

                var index = 1;
                foreach (var record in reviewRecords)
                {
                    table.Cell().Element(BodyCell).Text(index++.ToString());
                    table.Cell().Element(BodyCell).Text(record.StepName);
                    table.Cell().Element(BodyCell).Text(record.OperationUserName);
                    table.Cell().Element(BodyCell).AlignCenter().Text(record.ReviewResultName);
                    table.Cell().Element(BodyCell).Text(record.Comment);
                    table.Cell().Element(BodyCell).AlignCenter().Text(record.ReviewDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                }
            });
        }

        /// <summary>
        /// 组装审批完成印章（仅已批准状态加盖；页面前景层右上角，覆盖在所有内容之上）
        /// </summary>
        private void ComposeApprovalStamp(PageDescriptor page, string formStatus, List<FormReviewRecordDto> reviewRecords)
        {
            if (formStatus != FormStatus.Approved.ToEnumString())
            {
                return;
            }

            var approvedDateTime = reviewRecords.Count > 0
                ? reviewRecords.Max(record => record.ReviewDateTime)
                : (DateTime?)null;

            var svg = BuildApprovalStampSvg(
                Msg("PdfStampApproved"),
                _lang.IsChinese ? "APPROVED" : string.Empty,
                approvedDateTime?.ToString("yyyy-MM-dd") ?? string.Empty);

            page.Foreground()
                .AlignTop()
                .AlignRight()
                .PaddingTop(68)
                .PaddingRight(18)
                .Width(118)
                .Height(118)
                .Svg(svg);
        }

        /// <summary>
        /// 生成审批完成印章SVG（双环+五角星+主文字+副文字+日期，微倾斜盖章效果）
        /// </summary>
        private static string BuildApprovalStampSvg(string mainText, string subText, string dateText)
        {
            static string Escape(string value) => value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

            // 主文字自适应：中文短文字用大号字，英文长文字缩小并加字距
            var mainFontSize = mainText.Length > 6 ? 15 : 21;
            var mainLetterSpacing = mainText.Length > 6 ? 1 : 2;

            var subLine = string.IsNullOrEmpty(subText)
                ? string.Empty
                : $"<text x=\"80\" y=\"105\" font-size=\"8.5\" letter-spacing=\"3\" text-anchor=\"middle\">{Escape(subText)}</text>";

            var dateLine = string.IsNullOrEmpty(dateText)
                ? string.Empty
                : $"<text x=\"80\" y=\"121\" font-size=\"9\" letter-spacing=\"1\" text-anchor=\"middle\">{Escape(dateText)}</text>";

            return $"""
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 160 160">
                  <g transform="rotate(-12 80 80)" opacity="0.88">
                    <circle cx="80" cy="80" r="74" fill="none" stroke="{StampColor}" stroke-width="3"/>
                    <circle cx="80" cy="80" r="65" fill="none" stroke="{StampColor}" stroke-width="1"/>
                    <g fill="{StampColor}" font-family="{FontFamilyName}">
                      <path d="M80 41 L81.59 45.82 L86.66 45.84 L82.57 48.83 L84.11 53.66 L80 50.7 L75.89 53.66 L77.43 48.83 L73.34 45.84 L78.41 45.82 Z"/>
                      <text x="80" y="88" font-size="{mainFontSize}" font-weight="bold" letter-spacing="{mainLetterSpacing}" text-anchor="middle">{Escape(mainText)}</text>
                      {subLine}
                      {dateLine}
                    </g>
                  </g>
                </svg>
                """;
        }

        /// <summary>
        /// 文档生成PDF流
        /// </summary>
        private static MemoryStream GeneratePdfStream(Document document)
        {
            var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0;    // 必须归零，否则 File() 读到 0 字节
            return stream;
        }

        #region 单元格样式

        /// <summary>
        /// 标签单元格样式
        /// </summary>
        private static IContainer LabelCell(IContainer container)
        {
            return container.Border(0.5f)
                            .BorderColor(BorderColor)
                            .Background(LabelBgColor)
                            .PaddingVertical(5)
                            .PaddingHorizontal(6)
                            .AlignMiddle();
        }

        /// <summary>
        /// 值单元格样式（minHeight 大于 0 时为多行文本区域）
        /// </summary>
        private static IContainer ValueCell(IContainer container, float minHeight = 0f)
        {
            container = container.Border(0.5f)
                                 .BorderColor(BorderColor)
                                 .PaddingVertical(5)
                                 .PaddingHorizontal(6);
            return minHeight > 0 ? container.MinHeight(minHeight) : container.AlignMiddle();
        }

        /// <summary>
        /// 表格表头单元格样式
        /// </summary>
        private static IContainer HeaderCell(IContainer container)
        {
            return container.Border(0.5f)
                            .BorderColor(BorderColor)
                            .Background(LabelBgColor)
                            .PaddingVertical(5)
                            .PaddingHorizontal(6)
                            .AlignMiddle();
        }

        /// <summary>
        /// 表格内容单元格样式（垂直居中，行内某列换行时其余列保持居中）
        /// </summary>
        private static IContainer BodyCell(IContainer container)
        {
            return container.Border(0.5f)
                            .BorderColor(BorderColor)
                            .PaddingVertical(5)
                            .PaddingHorizontal(6)
                            .AlignMiddle();
        }

        #endregion

        #endregion
    }
}
