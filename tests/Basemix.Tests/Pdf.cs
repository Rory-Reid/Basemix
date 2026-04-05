using Basemix.Lib.Pedigrees;
using Basemix.Lib.Rats;
using Basemix.Tests.sdk;
using Bogus;

namespace Basemix.Tests;

public class Pdf
{
    private readonly Faker faker = new();

    [Fact]
    public void Test_pedigree_generation()
    {
        var svgGen = new PedigreeSvgGenerator();
        var html = svgGen.GenerateHtml(
            this.faker.CodedPedigree(),
            this.faker.PickNonDefault<Sex>(),
            this.faker.Date.RecentDateOnly(),
            "Twin Squeaks",
            "Dam & Sire",
            "Footer text",
            true);

        Assert.NotEmpty(html);
        Assert.Contains("<svg", html);
        Assert.Contains("Twin Squeaks", html);
    }
}
