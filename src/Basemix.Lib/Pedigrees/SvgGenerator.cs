using System.Text;
using Basemix.Lib.Rats;

namespace Basemix.Lib.Pedigrees;

public class PedigreeSvgGenerator
{
    // A4 landscape in mm: 297 x 210
    // We work in a coordinate system that maps to these dimensions
    private const double PageWidth = 297;
    private const double PageHeight = 210;
    private const double Margin = 5;
    private const double HeaderHeight = 10;
    private const double SubheaderHeight = 8;
    private const double FooterHeight = 8;

    private const string BuckColour = "#0a53be";
    private const string DoeColour = "#d63384";
    private const string HeaderBg = "#e8e8e8";
    private const string CellBorderColour = "#000000";

    public string GenerateHtml(
        Node root,
        Sex? ratSex,
        DateOnly? dateOfBirth,
        string? ratteryName,
        string? litterName,
        string? footerText,
        bool showSex)
    {
        return
            $$"""
              <!DOCTYPE html>
              <html lang="en">
              <head>
                <meta charset="utf-8" />
                  <title>{{root.Name}} Pedigree</title>
                  <style>
                      @page { size: A4 landscape; margin: 0; }
                      html { margin: 0; padding: 0; }
                      body { margin: 0; padding: 0; width: 297mm; height: 210mm; overflow: hidden; page-break-after: avoid; }
                      svg { display: block; width: 297mm; height: 210mm; }
                  </style>
              </head>
              <body>
                  {{this.GenerateSvg(root, ratSex, dateOfBirth, ratteryName, litterName, footerText, showSex)}}
              </body>
              </html>
              """;
    }

    public string GenerateSvg(
        Node root,
        Sex? ratSex,
        DateOnly? dateOfBirth,
        string? ratteryName,
        string? litterName,
        string? footerText,
        bool showSex)
    {
        var nullNode = new Node {Name = "-", Variety = string.Empty};

        const double treeTop = Margin + HeaderHeight + SubheaderHeight;
        const double treeHeight = PageHeight - treeTop - FooterHeight - Margin;
        const double treeWidth = PageWidth - (2 * Margin);

        const double colWidth = treeWidth / 5;
        const double rowHeight = treeHeight / 16;

        var sb = new StringBuilder();
        sb.AppendLine(
            FormattableString.Invariant(
                $"""<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {PageWidth} {PageHeight}">"""));

        // Background
        sb.AppendLine($"""<rect width="{PageWidth}" height="{PageHeight}" fill="white"/>""");

        // Header - Rattery Name
        DrawRect(sb, Margin, Margin, treeWidth, HeaderHeight, HeaderBg, CellBorderColour);
        DrawText(sb, Margin + treeWidth / 2, Margin + HeaderHeight / 2, Escape(ratteryName ?? ""), 5, true, "black",
            "middle");

        // Subheader - Litter Name
        const double shy = Margin + HeaderHeight;
        DrawRect(sb, Margin, shy, treeWidth, SubheaderHeight, HeaderBg, CellBorderColour);
        DrawText(sb, Margin + treeWidth / 2, shy + SubheaderHeight / 2, Escape(litterName ?? ""), 3.5, false, "black",
            "middle");

        // Pedigree cells - structured as 5 columns x 16 rows with merging
        // Column 0: Subject (spans all 16 rows)
        // Column 1: Parents (span 8 rows each)
        // Column 2: Grandparents (span 4 rows each)
        // Column 3: Great-grandparents (span 2 rows each)
        // Column 4: Great-great-grandparents (1 row each)

        // Each ancestor occupies TWO visual slots: top = name (bold), bottom = variety

        // Define all cells: (col, startRow, rowSpan, node, isSire, isNameRow)
        // The PDF layout splits each ancestor into a "top block" (name) and "bottom block" (variety)
        // except for the rightmost column where each row is one slot

        // Column 0: Subject
        DrawPedigreeCell(sb, 0, 0, 16, colWidth, rowHeight, treeTop, Margin,
            root.Name ?? "Unnamed", true, "black");
        DrawPedigreeCell(sb, 0, 0, 16, colWidth, rowHeight, treeTop, Margin,
            VarietyText(root, showSex, ratSex), false, "black", true);

        // Column 1: Sire (rows 0-7), Dam (rows 8-15)
        var s = root.Sire ?? nullNode;
        var d = root.Dam ?? nullNode;
        DrawAncestorPair(sb, 1, 0, 8, s, true, showSex, colWidth, rowHeight, treeTop, Margin);
        DrawAncestorPair(sb, 1, 8, 8, d, false, showSex, colWidth, rowHeight, treeTop, Margin);

        // Column 2: Grandparents (4 rows each)
        var ss = root.Sire?.Sire ?? nullNode;
        var sd = root.Sire?.Dam ?? nullNode;
        var ds = root.Dam?.Sire ?? nullNode;
        var dd = root.Dam?.Dam ?? nullNode;
        DrawAncestorPair(sb, 2, 0, 4, ss, true, showSex, colWidth, rowHeight, treeTop, Margin);
        DrawAncestorPair(sb, 2, 4, 4, sd, false, showSex, colWidth, rowHeight, treeTop, Margin);
        DrawAncestorPair(sb, 2, 8, 4, ds, true, showSex, colWidth, rowHeight, treeTop, Margin);
        DrawAncestorPair(sb, 2, 12, 4, dd, false, showSex, colWidth, rowHeight, treeTop, Margin);

        // Column 3: Great-grandparents (2 rows each)
        var sss = root.Sire?.Sire?.Sire ?? nullNode;
        var ssd = root.Sire?.Sire?.Dam ?? nullNode;
        var sds = root.Sire?.Dam?.Sire ?? nullNode;
        var sdd = root.Sire?.Dam?.Dam ?? nullNode;
        var dss = root.Dam?.Sire?.Sire ?? nullNode;
        var dsd = root.Dam?.Sire?.Dam ?? nullNode;
        var dds = root.Dam?.Dam?.Sire ?? nullNode;
        var ddd = root.Dam?.Dam?.Dam ?? nullNode;
        DrawAncestorPair(sb, 3, 0, 2, sss, true, showSex, colWidth, rowHeight, treeTop, Margin);
        DrawAncestorPair(sb, 3, 2, 2, ssd, false, showSex, colWidth, rowHeight, treeTop, Margin);
        DrawAncestorPair(sb, 3, 4, 2, sds, true, showSex, colWidth, rowHeight, treeTop, Margin);
        DrawAncestorPair(sb, 3, 6, 2, sdd, false, showSex, colWidth, rowHeight, treeTop, Margin);
        DrawAncestorPair(sb, 3, 8, 2, dss, true, showSex, colWidth, rowHeight, treeTop, Margin);
        DrawAncestorPair(sb, 3, 10, 2, dsd, false, showSex, colWidth, rowHeight, treeTop, Margin);
        DrawAncestorPair(sb, 3, 12, 2, dds, true, showSex, colWidth, rowHeight, treeTop, Margin);
        DrawAncestorPair(sb, 3, 14, 2, ddd, false, showSex, colWidth, rowHeight, treeTop, Margin);

        // Column 4: Great-great-grandparents (1 row each)
        // These are special: each gets a single row, name and variety squeezed in
        var ggParents = new (Node node, bool isSire)[]
        {
            (root.Sire?.Sire?.Sire?.Sire ?? nullNode, true),
            (root.Sire?.Sire?.Sire?.Dam ?? nullNode, false),
            (root.Sire?.Sire?.Dam?.Sire ?? nullNode, true),
            (root.Sire?.Sire?.Dam?.Dam ?? nullNode, false),
            (root.Sire?.Dam?.Sire?.Sire ?? nullNode, true),
            (root.Sire?.Dam?.Sire?.Dam ?? nullNode, false),
            (root.Sire?.Dam?.Dam?.Sire ?? nullNode, true),
            (root.Sire?.Dam?.Dam?.Dam ?? nullNode, false),
            (root.Dam?.Sire?.Sire?.Sire ?? nullNode, true),
            (root.Dam?.Sire?.Sire?.Dam ?? nullNode, false),
            (root.Dam?.Sire?.Dam?.Sire ?? nullNode, true),
            (root.Dam?.Sire?.Dam?.Dam ?? nullNode, false),
            (root.Dam?.Dam?.Sire?.Sire ?? nullNode, true),
            (root.Dam?.Dam?.Sire?.Dam ?? nullNode, false),
            (root.Dam?.Dam?.Dam?.Sire ?? nullNode, true),
            (root.Dam?.Dam?.Dam?.Dam ?? nullNode, false),
        };

        for (var i = 0; i < 16; i++)
        {
            var (node, isSire) = ggParents[i];
            DrawSingleRowCell(sb, 4, i, node, isSire, showSex, colWidth, rowHeight, treeTop, Margin);
        }

        // Footer
        const double fy = treeTop + treeHeight;
        DrawRect(sb, Margin, fy, treeWidth, FooterHeight, HeaderBg, CellBorderColour);
        var footerContent = $"Date of birth: {dateOfBirth.ToLocalizedPdfString()} {footerText ?? ""}".Trim();
        DrawText(sb, Margin + 2, fy + FooterHeight / 2, Escape(footerContent), 2.8, false, "black", "start");

        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    private static void DrawAncestorPair(StringBuilder sb, int col, int startRow, int rowSpan, Node node, bool isSire,
        bool showSex, double colWidth, double rowHeight, double treeTop, double margin)
    {
        DrawPedigreeCell(sb, col, startRow, rowSpan, colWidth, rowHeight, treeTop, margin, node.Name ?? "-", true,
            isSire ? BuckColour : DoeColour);
        DrawPedigreeCell(sb, col, startRow, rowSpan, colWidth, rowHeight, treeTop, margin,
            VarietyText(node, showSex, isSire ? Sex.Buck : Sex.Doe), false, "black");
    }

    private static void DrawSingleRowCell(StringBuilder sb, int col, int row, Node node, bool isSire, bool showSex,
        double colWidth, double rowHeight, double treeTop, double margin)
    {
        var x = margin + col * colWidth;
        var y = treeTop + row * rowHeight;
        var nameColour = isSire ? BuckColour : DoeColour;

        DrawRect(sb, x, y, colWidth, rowHeight, "white", CellBorderColour);

        // Name in top portion
        var fontSize = Math.Min(2.2, rowHeight * 0.35);
        DrawText(sb, x + colWidth / 2, y + rowHeight * 0.35, Escape(node.Name ?? "-"),
            fontSize, true, nameColour, "middle");

        // Variety in bottom portion
        var variety = isSire ? VarietyText(node, showSex, Sex.Buck) : VarietyText(node, showSex, Sex.Doe);
        DrawText(sb, x + colWidth / 2, y + rowHeight * 0.72, Escape(variety),
            fontSize * 0.85, false, "black", "middle");
    }

    private static void DrawPedigreeCell(StringBuilder sb, int col, int startRow, int rowSpan, double colWidth,
        double rowHeight, double treeTop, double margin, string text, bool isName, string colour,
        bool isLowerHalf = false)
    {
        var x = margin + col * colWidth;
        var y = treeTop + startRow * rowHeight;
        var h = rowSpan * rowHeight;

        if (isName && !isLowerHalf)
        {
            // Draw the full cell rect only once (for the name/top portion)
            DrawRect(sb, x, y, colWidth, h, "white", CellBorderColour);

            // Name goes in upper portion of cell
            var fontSize = Math.Min(3.2, h * 0.15);
            if (col == 0) fontSize = Math.Min(4, h * 0.08); // Subject gets bigger text
            DrawText(sb, x + colWidth / 2, y + h * 0.42, Escape(text),
                fontSize, true, colour, "middle");
        }
        else
        {
            // Variety goes in lower portion of cell
            var fontSize = Math.Min(2.8, h * 0.12);
            if (col == 0) fontSize = Math.Min(3.2, h * 0.06);
            DrawText(sb, x + colWidth / 2, y + h * 0.58, Escape(text),
                fontSize, false, colour, "middle");
        }
    }

    private static void DrawRect(StringBuilder sb, double x, double y, double w, double h, string fill, string stroke)
    {
        sb.AppendLine(FormattableString.Invariant(
            $"""
             <rect x="{x:F2}" y="{y:F2}" width="{w:F2}" height="{h:F2}"
                   fill="{fill}" stroke="{stroke}" stroke-width="0.3"/>
             """));
    }

    private static void DrawText(StringBuilder sb, double x, double y, string text, double fontSize, bool bold,
        string colour, string anchor)
    {
        var weight = bold ? "font-weight=\"bold\"" : "";
        sb.AppendLine(FormattableString.Invariant(
            $"""
             <text x="{x:F2}" y="{y:F2}" font-size="{fontSize:F1}"
                   font-family="Arial, Helvetica, sans-serif" fill="{colour}"
                   text-anchor="{anchor}" dominant-baseline="central" {weight}>
                 {text}
             </text>
             """));
    }

    private static string VarietyText(Node node, bool showSex, Sex? sex)
    {
        var varietyParts = new List<string>();
        if (showSex && sex != null)
        {
            varietyParts.Add($"({sex.ToString()})");
        }

        if (!string.IsNullOrEmpty(node.Variety))
        {
            varietyParts.Add(node.Variety);
        }

        return string.Join(" ", varietyParts);
    }

    private static string Escape(string text) =>
        text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
}