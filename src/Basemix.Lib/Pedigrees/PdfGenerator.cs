using System.Reflection;
using Basemix.Lib.Rats;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Basemix.Lib.Pedigrees;

public class PdfGenerator
{
    public static void RegisterFonts()
    {
        var assembly = Assembly.GetAssembly(typeof(PdfGenerator));
        var fonts = assembly?.GetManifestResourceNames().Where(n => n.EndsWith(".ttf")) ?? Enumerable.Empty<string>();
        foreach (var font in fonts)
        {
            using var fontStream = assembly?.GetManifestResourceStream(font);
            if (fontStream != null)
            {
                FontManager.RegisterFont(fontStream);
            }
        }
    }
    
    public Document CreateFromPedigree(Node root, Sex? ratSex, DateOnly? dateOfBirth, string? ratteryName, string? litterName, string? footerText, bool showSex)
    {
        var nullNode = new Node {Name = "-", Variety = string.Empty};
        var s = root.Sire ?? nullNode;
        var ss = root.Sire?.Sire ?? nullNode;
        var sss = root.Sire?.Sire?.Sire ?? nullNode;
        var ssss = root.Sire?.Sire?.Sire?.Sire ?? nullNode;
        var sssd = root.Sire?.Sire?.Sire?.Dam ?? nullNode;
        var ssd = root.Sire?.Sire?.Dam ?? nullNode;
        var ssds = root.Sire?.Sire?.Dam?.Sire ?? nullNode;
        var ssdd = root.Sire?.Sire?.Dam?.Dam ?? nullNode;
        var sd = root.Sire?.Dam ?? nullNode;
        var sds = root.Sire?.Dam?.Sire ?? nullNode;
        var sdss = root.Sire?.Dam?.Sire?.Sire ?? nullNode;
        var sdsd = root.Sire?.Dam?.Sire?.Dam ?? nullNode;
        var sdd = root.Sire?.Dam?.Dam ?? nullNode;
        var sdds = root.Sire?.Dam?.Dam?.Sire ?? nullNode;
        var sddd = root.Sire?.Dam?.Dam?.Dam ?? nullNode;

        var d = root.Dam ?? nullNode;
        var ds = root.Dam?.Sire ?? nullNode;
        var dss = root.Dam?.Sire?.Sire ?? nullNode;
        var dsss = root.Dam?.Sire?.Sire?.Sire ?? nullNode;
        var dssd = root.Dam?.Sire?.Sire?.Dam ?? nullNode;
        var dsd = root.Dam?.Sire?.Dam ?? nullNode;
        var dsds = root.Dam?.Sire?.Dam?.Sire ?? nullNode;
        var dsdd = root.Dam?.Sire?.Dam?.Dam ?? nullNode;
        var dd = root.Dam?.Dam ?? nullNode;
        var dds = root.Dam?.Dam?.Sire ?? nullNode;
        var ddss = root.Dam?.Dam?.Sire?.Sire ?? nullNode;
        var ddsd = root.Dam?.Dam?.Sire?.Dam ?? nullNode;
        var ddd = root.Dam?.Dam?.Dam ?? nullNode;
        var ddds = root.Dam?.Dam?.Dam?.Sire ?? nullNode;
        var dddd = root.Dam?.Dam?.Dam?.Dam ?? nullNode;

        return Document.Create(container => // TODO blank out root name
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                    });

                    var fontName = "Carlito";

                    var cellFont = 10;
                    table.Cell().ColumnSpan(5).Element(RatteryHeader).Text(ratteryName).FontFamily(fontName)
                        .FontSize(36).Bold();
                    table.Cell().ColumnSpan(5).Element(LitterHeader)
                        .Text(litterName).FontFamily(fontName).FontSize(26);

                    table.Cell().RowSpan(16).Element(TopBlock).Text(root.Name).FontFamily(fontName)
                        .FontSize(cellFont).Bold();

                    table.Cell().RowSpan(8).Element(TopBlock).Text(s.Name).FontFamily(fontName)
                        .FontSize(cellFont).Bold();
                    table.Cell().RowSpan(4).Element(TopBlock).Text(ss.Name).FontFamily(fontName)
                        .FontSize(cellFont).Bold();
                    table.Cell().RowSpan(2).Element(TopBlock).Text(sss.Name).FontFamily(fontName)
                        .FontSize(cellFont).Bold();
                    table.Cell().Element(TopBlock).Text(ssss.Name).FontFamily(fontName).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(BuckVariety(ssss, showSex)).FontFamily(fontName).FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(BottomBlock).Text(BuckVariety(sss, showSex)).FontFamily(fontName)
                        .FontSize(cellFont);

                    table.Cell().Element(TopBlock).Text(sssd.Name).FontFamily(fontName).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(DoeVariety(sssd, showSex)).FontFamily(fontName).FontSize(cellFont);

                    table.Cell().RowSpan(4).Element(BottomBlock).Text(BuckVariety(ss, showSex)).FontFamily(fontName)
                        .FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(TopBlock).Text(ssd.Name).FontFamily(fontName)
                        .FontSize(cellFont).Bold();
                    table.Cell().Element(TopBlock).Text(ssds.Name).FontFamily(fontName).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(BuckVariety(ssds, showSex)).FontFamily(fontName).FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(BottomBlock).Text(DoeVariety(ssd, showSex)).FontFamily(fontName)
                        .FontSize(cellFont);

                    table.Cell().Element(TopBlock).Text(ssdd.Name).FontFamily(fontName).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(DoeVariety(ssdd, showSex)).FontFamily(fontName).FontSize(cellFont);

                    table.Cell().RowSpan(8).Element(BottomBlock).Text(BuckVariety(s, showSex)).FontFamily(fontName)
                        .FontSize(cellFont);

                    table.Cell().RowSpan(4).Element(TopBlock).Text(sd.Name).FontFamily(fontName)
                        .FontSize(cellFont).Bold();
                    table.Cell().RowSpan(2).Element(TopBlock).Text(sds.Name).FontFamily(fontName)
                        .FontSize(cellFont).Bold();
                    table.Cell().Element(TopBlock).Text(sdss.Name).FontFamily(fontName).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(BuckVariety(sdss, showSex)).FontFamily(fontName).FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(BottomBlock).Text(BuckVariety(sds, showSex)).FontFamily(fontName)
                        .FontSize(cellFont);

                    table.Cell().Element(TopBlock).Text(sdsd.Name).FontFamily(fontName).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(DoeVariety(sdsd, showSex)).FontFamily(fontName).FontSize(cellFont);

                    table.Cell().RowSpan(4).Element(BottomBlock).Text(DoeVariety(sd, showSex)).FontFamily(fontName)
                        .FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(TopBlock).Text(sdd.Name).FontFamily(fontName)
                        .FontSize(cellFont).Bold();
                    table.Cell().Element(TopBlock).Text(sdds.Name).FontFamily(fontName).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(BuckVariety(sdds, showSex)).FontFamily(fontName).FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(BottomBlock).Text(DoeVariety(sdd, showSex)).FontFamily(fontName)
                        .FontSize(cellFont);

                    table.Cell().Element(TopBlock).Text(sddd.Name).FontFamily(fontName).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(DoeVariety(sddd, showSex)).FontFamily(fontName).FontSize(cellFont);

                    table.Cell().RowSpan(16).Element(BottomBlock)
                        .Text(ratSex switch
                        {
                            Sex.Buck => BuckVariety(root, showSex),
                            Sex.Doe => DoeVariety(root, showSex),
                            _ => root.Variety
                        })
                        .FontFamily(fontName)
                        .FontSize(cellFont);

                    table.Cell().RowSpan(8).Element(TopBlock).Text(d.Name).FontFamily(fontName)
                        .FontSize(cellFont).Bold();
                    table.Cell().RowSpan(4).Element(TopBlock).Text(ds.Name).FontFamily(fontName)
                        .FontSize(cellFont).Bold();
                    table.Cell().RowSpan(2).Element(TopBlock).Text(dss.Name).FontFamily(fontName)
                        .FontSize(cellFont).Bold();
                    table.Cell().Element(TopBlock).Text(dsss.Name).FontFamily(fontName).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(BuckVariety(dsss, showSex)).FontFamily(fontName).FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(BottomBlock).Text(BuckVariety(dss, showSex)).FontFamily(fontName)
                        .FontSize(cellFont);

                    table.Cell().Element(TopBlock).Text(dssd.Name).FontFamily(fontName).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(DoeVariety(dssd, showSex)).FontFamily(fontName).FontSize(cellFont);

                    table.Cell().RowSpan(4).Element(BottomBlock).Text(BuckVariety(ds, showSex)).FontFamily(fontName)
                        .FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(TopBlock).Text(dsd.Name).FontFamily(fontName)
                        .FontSize(cellFont).Bold();
                    table.Cell().Element(TopBlock).Text(dsds.Name).FontFamily(fontName).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(BuckVariety(dsds, showSex)).FontFamily(fontName).FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(BottomBlock).Text(DoeVariety(dsd, showSex)).FontFamily(fontName)
                        .FontSize(cellFont);

                    table.Cell().Element(TopBlock).Text(dsdd.Name).FontFamily(fontName).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(DoeVariety(dsdd, showSex)).FontFamily(fontName).FontSize(cellFont);

                    table.Cell().RowSpan(8).Element(BottomBlock).Text(DoeVariety(d, showSex)).FontFamily(fontName)
                        .FontSize(cellFont);

                    table.Cell().RowSpan(4).Element(TopBlock).Text(dd.Name).FontFamily(fontName)
                        .FontSize(cellFont).Bold();
                    table.Cell().RowSpan(2).Element(TopBlock).Text(dds.Name).FontFamily(fontName)
                        .FontSize(cellFont).Bold();
                    table.Cell().Element(TopBlock).Text(ddss.Name).FontFamily(fontName).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(BuckVariety(ddss, showSex)).FontFamily(fontName).FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(BottomBlock).Text(BuckVariety(dds, showSex)).FontFamily(fontName)
                        .FontSize(cellFont);

                    table.Cell().Element(TopBlock).Text(ddsd.Name).FontFamily(fontName).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(DoeVariety(ddsd, showSex)).FontFamily(fontName).FontSize(cellFont);

                    table.Cell().RowSpan(4).Element(BottomBlock).Text(DoeVariety(dd, showSex)).FontFamily(fontName)
                        .FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(TopBlock).Text(ddd.Name).FontFamily(fontName)
                        .FontSize(cellFont).Bold();
                    table.Cell().Element(TopBlock).Text(ddds.Name).FontFamily(fontName).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(BuckVariety(ddds, showSex)).FontFamily(fontName).FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(BottomBlock).Text(DoeVariety(ddd, showSex)).FontFamily(fontName)
                        .FontSize(cellFont);

                    table.Cell().Element(TopBlock).Text(dddd.Name).FontFamily(fontName).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(DoeVariety(dddd, showSex)).FontFamily(fontName).FontSize(cellFont);

                    table.Cell().ColumnSpan(5).Element(Footer).Text($"Date of birth: {dateOfBirth?.ToString("dd/MM/yyyy.")} {footerText}");
                });
            });
        });
    }

    public void WriteToStream(Document document, Stream stream)
    {
        document.GeneratePdf(stream);
    }
    
    private static string BuckVariety(Node node, bool showSex) => string.IsNullOrEmpty(node.Variety) 
        ? SexText(showSex, "(Buck)")
        : $"{SexText(showSex, "(Buck) ")}{node.Variety}";
    private static string DoeVariety(Node node, bool showSex) => string.IsNullOrEmpty(node.Variety)
        ? SexText(showSex, "(Doe)")
        : $"{SexText(showSex, "(Doe) ")}{node.Variety}";

    private static string SexText(bool showSex, string sex) =>
        showSex ? sex : string.Empty;

    private static IContainer RatteryHeader(IContainer container) =>
        container
            .BorderLeft(1).BorderTop(1).BorderRight(1)
            .Background(Colors.Grey.Lighten4)
            .ShowOnce()
            .AlignCenter()
            .AlignMiddle();

    private static IContainer LitterHeader(IContainer container) =>
        container
            .BorderLeft(1).BorderBottom(1).BorderRight(1)
            .Background(Colors.Grey.Lighten4)
            .ShowOnce()
            .AlignCenter()
            .AlignMiddle();

    private static IContainer TopBlock(IContainer container) =>
        container
            .BorderLeft(1).BorderTop(1).BorderRight(1)
            .Background(Colors.White)
            .ShowOnce()
            .AlignCenter()
            .AlignBottom();

    private static IContainer BottomBlock(IContainer container) =>
        container
            .BorderLeft(1).BorderBottom(1).BorderRight(1)
            .Background(Colors.White)
            .ShowOnce()
            .AlignCenter()
            .AlignTop();

    private static IContainer Footer(IContainer container) =>
        container
            .Border(1)
            .Background(Colors.Grey.Lighten4)
            .ShowOnce()
            .AlignLeft()
            .AlignMiddle().PaddingLeft(5).PaddingRight(5);
}