using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.UI.Pages;

public class LittersTests : RazorPageTests<Basemix.Pages.Litters>
{
    private readonly Faker faker = new();
    private readonly MemoryLittersRepository repository = new();
    private readonly TestNavigationManager nav = new();

    protected override Basemix.Pages.Litters CreatePage() =>
        new()
        {
            Repository = this.repository,
            Nav = this.nav
        };

    [Fact]
    public async Task Loads_litters_from_repo_on_parameters_set()
    {
        this.faker.Make(100, () => this.repository.CreateLitter());

        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        
        this.Page.LitterList.Count.ShouldBe(100);
    }

    [Fact]
    public void Open_litter_profile_navigates_to_litter()
    {
        var litterId = this.faker.Id();
        
        this.Page.OpenLitterProfile(litterId);
        
        this.nav.CurrentUri.ShouldBe($"/litters/{litterId}");
    }

    [Fact]
    public async Task Create_litter_creates_and_navigates_to_litter()
    {
        await this.Page.CreateLitter();

        var litter = this.repository.Litters.ShouldHaveSingleItem().Value;
        this.nav.CurrentUri.ShouldBe($"/litters/{litter.Id.Value}/edit");
    }
}