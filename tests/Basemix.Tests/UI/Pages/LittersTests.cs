using Basemix.Lib.Litters;
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
            Nav = this.nav,
            Filter = new FilterContext()
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

    [Theory]
    [InlineData("", "", "", "Anonymous litter")]
    [InlineData("My Litter", "", "", "My Litter")]
    [InlineData("My Litter", "Dam Name", "", "My Litter")]
    [InlineData("My Litter", "", "Sire Name", "My Litter")]
    [InlineData("My Litter", "Dam Name", "Sire Name", "My Litter")]
    [InlineData("", "Dam Name", "", "Dam Name's litter")]
    [InlineData("", "", "Sire Name", "Sire Name's litter")]
    [InlineData("", "Dam Name", "Sire Name", "Dam Name & Sire Name's litter")]
    public async Task Litter_name_in_list_is_set_correctly(string litterName, string damName, string sireName,
        string expectedLitterName)
    {
        this.repository.Seed(new Litter(
            dam: (this.faker.Id(), damName),
            sire: (this.faker.Id(), sireName))
        {
            Name = litterName,
            BredByMe = true
        });
        
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        
        this.Page.LitterList.ShouldHaveSingleItem().LitterName.ShouldBe(expectedLitterName);
    }

    [Fact]
    public void Bred_by_me_only_true_by_default() => this.Page.Filter.LittersShowOnlyBredByMe.ShouldBeTrue();

    [Fact]
    public async Task Search_bred_by_me_only_true_returns_bred_litters_only()
    {
        var matchingLitter = this.faker.Litter(bredByMe: true);
        
        this.repository.Litters[matchingLitter.Id] = matchingLitter;
        this.repository.Litters[this.faker.Id()] = this.faker.Litter(bredByMe: false);

        this.Page.Filter.LittersShowOnlyBredByMe = true;
        await this.Page.Search();
        
        this.Page.LitterList
            .ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(
                litter => litter.Id.ShouldBe(matchingLitter.Id),
                litter => litter.DateOfBirth.ShouldBe(matchingLitter.DateOfBirth),
                litter => litter.Name.ShouldBe(matchingLitter.Name),
                litter => litter.OffspringCount.ShouldBe(matchingLitter.Offspring.Count),
                litter => litter.Dam.ShouldBe(matchingLitter.DamName),
                litter => litter.Sire.ShouldBe(matchingLitter.SireName));
    }
    
    [Fact]
    public async Task Search_bred_by_me_only_false_includes_litters_not_bred_by_user()
    {
        var matchingLitter = this.faker.Litter(bredByMe: true);
        
        this.repository.Litters[matchingLitter.Id] = matchingLitter;
        this.repository.Litters[this.faker.Id()] = this.faker.Litter(bredByMe: false);

        this.Page.Filter.LittersShowOnlyBredByMe = false;
        await this.Page.Search();
        
        this.Page.LitterList.Count.ShouldBe(2);
    }
}