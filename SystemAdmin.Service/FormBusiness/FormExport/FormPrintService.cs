using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.FormOperate.Dto;
using SystemAdmin.Model.FormBusiness.Forms.DocumentCirculate.Dto;
using SystemAdmin.Model.FormBusiness.Forms.LeaveCancell.Dto;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Dto;
using SystemAdmin.Repository.FormBusiness.Forms;
using SystemAdmin.Repository.FormBusiness.Workflow;
using System.IO.Compression;

namespace SystemAdmin.Service.FormBusiness.FormOperate
{
    /// <summary>
    /// 新表单需在 PrintFormPdf 的 switch 中按前缀登记
    /// </summary>
    public partial class FormPrintPdfService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<FormPrintPdfService> _logger;
        private readonly Language _lang;
        private readonly FormPermissionChecker _formChecker;
        private readonly LeaveRequestRepository _leaveRequestRepo;
        private readonly LeaveCancellRepository _leaveCancellRepo;
        private readonly DocumentCirculateRepository _documentCirculateRepo;
        private readonly FormManager _formmanger;
        private readonly LocalizationService _localization;
        private readonly string _this = "FormBusiness.FormOperate.FormPending";
        private readonly string _forms = "FormBusiness.Forms.";

        static FormPrintPdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
        }

        public FormPrintPdfService(CurrentUser loginuser, ILogger<FormPrintPdfService> logger, Language lang, FormPermissionChecker formChecker, LeaveRequestRepository leaveRequestRepo, LeaveCancellRepository leaveCancellRepo, DocumentCirculateRepository documentCirculateRepo, FormManager formmanger, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _lang = lang;
            _formChecker = formChecker;
            _leaveRequestRepo = leaveRequestRepo;
            _leaveCancellRepo = leaveCancellRepo;
            _documentCirculateRepo = documentCirculateRepo;
            _formmanger = formmanger;
            _localization = localization;
        }

        /// <summary>
        /// LVR=请假单，LCF=销假单，DCS=传签单
        /// </summary>
        public async Task<Result<FormPdfDto>> PrintFormPdf(string formId)
        {
            try
            {
                var id = long.Parse(formId);
                var prefix = await _formmanger.GetFormPrefix(id);

                return prefix switch
                {
                    "LVR" => await PrintLeaveRequestPdf(id),
                    "LCF" => await PrintLeaveCancellPdf(id),
                    "DCS" => await PrintDocumentCirculatePdf(id),
                    _ => Result<FormPdfDto>.Failure(400, _localization.ReturnMsg($"{_this}PrintNotSupport"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<FormPdfDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 单个表单失败会跳过，不影响其余表单；全部失败才返回错误
        /// </summary>
        public async Task<Result<FormPdfDto>> PrintFormPdfBatch(List<string> formIds)
        {
            if (formIds is not { Count: > 0 })
            {
                return Result<FormPdfDto>.Failure(400, _localization.ReturnMsg($"{_this}BatchPrintNoFormIds"));
            }

            var zipStream = new MemoryStream();
            try
            {
                var successCount = 0;
                var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    foreach (var formId in formIds)
                    {
                        var result = await PrintFormPdf(formId);
                        if (result.Code != 200 || result.Data is null)
                        {
                            continue;
                        }

                        using var pdfStream = result.Data.FileStream;
                        var entryName = GetUniqueZipEntryName(usedNames, result.Data.FileName);
                        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                        using var entryStream = entry.Open();
                        await pdfStream.CopyToAsync(entryStream);
                        successCount++;
                    }
                }

                if (successCount == 0)
                {
                    await zipStream.DisposeAsync();
                    return Result<FormPdfDto>.Failure(400, _localization.ReturnMsg($"{_this}BatchPrintAllFailed"));
                }

                zipStream.Position = 0;
                var pdf = new FormPdfDto
                {
                    FileName = $"{Msg("PdfBatchPrintFileName")}_{DateTime.Now:yyyyMMddHHmmss}.zip",
                    FileStream = zipStream
                };
                return Result<FormPdfDto>.Ok(pdf);
            }
            catch (Exception ex)
            {
                await zipStream.DisposeAsync();
                _logger.LogError(ex, ex.Message);
                return Result<FormPdfDto>.Failure(500, ex.Message);
            }
        }

        private static string GetUniqueZipEntryName(HashSet<string> usedNames, string fileName)
        {
            var name = fileName;
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var index = 1;
            while (!usedNames.Add(name))
            {
                name = $"{baseName}({index++}){extension}";
            }
            return name;
        }

        #region 请假单PDF

        private async Task<Result<FormPdfDto>> PrintLeaveRequestPdf(long formId)
        {
            var isCan = await _formChecker.CanView(formId, "View");
            if (!isCan)
            {
                return Result<FormPdfDto>.Failure(400, _localization.ReturnMsg($"{_forms}NotCanView"));
            }

            var form = await _leaveRequestRepo.GetLeaveRequest(formId);
            form.Attachment = await _formmanger.GetAttachmentList(formId);
            form.ReviewRecord = await _formmanger.GetReviewRecordList(formId);
            form.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(formId, _loginuser.UserId);

            var leaveTypeDics = await _leaveRequestRepo.GetLeaveTypeDictionary();
            var leaveTypeDic = leaveTypeDics.FirstOrDefault(dic => dic.DicCode == form.LeaveType);
            var leaveTypeName = _lang.Locale == "zh-CN" ? leaveTypeDic?.DicNameCn : leaveTypeDic?.DicNameEn;

            var pdf = new FormPdfDto
            {
                FileName = $"{Msg("PdfLeaveRequestTitle")}_{form.FormNo}.pdf",
                FileStream = BuildLeaveRequestPdf(form, leaveTypeName)
            };
            return Result<FormPdfDto>.Ok(pdf);
        }

        private MemoryStream BuildLeaveRequestPdf(Model.FormBusiness.Forms.LeaveRequest.Dto.LeaveRequestDto form, string? leaveTypeName)
        {
            var show = BuildFieldVisibility(form.StepFieldPermission);

            var leavePeriod = form.StartDateTime.HasValue && form.EndDateTime.HasValue
                ? $"{form.StartDateTime:yyyy-MM-dd  HH:mm}  ~  {form.EndDateTime:yyyy-MM-dd  HH:mm}"
                : string.Empty;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurePage(page);
                    ComposeApprovalStamp(page, form.FormStatus, form.ReviewRecord);

                    page.Content().Column(column =>
                    {
                        ComposeTitle(column, Msg("PdfLeaveRequestTitle"));

                        var row1 = new List<PdfField>();
                        if (show("FormNo")) row1.Add(new PdfField(Msg("PdfFormNo"), form.FormNo, Width: FirstValueCellWidth));
                        if (show("ApplyDate")) row1.Add(new PdfField(Msg("PdfApplicantDate"), form.ApplicantDate.ToString("yyyy-MM-dd")));
                        ComposeFieldRow(column, row1);

                        var row2 = new List<PdfField>();
                        if (show("UserNo")) row2.Add(new PdfField(Msg("PdfUserNo"), form.ApplicantUserNo));
                        if (show("UserName")) row2.Add(new PdfField(Msg("PdfUserName"), form.ApplicantUserName));
                        if (show("Department")) row2.Add(new PdfField(Msg("PdfDepartment"), form.ApplicantDeptName));
                        ComposeFieldRow(column, row2);

                        var row3 = new List<PdfField>();
                        if (show("LeaveType")) row3.Add(new PdfField(Msg("PdfLeaveType"), leaveTypeName ?? string.Empty, Width: FirstValueCellWidth));
                        if (show("SelectAgent")) row3.Add(new PdfField(Msg("PdfAgentUser"), form.AgentUserName ?? string.Empty));
                        ComposeFieldRow(column, row3);

                        var row4 = new List<PdfField>();
                        if (show("LeavePeriod")) row4.Add(new PdfField(Msg("PdfLeavePeriod"), leavePeriod, Weight: 3f));
                        if (show("LeaveHours")) row4.Add(new PdfField(Msg("PdfLeaveHours"), (form.LeaveHours ?? 0).ToString("0.00"), Emphasized: true));
                        ComposeFieldRow(column, row4);

                        if (show("LeaveReason"))
                        {
                            ComposeFieldRow(column, new List<PdfField>
                            {
                                new PdfField(Msg("PdfLeaveReason"), form.LeaveReason ?? string.Empty, MinHeight: 44f)
                            });
                        }

                        if (show("Upload"))
                        {
                            ComposeAttachmentTable(column, form.Attachment);
                        }

                        ComposeReviewRecordTable(column, form.ReviewRecord);
                    });
                });
            });

            return GeneratePdfStream(document);
        }

        #endregion

        #region 销假单PDF

        private async Task<Result<FormPdfDto>> PrintLeaveCancellPdf(long formId)
        {
            var isCan = await _formChecker.CanView(formId, "View");
            if (!isCan)
            {
                return Result<FormPdfDto>.Failure(400, _localization.ReturnMsg($"{_forms}NotCanView"));
            }

            var form = await _leaveCancellRepo.GetLeaveCancell(formId);
            form.ReviewRecord = await _formmanger.GetReviewRecordList(formId);
            form.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(formId, _loginuser.UserId);

            var leaveRequest = form.LeaveRequestId.HasValue
                ? await _leaveCancellRepo.GetLeaveRequestDetail(form.LeaveRequestId.Value)
                : null;

            var pdf = new FormPdfDto
            {
                FileName = $"{Msg("PdfLeaveCancellTitle")}_{form.FormNo}.pdf",
                FileStream = BuildLeaveCancellPdf(form, leaveRequest)
            };
            return Result<FormPdfDto>.Ok(pdf);
        }

        private MemoryStream BuildLeaveCancellPdf(LeaveCancellDto form, LeaveRequestDetailDto? leaveRequest)
        {
            var show = BuildFieldVisibility(form.StepFieldPermission);

            var cancellPeriod = form.StartDateTime.HasValue && form.EndDateTime.HasValue
                ? $"{form.StartDateTime:yyyy-MM-dd  HH:mm}  ~  {form.EndDateTime:yyyy-MM-dd  HH:mm}"
                : string.Empty;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurePage(page);
                    ComposeApprovalStamp(page, form.FormStatus, form.ReviewRecord);

                    page.Content().Column(column =>
                    {
                        ComposeTitle(column, Msg("PdfLeaveCancellTitle"));

                        var row1 = new List<PdfField>();
                        if (show("FormNo")) row1.Add(new PdfField(Msg("PdfFormNo"), form.FormNo, Width: FirstValueCellWidth));
                        if (show("ApplyDate")) row1.Add(new PdfField(Msg("PdfApplicantDate"), form.ApplicantDate.ToString("yyyy-MM-dd")));
                        ComposeFieldRow(column, row1);

                        var row2 = new List<PdfField>();
                        if (show("UserNo")) row2.Add(new PdfField(Msg("PdfUserNo"), form.ApplicantUserNo));
                        if (show("UserName")) row2.Add(new PdfField(Msg("PdfUserName"), form.ApplicantUserName));
                        if (show("Department")) row2.Add(new PdfField(Msg("PdfDepartment"), form.ApplicantDeptName));
                        ComposeFieldRow(column, row2);

                        if (show("LeaveRequestTable"))
                        {
                            ComposeLeaveRequestTable(column, leaveRequest);
                        }

                        var row3 = new List<PdfField>();
                        if (show("TimePeriod")) row3.Add(new PdfField(Msg("PdfCancellPeriod"), cancellPeriod, Weight: 3f));
                        if (show("Hour")) row3.Add(new PdfField(Msg("PdfCancellHours"), (form.CancellHours ?? 0).ToString("0.00"), Emphasized: true, ValueColor: HighlightColor));
                        ComposeFieldRow(column, row3);

                        ComposeReviewRecordTable(column, form.ReviewRecord);
                    });
                });
            });

            return GeneratePdfStream(document);
        }

        private void ComposeLeaveRequestTable(ColumnDescriptor column, LeaveRequestDetailDto? leaveRequest)
        {
            column.Item().PaddingBottom(10).Row(row =>
            {
                row.ConstantItem(LabelCellWidth).Element(LabelCell).Text(Msg("PdfOriginalLeaveRequest")).FontColor(LabelTextColor);
                row.RelativeItem().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2f);
                        columns.RelativeColumn(1.6f);
                        columns.RelativeColumn(4f);
                        columns.RelativeColumn(1.4f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).AlignCenter().Text(Msg("PdfLeaveRequestNo")).FontColor(LabelTextColor);
                        header.Cell().Element(HeaderCell).AlignCenter().Text(Msg("PdfLeaveType")).FontColor(LabelTextColor);
                        header.Cell().Element(HeaderCell).AlignCenter().Text(Msg("PdfLeavePeriod")).FontColor(LabelTextColor);
                        header.Cell().Element(HeaderCell).AlignCenter().Text(Msg("PdfLeaveRequestHours")).FontColor(LabelTextColor);
                    });

                    if (leaveRequest == null)
                    {
                        table.Cell().ColumnSpan(4).Element(BodyCell).AlignCenter().Text(Msg("PdfNoData")).FontColor(MutedTextColor);
                        return;
                    }

                    var leavePeriod = leaveRequest.StartDateTime.HasValue && leaveRequest.EndDateTime.HasValue
                        ? $"{leaveRequest.StartDateTime:yyyy-MM-dd  HH:mm}  ~  {leaveRequest.EndDateTime:yyyy-MM-dd  HH:mm}"
                        : string.Empty;

                    table.Cell().Element(BodyCell).AlignCenter().Text(leaveRequest.LeaveRequestNo);
                    table.Cell().Element(BodyCell).AlignCenter().Text(leaveRequest.LeaveTypeName ?? string.Empty);
                    table.Cell().Element(BodyCell).AlignCenter().Text(leavePeriod);
                    table.Cell().Element(BodyCell).AlignCenter().Text((leaveRequest.LeaveHours ?? 0).ToString("0.00")).FontColor(EmphasizedColor).SemiBold();
                });
            });
        }

        #endregion

        #region 传签单PDF

        private async Task<Result<FormPdfDto>> PrintDocumentCirculatePdf(long formId)
        {
            var isCan = await _formChecker.CanView(formId, "View");
            if (!isCan)
            {
                return Result<FormPdfDto>.Failure(400, _localization.ReturnMsg($"{_forms}NotCanView"));
            }

            var form = await _documentCirculateRepo.GetDocumentCirculate(formId);
            form.Attachment = await _formmanger.GetAttachmentList(formId);
            form.AddReview = await _formmanger.GetAddReviewList(formId);
            form.ReviewRecord = await _formmanger.GetReviewRecordList(formId);
            form.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(formId, _loginuser.UserId);

            var pdf = new FormPdfDto
            {
                FileName = $"{Msg("PdfDocumentCirculateTitle")}_{form.FormNo}.pdf",
                FileStream = BuildDocumentCirculatePdf(form)
            };
            return Result<FormPdfDto>.Ok(pdf);
        }

        private MemoryStream BuildDocumentCirculatePdf(DocumentCirculateDto form)
        {
            var show = BuildFieldVisibility(form.StepFieldPermission);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurePage(page);
                    ComposeApprovalStamp(page, form.FormStatus, form.ReviewRecord);

                    page.Content().Column(column =>
                    {
                        ComposeTitle(column, Msg("PdfDocumentCirculateTitle"));

                        var row1 = new List<PdfField>();
                        if (show("FormNo")) row1.Add(new PdfField(Msg("PdfFormNo"), form.FormNo, Width: FirstValueCellWidth));
                        if (show("ApplyDate")) row1.Add(new PdfField(Msg("PdfApplicantDate"), form.ApplicantDate.ToString("yyyy-MM-dd")));
                        ComposeFieldRow(column, row1);

                        var row2 = new List<PdfField>();
                        if (show("UserNo")) row2.Add(new PdfField(Msg("PdfUserNo"), form.ApplicantUserNo));
                        if (show("UserName")) row2.Add(new PdfField(Msg("PdfUserName"), form.ApplicantUserName));
                        if (show("Department")) row2.Add(new PdfField(Msg("PdfDepartment"), form.ApplicantDeptName));
                        ComposeFieldRow(column, row2);

                        if (show("IssueDept"))
                        {
                            ComposeFieldRow(column, new List<PdfField>
                            {
                                new PdfField(Msg("PdfIssueDept"), form.IssueDept ?? string.Empty)
                            });
                        }

                        if (show("CirculationPurpose"))
                        {
                            ComposeFieldRow(column, new List<PdfField>
                            {
                                new PdfField(Msg("PdfCirculationPurpose"), form.CirculationPurpose ?? string.Empty, MinHeight: 44f)
                            });
                        }

                        // 内容摘要为富文本编辑器产生的HTML
                        if (show("ContentSummary"))
                        {
                            ComposeHtmlFieldRow(column, Msg("PdfContentSummary"), form.ContentSummary, 66f);
                        }

                        if (show("Upload"))
                        {
                            ComposeAttachmentTable(column, form.Attachment);
                        }

                        // AddReivew 为表单栏位配置中的原始拼写
                        if (show("AddReivew"))
                        {
                            ComposeAddReviewTable(column, form.AddReview);
                        }

                        ComposeReviewRecordTable(column, form.ReviewRecord);
                    });
                });
            });

            return GeneratePdfStream(document);
        }

        private void ComposeAddReviewTable(ColumnDescriptor column, List<FormAddReviewDto> addReviews)
        {
            column.Item().PaddingBottom(10).Row(row =>
            {
                row.ConstantItem(LabelCellWidth).Element(LabelCell).Text(Msg("PdfAddReview")).FontColor(LabelTextColor);
                row.RelativeItem().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);
                        columns.RelativeColumn(4f);
                        columns.RelativeColumn(2f);
                        columns.RelativeColumn(2.4f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).AlignCenter().Text(Msg("PdfAddReviewOrder")).FontColor(LabelTextColor);
                        header.Cell().Element(HeaderCell).Text(Msg("PdfAddReviewDept")).FontColor(LabelTextColor);
                        header.Cell().Element(HeaderCell).Text(Msg("PdfUserNo")).FontColor(LabelTextColor);
                        header.Cell().Element(HeaderCell).Text(Msg("PdfUserName")).FontColor(LabelTextColor);
                    });

                    if (addReviews.Count == 0)
                    {
                        table.Cell().ColumnSpan(4).Element(BodyCell).AlignCenter().Text(Msg("PdfNoData")).FontColor(MutedTextColor);
                        return;
                    }

                    foreach (var addReview in addReviews)
                    {
                        table.Cell().Element(BodyCell).AlignCenter().Text(addReview.SortOrder.ToString());
                        table.Cell().Element(BodyCell).Text(addReview.DeptName);
                        table.Cell().Element(BodyCell).Text(addReview.UserNo);
                        table.Cell().Element(BodyCell).Text(addReview.UserName);
                    }
                });
            });
        }

        #endregion

        #region PDF通用组件（多表单共用：样式、页面、标题、栏位行、表格、印章）

        private const string FontFamilyName = "Microsoft YaHei";
        private const string BorderColor = "#DCDFE6";
        private const string LabelBgColor = "#F5F7FA";
        private const string LabelTextColor = "#606266";
        private const string BodyTextColor = "#303133";
        private const string MutedTextColor = "#909399";
        private const string EmphasizedColor = "#F56C6C";
        private const string HighlightColor = "#409EFF";
        private const string StampColor = "#1F9254";

        // 标签格宽度 / 每行第一个值格的固定宽度（保证各行第二个栏位起始位置对齐）
        private const float LabelCellWidth = 66f;
        private const float FirstValueCellWidth = 136f;

        // Width 大于 0 时值格固定宽度（用于跨行对齐），否则按 Weight 分配剩余宽度
        private sealed record PdfField(string Label, string Value, float Weight = 1f, bool Emphasized = false, float MinHeight = 0f, float Width = 0f, string? ValueColor = null);

        private string Msg(string key)
        {
            return _localization.ReturnMsg($"{_forms}{key}");
        }

        private static void ConfigurePage(PageDescriptor page)
        {
            page.Size(PageSizes.A4);
            page.Margin(28);
            page.DefaultTextStyle(style => style.FontSize(9).FontFamily(FontFamilyName).FontColor(BodyTextColor));
        }

        private static void ComposeTitle(ColumnDescriptor column, string title)
        {
            column.Item().AlignCenter().Text(title).FontSize(14).SemiBold();
            column.Item().PaddingTop(10).LineHorizontal(0.75f).LineColor(BorderColor);
            column.Item().PaddingTop(14);
        }

        // 未配置的栏位默认显示
        private static Func<string, bool> BuildFieldVisibility(List<StepFieldPermissionDto> permissions)
        {
            var fieldVisible = permissions
                .GroupBy(permission => permission.FieldKey)
                .ToDictionary(group => group.Key, group => group.Any(permission => permission.IsVisible == 1));

            return fieldKey => !fieldVisible.TryGetValue(fieldKey, out var isVisible) || isVisible;
        }

        private static void ComposeFieldRow(ColumnDescriptor column, List<PdfField> fields)
        {
            if (fields.Count == 0)
            {
                return;
            }

            column.Item().PaddingBottom(10).Row(row =>
            {
                foreach (var field in fields)
                {
                    row.ConstantItem(LabelCellWidth).Element(LabelCell).Text(field.Label).FontColor(LabelTextColor);

                    // 指定宽度的值格用于跨行对齐（如表单编号、请假类型），其余按权重分配
                    var valueItem = field.Width > 0
                        ? row.ConstantItem(field.Width)
                        : row.RelativeItem(field.Weight);

                    var text = valueItem.Element(container => ValueCell(container, field.MinHeight))
                                        .Text(field.Value);
                    if (field.Emphasized)
                    {
                        text.FontColor(field.ValueColor ?? EmphasizedColor).SemiBold();
                    }
                }
            });
        }

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
                        table.Cell().Element(BodyCell).AlignCenter().Text($"{attachment.AttachmentSize} KB");
                    }
                });
            });
        }

        private void ComposeReviewRecordTable(ColumnDescriptor column, List<FormReviewRecordDto> reviewRecords)
        {
            column.Item().PaddingTop(6).Text(Msg("PdfReviewRecord")).FontSize(11).SemiBold();

            column.Item().PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);
                    columns.RelativeColumn(2f);
                    columns.RelativeColumn(2f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(4f);
                    columns.RelativeColumn(2.4f);
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

                    // 审批人：非本人审批（兼、代、自动指派）时在姓名右侧标注审批身份
                    table.Cell().Element(BodyCell).Row(user =>
                    {
                        user.RelativeItem().Text(record.OperationUserName);
                        if (record.AppointmentType != AppointmentType.Actual.ToEnumString() && !string.IsNullOrWhiteSpace(record.AppointmentTypeName))
                        {
                            user.AutoItem().PaddingLeft(4).Text(record.AppointmentTypeName).FontSize(7f).FontColor(MutedTextColor);
                        }
                    });

                    // 审批结果：驳回时在结果下方补充驳回至的步骤
                    table.Cell().Element(BodyCell).Column(result =>
                    {
                        result.Item().Text(record.ReviewResultName);
                        if (record.ReviewResult == ReviewResult.Reject.ToEnumString() && !string.IsNullOrWhiteSpace(record.RejectStepName))
                        {
                            result.Item().Text($"({record.RejectStepName})").FontSize(7.5f).FontColor(MutedTextColor);
                        }
                    });
                    table.Cell().Element(BodyCell).Text(record.Comment);
                    table.Cell().Element(BodyCell).Text(record.ReviewDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                }
            });
        }

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
                .AlignBottom()
                .AlignRight()
                .PaddingBottom(24)
                .PaddingRight(18)
                .Width(118)
                .Height(118)
                .Svg(svg);
        }

        private static string BuildApprovalStampSvg(string mainText, string subText, string dateText)
        {
            static string Escape(string value) => value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

            // 中文短文字用大号字，英文长文字缩小并加字距
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

        private static MemoryStream GeneratePdfStream(Document document)
        {
            var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0;    // 必须归零，否则 File() 读到 0 字节
            return stream;
        }

        #region 单元格样式

        private static IContainer LabelCell(IContainer container)
        {
            return container.Border(0.5f)
                            .BorderColor(BorderColor)
                            .Background(LabelBgColor)
                            .PaddingVertical(5)
                            .PaddingHorizontal(6)
                            .AlignMiddle();
        }

        // minHeight 大于 0 时为多行文本区域
        private static IContainer ValueCell(IContainer container, float minHeight = 0f)
        {
            container = container.Border(0.5f)
                                 .BorderColor(BorderColor)
                                 .PaddingVertical(5)
                                 .PaddingHorizontal(6);
            return minHeight > 0 ? container.MinHeight(minHeight) : container.AlignMiddle();
        }

        private static IContainer HeaderCell(IContainer container)
        {
            return container.Border(0.5f)
                            .BorderColor(BorderColor)
                            .Background(LabelBgColor)
                            .PaddingVertical(5)
                            .PaddingHorizontal(6)
                            .AlignMiddle();
        }

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
