using Basemix.Rats;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.UI.Pages;

public class RatsTests : RazorPageTests<Basemix.UI.Pages.Rats>
{
    private readonly Faker faker = new();
    private readonly MemoryRatsRepository repository = new();
    private readonly TestNavigationManager nav = new();
    
    protected override Basemix.UI.Pages.Rats CreatePage() =>
        new()
        {
            Repository = this.repository,
            Nav = this.nav
        };
    
    [Fact]
    public async Task Loads_rats_from_repo_into_rat_list_on_parameters_set()
    {
        var rat = await Rat.Create(this.repository);
        var otherRat = await Rat.Create(this.repository);
        
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        
        this.Page.RatsList.ShouldBeEquivalentTo(new List<RatSearchResult> {rat.ToSearchResult(), otherRat.ToSearchResult()});
    }

    [Fact]
    public void Open_rat_profile_button_navigates_to_rat()
    {
        var ratId = this.faker.Id();
        
        this.Page.OpenRatProfile(ratId);
        
        this.nav.CurrentUri.ShouldBe($"/rats/{ratId}");
    }

    [Fact]
    public async Task Create_rat_button_creates_and_navigates_to_new_rat()
    {
        await this.Page.CreateRat();

        var ratId = this.repository.Rats.ShouldHaveSingleItem().Key;
        this.nav.CurrentUri.ShouldBe($"/rats/{ratId.Value}/edit");
    }

    [Fact]
    public async Task Search_button_returns_all_rats_if_search_term_empty()
    {
        this.faker.Make(100, () => Rat.Create(this.repository));

        await this.Page.Search();
        
        this.Page.RatsList.Count.ShouldBe(100);
    }

    [Fact]
    public async Task Search_button_only_returns_rats_matching_search_term()
    {
        var matchingRat = this.faker.Rat(name: this.faker.Random.AlphaNumeric(10));
        var matchingRatId = this.faker.Id();
        this.repository.Rats[matchingRatId] = matchingRat;

        this.repository.Rats[this.faker.Id()] = this.faker.Rat(name: this.faker.Random.AlphaNumeric(10));

        this.Page.SearchTerm = matchingRat.Name;
        await this.Page.Search();
        
        this.Page.RatsList
            .ShouldHaveSingleItem()
            .ShouldBeEquivalentTo(matchingRat.ToSearchResult());
    }
}