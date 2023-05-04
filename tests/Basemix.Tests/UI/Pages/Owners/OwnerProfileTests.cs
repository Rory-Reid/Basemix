using System.Diagnostics.CodeAnalysis;
using Basemix.Pages;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.UI.Pages.Owners;

public class OwnerProfileTests : RazorPageTests<OwnerProfile>
{
    private readonly Faker faker = new();
    private readonly TestNavigationManager nav = new();
    private MemoryOwnersRepository repository = null!;
    
    [SuppressMessage("Usage", "BL0005:Component parameter should not be set outside of its component.")]
    protected override OwnerProfile CreatePage()
    {
        var backplane = new MemoryPersistenceBackplane();
        this.repository = new MemoryOwnersRepository(backplane);
        return new()
        {
            Id = this.faker.Id(),
            Repository = this.repository,
            Nav = this.nav
        };
    }

    [Fact]
    public async Task Loads_owner_from_repository_on_parameters_set()
    {
        var owner = this.faker.Owner(id: this.Page.Id);
        this.repository.Seed(owner);
        
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        
        this.Page.Owner.ShouldBeEquivalentTo(owner);
    }

    [Fact]
    public async Task OwnerLoaded_true_when_owner_exists()
    {
        var owner = this.faker.Owner(id: this.Page.Id);
        this.repository.Seed(owner);
        
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        
        this.Page.OwnerLoaded.ShouldBeTrue();
    }

    [Fact]
    public void OwnerLoaded_false_when_owner_doesnt_exist()
    {
        this.Page.OwnerLoaded.ShouldBeFalse();
    }
    
    [Fact]
    public void Edit_button_navigates_to_edit_owner()
    {
        this.Page.Edit();
        
        this.nav.CurrentUri.ShouldBe($"/owners/{this.Page.Id}/edit");
    }
}