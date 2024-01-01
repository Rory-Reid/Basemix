using Basemix.Lib.Settings.Persistence;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.Integration;

public class SqliteProfileRepositoryTests : SqliteIntegration
{
    private Faker faker = new();
    private SqliteFixture fixture;
    private SqliteProfileRepository repository;
    
    public SqliteProfileRepositoryTests(SqliteFixture fixture) : base(fixture)
    {
        this.fixture = fixture;
        this.repository = new SqliteProfileRepository(fixture.GetConnection);
    }

    [Fact]
    public async Task Returns_default_settings()
    {
        var profile = await this.repository.GetDefaultProfile();
        
        profile.ShouldSatisfyAllConditions(
            () => profile.Id.ShouldBe(1),
            () => profile.Name.ShouldBe("Default"),
            () => profile.RatteryName.ShouldBeNull(),
            () => profile.Pedigree.Footer.ShouldBeNull(),
            () => profile.Pedigree.ShowSex.ShouldBeTrue(),
            () => profile.Pedigree.Pdf.Font.ShouldBe("Carlito"),
            () => profile.Pedigree.Pdf.PageMargin.ShouldBe(25),
            () => profile.Pedigree.Pdf.FontSize.ShouldBe(10),
            () => profile.Pedigree.Pdf.FooterFontSize.ShouldBe(10),
            () => profile.Pedigree.Pdf.HeaderFontSize.ShouldBe(36),
            () => profile.Pedigree.Pdf.SubheaderFontSize.ShouldBe(26),
            () => profile.LitterEstimation.MinBirthDaysAfterPairing.ShouldBe(21),
            () => profile.LitterEstimation.MaxBirthDaysAfterPairing.ShouldBe(23),
            () => profile.LitterEstimation.MinWeaningDaysAfterBirth.ShouldBe(25),
            () => profile.LitterEstimation.MinSeparationDaysAfterBirth.ShouldBe(31),
            () => profile.LitterEstimation.MinRehomeDaysAfterBirth.ShouldBe(42));
    }

    [Fact]
    public async Task Can_save_changes_and_reset_to_defaults()
    {
        var defaultProfile = await this.repository.GetDefaultProfile();
        var profile = await this.repository.GetDefaultProfile();
        profile.RatteryName = this.faker.Hacker.Noun() + " Rattery";
        profile.Pedigree.Footer = this.faker.Internet.Url();
        profile.Pedigree.ShowSex = false;
        profile.Pedigree.Pdf.Font = this.faker.Lorem.Word();
        profile.Pedigree.Pdf.PageMargin = this.faker.Random.Int(0, 100);
        profile.Pedigree.Pdf.FontSize = this.faker.Random.Int(0, 100);
        profile.Pedigree.Pdf.FooterFontSize = this.faker.Random.Int(0, 100);
        profile.Pedigree.Pdf.HeaderFontSize = this.faker.Random.Int(0, 100);
        profile.Pedigree.Pdf.SubheaderFontSize = this.faker.Random.Int(0, 100);
        profile.LitterEstimation.MinBirthDaysAfterPairing = this.faker.Random.Int(0, 100);
        profile.LitterEstimation.MaxBirthDaysAfterPairing = this.faker.Random.Int(0, 100);
        profile.LitterEstimation.MinWeaningDaysAfterBirth = this.faker.Random.Int(0, 100);
        profile.LitterEstimation.MinSeparationDaysAfterBirth = this.faker.Random.Int(0, 100);
        profile.LitterEstimation.MinRehomeDaysAfterBirth = this.faker.Random.Int(0, 100);
        
        await this.repository.UpdateProfileSettings(profile);
        var updatedProfile = await this.repository.GetDefaultProfile();
        updatedProfile.ShouldBeEquivalentTo(profile);
        
        await this.repository.ResetProfileDefaults(profile.Id);
        var resetProfile = await this.repository.GetDefaultProfile();
        resetProfile.ShouldBeEquivalentTo(defaultProfile);
    }
}