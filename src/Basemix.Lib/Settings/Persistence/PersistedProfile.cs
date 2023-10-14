using Basemix.Lib.Rats.Persistence;

namespace Basemix.Lib.Settings.Persistence;

public class PersistedProfile
{
    public PersistedProfile() {}

    public PersistedProfile(Profile profile)
    {
        this.Id = profile.Id;
        this.Name = profile.Name;
        this.RatteryName = profile.RatteryName;
        this.PedigreeFooter = profile.Pedigree.Footer;
        this.PedigreeShowSex = profile.Pedigree.ShowSex;
        this.PedigreePdfPageMargin = profile.Pedigree.Pdf.PageMargin;
        this.PedigreePdfFont = profile.Pedigree.Pdf.Font;
        this.PedigreePdfHeaderFontSize = profile.Pedigree.Pdf.HeaderFontSize;
        this.PedigreePdfSubheaderFontSize = profile.Pedigree.Pdf.SubheaderFontSize;
        this.PedigreePdfFontSize = profile.Pedigree.Pdf.FontSize;
        this.PedigreePdfFooterFontSize = profile.Pedigree.Pdf.FooterFontSize;
    }
    
    public long Id { get; init; }
    public string Name { get; init; } = null!;
    public string? RatteryName { get; init; }
    
    public string? PedigreeFooter { get; init; }
    public bool PedigreeShowSex { get; init; }
    
    public int PedigreePdfPageMargin { get; init; }
    public string PedigreePdfFont { get; init; } = null!;
    public int PedigreePdfHeaderFontSize { get; init; }
    public int PedigreePdfSubheaderFontSize { get; init; }
    public int PedigreePdfFontSize { get; init; }
    public int PedigreePdfFooterFontSize { get; init; }

    public Profile ToModelledProfile() =>
        new()
        {
            Id = this.Id,
            Name = this.Name,
            RatteryName = this.RatteryName,
            Pedigree = new Profile.PedigreeSettings
            {
                Footer = this.PedigreeFooter,
                ShowSex = this.PedigreeShowSex,
                Pdf = new Profile.PedigreeSettings.PdfSettings
                {
                    PageMargin = this.PedigreePdfPageMargin,
                    Font = this.PedigreePdfFont,
                    HeaderFontSize = this.PedigreePdfHeaderFontSize,
                    SubheaderFontSize = this.PedigreePdfSubheaderFontSize,
                    FontSize = this.PedigreePdfFontSize,
                    FooterFontSize = this.PedigreePdfFooterFontSize
                }
            }
        };
}