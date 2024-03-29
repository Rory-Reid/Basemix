using System.Diagnostics;
using Basemix.Lib;
using Basemix.Lib.Pedigrees;
using Basemix.Lib.Rats;
using Basemix.Lib.Settings;
using Basemix.Tests.sdk;
using Bogus;
using QuestPDF.Fluent;

namespace Basemix.Tests;

public class Pdf
{
    private readonly Faker faker = new();

    [Fact]
    public void Test_pedigree_generation()
    {
        var pdfGen = new PdfGenerator();
        var path = "/Users/rory/Documents/pdf/pedigree.pdf";
        var doc = pdfGen.CreateFromPedigree(
            this.faker.CodedPedigree(), 
            this.faker.PickNonDefault<Sex>(), 
            this.faker.Date.RecentDateOnly(),
            "Twin Squeaks",
            "Dam & Sire",
            $"Saved to {path}",
            true,
            new Profile.PedigreeSettings.PdfSettings
            {
                Font = "Carlito",
                FooterFontSize = 10,
                HeaderFontSize = 36,
                SubheaderFontSize = 26,
                FontSize = 10,
                PageMargin = 25
            });

        // Uncomment below to save and open the PDF
        // doc.GeneratePdf(path);
        // Process.Start("open", path);
    }
}