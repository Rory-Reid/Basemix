using Basemix.Persistence;
using Basemix.Rats;
using Basemix.Rats.Persistence;
using Basemix.Tests.sdk;
using Bogus;
using Dapper;
using Shouldly;

namespace Basemix.Tests.Integration;

public class RatRepositoryTests : SqliteIntegration
{
    private readonly Faker faker = new();
    private readonly SqliteFixture fixture;
    private readonly SqliteRatsRepository repository;

    public RatRepositoryTests(SqliteFixture fixture) : base(fixture)
    {
        this.fixture = fixture;
        this.repository = new SqliteRatsRepository(fixture.GetConnection);
    }
    
    [Fact]
    public async Task Can_add_and_get_rat()
    {
        var rat = this.faker.Rat();

        var id = await this.repository.AddRat(rat);
        var storedRat = await this.repository.GetRat(id);
        
        storedRat.ShouldNotBeNull().ShouldSatisfyAllConditions(
            () => storedRat.Id.Value.ShouldBe(id),
            () => storedRat.Name.ShouldBe(rat.Name),
            () => storedRat.DateOfBirth.ShouldBe(rat.DateOfBirth),
            () => storedRat.Sex.ShouldBe(rat.Sex));
    }

    [Fact]
    public async Task Can_get_all_rats()
    {
        var rats = this.faker.Make(100, () => this.faker.Rat());

        var tasks = rats.Select(x => this.repository.AddRat(x));
        var ids = await Task.WhenAll(tasks);

        var allRats = await this.repository.GetAll();
        allRats.Count.ShouldBeGreaterThanOrEqualTo(rats.Count);
        foreach (var id in ids)
        {
            allRats.Any(x => x.Id == id).ShouldBeTrue();
        }
    }

    [Fact]
    public async Task Returns_null_when_rat_not_found()
    {
        var rat = await this.repository.GetRat(long.MaxValue);
        rat.ShouldBeNull();
    }

    [Fact]
    public async Task Create_rat_creates_rat_with_only_id()
    {
        var id = await this.repository.CreateRat();

        using var db = this.fixture.GetConnection();
        var rat = await db.QuerySingleAsync<RatRow>(
            @"SELECT * FROM rat WHERE id=@Id", new {Id = id});
        
        rat.ShouldSatisfyAllConditions(
            () => rat.id.ShouldBe(id),
            () => rat.name.ShouldBeNull(),
            () => rat.date_of_birth.ShouldBeNull(),
            () => rat.sex.ShouldBeNull(),
            () => rat.notes.ShouldBeNull());
    }

    [Fact]
    public async Task Update_rat_updates()
    {
        var id = await this.repository.CreateRat();
        var rat = new Rat(id,
            this.faker.Person.FirstName,
            this.faker.PickNonDefault<Sex>(),
            this.faker.Variety(),
            this.faker.Date.PastDateOnly())
        {
            Notes = this.faker.Lorem.Paragraphs()
        };

        await this.repository.UpdateRat(rat);
        
        using var db = this.fixture.GetConnection();
        var storedRat = await db.QuerySingleAsync<RatRow>(
            @"SELECT * FROM rat WHERE id=@Id", new {Id = id});
        
        storedRat.ShouldSatisfyAllConditions(
            () => storedRat.id.ShouldBe(id),
            () => storedRat.name.ShouldBe(rat.Name),
            () => storedRat.date_of_birth!.ShouldBe(rat.DateOfBirth?.ToPersistedDateTime()),
            () => storedRat.sex.ShouldBe(rat.Sex.ToString()),
            () => storedRat.variety.ShouldBe(rat.Variety),
            () => storedRat.notes.ShouldBe(rat.Notes));
    }

    [Fact]
    public async Task Delete_rat_deletes()
    {
        var id = await this.repository.CreateRat();

        await this.repository.DeleteRat(id);

        using var db = this.fixture.GetConnection();
        var rat = await db.QuerySingleOrDefaultAsync<RatRow>(
            @"SELECT * FROM rat WHERE id=@Id", new {Id = id});
        
        rat.ShouldBeNull();
    }

    [Fact]
    public async Task Search_rat_returns_rats_matching_term()
    {
        var expectedRat1 = await Rat.Create(this.repository);
        var expectedRat2 = await Rat.Create(this.repository);
        var otherRat = await Rat.Create(this.repository);

        expectedRat1.Name = this.faker.Random.Hash();
        expectedRat2.Name = expectedRat1.Name;
        otherRat.Name = this.faker.Random.Hash();

        await this.repository.UpdateRat(expectedRat1);
        await this.repository.UpdateRat(expectedRat2);
        await this.repository.UpdateRat(otherRat);

        var results = await this.repository.SearchRat(expectedRat1.Name);
        results.ShouldSatisfyAllConditions(
            () => results.Count.ShouldBe(2),
            () => results.ShouldContain(r => r.Id == expectedRat1.Id),
            () => results.ShouldContain(r => r.Id == expectedRat2.Id));
    }

    [Fact]
    public async Task Search_returns_rat_matching_start_of_search_term()
    {
        var rat = await Rat.Create(this.repository);
        rat.Name = this.faker.Random.Hash(40);
        await this.repository.UpdateRat(rat);

        var results = await this.repository.SearchRat(rat.Name[..10]);
        results.ShouldHaveSingleItem().Id.ShouldBe(rat.Id);
    }

    [Fact]
    public async Task Search_returns_rat_matching_end_of_search_term()
    {
        var rat = await Rat.Create(this.repository);
        rat.Name = this.faker.Random.Hash(40);
        await this.repository.UpdateRat(rat);

        var results = await this.repository.SearchRat(rat.Name[29..]);
        results.ShouldHaveSingleItem().Id.ShouldBe(rat.Id);
    }

    [Fact]
    public async Task Search_returns_rat_matching_part_of_search_term()
    {
        var rat = await Rat.Create(this.repository);
        rat.Name = this.faker.Random.Hash(40);
        await this.repository.UpdateRat(rat);

        var results = await this.repository.SearchRat(rat.Name.Substring(10, 10));
        results.ShouldHaveSingleItem().Id.ShouldBe(rat.Id);
    }

    [Fact]
    public async Task Search_is_case_insensitive()
    {
        var rat = await Rat.Create(this.repository);
        rat.Name = this.faker.Random.Hash().ToLower();
        await this.repository.UpdateRat(rat);

        var results = await this.repository.SearchRat(rat.Name.ToUpper());
        results.ShouldHaveSingleItem().Id.ShouldBe(rat.Id);
    }

    // ReSharper disable InconsistentNaming
    private record RatRow(long id, string? name, string? sex, string? variety, long? date_of_birth, string? notes);
    // ReSharper restore InconsistentNaming
}
