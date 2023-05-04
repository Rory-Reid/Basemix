using Basemix.Lib.Owners;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.UI.Pages.Owners;

public class OwnersTests : RazorPageTests<Basemix.Pages.Owners>
{
    private readonly Faker faker = new();
    private readonly MemoryOwnersRepository repository = new();
    private readonly TestNavigationManager nav = new();
    
    protected override Basemix.Pages.Owners CreatePage() =>
        new()
        {
            Repository = this.repository,
            Nav = this.nav
        };

    [Fact]
    public async Task Loads_owners_from_repository_into_owner_list_on_parameters_set()
    {
        this.faker.Make(100, () => this.repository.CreateOwner());

        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        
        this.Page.OwnersList.Count.ShouldBe(100);
    }

    [Fact]
    public async Task Create_owner_button_creates_and_navigates_to_new_owner()
    {
        await this.Page.CreateOwner();
        
        var owner = this.repository.Owners.ShouldHaveSingleItem().Value;
        this.nav.CurrentUri.ShouldBe($"/owners/{owner.Id.Value}/edit");
    }

    [Fact]
    public void Open_owner_profile_button_navigates_to_owner()
    {
        var ownerId = this.faker.Id();
        
        this.Page.OpenOwnerProfile(ownerId);
        
        this.nav.CurrentUri.ShouldBe($"/owners/{ownerId}");
    }
    
    [Fact]
    public async Task Search_button_returns_all_owners_if_search_term_empty()
    {
        this.faker.Make(100, () => this.repository.CreateOwner());

        await this.Page.Search();
        
        this.Page.OwnersList.Count.ShouldBe(100);
    }

    [Fact]
    public async Task Search_button_only_returns_owners_matching_search_term()
    {
        var matchingOwnerId = this.faker.Id();
        var matchingOwner = new Owner(matchingOwnerId) {Name = this.faker.Random.AlphaNumeric(10)};
        this.repository.Owners.Add(matchingOwnerId, matchingOwner);
        
        this.repository.Owners.Add(this.faker.Id(), new Owner(this.faker.Id()) {Name = this.faker.Random.AlphaNumeric(10)});
        
        this.Page.SearchTerm = matchingOwner.Name;
        await this.Page.Search();

        this.Page.OwnersList
            .ShouldHaveSingleItem()
            .ShouldBeEquivalentTo(new OwnerSearchResult(matchingOwnerId, matchingOwner.Name));
    }
}