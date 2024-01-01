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
        this.Page.Profile.LitterEstimation.MinBirthDaysAfterPairing = this.faker.Random.Int(19, 23);
        this.Page.Profile.LitterEstimation.MaxBirthDaysAfterPairing = this.faker.Random.Int(21, 25);
        this.Page.Profile.LitterEstimation.MinWeaningDaysAfterBirth = this.faker.Random.Int(7*3, (7*4)+4);
        this.Page.Profile.LitterEstimation.MinSeparationDaysAfterBirth = this.faker.Random.Int((7*4), (7*5));
        this.Page.Profile.LitterEstimation.MinRehomeDaysAfterBirth = this.faker.Random.Int(7*6, 100);
        
        (await this.profileRepository.GetDefaultProfile())
            .Name
            .ShouldNotBe(this.Page.Profile.Name); // Lazy basic proof of difference
        
        await this.Page.SaveAndGoBack();
        this.Page.ShowError.ShouldBeFalse();

        (await this.profileRepository.GetDefaultProfile())
            .ShouldBeEquivalentTo(this.Page.Profile);
    }

    [Fact]
    public async Task Invalid_min_birth_date_should_show_error_and_not_save()
    {
        this.Page.Profile.LitterEstimation.MinBirthDaysAfterPairing = this.faker.Random.Bool()
            ? this.faker.Random.Int(24, 100)
            : this.faker.Random.Int(0, 18);
        
        await this.Page.SaveAndGoBack();
        
        this.Page.ShowError.ShouldBeTrue();
        this.Page.ErrorMessages.ShouldContain("Gestation is typically 21-23 days.");
        (await this.profileRepository.GetDefaultProfile())
            .LitterEstimation.MinBirthDaysAfterPairing
            .ShouldNotBe(this.Page.Profile.LitterEstimation.MinBirthDaysAfterPairing);
    }
    
    [Fact]
    public async Task Invalid_max_birth_date_should_show_error_and_not_save()
    {
        this.Page.Profile.LitterEstimation.MaxBirthDaysAfterPairing = this.faker.Random.Bool()
            ? this.faker.Random.Int(26, 100)
            : this.faker.Random.Int(0, 20);
        
        await this.Page.SaveAndGoBack();
        
        this.Page.ShowError.ShouldBeTrue();
        this.Page.ErrorMessages.ShouldContain("Gestation is typically 21-23 days.");
        (await this.profileRepository.GetDefaultProfile())
            .LitterEstimation.MaxBirthDaysAfterPairing
            .ShouldNotBe(this.Page.Profile.LitterEstimation.MaxBirthDaysAfterPairing);
    }
    
    [Fact]
    public async Task Invalid_min_weaning_date_should_show_error_and_not_save()
    {
        this.Page.Profile.LitterEstimation.MinWeaningDaysAfterBirth = this.faker.Random.Bool()
            ? this.faker.Random.Int(0, (7*3)-1)
            : this.faker.Random.Int((7*4)+5, 100);
        
        await this.Page.SaveAndGoBack();
        
        this.Page.ShowError.ShouldBeTrue();
        this.Page.ErrorMessages.ShouldContain("Rats should be fully weaned around 3.5-4 weeks after birth.");
        (await this.profileRepository.GetDefaultProfile())
            .LitterEstimation.MinWeaningDaysAfterBirth
            .ShouldNotBe(this.Page.Profile.LitterEstimation.MinWeaningDaysAfterBirth);
    }
    
    [Fact]
    public async Task Invalid_min_separation_date_should_show_error_and_not_save()
    {
        this.Page.Profile.LitterEstimation.MinSeparationDaysAfterBirth = this.faker.Random.Bool()
            ? this.faker.Random.Int(0, (7*4)-2)
            : this.faker.Random.Int((7*5)+2, 100);
        
        await this.Page.SaveAndGoBack();
        
        this.Page.ShowError.ShouldBeTrue();
        this.Page.ErrorMessages.ShouldContain("Rats should be separated around 4-5 weeks after birth.");
        (await this.profileRepository.GetDefaultProfile())
            .LitterEstimation.MinSeparationDaysAfterBirth
            .ShouldNotBe(this.Page.Profile.LitterEstimation.MinSeparationDaysAfterBirth);
    }
    
    [Fact]
    public async Task Invalid_min_rehome_date_should_show_error_and_not_save()
    {
        this.Page.Profile.LitterEstimation.MinRehomeDaysAfterBirth = this.faker.Random.Int(0, (7 * 6) - 1);
        
        await this.Page.SaveAndGoBack();
        
        this.Page.ShowError.ShouldBeTrue();
        this.Page.ErrorMessages.ShouldContain("Rehoming should be no sooner than 6 weeks after birth.");
        (await this.profileRepository.GetDefaultProfile())
            .LitterEstimation.MinRehomeDaysAfterBirth
            .ShouldNotBe(this.Page.Profile.LitterEstimation.MinRehomeDaysAfterBirth);
    }
    
    [Fact]
    public async Task Invalid_min_birth_date_should_show_error_and_not_save_when_min_is_after_max()
    {
        this.Page.Profile.LitterEstimation.MinBirthDaysAfterPairing = this.faker.Random.Int(24, 100);
        this.Page.Profile.LitterEstimation.MaxBirthDaysAfterPairing = this.faker.Random.Int(0, 20);
        
        await this.Page.SaveAndGoBack();
        
        this.Page.ShowError.ShouldBeTrue();
        this.Page.ErrorMessages.ShouldContain("Minimum DOB after pairing must be before maximum.");
        (await this.profileRepository.GetDefaultProfile()).LitterEstimation.ShouldSatisfyAllConditions(
                estimation => estimation.MinBirthDaysAfterPairing.ShouldNotBe(this.Page.Profile.LitterEstimation.MinBirthDaysAfterPairing),
                estimation => estimation.MaxBirthDaysAfterPairing.ShouldNotBe(this.Page.Profile.LitterEstimation.MaxBirthDaysAfterPairing));
    }
    
    [Fact]
    public async Task Invalid_min_weaning_date_should_show_error_and_not_save_when_min_is_after_separation()
    {
        this.Page.Profile.LitterEstimation.MinWeaningDaysAfterBirth = this.faker.Random.Int(30, 100);
        this.Page.Profile.LitterEstimation.MinSeparationDaysAfterBirth = this.faker.Random.Int(0, 29);
        
        await this.Page.SaveAndGoBack();
        
        this.Page.ShowError.ShouldBeTrue();
        this.Page.ErrorMessages.ShouldContain("Rats should be weaned before they are separated.");
        (await this.profileRepository.GetDefaultProfile()).LitterEstimation.ShouldSatisfyAllConditions(
            estimation => estimation.MinWeaningDaysAfterBirth.ShouldNotBe(this.Page.Profile.LitterEstimation.MinWeaningDaysAfterBirth),
            estimation => estimation.MinSeparationDaysAfterBirth.ShouldNotBe(this.Page.Profile.LitterEstimation.MinSeparationDaysAfterBirth));
    }
}