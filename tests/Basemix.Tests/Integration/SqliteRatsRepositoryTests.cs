using Basemix.Lib.Owners;
using Basemix.Lib.Owners.Persistence;
using Basemix.Lib.Rats;
using Basemix.Lib.Rats.Persistence;
using Basemix.Lib.Settings.Persistence;
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
        var owned = this.faker.Random.Bool();
        var rat = new Rat(id,
            this.faker.Person.FirstName,
            this.faker.PickNonDefault<Sex>(),
            this.faker.Variety(),
            this.faker.Date.PastDateOnly(),
            ownerId: owned 
                ? null
                : (await Owner.Create(new SqliteOwnersRepository(this.fixture.GetConnection))).Id)
        {
            Notes = this.faker.Lorem.Paragraphs(),
            Owned = owned,
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
            () => storedRat.Owned.ShouldBe(rat.Owned),
            () => storedRat.OwnerId.ShouldBe(rat.OwnerId));
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
    public async Task Search_deceased_only_returns_dead_rats()
    {
        var expectedRat = await Rat.Create(this.repository);
        var otherRat = await Rat.Create(this.repository);

        expectedRat.Dead = true;
        otherRat.Dead = false;

        await this.repository.UpdateRat(expectedRat);
        await this.repository.UpdateRat(otherRat);

        var results = await this.repository.SearchRat(deceased: true);
        
        results.ShouldSatisfyAllConditions(
            () => results.ShouldContain(rat => rat.Id == expectedRat.Id),
            () => results.ShouldNotContain(rat => rat.Id == otherRat.Id));

    }

    [Fact]
    public async Task Search_deceased_false_only_returns_rats_not_marked_as_dead()
    {
        var expectedRat = await Rat.Create(this.repository);
        var otherRat = await Rat.Create(this.repository);

        expectedRat.Dead = false;
        otherRat.Dead = true;

        await this.repository.UpdateRat(expectedRat);
        await this.repository.UpdateRat(otherRat);

        var results = await this.repository.SearchRat(deceased: false);
        
        results.ShouldSatisfyAllConditions(
            () => results.ShouldContain(rat => rat.Id == expectedRat.Id),
            () => results.ShouldNotContain(rat => rat.Id == otherRat.Id));
    }

    [Fact]
    public async Task Search_deceased_null_returns_rats_whether_dead_or_alive()
    {
        var expectedRat = await Rat.Create(this.repository);
        var otherRat = await Rat.Create(this.repository);

        expectedRat.Dead = false;
        otherRat.Dead = true;

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

    [Fact]
    public async Task Get_rat_gets_owner_details()
    {
        var ownerRepository = new SqliteOwnersRepository(this.fixture.GetConnection);
        var owner = await Owner.Create(ownerRepository);
        owner.Name = this.faker.Person.FullName;
        await owner.Save(ownerRepository);
        
        var rat = await Rat.Create(this.repository);
        rat.Owned = false;
        await rat.SetOwner(this.repository, owner);
        
        var result = await this.repository.GetRat(rat.Id);
        result.ShouldNotBeNull().ShouldSatisfyAllConditions(
            () => result.OwnerId.ShouldBe(owner.Id),
            () => result.OwnerName.ShouldBe(owner.Name));
    }

    [Fact]
    public async Task Can_save_and_load_death_reason_if_rat_set_to_dead()
    {
        var deathReason = this.faker.PickRandom(this.fixture.DeathReasons);
        
        var rat = await Rat.Create(this.repository);
        rat.Dead = true;
        rat.DeathReason = new DeathReason(deathReason.Id, deathReason.Reason);
        await rat.Save(this.repository);
        
        var result = await this.repository.GetRat(rat.Id);
        result.ShouldNotBeNull().ShouldSatisfyAllConditions(
            () => result.Dead.ShouldBeTrue(),
            () => result.DeathReason.ShouldBe(rat.DeathReason));
    }

    [Fact]
    public async Task Can_save_and_load_death_date_if_rat_set_to_dead()
    {
        using var db = this.fixture.GetConnection();
        
        var rat = await Rat.Create(this.repository);
        rat.Dead = true;
        rat.DateOfDeath = this.faker.Date.PastDateOnly();
        await rat.Save(this.repository);
        
        var result = await this.repository.GetRat(rat.Id);
        result.ShouldNotBeNull().ShouldSatisfyAllConditions(
            () => result.Dead.ShouldBeTrue(),
            () => result.DateOfDeath.ShouldBe(rat.DateOfDeath));
    }

    [Fact]
    public async Task Cannot_save_death_date_or_reason_if_rat_not_dead()
    {
        var deathReason = this.faker.PickRandom(this.fixture.DeathReasons);
        
        var rat = await Rat.Create(this.repository);
        rat.Dead = false;
        rat.DeathReason = new DeathReason(deathReason.Id, deathReason.Reason);
        rat.DateOfDeath = this.faker.Date.PastDateOnly();
        await rat.Save(this.repository);
        
        var result = await this.repository.GetRat(rat.Id);
        result.ShouldNotBeNull().ShouldSatisfyAllConditions(
            () => result.Dead.ShouldBeFalse(),
            () => result.DeathReason.ShouldBeNull(),
            () => result.DateOfDeath.ShouldBeNull());
    }

    [Fact]
    public async Task Death_date_and_reason_removed_if_rat_unmarked_dead()
    {
        var db = this.fixture.GetConnection();
        var deathReason = this.faker.PickRandom(this.fixture.DeathReasons);
        
        var rat = await Rat.Create(this.repository);
        rat.Dead = true;
        rat.DeathReason = new DeathReason(deathReason.Id, deathReason.Reason);
        rat.DateOfDeath = this.faker.Date.PastDateOnly();
        await rat.Save(this.repository);
        
        rat.Dead = false;
        await rat.Save(this.repository);
        
        var result = await this.repository.GetRat(rat.Id);
        result.ShouldNotBeNull().ShouldSatisfyAllConditions(
            () => result.Dead.ShouldBeFalse(),
            () => result.DeathReason.ShouldBeNull(),
            () => result.DateOfDeath.ShouldBeNull());
    }
    
    // ReSharper disable InconsistentNaming
    private record RatRow(long id, string? name, string? sex, string? variety, long? date_of_birth, string? notes,
        long? litter_id, long? date_of_death, long owned, long owner_id, long dead, long? death_reason_id);
    // ReSharper restore InconsistentNaming
}
