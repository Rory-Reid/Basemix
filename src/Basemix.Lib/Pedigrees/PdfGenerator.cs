using Basemix.Lib.Rats;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Basemix.Lib.Pedigrees;

public class PdfGenerator
{
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

                    var cellFont = 10;
                    table.Cell().ColumnSpan(5).Element(RatteryHeader).Text(ratteryName).FontFamily(Fonts.Arial)
                        .FontSize(36).Bold();
                    table.Cell().ColumnSpan(5).Element(LitterHeader)
                        .Text(litterName).FontFamily(Fonts.Arial).FontSize(26); // TODO better fonts for android

                    table.Cell().RowSpan(16).Element(TopBlock).Text(root.Name).FontFamily(Fonts.Arial)
                        .FontSize(cellFont).Bold();

                    table.Cell().RowSpan(8).Element(TopBlock).Text(s.Name).FontFamily(Fonts.Arial)
                        .FontSize(cellFont).Bold();
                    table.Cell().RowSpan(4).Element(TopBlock).Text(ss.Name).FontFamily(Fonts.Arial)
                        .FontSize(cellFont).Bold();
                    table.Cell().RowSpan(2).Element(TopBlock).Text(sss.Name).FontFamily(Fonts.Arial)
                        .FontSize(cellFont).Bold();
                    table.Cell().Element(TopBlock).Text(ssss.Name).FontFamily(Fonts.Arial).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(BuckVariety(ssss, showSex)).FontFamily(Fonts.Arial).FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(BottomBlock).Text(BuckVariety(sss, showSex)).FontFamily(Fonts.Arial)
                        .FontSize(cellFont);

                    table.Cell().Element(TopBlock).Text(sssd.Name).FontFamily(Fonts.Arial).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(DoeVariety(sssd, showSex)).FontFamily(Fonts.Arial).FontSize(cellFont);

                    table.Cell().RowSpan(4).Element(BottomBlock).Text(BuckVariety(ss, showSex)).FontFamily(Fonts.Arial)
                        .FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(TopBlock).Text(ssd.Name).FontFamily(Fonts.Arial)
                        .FontSize(cellFont).Bold();
                    table.Cell().Element(TopBlock).Text(ssds.Name).FontFamily(Fonts.Arial).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(BuckVariety(ssds, showSex)).FontFamily(Fonts.Arial).FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(BottomBlock).Text(DoeVariety(ssd, showSex)).FontFamily(Fonts.Arial)
                        .FontSize(cellFont);

                    table.Cell().Element(TopBlock).Text(ssdd.Name).FontFamily(Fonts.Arial).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(DoeVariety(ssdd, showSex)).FontFamily(Fonts.Arial).FontSize(cellFont);

                    table.Cell().RowSpan(8).Element(BottomBlock).Text(BuckVariety(s, showSex)).FontFamily(Fonts.Arial)
                        .FontSize(cellFont);

                    table.Cell().RowSpan(4).Element(TopBlock).Text(sd.Name).FontFamily(Fonts.Arial)
                        .FontSize(cellFont).Bold();
                    table.Cell().RowSpan(2).Element(TopBlock).Text(sds.Name).FontFamily(Fonts.Arial)
                        .FontSize(cellFont).Bold();
                    table.Cell().Element(TopBlock).Text(sdss.Name).FontFamily(Fonts.Arial).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(BuckVariety(sdss, showSex)).FontFamily(Fonts.Arial).FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(BottomBlock).Text(BuckVariety(sds, showSex)).FontFamily(Fonts.Arial)
                        .FontSize(cellFont);

                    table.Cell().Element(TopBlock).Text(sdsd.Name).FontFamily(Fonts.Arial).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(DoeVariety(sdsd, showSex)).FontFamily(Fonts.Arial).FontSize(cellFont);

                    table.Cell().RowSpan(4).Element(BottomBlock).Text(DoeVariety(sd, showSex)).FontFamily(Fonts.Arial)
                        .FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(TopBlock).Text(sdd.Name).FontFamily(Fonts.Arial)
                        .FontSize(cellFont).Bold();
                    table.Cell().Element(TopBlock).Text(sdds.Name).FontFamily(Fonts.Arial).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(BuckVariety(sdds, showSex)).FontFamily(Fonts.Arial).FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(BottomBlock).Text(DoeVariety(sdd, showSex)).FontFamily(Fonts.Arial)
                        .FontSize(cellFont);

                    table.Cell().Element(TopBlock).Text(sddd.Name).FontFamily(Fonts.Arial).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(DoeVariety(sddd, showSex)).FontFamily(Fonts.Arial).FontSize(cellFont);

                    table.Cell().RowSpan(16).Element(BottomBlock)
                        .Text(ratSex switch
                        {
                            Sex.Buck => BuckVariety(root, showSex),
                            Sex.Doe => DoeVariety(root, showSex),
                            _ => root.Variety
                        })
                        .FontFamily(Fonts.Arial)
                        .FontSize(cellFont);

                    table.Cell().RowSpan(8).Element(TopBlock).Text(d.Name).FontFamily(Fonts.Arial)
                        .FontSize(cellFont).Bold();
                    table.Cell().RowSpan(4).Element(TopBlock).Text(ds.Name).FontFamily(Fonts.Arial)
                        .FontSize(cellFont).Bold();
                    table.Cell().RowSpan(2).Element(TopBlock).Text(dss.Name).FontFamily(Fonts.Arial)
                        .FontSize(cellFont).Bold();
                    table.Cell().Element(TopBlock).Text(dsss.Name).FontFamily(Fonts.Arial).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(BuckVariety(dsss, showSex)).FontFamily(Fonts.Arial).FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(BottomBlock).Text(BuckVariety(dss, showSex)).FontFamily(Fonts.Arial)
                        .FontSize(cellFont);

                    table.Cell().Element(TopBlock).Text(dssd.Name).FontFamily(Fonts.Arial).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(DoeVariety(dssd, showSex)).FontFamily(Fonts.Arial).FontSize(cellFont);

                    table.Cell().RowSpan(4).Element(BottomBlock).Text(BuckVariety(ds, showSex)).FontFamily(Fonts.Arial)
                        .FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(TopBlock).Text(dsd.Name).FontFamily(Fonts.Arial)
                        .FontSize(cellFont).Bold();
                    table.Cell().Element(TopBlock).Text(dsds.Name).FontFamily(Fonts.Arial).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(BuckVariety(dsds, showSex)).FontFamily(Fonts.Arial).FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(BottomBlock).Text(DoeVariety(dsd, showSex)).FontFamily(Fonts.Arial)
                        .FontSize(cellFont);

                    table.Cell().Element(TopBlock).Text(dsdd.Name).FontFamily(Fonts.Arial).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(DoeVariety(dsdd, showSex)).FontFamily(Fonts.Arial).FontSize(cellFont);

                    table.Cell().RowSpan(8).Element(BottomBlock).Text(DoeVariety(d, showSex)).FontFamily(Fonts.Arial)
                        .FontSize(cellFont);

                    table.Cell().RowSpan(4).Element(TopBlock).Text(dd.Name).FontFamily(Fonts.Arial)
                        .FontSize(cellFont).Bold();
                    table.Cell().RowSpan(2).Element(TopBlock).Text(dds.Name).FontFamily(Fonts.Arial)
                        .FontSize(cellFont).Bold();
                    table.Cell().Element(TopBlock).Text(ddss.Name).FontFamily(Fonts.Arial).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(BuckVariety(ddss, showSex)).FontFamily(Fonts.Arial).FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(BottomBlock).Text(BuckVariety(dds, showSex)).FontFamily(Fonts.Arial)
                        .FontSize(cellFont);

                    table.Cell().Element(TopBlock).Text(ddsd.Name).FontFamily(Fonts.Arial).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(DoeVariety(ddsd, showSex)).FontFamily(Fonts.Arial).FontSize(cellFont);

                    table.Cell().RowSpan(4).Element(BottomBlock).Text(DoeVariety(dd, showSex)).FontFamily(Fonts.Arial)
                        .FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(TopBlock).Text(ddd.Name).FontFamily(Fonts.Arial)
                        .FontSize(cellFont).Bold();
                    table.Cell().Element(TopBlock).Text(ddds.Name).FontFamily(Fonts.Arial).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(BuckVariety(ddds, showSex)).FontFamily(Fonts.Arial).FontSize(cellFont);

                    table.Cell().RowSpan(2).Element(BottomBlock).Text(DoeVariety(ddd, showSex)).FontFamily(Fonts.Arial)
                        .FontSize(cellFont);

                    table.Cell().Element(TopBlock).Text(dddd.Name).FontFamily(Fonts.Arial).FontSize(cellFont)
                        .Bold();
                    table.Cell().Element(BottomBlock).Text(DoeVariety(dddd, showSex)).FontFamily(Fonts.Arial).FontSize(cellFont);

                    table.Cell().ColumnSpan(5).Element(Footer).Text($"Date of birth: {dateOfBirth?.ToString("dd/MM/yyyy.")} {footerText}");
                });
            });
        });
    }

    public void WriteToStream(Document document, Stream stream)
    {
        document.GeneratePdf(stream);
    }

    private static string BuckVariety(Node node, bool showSex) => string.IsNullOrEmpty(node.Variety) ? string.Empty : $"{(showSex ? "♂ " : "")}{node.Variety}";
    private static string DoeVariety(Node node, bool showSex) => string.IsNullOrEmpty(node.Variety) ? string.Empty : $"{(showSex ? "♀ " : "")}{node.Variety}";
    
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