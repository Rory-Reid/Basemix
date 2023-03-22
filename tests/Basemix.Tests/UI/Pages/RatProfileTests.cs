using System.Diagnostics.CodeAnalysis;
using Basemix.Lib.Rats;
using Basemix.Pages;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.UI.Pages;

public class RatProfileTests : RazorPageTests<RatProfile>
{
    private readonly Faker faker = new();
    private readonly TestNavigationManager nav = new();
    private MemoryRatsRepository repository = null!;
    private MemoryLittersRepository littersRepository = null!;
    private MemoryPedigreesRepository pedigreesRepository = null!;

    [SuppressMessage("Usage", "BL0005:Component parameter should not be set outside of its component.")]
    protected override RatProfile CreatePage()
    {
        var backplane = new MemoryPersistenceBackplane();
        this.repository = new MemoryRatsRepository(backplane);
        this.littersRepository = new MemoryLittersRepository(backplane);
        this.pedigreesRepository = new MemoryPedigreesRepository(backplane);
        return new()
        {
            Id = this.faker.Id(),
            Repository = this.repository,
            LittersRepository = this.littersRepository,
            PedigreeRepository = this.pedigreesRepository,
            Nav = this.nav
        };
    }

    [Fact]
    public async Task Loads_rat_from_repository_on_parameters_set()
    {
        var rat = this.faker.Rat(id: this.Page.Id);
        this.repository.Rats[this.Page.Id] = rat;

        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        
        this.Page.Rat.ShouldBe(rat);
    }

    [Fact]
    public async Task New_litter_button_creates_new_litter_sets_dam_and_navigates_to_edit_litter()
    {
        var rat = this.faker.Rat(id: this.Page.Id, sex: Sex.Doe);
        this.repository.Rats[this.Page.Id] = rat;
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);

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
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);

        await this.Page.NewLitter();
        
        var litter = this.littersRepository.Litters.ShouldHaveSingleItem().Value;
        litter.ShouldSatisfyAllConditions(
            () => litter.SireId.ShouldBe(rat.Id),
            () => litter.SireName.ShouldBe(rat.Name));
        this.nav.CurrentUri.ShouldBe($"/litters/{litter.Id.Value}/edit");
    }

    [Fact]
    public void Open_litter_profile_navigates_to_litter()
    {
        var litterId = this.faker.Id();
        
        this.Page.OpenLitterProfile(litterId);
        
        this.nav.CurrentUri.ShouldBe($"/litters/{litterId}");
    }
}