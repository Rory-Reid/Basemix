using System.Diagnostics.CodeAnalysis;
using Basemix.Rats;
using Basemix.Tests.sdk;
using Basemix.UI.Pages;
using Bogus;
using Shouldly;

namespace Basemix.Tests.UI.Pages;

public class EditRatTests : RazorPageTests<EditRat>
{
    private readonly Faker faker = new();
    private readonly MemoryPersistenceBackplane backplane = new();
    private readonly TestNavigationManager nav = new();
    private MemoryRatsRepository ratsRepository = null!;
    private MemoryLittersRepository littersRepository = null!;
    
    [SuppressMessage("Usage", "BL0005:Component parameter should not be set outside of its component.")]
    protected override EditRat CreatePage()
    {
        this.ratsRepository = new MemoryRatsRepository(this.backplane);
        this.littersRepository = new MemoryLittersRepository(this.backplane);
        
        // TODO - edit rat shouldn't crash without a rat
        var rat = this.faker.Rat(id: this.faker.Id());
        this.ratsRepository.Rats[rat.Id] = rat;
        
        return new()
        {
            Id = rat.Id,
            Repository = this.ratsRepository,
            LittersRepository = this.littersRepository,
            JsRuntime = new NullJsRuntime(),
            Nav = this.nav
        };
    }

    [Fact]
    public async Task Loads_rat_into_form_on_initialisation()
    {
        var rat = this.faker.Rat(id: this.Page.Id);
        this.ratsRepository.Rats[rat.Id] = rat;

        await RazorEngine.InvokeOnInitializedAsync(this.Page);
        
        this.Page.ShouldSatisfyAllConditions(
            page => page.Rat.ShouldBe(rat),
            page => page.RatForm.ShouldSatisfyAllConditions(
                form => form.Name.ShouldBe(rat.Name),
                form => form.Sex.ShouldBe(rat.Sex.ToString()),
                form => form.DateOfBirth.ShouldBe(rat.DateOfBirth),
                form => form.Notes.ShouldBe(rat.Notes)));
    }

    [Theory]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(false, false, true)]
    public void Disables_create_litter_button_based_on_form_input(bool nameSet, bool sexSet, bool expectedDisabled) // TODO - wrong, why is this on notes?
    {
        this.Page.RatForm.Name = nameSet ? this.faker.Lorem.Word() : null;
        this.Page.RatForm.Sex = sexSet ? this.faker.PickNonDefault<Sex>().ToString() : null;
        
        this.Page.DisableCreateLitter.ShouldBe(expectedDisabled);
    }

    [Fact]
    public async Task Save_rat_button_saves()
    {
        var rat = this.faker.Rat(id: this.Page.Id);
        this.ratsRepository.Rats[rat.Id] = rat;
        await RazorEngine.InvokeOnInitializedAsync(this.Page);

        var newSex = this.faker.PickNonDefault<Sex>();
        this.Page.RatForm.Name = this.faker.Lorem.Word();
        this.Page.RatForm.Sex = newSex.ToString();
        this.Page.RatForm.DateOfBirth = this.faker.Date.PastDateOnly();
        this.Page.RatForm.Notes = this.faker.Lorem.Paragraphs();
        await this.Page.SaveAndGoBack();
        
        rat.ShouldSatisfyAllConditions(
            () => rat.Name.ShouldBe(this.Page.RatForm.Name),
            () => rat.Sex.ShouldBe(newSex),
            () => rat.DateOfBirth.ShouldBe(this.Page.RatForm.DateOfBirth),
            () => rat.Notes.ShouldBe(this.Page.RatForm.Notes));
    }
    
    [Theory]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public async Task New_litter_button_does_nothing_if_sex_or_name_missing(bool sexMissing, bool nameMissing)
    {
        var rat = this.faker.Rat(id: this.Page.Id);
        this.ratsRepository.Rats[rat.Id] = rat;
        await RazorEngine.InvokeOnInitializedAsync(this.Page);

        if (nameMissing)
        {
            this.Page.RatForm.Name = null;
        }

        if (sexMissing)
        {
            this.Page.RatForm.Sex = null;
        }

        await this.Page.NewLitter();
        rat.ShouldSatisfyAllConditions(
            () => rat.Name.ShouldNotBeNullOrEmpty(),
            () => rat.Sex.ShouldNotBeNull());
        this.littersRepository.Litters.Values
            .ShouldSatisfyAllConditions(
                litters => litters.ShouldNotContain(x => x.DamId == rat.Id),
                litters => litters.ShouldNotContain(x => x.SireId == rat.Id));
    }

    [Fact]
    public async Task New_litter_button_saves_rat_edits()
    {
        var rat = this.faker.Rat(id: this.Page.Id);
        this.ratsRepository.Rats[rat.Id] = rat;
        await RazorEngine.InvokeOnInitializedAsync(this.Page);
        
        var newSex = this.faker.PickNonDefault<Sex>();
        this.Page.RatForm.Name = this.faker.Lorem.Word();
        this.Page.RatForm.Sex = newSex.ToString();
        this.Page.RatForm.DateOfBirth = this.faker.Date.PastDateOnly();
        this.Page.RatForm.Notes = this.faker.Lorem.Paragraphs();

        await this.Page.NewLitter();
        
        rat.ShouldSatisfyAllConditions(
            () => rat.Name.ShouldBe(this.Page.RatForm.Name),
            () => rat.Sex.ShouldBe(newSex),
            () => rat.DateOfBirth.ShouldBe(this.Page.RatForm.DateOfBirth),
            () => rat.Notes.ShouldBe(this.Page.RatForm.Notes));
    }

    [Fact]
    public async Task New_litter_button_creates_with_dam_and_navigates()
    {
        var rat = this.faker.Rat(id: this.Page.Id);
        this.ratsRepository.Rats[rat.Id] = rat;
        await RazorEngine.InvokeOnInitializedAsync(this.Page);
        
        this.Page.RatForm.Sex = Sex.Doe.ToString();
        await this.Page.NewLitter();
        
        var litter = this.littersRepository.Litters.ShouldHaveSingleItem().Value;
        litter.ShouldSatisfyAllConditions(
            () => litter.DamId.ShouldBe(rat.Id),
            () => litter.DamName.ShouldBe(rat.Name));
        this.nav.CurrentUri.ShouldBe($"/litters/{litter.Id.Value}/edit");
        
    }

    [Fact]
    public async Task New_litter_button_creates_with_sire_and_navigates()
    {
        var rat = this.faker.Rat(id: this.Page.Id);
        this.ratsRepository.Rats[rat.Id] = rat;
        await RazorEngine.InvokeOnInitializedAsync(this.Page);
        
        this.Page.RatForm.Sex = Sex.Buck.ToString();
        await this.Page.NewLitter();
        
        var litter = this.littersRepository.Litters.ShouldHaveSingleItem().Value;
        litter.ShouldSatisfyAllConditions(
            () => litter.SireId.ShouldBe(rat.Id),
            () => litter.SireName.ShouldBe(rat.Name));
        this.nav.CurrentUri.ShouldBe($"/litters/{litter.Id.Value}/edit");
    }

    [Fact]
    public async Task Delete_rat_removes_from_repo_and_navigates_to_rats_page()
    {
        var rat = this.faker.Rat(id: this.Page.Id);
        this.ratsRepository.Rats[rat.Id] = rat;
        await RazorEngine.InvokeOnInitializedAsync(this.Page);

        await this.Page.DeleteRat();
        
        this.ratsRepository.Rats.ShouldNotContainKey(rat.Id);
        this.nav.CurrentUri.ShouldBe("/rats");
    }
}