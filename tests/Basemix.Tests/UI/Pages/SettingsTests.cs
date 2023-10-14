using System.Diagnostics.CodeAnalysis;
using Basemix.Pages;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.UI.Pages;

public class SettingsTests : RazorPageTests<Settings>
{
    private readonly Faker faker = new();
    private readonly MemoryPersistenceBackplane backplane = new();
    private MemoryProfileRepository profileRepository = null!;
    
    [SuppressMessage("Usage", "BL0005:Component parameter should not be set outside of its component.")]
    protected override Settings CreatePage()
    {
        this.profileRepository = new MemoryProfileRepository(this.backplane);

        return new()
        {
            Repository = this.profileRepository,
            JsRuntime = new NullJsRuntime()
        };
    }

    [Fact]
    public async Task Loads_default_profile_on_parameters_set()
    {
        var defaultProfile = await this.profileRepository.GetDefaultProfile();

        await RazorEngine.InvokeOnParametersSetAsync(this.Page);

        this.Page.Profile.ShouldBeEquivalentTo(defaultProfile);
    }

    [Fact]
    public async Task Edits_default_profile_on_save_and_go_back()
    {
        this.Page.Profile.Name = this.faker.Hacker.Noun();
        this.Page.Profile.RatteryName = this.faker.Lorem.Sentence();
        this.Page.Profile.Pedigree.Footer = this.faker.Lorem.Sentence();
        this.Page.Profile.Pedigree.ShowSex = this.faker.Random.Bool();
        this.Page.Profile.Pedigree.Pdf.PageMargin = this.faker.Random.Int(0, 100);
        this.Page.Profile.Pedigree.Pdf.Font = this.faker.Hacker.Noun();
        this.Page.Profile.Pedigree.Pdf.FontSize = this.faker.Random.Int(0, 100);
        this.Page.Profile.Pedigree.Pdf.FooterFontSize = this.faker.Random.Int(0, 100);
        this.Page.Profile.Pedigree.Pdf.HeaderFontSize = this.faker.Random.Int(0, 100);
        this.Page.Profile.Pedigree.Pdf.SubheaderFontSize = this.faker.Random.Int(0, 100);
        (await this.profileRepository.GetDefaultProfile())
            .Name
            .ShouldNotBe(this.Page.Profile.Name); // Lazy basic proof of difference
        
        await this.Page.SaveAndGoBack();
        
        (await this.profileRepository.GetDefaultProfile())
            .ShouldBeEquivalentTo(this.Page.Profile);
    }
}