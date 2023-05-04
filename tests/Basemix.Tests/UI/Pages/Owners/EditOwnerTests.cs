using System.Diagnostics.CodeAnalysis;
using Basemix.Pages;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.UI.Pages.Owners;

public class EditOwnerTests : RazorPageTests<EditOwner>
{
    private readonly Faker faker = new();
    private readonly MemoryPersistenceBackplane backplane = new();
    private readonly TestNavigationManager nav = new();
    private MemoryOwnersRepository ownersRepository = null!;
    
    [SuppressMessage("Usage", "BL0005:Component parameter should not be set outside of its component.")]
    protected override EditOwner CreatePage()
    {
        this.ownersRepository = new MemoryOwnersRepository(this.backplane);
        return new()
        {
            Id = this.faker.Id(),
            Repository = this.ownersRepository,
            Nav = this.nav,
            JsRuntime = new NullJsRuntime()
        };
    }

    [Fact]
    public async Task Loads_owner_on_parameters_set()
    {
        var owner = this.faker.Owner(id: this.Page.Id);
        this.ownersRepository.Seed(owner);
        
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        
        this.Page.Owner.ShouldBeEquivalentTo(owner);
    }

    [Fact]
    public void OwnerLoaded_is_false_if_owner_does_not_exist()
    {
        this.Page.OwnerLoaded.ShouldBeFalse();
    }

    [Fact]
    public async Task OwnerLoaded_is_true_if_owner_exists()
    {
        this.ownersRepository.Seed(this.faker.Owner(id: this.Page.Id));
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        
        this.Page.OwnerLoaded.ShouldBeTrue();
    }

    [Fact]
    public async Task Save_saves_owner()
    {
        this.ownersRepository.Seed(this.faker.Owner(id: this.Page.Id));
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);

        var name = this.faker.Person.FullName;
        this.Page.Owner.Name = name;
        var email = this.faker.Person.Email;
        this.Page.Owner.Email = email;
        var phone = this.faker.Person.Phone;
        this.Page.Owner.Phone = phone;
        var notes = this.faker.Lorem.Paragraphs();
        this.Page.Owner.Notes = notes;
        
        await this.Page.SaveAndGoBack();
        
        this.ownersRepository.Owners[this.Page.Id].ShouldSatisfyAllConditions(
            owner => owner.Name.ShouldBe(name),
            owner => owner.Email.ShouldBe(email),
            owner => owner.Phone.ShouldBe(phone),
            owner => owner.Notes.ShouldBe(notes));
    }

    [Fact]
    public async Task Delete_deletes_owner_and_navigates_to_owners_page()
    {
        this.ownersRepository.Seed(this.faker.Owner(id: this.Page.Id));
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        
        await this.Page.Delete();
        
        this.ownersRepository.Owners.ShouldNotContainKey(this.Page.Id);
        this.nav.CurrentUri.ShouldBe("/owners");
    }
}