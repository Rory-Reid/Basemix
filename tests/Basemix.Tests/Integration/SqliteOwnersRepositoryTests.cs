using Basemix.Lib.Owners;
using Basemix.Lib.Owners.Persistence;
using Basemix.Tests.sdk;
using Bogus;
using Dapper;
using Shouldly;

namespace Basemix.Tests.Integration;

public class SqliteOwnersRepositoryTests : SqliteIntegration
{
    private readonly Faker faker = new();
    private readonly SqliteFixture fixture;
    private readonly SqliteOwnersRepository repository;

    public SqliteOwnersRepositoryTests(SqliteFixture fixture) : base(fixture)
    {
        this.fixture = fixture;
        this.repository = new SqliteOwnersRepository(fixture.GetConnection);
    }

    [Fact]
    public async Task Create_owner_creates_new_record()
    {
        var id = await this.repository.CreateOwner();
        
        using var db = this.fixture.GetConnection();
        var owner = await db.QuerySingleAsync<OwnerRow>(
            @"SELECT * FROM owner WHERE id=@Id", new {Id = id.Value});
        
        owner.ShouldSatisfyAllConditions(
            () => owner.id.ShouldBe(id.Value),
            () => owner.name.ShouldBeNull(),
            () => owner.email.ShouldBeNull(),
            () => owner.phone.ShouldBeNull(),
            () => owner.notes.ShouldBeNull());
    }

    [Fact]
    public async Task Update_owner_updates()
    {
        var id = await this.repository.CreateOwner();
        var updatedOwner = new Owner(id)
        {
            Name = this.faker.Person.FullName,
            Email = this.faker.Person.Email,
            Phone = this.faker.Person.Phone,
            Notes = this.faker.Lorem.Paragraphs()
        };
        await this.repository.UpdateOwner(updatedOwner);
        
        using var db = this.fixture.GetConnection();
        var owner = await db.QuerySingleAsync<OwnerRow>(
            @"SELECT * FROM owner WHERE id=@Id", new {Id = id.Value});
        
        owner.ShouldSatisfyAllConditions(
            () => owner.name.ShouldBe(updatedOwner.Name),
            () => owner.email.ShouldBe(updatedOwner.Email),
            () => owner.phone.ShouldBe(updatedOwner.Phone),
            () => owner.notes.ShouldBe(updatedOwner.Notes));
    }

    [Fact]
    public async Task Get_owner_gets_details()
    {
        var owner = new Owner(await this.repository.CreateOwner())
        {
            Name = this.faker.Person.FullName,
            Email = this.faker.Person.Email,
            Phone = this.faker.Person.Phone,
            Notes = this.faker.Lorem.Paragraphs()
        };
        await this.repository.UpdateOwner(owner);
        
        var result = await this.repository.GetOwner(owner.Id);
        result.ShouldBeEquivalentTo(owner);
    }
    
    [Fact]
    public async Task Get_owner_gets_rats()
    {
        var owner = await this.fixture.Seed(this.faker.Owner());
        var rat = await this.fixture.Seed(this.faker.Rat(), owner);
        
        var result = await this.repository.GetOwner(owner.Id);
        result.ShouldNotBeNull().Rats.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            ownedRat => ownedRat.Id.ShouldBe(rat.Id),
            ownedRat => ownedRat.Name.ShouldBe(rat.Name));
    }

    [Fact]
    public async Task Delete_owner_deletes_and_removes_associations_from_any_rats()
    {
        var owner = new Owner(await this.repository.CreateOwner());
        await this.fixture.Seed(this.faker.Rat(), owner);
        
        await this.repository.DeleteOwner(owner.Id);

        using var db = this.fixture.GetConnection();
        var ownedRats = await db.QueryAsync<long>(
            @"SELECT rat.id FROM rat WHERE owner_id=@OwnerId", new {OwnerId = owner.Id.Value});
        var deletedOwner = await db.QuerySingleOrDefaultAsync<OwnerRow>(
            @"SELECT * FROM owner WHERE id=@Id", new {Id = owner.Id.Value});
        
        ownedRats.ShouldBeEmpty();
        deletedOwner.ShouldBeNull();
    }
    
    [Fact]
    public async Task Search_owner_returns_owners_matching_term()
    {
        var expectedOwner1 = await Owner.Create(this.repository);
        var expectedOwner2 = await Owner.Create(this.repository);
        var otherOwner = await Owner.Create(this.repository);

        expectedOwner1.Name = this.faker.Random.Hash();
        expectedOwner2.Name = expectedOwner1.Name;
        otherOwner.Name = this.faker.Random.Hash();

        await this.repository.UpdateOwner(expectedOwner1);
        await this.repository.UpdateOwner(expectedOwner2);
        await this.repository.UpdateOwner(otherOwner);

        var results = await this.repository.SearchOwner(expectedOwner1.Name);
        results.ShouldSatisfyAllConditions(
            () => results.Count.ShouldBe(2),
            () => results.ShouldContain(r => r.Id == expectedOwner1.Id),
            () => results.ShouldContain(r => r.Id == expectedOwner2.Id));
    }

    [Fact]
    public async Task Search_returns_owner_matching_start_of_term()
    {
        var owner = await Owner.Create(this.repository);
        owner.Name = this.faker.Random.Hash(40);
        await this.repository.UpdateOwner(owner);
        
        var results = await this.repository.SearchOwner(owner.Name[..10]);
        results.ShouldHaveSingleItem().Id.ShouldBe(owner.Id);
    }

    [Fact]
    public async Task Search_returns_owner_matching_end_of_term()
    {
        var owner = await Owner.Create(this.repository);
        owner.Name = this.faker.Random.Hash(40);
        await this.repository.UpdateOwner(owner);
        
        var results = await this.repository.SearchOwner(owner.Name[29..]);
        results.ShouldHaveSingleItem().Id.ShouldBe(owner.Id);
    }

    [Fact]
    public async Task Search_returns_owner_matching_part_of_term()
    {
        var owner = await Owner.Create(this.repository);
        owner.Name = this.faker.Random.Hash(40);
        await this.repository.UpdateOwner(owner);
        
        var results = await this.repository.SearchOwner(owner.Name.Substring(10, 10));
        results.ShouldHaveSingleItem().Id.ShouldBe(owner.Id);
    }

    // ReSharper disable InconsistentNaming
    private record OwnerRow(long id, string? name, string? email, string? phone, string? notes);
    // ReSharper restore InconsistentNaming
}