namespace Basemix.Lib.Settings;

public class Profile
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? RatteryName { get; set; }
    public PedigreeSettings Pedigree { get; set; } = null!;

    public class PedigreeSettings
    {
        public string? Footer { get; set; }
        public bool ShowSex { get; set; }
        public PdfSettings Pdf { get; set; } = null!;

        public class PdfSettings
        {
            public int PageMargin { get; set; }
            public string Font { get; set; } = null!;
            public int HeaderFontSize { get; set; }
            public int SubheaderFontSize { get; set; }
            public int FontSize { get; set; }
            public int FooterFontSize { get; set; }
        }
    }
}