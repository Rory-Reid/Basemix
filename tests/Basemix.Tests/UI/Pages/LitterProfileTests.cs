using System.Diagnostics.CodeAnalysis;
using Basemix.Rats;
using Basemix.Tests.sdk;
using Basemix.UI.Pages;
using Bogus;
using Shouldly;

namespace Basemix.Tests.UI.Pages;

public class LitterProfileTests : RazorPageTests<LitterProfile>
{
    private readonly Faker faker = new();
    private readonly MemoryPersistenceBackplane backplane = new();
    private TestNavigationManager nav = new();
    private MemoryLittersRepository littersRepository;
    private MemoryRatsRepository ratsRepository;

    [SuppressMessage("Usage", "BL0005:Component parameter should not be set outside of its component.")]
    protected override LitterProfile CreatePage()
    {
        this.littersRepository = new MemoryLittersRepository(this.backplane);
        this.ratsRepository = new MemoryRatsRepository(this.backplane);
        
        return new LitterProfile
        {
            Id = this.faker.Id(),
            Repository = this.littersRepository,
            RatsRepository = this.ratsRepository,
            Nav = this.nav,
            JsRuntime = new NullJsRuntime()
        };
    }

    [Fact]
    public async Task Loads_rat_litter_on_initialisation()
    {
        var litter = this.faker.Litter(id: this.Page.Id);
        this.littersRepository.Seed(litter);

        await RazorEngine.InvokeOnInitializedAsync(this.Page);
        
        this.Page.Litter.ShouldBeEquivalentTo(litter);
    }

    [Fact]
    public async Task Has_dam_properties_false_when_litter_without_dam()
    {
        var litter = this.faker.Litter(id: this.Page.Id, damProbability: 0);
        this.littersRepository.Seed(litter);
        await RazorEngine.InvokeOnInitializedAsync(this.Page);
        
        this.Page.ShouldSatisfyAllConditions(
            page => page.HasDam.ShouldBeFalse(),
            page => page.HasDamName.ShouldBeFalse());
    }

    [Fact]
    public async Task Page_can_have_dam_without_name()
    {
        var litter = this.faker.Litter(id: this.Page.Id, dam: new(this.faker.Id(), null));
        this.littersRepository.Seed(litter);
        await RazorEngine.InvokeOnInitializedAsync(this.Page);
        
        this.Page.ShouldSatisfyAllConditions(
            page => page.HasDam.ShouldBeTrue(),
            page => page.HasDamName.ShouldBeFalse());
    }

    [Fact]
    public async Task Page_can_have_dam_with_name()
    {
        var litter = this.faker.Litter(id: this.Page.Id, dam: new(this.faker.Id(), this.faker.Name.FirstName()));
        this.littersRepository.Seed(litter);
        await RazorEngine.InvokeOnInitializedAsync(this.Page);
        
        this.Page.ShouldSatisfyAllConditions(
            page => page.HasDam.ShouldBeTrue(),
            page => page.HasDamName.ShouldBeTrue());
    }
    
    [Fact]
    public async Task Has_sire_properties_false_when_litter_without_dam()
    {
        var litter = this.faker.Litter(id: this.Page.Id, sireProbability: 0);
        this.littersRepository.Seed(litter);
        await RazorEngine.InvokeOnInitializedAsync(this.Page);
        
        this.Page.ShouldSatisfyAllConditions(
            page => page.HasSire.ShouldBeFalse(),
            page => page.HasSireName.ShouldBeFalse());
    }

    [Fact]
    public async Task Page_can_have_sire_without_name()
    {
        var litter = this.faker.Litter(id: this.Page.Id, sire: new(this.faker.Id(), null));
        this.littersRepository.Seed(litter);
        await RazorEngine.InvokeOnInitializedAsync(this.Page);
        
        this.Page.ShouldSatisfyAllConditions(
            page => page.HasSire.ShouldBeTrue(),
            page => page.HasSireName.ShouldBeFalse());
    }

    [Fact]
    public async Task Page_can_have_sire_with_name()
    {
        var litter = this.faker.Litter(id: this.Page.Id, sire: new(this.faker.Id(), this.faker.Name.FirstName()));
        this.littersRepository.Seed(litter);
        await RazorEngine.InvokeOnInitializedAsync(this.Page);
        
        this.Page.ShouldSatisfyAllConditions(
            page => page.HasSire.ShouldBeTrue(),
            page => page.HasSireName.ShouldBeTrue());
    }

    [Fact]
    public void Open_rat_profile_navigates_to_rat()
    {
        var id = this.faker.Id();
        
        this.Page.OpenRatProfile(id);
        
        this.nav.CurrentUri.ShouldBe($"/rats/{id}");
    }

    [Theory]
    [InlineData(null, null, "Anonymous litter")]
    [InlineData("Alice", null, "Alice's litter")]
    [InlineData(null, "Bob", "Bob's litter")]
    [InlineData("Alice", "Bob", "Alice & Bob's litter")]
    public async Task Litter_name_interpolates_dam_and_sire_names(string damName, string sireName, string expectedLitterName)
    {
        var litter = this.faker.Litter(id: this.Page.Id,
            dam: new(this.faker.Id(), damName),
            sire: new(this.faker.Id(), sireName));
        this.littersRepository.Seed(litter);
        await RazorEngine.InvokeOnInitializedAsync(this.Page);
        
        this.Page.LitterName().ShouldBe(expectedLitterName);
    }

    [Fact]
    public async Task Add_parent_with_doe_creates_linked_dam_and_navigates_to_edit()
    {
        this.littersRepository.Seed(this.faker.BlankLitter(id: this.Page.Id));
        await RazorEngine.InvokeOnInitializedAsync(this.Page);

        await this.Page.AddParent(sex: Sex.Doe);

        var damId = this.littersRepository.Litters[this.Page.Id].DamId.ShouldNotBeNull();
        this.ratsRepository.Rats[damId].Sex.ShouldBe(Sex.Doe);
        this.nav.CurrentUri.ShouldBe($"/rats/{damId.Value}/edit");
    }

    [Fact]
    public async Task Add_parent_with_buck_creates_linked_sire_and_navigates_to_edit()
    {
        this.littersRepository.Seed(this.faker.BlankLitter(id: this.Page.Id));
        await RazorEngine.InvokeOnInitializedAsync(this.Page);

        await this.Page.AddParent(sex: Sex.Buck);

        var damId = this.littersRepository.Litters[this.Page.Id].SireId.ShouldNotBeNull();
        this.ratsRepository.Rats[damId].Sex.ShouldBe(Sex.Buck);
        this.nav.CurrentUri.ShouldBe($"/rats/{damId.Value}/edit");
    }

    [Fact]
    public async Task Add_offspring_creates_linked_rat_and_navigates_to_edit()
    {
        this.littersRepository.Seed(this.faker.BlankLitter(id: this.Page.Id));
        await RazorEngine.InvokeOnInitializedAsync(this.Page);

        await this.Page.AddOffspring();

        var litter = this.littersRepository.Litters[this.Page.Id];
        var offspring = litter.Offspring.ShouldHaveSingleItem();
        this.nav.CurrentUri.ShouldBe($"/rats/{offspring.Id.Value}/edit");
    }

    [Fact]
    public async Task Add_offspring_sets_date_of_birth()
    {
        var newLitter = this.faker.BlankLitter(id: this.Page.Id);
        newLitter.DateOfBirth = this.faker.Date.PastDateOnly();
        this.littersRepository.Seed(newLitter);
        await RazorEngine.InvokeOnInitializedAsync(this.Page);

        await this.Page.AddOffspring();

        var litter = this.littersRepository.Litters[this.Page.Id];
        var offspring = litter.Offspring.ShouldHaveSingleItem();
        this.ratsRepository.Rats[offspring.Id].DateOfBirth.ShouldBe(newLitter.DateOfBirth);
    }

    [Fact]
    public void Open_dam_search_clears_existing_search_props_and_shows_search()
    {
        this.Page.RatSearchTerm = this.faker.Lorem.Sentence();
        this.Page.RatSearchSex = Sex.Buck;
        this.Page.RatSearchResults.Add(new RatSearchResult(this.faker.Id(), null, null, null));
        this.Page.ShowRatSearch = false;
        
        this.Page.OpenDamSearch();
        
        this.Page.ShouldSatisfyAllConditions(
            page => page.RatSearchTerm.ShouldBeNullOrEmpty(),
            page => page.RatSearchSex.ShouldBe(Sex.Doe),
            page => page.RatSearchResults.ShouldBeEmpty(),
            page => page.ShowRatSearch.ShouldBeTrue());
    }

    [Fact]
    public void Open_sire_search_clears_existing_search_props_and_shows_search()
    {
        this.Page.RatSearchTerm = this.faker.Lorem.Sentence();
        this.Page.RatSearchSex = Sex.Doe;
        this.Page.RatSearchResults.Add(new RatSearchResult(this.faker.Id(), null, null, null));
        this.Page.ShowRatSearch = false;
        
        this.Page.OpenSireSearch();
        
        this.Page.ShouldSatisfyAllConditions(
            page => page.RatSearchTerm.ShouldBeNullOrEmpty(),
            page => page.RatSearchSex.ShouldBe(Sex.Buck),
            page => page.RatSearchResults.ShouldBeEmpty(),
            page => page.ShowRatSearch.ShouldBeTrue());
    }

    [Fact]
    public void Open_offspring_search_clears_existing_search_props_and_shows_search()
    {
        this.Page.RatSearchTerm = this.faker.Lorem.Sentence();
        this.Page.RatSearchSex = Sex.Doe;
        this.Page.RatSearchResults.Add(new RatSearchResult(this.faker.Id(), null, null, null));
        this.Page.ShowRatSearch = false;
        
        this.Page.OpenOffspringSearch();
        
        this.Page.ShouldSatisfyAllConditions(
            page => page.RatSearchTerm.ShouldBeNullOrEmpty(),
            page => page.RatSearchSex.ShouldBeNull(),
            page => page.RatSearchResults.ShouldBeEmpty(),
            page => page.ShowRatSearch.ShouldBeTrue());
    }

    [Fact]
    public async Task Can_search_for_dam()
    {
        var doe = this.faker.Rat(id: this.faker.Id(), sex: Sex.Doe);
        var otherDoe = this.faker.Rat(id: this.faker.Id(), sex: Sex.Doe);
        // TODO add buck with same name once sex filter exists
        this.backplane.Seed(doe); // TODO replace with ratrepository.Seed
        this.backplane.Seed(otherDoe); // TODO replace with ratrepository.Seed
        this.littersRepository.Seed(this.faker.BlankLitter(this.Page.Id));
        
        this.Page.OpenDamSearch();
        this.Page.RatSearchTerm = doe.Name!;

        await this.Page.Search();
        
        this.Page.RatSearchResults
            .ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(
                result => result.Id.ShouldBe(doe.Id),
                result => result.Name.ShouldBe(doe.Name),
                result => result.Sex.ShouldBe(doe.Sex),
                result => result.DateOfBirth.ShouldBe(doe.DateOfBirth));
    }

    [Fact]
    public async Task Setting_result_of_dam_search_sets_dam_and_closes_search()
    {
        var doe = this.faker.Rat(id: this.faker.Id(), sex: Sex.Doe);
        this.backplane.Seed(doe); // TODO replace with ratrepository.Seed
        this.littersRepository.Seed(this.faker.BlankLitter(this.Page.Id));
        await RazorEngine.InvokeOnInitializedAsync(this.Page);
        
        this.Page.OpenDamSearch();
        this.Page.RatSearchTerm = doe.Name!;

        await this.Page.Search();
        await this.Page.SetResult(this.Page.RatSearchResults.ShouldHaveSingleItem());
        
        this.littersRepository.Litters[this.Page.Id].DamId.ShouldBe(doe.Id);
        this.Page.ShowRatSearch.ShouldBeFalse();
    }
    
    [Fact]
    public async Task Can_search_for_sire()
    {
        var buck = this.faker.Rat(id: this.faker.Id(), sex: Sex.Buck);
        var otherBuck = this.faker.Rat(id: this.faker.Id(), sex: Sex.Buck);
        // TODO add doe with same name once sex filter exists
        this.backplane.Seed(buck); // TODO replace with ratrepository.Seed
        this.backplane.Seed(otherBuck); // TODO replace with ratrepository.Seed
        this.littersRepository.Seed(this.faker.BlankLitter(this.Page.Id));
        
        this.Page.OpenSireSearch();
        this.Page.RatSearchTerm = buck.Name!;

        await this.Page.Search();
        
        this.Page.RatSearchResults
            .ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(
                result => result.Id.ShouldBe(buck.Id),
                result => result.Name.ShouldBe(buck.Name),
                result => result.Sex.ShouldBe(buck.Sex),
                result => result.DateOfBirth.ShouldBe(buck.DateOfBirth));
    }

    [Fact]
    public async Task Setting_result_of_sire_search_sets_dam_and_closes_search()
    {
        var buck = this.faker.Rat(id: this.faker.Id(), sex: Sex.Buck);
        this.backplane.Seed(buck); // TODO replace with ratrepository.Seed
        this.littersRepository.Seed(this.faker.BlankLitter(this.Page.Id));
        await RazorEngine.InvokeOnInitializedAsync(this.Page);
        
        this.Page.OpenSireSearch();
        this.Page.RatSearchTerm = buck.Name!;

        await this.Page.Search();
        await this.Page.SetResult(this.Page.RatSearchResults.ShouldHaveSingleItem());
        
        this.littersRepository.Litters[this.Page.Id].SireId.ShouldBe(buck.Id);
        this.Page.ShowRatSearch.ShouldBeFalse();
    }
    
    [Fact]
    public async Task Can_search_for_offspring()
    {
        var rat = this.faker.Rat(id: this.faker.Id());
        var otherRat = this.faker.Rat(id: this.faker.Id());
        this.backplane.Seed(rat); // TODO replace with ratrepository.Seed
        this.backplane.Seed(otherRat); // TODO replace with ratrepository.Seed
        this.littersRepository.Seed(this.faker.BlankLitter(this.Page.Id));
        
        this.Page.OpenOffspringSearch();
        this.Page.RatSearchTerm = rat.Name!;

        await this.Page.Search();
        
        this.Page.RatSearchResults
            .ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(
                result => result.Id.ShouldBe(rat.Id),
                result => result.Name.ShouldBe(rat.Name),
                result => result.Sex.ShouldBe(rat.Sex),
                result => result.DateOfBirth.ShouldBe(rat.DateOfBirth));
    }

    [Fact]
    public async Task Setting_result_of_offspring_search_sets_offpsring_and_closes_search()
    {
        var rat = this.faker.Rat(id: this.faker.Id());
        this.backplane.Seed(rat); // TODO replace with ratrepository.Seed
        this.littersRepository.Seed(this.faker.BlankLitter(this.Page.Id));
        await RazorEngine.InvokeOnInitializedAsync(this.Page);
        
        this.Page.OpenOffspringSearch();
        this.Page.RatSearchTerm = rat.Name!;

        await this.Page.Search();
        await this.Page.SetResult(this.Page.RatSearchResults.ShouldHaveSingleItem());
        
        this.littersRepository.Litters[this.Page.Id].Offspring
            .ShouldHaveSingleItem().Id.ShouldBe(rat.Id);
        this.Page.ShowRatSearch.ShouldBeFalse();
    }
}