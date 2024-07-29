using System.Diagnostics.CodeAnalysis;
using Basemix.Lib.Owners;
using Basemix.Lib.Rats;
using Basemix.Pages;
using Basemix.Tests.sdk;
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
    private MemoryOwnersRepository ownersRepository = null!;
    private MemoryOptionsRepository optionsRepository = null!;
    
    [SuppressMessage("Usage", "BL0005:Component parameter should not be set outside of its component.")]
    protected override EditRat CreatePage()
    {
        this.ratsRepository = new MemoryRatsRepository(this.backplane);
        this.littersRepository = new MemoryLittersRepository(this.backplane);
        this.ownersRepository = new MemoryOwnersRepository(this.backplane);
        this.optionsRepository = new MemoryOptionsRepository(this.backplane);
        
        // TODO - edit rat shouldn't crash without a rat
        var rat = this.faker.Rat(id: this.faker.Id());
        this.ratsRepository.Rats[rat.Id] = rat;
        
        return new()
        {
            Id = rat.Id,
            Repository = this.ratsRepository,
            LittersRepository = this.littersRepository,
            OwnersRepository = this.ownersRepository,
            OptionsRepository = this.optionsRepository,
            JsRuntime = new NullJsRuntime(),
            Nav = this.nav
        };
    }

    [Fact]
    public async Task Loads_rat_into_form_on_parameters_set()
    {
        var rat = this.faker.Rat(id: this.Page.Id);
        this.ratsRepository.Rats[rat.Id] = rat;

        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        
        this.Page.ShouldSatisfyAllConditions(
            page => page.Rat.ShouldBe(rat),
            page => page.RatForm.ShouldSatisfyAllConditions(
                form => form.Name.ShouldBe(rat.Name),
                form => form.Sex.ShouldBe(rat.Sex?.ToString()),
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
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);

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
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);

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
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        
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
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        
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
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        
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
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);

        await this.Page.DeleteRat();
        
        this.ratsRepository.Rats.ShouldNotContainKey(rat.Id);
        this.nav.CurrentUri.ShouldBe("/rats");
    }
    
    [Fact]
    public void Edit_litter_navigates_to_litter_edit()
    {
        var id = this.faker.Id();
        
        this.Page.EditLitter(id);
        
        this.nav.CurrentUri.ShouldBe($"/litters/{id}/edit");
    }

    [Fact]
    public void Open_owner_search_clears_existing_search_props_and_shows_search()
    {
        this.Page.OwnerSearchTerm = this.faker.Lorem.Sentence();
        this.Page.OwnerSearchResults.Add(new OwnerSearchResult(this.faker.Id(), this.faker.Person.FullName));
        this.Page.ShowOwnerSearch = false;
        
        this.Page.OpenOwnerSearch();
        
        this.Page.ShouldSatisfyAllConditions(
            page => page.OwnerSearchTerm.ShouldBeNullOrEmpty(),
            page => page.OwnerSearchResults.ShouldBeEmpty(),
            page => page.ShowOwnerSearch.ShouldBeTrue());
    }

    [Fact]
    public async Task Can_search_for_owner()
    {
        var ownerMatch = this.faker.Owner(id: this.faker.Id());
        var otherOwner = this.faker.Owner(id: this.faker.Id());
        
        this.ownersRepository.Seed(ownerMatch);
        this.ownersRepository.Seed(otherOwner);
        
        this.Page.OpenOwnerSearch();
        this.Page.OwnerSearchTerm = ownerMatch.Name;
        
        await this.Page.SearchOwner();
        
        this.Page.OwnerSearchResults.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            result => result.Id.ShouldBe(ownerMatch.Id),
            result => result.Name.ShouldBe(ownerMatch.Name));
    }

    [Fact]
    public async Task Setting_owner_from_search_updates_rat_owner()
    {
        var rat = this.faker.Rat(id: this.Page.Id, owned: false);
        this.ratsRepository.Rats[rat.Id] = rat; // TODO replace with ratrepository.seed
        
        var owner = this.faker.Owner(this.faker.Id());
        this.ownersRepository.Seed(owner);

        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        
        this.Page.OpenOwnerSearch();
        this.Page.OwnerSearchTerm = owner.Name;
        await this.Page.SearchOwner();
        await this.Page.SetResult(new OwnerSearchResult(owner.Id, owner.Name));
        
        this.ratsRepository.Rats[this.Page.Id].Owned.ShouldBe(false);
        this.ratsRepository.Rats[this.Page.Id].OwnerId.ShouldBe(owner.Id);
    }

    /// <summary>
    /// It's important that we save the rat before adding an owner because we intend to navigate away to the owner after
    /// </summary>
    [Fact]
    public async Task Add_owner_saves_rat()
    {
        var rat = this.faker.Rat(id: this.Page.Id, owned: true);
        this.ratsRepository.Rats[rat.Id] = rat; // TODO replace with ratrepository.seed

        await RazorEngine.InvokeOnParametersSetAsync(this.Page);

        this.Page.RatForm.Owned = false; // Edit rat
        await this.Page.AddOwner();

        this.ratsRepository.Rats[this.Page.Id].Owned.ShouldBeFalse();
    }

    [Fact]
    public async Task Add_owner_creates_linked_owner_and_navigates()
    {
        var rat = this.faker.Rat(id: this.Page.Id, owned: false);
        this.ratsRepository.Rats[rat.Id] = rat; // TODO replace with ratrepository.seed
        
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);

        await this.Page.AddOwner();

        var ownerId = this.ratsRepository.Rats[this.Page.Id].OwnerId.ShouldNotBeNull();
        this.nav.CurrentUri.ShouldBe($"/owners/{ownerId.Value}/edit");
    }

    [Fact]
    public async Task Remove_owner_removes_from_rat_and_saves()
    {
        var owner = this.faker.Owner(this.faker.Id());
        this.backplane.Seed(owner);
        var rat = this.faker.Rat(id: this.Page.Id, owner: owner);
        this.ratsRepository.Rats[this.Page.Id] = rat; // TODO replace with ratrepository.seed
        
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        this.Page.Rat.ShouldSatisfyAllConditions(
            loadedRat => loadedRat.OwnerId.ShouldBe(owner.Id),
            loadedRat => loadedRat.OwnerName.ShouldBe(owner.Name),
            loadedRat => loadedRat.Owned.ShouldBe(false));
        this.Page.RatForm.ShouldSatisfyAllConditions(
            form => form.Owned.ShouldBeFalse());
        
        await this.Page.RemoveOwner();
        
        this.Page.Rat.ShouldSatisfyAllConditions(
            loadedRat => loadedRat.OwnerId.ShouldBeNull(),
            loadedRat => loadedRat.OwnerName.ShouldBeNull(),
            loadedRat => loadedRat.Owned.ShouldBe(false));
        this.Page.RatForm.ShouldSatisfyAllConditions(
            form => form.Owned.ShouldBeFalse());
        this.ratsRepository.Rats[this.Page.Id].ShouldSatisfyAllConditions(
            storedRat => storedRat.OwnerId.ShouldBeNull(),
            storedRat => storedRat.OwnerName.ShouldBeNull());
    }

    [Fact]
    public async Task Loads_death_reason_on_parameters_set()
    {
        var deathReason = this.faker.DeathReason();
        this.backplane.Seed(deathReason);
        
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);

        this.Page.DeathReasonOptions.ShouldContain(deathReason);
    }

    [Fact]
    public async Task Can_set_rat_is_dead()
    {
        var rat = this.faker.Rat(id: this.Page.Id);
        this.ratsRepository.Rats[rat.Id] = rat;

        await RazorEngine.InvokeOnParametersSetAsync(this.Page);

        this.Page.RatForm.Dead = true;
        await this.Page.SaveAndGoBack();
        
        this.ratsRepository.Rats[rat.Id].Dead.ShouldBeTrue();
    }

    [Fact]
    public async Task Can_set_rat_death_reason_from_list()
    {
        var rat = this.faker.Rat(id: this.Page.Id);
        this.ratsRepository.Rats[rat.Id] = rat;

        for (var i = 0; i < 10; i++)
        {
            this.backplane.Seed(this.faker.DeathReason());
        }

        await RazorEngine.InvokeOnParametersSetAsync(this.Page);

        var selectedReason = this.faker.PickRandom(this.Page.DeathReasonOptions);
        this.Page.RatForm.Dead = true;
        this.Page.RatForm.DeathReasonId = selectedReason.Id;
        await this.Page.SaveAndGoBack();
        
        this.ratsRepository.Rats[rat.Id].ShouldSatisfyAllConditions(
            storedRat => storedRat.Dead.ShouldBeTrue(),
            storedRat => storedRat.DeathReason.ShouldBe(selectedReason));
    }
}