using Basemix.Lib.Persistence;
using Basemix.Lib.Rats;
using Basemix.Lib.Rats.Persistence;
using Basemix.Tests.sdk;
using Bogus;
using Dapper;
using Shouldly;

namespace Basemix.Tests.Integration;

public class SqliteRatRepositoryTests : SqliteIntegration
{
    private readonly Faker faker = new();
    private readonly SqliteFixture fixture;
    private readonly SqliteRatsRepository repository;

    public SqliteRatRepositoryTests(SqliteFixture fixture) : base(fixture)
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
            Notes = this.faker.Lorem.Paragraphs(),
            DateOfDeath = this.faker.Date.RecentDateOnly(),
            Owned = this.faker.Random.Bool()
        };

        await this.repository.UpdateRat(rat);

        var storedRat = await this.repository.GetRat(rat.Id);
        
        storedRat.ShouldNotBeNull().ShouldSatisfyAllConditions(
            () => storedRat.Id.ShouldBe(rat.Id),
            () => storedRat.Name.ShouldBe(rat.Name),
            () => storedRat.DateOfBirth.ShouldBe(rat.DateOfBirth),
            () => storedRat.Sex.ShouldBe(rat.Sex),
            () => storedRat.Variety.ShouldBe(rat.Variety),
            () => storedRat.Notes.ShouldBe(rat.Notes),
            () => storedRat.DateOfDeath.ShouldBe(rat.DateOfDeath),
            () => storedRat.Owned.ShouldBe(rat.Owned));
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
    
    [Fact]
    public async Task Search_deceased_only_returns_deceased_rats()
    {
        var expectedRat = await Rat.Create(this.repository);
        var otherRat = await Rat.Create(this.repository);

        expectedRat.DateOfDeath = this.faker.Date.RecentDateOnly();
        otherRat.DateOfDeath = null;

        await this.repository.UpdateRat(expectedRat);
        await this.repository.UpdateRat(otherRat);

        var results = await this.repository.SearchRat(deceased: true);
        
        results.ShouldSatisfyAllConditions(
            () => results.ShouldContain(rat => rat.Id == expectedRat.Id),
            () => results.ShouldNotContain(rat => rat.Id == otherRat.Id));

    }

    [Fact]
    public async Task Search_deceased_false_only_returns_rats_without_date_of_death()
    {
        var expectedRat = await Rat.Create(this.repository);
        var otherRat = await Rat.Create(this.repository);

        expectedRat.DateOfDeath = null;
        otherRat.DateOfDeath = this.faker.Date.RecentDateOnly();

        await this.repository.UpdateRat(expectedRat);
        await this.repository.UpdateRat(otherRat);

        var results = await this.repository.SearchRat(deceased: false);
        
        results.ShouldSatisfyAllConditions(
            () => results.ShouldContain(rat => rat.Id == expectedRat.Id),
            () => results.ShouldNotContain(rat => rat.Id == otherRat.Id));
    }

    [Fact]
    public async Task Search_deceased_null_returns_rats_with_or_without_date_of_death()
    {
        var expectedRat = await Rat.Create(this.repository);
        var otherRat = await Rat.Create(this.repository);

        expectedRat.DateOfDeath = null;
        otherRat.DateOfDeath = this.faker.Date.RecentDateOnly();

        await this.repository.UpdateRat(expectedRat);
        await this.repository.UpdateRat(otherRat);

        var results = await this.repository.SearchRat(deceased: null);
        
        results.ShouldSatisfyAllConditions(
            () => results.ShouldContain(rat => rat.Id == expectedRat.Id),
            () => results.ShouldContain(rat => rat.Id == otherRat.Id));
    }
    
    [Fact]
    public async Task Search_owned_only_returns_owned_rats()
    {
        var expectedRat = await Rat.Create(this.repository);
        var otherRat = await Rat.Create(this.repository);

        expectedRat.Owned = true;
        otherRat.Owned = false;

        await this.repository.UpdateRat(expectedRat);
        await this.repository.UpdateRat(otherRat);

        var results = await this.repository.SearchRat(owned: true);
        
        results.ShouldSatisfyAllConditions(
            () => results.ShouldContain(rat => rat.Id == expectedRat.Id),
            () => results.ShouldNotContain(rat => rat.Id == otherRat.Id));

    }
    
    [Fact]
    public async Task Search_owned_false_only_returns_unowned_rats()
    {
        var expectedRat = await Rat.Create(this.repository);
        var otherRat = await Rat.Create(this.repository);

        expectedRat.Owned = false;
        otherRat.Owned = true;

        await this.repository.UpdateRat(expectedRat);
        await this.repository.UpdateRat(otherRat);

        var results = await this.repository.SearchRat(owned: false);
        
        results.ShouldSatisfyAllConditions(
            () => results.ShouldContain(rat => rat.Id == expectedRat.Id),
            () => results.ShouldNotContain(rat => rat.Id == otherRat.Id));
    }
    
    [Fact]
    public async Task Search_owned_null_returns_owned_and_unowned_rats()
    {
        var expectedRat = await Rat.Create(this.repository);
        var otherRat = await Rat.Create(this.repository);

        expectedRat.Owned = false;
        otherRat.Owned = true;

        await this.repository.UpdateRat(expectedRat);
        await this.repository.UpdateRat(otherRat);

        var results = await this.repository.SearchRat(owned: null);
        
        results.ShouldSatisfyAllConditions(
            () => results.ShouldContain(rat => rat.Id == expectedRat.Id),
            () => results.ShouldContain(rat => rat.Id == otherRat.Id));
    }

    [Theory]
    [InlineData(Sex.Buck)]
    [InlineData(Sex.Doe)]
    public async Task Search_sex_returns_matching_sex(Sex sex)
    {
        var expectedRat = await Rat.Create(this.repository);
        var otherRat = await Rat.Create(this.repository);

        expectedRat.Sex = sex;
        otherRat.Sex = this.faker.PickRandom(this.faker.PickNonDefault(except: sex), (Sex?)null);

        await this.repository.UpdateRat(expectedRat);
        await this.repository.UpdateRat(otherRat);

        var results = await this.repository.SearchRat(sex: sex);
        
        results.ShouldSatisfyAllConditions(
            () => results.ShouldContain(rat => rat.Id == expectedRat.Id),
            () => results.ShouldNotContain(rat => rat.Id == otherRat.Id));
    }

    [Fact]
    public async Task Search_sex_null_returns_matching_sex()
    {
        var buck = await Rat.Create(this.repository);
        var doe = await Rat.Create(this.repository);
        var unset = await Rat.Create(this.repository);

        buck.Sex = Sex.Buck;
        doe.Sex = Sex.Doe;

        await this.repository.UpdateRat(buck);
        await this.repository.UpdateRat(doe);

        var results = await this.repository.SearchRat(sex: null);
        
        results.ShouldSatisfyAllConditions(
            () => results.ShouldContain(rat => rat.Id == buck.Id),
            () => results.ShouldContain(rat => rat.Id == doe.Id),
            () => results.ShouldContain(rat => rat.Id == unset.Id));
    }
    
    // ReSharper disable InconsistentNaming
    private record RatRow(long id, string? name, string? sex, string? variety, long? date_of_birth, string? notes,
        long? litter_id, long? date_of_death, string? death_reason, long owned);
    // ReSharper restore InconsistentNaming
}
