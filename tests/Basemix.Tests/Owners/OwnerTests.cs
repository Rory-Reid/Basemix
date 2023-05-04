using Basemix.Lib.Owners;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.Owners;

public class OwnerTests
{
    private readonly Faker faker = new();
    private readonly MemoryOwnersRepository ownersRepository;

    public OwnerTests()
    {
        var backplane = new MemoryPersistenceBackplane();
        this.ownersRepository = new MemoryOwnersRepository(backplane);
    }
    
    [Fact]
    public void Constructs_anonymous_owner()
    {
        var owner = new Owner();
        
        owner.ShouldSatisfyAllConditions(
            () => owner.Id.ShouldNotBeNull(),
            () => owner.Name.ShouldBeNull(),
            () => owner.Email.ShouldBeNull(),
            () => owner.Phone.ShouldBeNull(),
            () => owner.Notes.ShouldBeNull());
    }

    [Fact]
    public async Task Create_saves_and_returns_owner_with_id()
    {
        var owner = await Owner.Create(this.ownersRepository);
        
        owner.Id.Value.ShouldBePositive();
        
        this.ownersRepository.Owners.ShouldContainKey(owner.Id);
    }

    [Fact]
    public async Task Properties_do_not_persist_on_set()
    {
        var owner = await Owner.Create(this.ownersRepository);
        owner.Name = this.faker.Person.FullName;
        owner.Email = this.faker.Person.Email;
        owner.Phone = this.faker.Person.Phone;
        owner.Notes = this.faker.Lorem.Paragraphs();
        
        (await this.ownersRepository.GetOwner(owner.Id)).ShouldNotBeNull().ShouldSatisfyAllConditions(
            storedOwner => storedOwner.Name.ShouldBeNull(),
            storedOwner => storedOwner.Email.ShouldBeNull(),
            storedOwner => storedOwner.Phone.ShouldBeNull(),
            storedOwner => storedOwner.Notes.ShouldBeNull());
    }

    [Fact]
    public async Task Properties_persist_on_save()
    {
        var owner = await Owner.Create(this.ownersRepository);
        owner.Name = this.faker.Person.FullName;
        owner.Email = this.faker.Person.Email;
        owner.Phone = this.faker.Person.Phone;
        owner.Notes = this.faker.Lorem.Paragraphs();

        await owner.Save(this.ownersRepository);
        
        (await this.ownersRepository.GetOwner(owner.Id)).ShouldNotBeNull().ShouldSatisfyAllConditions(
            storedOwner => storedOwner.Name.ShouldBe(owner.Name),
            storedOwner => storedOwner.Email.ShouldBe(owner.Email),
            storedOwner => storedOwner.Phone.ShouldBe(owner.Phone),
            storedOwner => storedOwner.Notes.ShouldBe(owner.Notes));
    }
}