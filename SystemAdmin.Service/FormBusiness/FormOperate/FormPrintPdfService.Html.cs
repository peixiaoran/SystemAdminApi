using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using QuestPDF.Fluent;

namespace SystemAdmin.Service.FormBusiness.FormOperate
{
    /// <summary>
    /// 富文本栏位（TipTap HTML）转PDF版面
    /// 支持前端编辑器启用的格式：粗斜体/下划线/删除线、字体颜色、高亮、标题、对齐、列表、引用、代码块、分隔线、表格、超链接
    /// </summary>
    public partial class FormPrintPdfService
    {
        private const string LinkColor = "#409EFF";
        private const string CodeFontFamily = "Consolas";

        /// <summary>
        /// HTML渲染样式
        /// </summary>
        private sealed record HtmlStyle(bool Bold = false, bool Italic = false, bool Underline = false, bool Strikethrough = false, string? Color = null, string? Background = null, float? FontSize = null, string? Align = null);

        /// <summary>
        /// 组装富文本栏位行
        /// </summary>
        private static void ComposeHtmlFieldRow(ColumnDescriptor column, string label, string? html, float minHeight)
        {
            column.Item().PaddingBottom(10).Row(row =>
            {
                row.ConstantItem(LabelCellWidth).Element(LabelCell).Text(label).FontColor(LabelTextColor);
                row.RelativeItem()
                   .Element(container => ValueCell(container, minHeight))
                   .Column(content => ComposeHtmlContent(content, html));
            });
        }

        /// <summary>
        /// 渲染HTML内容
        /// </summary>
        private static void ComposeHtmlContent(ColumnDescriptor column, string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                column.Item().Text(string.Empty);
                return;
            }

            var document = new HtmlDocument();
            document.LoadHtml(html);
            ComposeHtmlNodes(column, document.DocumentNode.ChildNodes, new HtmlStyle());
        }

        /// <summary>
        /// 渲染同级节点：块级节点各占一块，连续的内联节点合并为一个段落
        /// </summary>
        private static void ComposeHtmlNodes(ColumnDescriptor column, IEnumerable<HtmlNode> nodes, HtmlStyle style)
        {
            var inlineNodes = new List<HtmlNode>();

            void FlushInlineNodes()
            {
                if (inlineNodes.Count == 0)
                {
                    return;
                }

                var paragraph = inlineNodes.ToList();
                column.Item().Text(text =>
                {
                    AlignHtmlText(text, style.Align);
                    foreach (var node in paragraph)
                    {
                        ComposeHtmlInline(text, node, style);
                    }
                });
                inlineNodes.Clear();
            }

            foreach (var node in nodes)
            {
                if (node.NodeType == HtmlNodeType.Comment)
                {
                    continue;
                }

                if (node.NodeType == HtmlNodeType.Text)
                {
                    if (!string.IsNullOrWhiteSpace(node.InnerText))
                    {
                        inlineNodes.Add(node);
                    }
                    continue;
                }

                switch (node.Name.ToLowerInvariant())
                {
                    case "p":
                    case "div":
                        FlushInlineNodes();
                        ComposeHtmlNodes(column, node.ChildNodes, MergeHtmlStyle(node, style));
                        break;

                    case "h1":
                    case "h2":
                    case "h3":
                    case "h4":
                    case "h5":
                    case "h6":
                        FlushInlineNodes();
                        var headingStyle = MergeHtmlStyle(node, style) with { Bold = true, FontSize = 13f - (node.Name[1] - '1') * 0.7f };
                        ComposeHtmlNodes(column, node.ChildNodes, headingStyle);
                        break;

                    case "ul":
                    case "ol":
                        FlushInlineNodes();
                        ComposeHtmlList(column, node, node.Name.Equals("ol", StringComparison.OrdinalIgnoreCase), style);
                        break;

                    case "table":
                        FlushInlineNodes();
                        ComposeHtmlTable(column, node, style);
                        break;

                    case "blockquote":
                        FlushInlineNodes();
                        column.Item().BorderLeft(2).BorderColor(BorderColor).PaddingLeft(6)
                              .Column(quote => ComposeHtmlNodes(quote, node.ChildNodes, style with { Color = MutedTextColor }));
                        break;

                    case "pre":
                        FlushInlineNodes();
                        column.Item().PaddingVertical(2).Background(LabelBgColor).Padding(6)
                              .Text(WebUtility.HtmlDecode(node.InnerText).TrimEnd()).FontFamily(CodeFontFamily).FontSize(8.5f);
                        break;

                    case "hr":
                        FlushInlineNodes();
                        column.Item().PaddingVertical(3).LineHorizontal(0.5f).LineColor(BorderColor);
                        break;

                    default:
                        inlineNodes.Add(node);
                        break;
                }
            }

            FlushInlineNodes();
        }

        /// <summary>
        /// 渲染内联节点（文字、样式标签、换行、超链接）
        /// </summary>
        private static void ComposeHtmlInline(TextDescriptor text, HtmlNode node, HtmlStyle style)
        {
            if (node.NodeType == HtmlNodeType.Text)
            {
                // HTML中的连续空白按一个空格处理
                var content = Regex.Replace(WebUtility.HtmlDecode(node.InnerText), @"\s+", " ");
                if (content.Length > 0)
                {
                    ApplyHtmlStyle(text.Span(content), style);
                }
                return;
            }

            if (node.NodeType != HtmlNodeType.Element)
            {
                return;
            }

            switch (node.Name.ToLowerInvariant())
            {
                case "br":
                    text.Span("\n");
                    return;

                case "a":
                    var url = node.GetAttributeValue("href", string.Empty);
                    var label = Regex.Replace(WebUtility.HtmlDecode(node.InnerText), @"\s+", " ");
                    if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(label))
                    {
                        break;
                    }
                    ApplyHtmlStyle(text.Hyperlink(label, url), style with { Color = LinkColor, Underline = true });
                    return;
            }

            var childStyle = MergeHtmlStyle(node, style);
            foreach (var child in node.ChildNodes)
            {
                ComposeHtmlInline(text, child, childStyle);
            }
        }

        /// <summary>
        /// 渲染列表（嵌套列表逐层缩进）
        /// </summary>
        private static void ComposeHtmlList(ColumnDescriptor column, HtmlNode node, bool isOrdered, HtmlStyle style)
        {
            var index = 1;

            foreach (var item in node.ChildNodes.Where(child => child.Name.Equals("li", StringComparison.OrdinalIgnoreCase)))
            {
                var marker = isOrdered ? $"{index++}." : "•";
                column.Item().PaddingLeft(8).Row(row =>
                {
                    row.ConstantItem(14).Text(marker).FontColor(style.Color ?? BodyTextColor);
                    row.RelativeItem().Column(content => ComposeHtmlNodes(content, item.ChildNodes, style));
                });
            }
        }

        /// <summary>
        /// 渲染表格（表头单元格加底色，支持跨行跨列）
        /// </summary>
        private static void ComposeHtmlTable(ColumnDescriptor column, HtmlNode node, HtmlStyle style)
        {
            var rows = node.SelectNodes("./tr|./thead/tr|./tbody/tr|./tfoot/tr");
            if (rows == null)
            {
                return;
            }

            var cellsOfRows = rows.Select(row => row.ChildNodes
                                                    .Where(cell => cell.Name.Equals("td", StringComparison.OrdinalIgnoreCase) || cell.Name.Equals("th", StringComparison.OrdinalIgnoreCase))
                                                    .ToList())
                                  .Where(cells => cells.Count > 0)
                                  .ToList();
            if (cellsOfRows.Count == 0)
            {
                return;
            }

            var columnCount = cellsOfRows.Max(cells => cells.Sum(cell => Math.Max(cell.GetAttributeValue("colspan", 1), 1)));

            column.Item().PaddingVertical(2).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    for (var i = 0; i < columnCount; i++)
                    {
                        columns.RelativeColumn();
                    }
                });

                foreach (var cell in cellsOfRows.SelectMany(cells => cells))
                {
                    var cellNode = cell;
                    var columnSpan = Math.Clamp(cell.GetAttributeValue("colspan", 1), 1, columnCount);
                    var rowSpan = Math.Max(cell.GetAttributeValue("rowspan", 1), 1);
                    var isHeader = cell.Name.Equals("th", StringComparison.OrdinalIgnoreCase);

                    table.Cell()
                         .ColumnSpan((uint)columnSpan)
                         .RowSpan((uint)rowSpan)
                         .Element(isHeader ? HeaderCell : BodyCell)
                         .Column(content => ComposeHtmlCell(content, cellNode, style with { Bold = style.Bold || isHeader }));
                }
            });
        }

        /// <summary>
        /// 渲染表格单元格（空单元格补一个空文本，避免行高塌陷）
        /// </summary>
        private static void ComposeHtmlCell(ColumnDescriptor column, HtmlNode cell, HtmlStyle style)
        {
            if (string.IsNullOrWhiteSpace(cell.InnerText))
            {
                column.Item().Text(string.Empty);
                return;
            }

            ComposeHtmlNodes(column, cell.ChildNodes, style);
        }

        /// <summary>
        /// 合并节点自身的样式（样式标签、字体颜色、高亮、对齐）到继承样式
        /// </summary>
        private static HtmlStyle MergeHtmlStyle(HtmlNode node, HtmlStyle style)
        {
            switch (node.Name.ToLowerInvariant())
            {
                case "strong":
                    style = style with { Bold = true };
                    break;
                case "em":
                    style = style with { Italic = true };
                    break;
                case "u":
                    style = style with { Underline = true };
                    break;
                case "s":
                    style = style with { Strikethrough = true };
                    break;
                case "code":
                    style = style with { Background = LabelBgColor };
                    break;
            }

            foreach (var declaration in node.GetAttributeValue("style", string.Empty)
                                            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var separatorIndex = declaration.IndexOf(':');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                var name = declaration[..separatorIndex].Trim().ToLowerInvariant();
                var value = declaration[(separatorIndex + 1)..].Trim().ToLowerInvariant();

                switch (name)
                {
                    case "color":
                        style = style with { Color = ParseHtmlColor(value) ?? style.Color };
                        break;
                    case "background-color":
                        style = style with { Background = ParseHtmlColor(value) ?? style.Background };
                        break;
                    case "text-align":
                        style = style with { Align = value };
                        break;
                }
            }

            return style;
        }

        /// <summary>
        /// 解析CSS颜色（rgb(r,g,b) 与 #rrggbb）
        /// </summary>
        private static string? ParseHtmlColor(string value)
        {
            if (value.StartsWith('#'))
            {
                return value.Length == 7 ? value : null;
            }

            var rgb = Regex.Match(value, @"^rgba?\(\s*(\d+)\D+(\d+)\D+(\d+)");
            if (!rgb.Success)
            {
                return null;
            }

            var red = Math.Clamp(int.Parse(rgb.Groups[1].Value), 0, 255);
            var green = Math.Clamp(int.Parse(rgb.Groups[2].Value), 0, 255);
            var blue = Math.Clamp(int.Parse(rgb.Groups[3].Value), 0, 255);
            return $"#{red:X2}{green:X2}{blue:X2}";
        }

        /// <summary>
        /// 应用段落对齐
        /// </summary>
        private static void AlignHtmlText(TextDescriptor text, string? align)
        {
            switch (align)
            {
                case "center":
                    text.AlignCenter();
                    break;
                case "right":
                    text.AlignRight();
                    break;
                case "justify":
                    text.Justify();
                    break;
            }
        }

        /// <summary>
        /// 应用内联文字样式
        /// </summary>
        private static void ApplyHtmlStyle(TextSpanDescriptor span, HtmlStyle style)
        {
            if (style.Bold)
            {
                span.SemiBold();
            }
            if (style.Italic)
            {
                span.Italic();
            }
            if (style.Underline)
            {
                span.Underline();
            }
            if (style.Strikethrough)
            {
                span.Strikethrough();
            }
            if (style.FontSize.HasValue)
            {
                span.FontSize(style.FontSize.Value);
            }
            if (!string.IsNullOrEmpty(style.Color))
            {
                span.FontColor(style.Color);
            }
            if (!string.IsNullOrEmpty(style.Background))
            {
                span.BackgroundColor(style.Background);
            }
        }
    }
}
