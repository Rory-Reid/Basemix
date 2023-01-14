using System.Diagnostics.CodeAnalysis;
using Basemix.Rats;
using Basemix.Tests.sdk;
using Basemix.UI.Pages;
using Bogus;
using Shouldly;

namespace Basemix.Tests.UI.Pages;

public class RatProfileTests : RazorPageTests<RatProfile>
{
    private readonly Faker faker = new();
    private readonly TestNavigationManager nav = new();
    private MemoryRatsRepository repository;
    private MemoryLittersRepository littersRepository;

    [SuppressMessage("Usage", "BL0005:Component parameter should not be set outside of its component.")]
    protected override RatProfile CreatePage()
    {
        var backplane = new MemoryPersistenceBackplane();
        this.repository = new MemoryRatsRepository(backplane);
        this.littersRepository = new MemoryLittersRepository(backplane);
        return new()
        {
            Id = this.faker.Id(),
            Repository = this.repository,
            LittersRepository = this.littersRepository,
            JsRuntime = new NullJsRuntime(),
            Nav = this.nav
        };
    }

    [Fact]
    public async Task Loads_rat_from_repository_on_initialisation()
    {
        var rat = this.faker.Rat(id: this.Page.Id);
        this.repository.Rats[this.Page.Id] = rat;

        await RazorEngine.InvokeOnInitializedAsync(this.Page);
        
        this.Page.Rat.ShouldBe(rat);
    }

    [Fact]
    public async Task New_litter_button_creates_new_litter_sets_dam_and_navigates_to_edit_litter()
    {
        var rat = this.faker.Rat(id: this.Page.Id, sex: Sex.Doe);
        this.repository.Rats[this.Page.Id] = rat;
        await RazorEngine.InvokeOnInitializedAsync(this.Page);

        await this.Page.NewLitter();
        
        var litter = this.littersRepository.Litters.ShouldHaveSingleItem().Value;
        litter.ShouldSatisfyAllConditions(
                () => litter.DamId.ShouldBe(rat.Id),
                () => litter.DamName.ShouldBe(rat.Name));
        this.nav.CurrentUri.ShouldBe($"/litters/{litter.Id.Value}/edit");
    }

    [Fact]
    public async Task New_litter_button_creates_new_litter_sets_sire_and_navigates_to_edit_litter()
    {
        var rat = this.faker.Rat(id: this.Page.Id, sex: Sex.Buck);
        this.repository.Rats[this.Page.Id] = rat;
        await RazorEngine.InvokeOnInitializedAsync(this.Page);

        await this.Page.NewLitter();
        
        var litter = this.littersRepository.Litters.ShouldHaveSingleItem().Value;
        litter.ShouldSatisfyAllConditions(
            () => litter.SireId.ShouldBe(rat.Id),
            () => litter.SireName.ShouldBe(rat.Name));
        this.nav.CurrentUri.ShouldBe($"/litters/{litter.Id.Value}/edit");
    }

    [Fact]
    public async Task Open_litter_profile_navigates_to_litter()
    {
        var litterId = this.faker.Id();
        
        this.Page.OpenLitterProfile(litterId);
        
        this.nav.CurrentUri.ShouldBe($"/litters/{litterId}");
    }
}