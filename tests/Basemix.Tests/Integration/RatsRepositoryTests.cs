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
    public async Task Can_set_and_get_rat()
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
            () => storedRat.notes.ShouldBe(rat.Notes));
    }

    // ReSharper disable InconsistentNaming
    private record RatRow(long id, string? name, string? sex, long? date_of_birth, string? notes);
    // ReSharper restore InconsistentNaming
}
