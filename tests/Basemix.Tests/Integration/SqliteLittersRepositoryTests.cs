using Basemix.Litters;
using Basemix.Litters.Persistence;
using Basemix.Persistence;
using Basemix.Rats;
using Basemix.Tests.sdk;
using Bogus;
using Dapper;
using Shouldly;

namespace Basemix.Tests.Integration;

public class LittersRepositoryTests : SqliteIntegration
{
    private readonly Faker faker = new();
    private readonly SqliteFixture fixture;
    private readonly LittersRepository repository;
    
    public LittersRepositoryTests(SqliteFixture fixture) : base(fixture)
    {
        this.fixture = fixture;
        this.repository = new LittersRepository(this.fixture.GetConnection);
    }

    [Fact]
    public async Task Add_litter_creates_new_record()
    {
        var id = await this.repository.CreateLitter();
        
        using var db = this.fixture.GetConnection();
        var litter = await db.QuerySingleAsync<LitterRow>(
            @"SELECT * FROM litter WHERE id=@Id", new {Id = id});
        
        litter.ShouldSatisfyAllConditions(
            () => litter.id.ShouldBe(id),
            () => litter.dam_id.ShouldBeNull(),
            () => litter.sire_id.ShouldBeNull(),
            () => litter.date_of_birth.ShouldBeNull());
    }

    [Fact]
    public async Task Add_litter_optionally_sets_dam()
    {
        var rat = this.faker.Rat(sex: Sex.Doe);
        var ratId = await this.fixture.Seed(rat);
        var id = await this.repository.CreateLitter(damId: ratId);
        
        using var db = this.fixture.GetConnection();
        var litter = await db.QuerySingleAsync<LitterRow>(
            @"SELECT * FROM litter WHERE id=@Id", new {Id = id});
        
        litter.ShouldSatisfyAllConditions(
            () => litter.id.ShouldBe(id),
            () => litter.dam_id.ShouldBe(ratId),
            () => litter.sire_id.ShouldBeNull(),
            () => litter.date_of_birth.ShouldBeNull());
    }

    [Fact]
    public async Task Add_litter_optionally_sets_sire()
    {
        var rat = this.faker.Rat(sex: Sex.Buck);
        var ratId = await this.fixture.Seed(rat);
        var id = await this.repository.CreateLitter(sireId: ratId);
        
        using var db = this.fixture.GetConnection();
        var litter = await db.QuerySingleAsync<LitterRow>(
            @"SELECT * FROM litter WHERE id=@Id", new {Id = id});
        
        litter.ShouldSatisfyAllConditions(
            () => litter.id.ShouldBe(id),
            () => litter.dam_id.ShouldBeNull(),
            () => litter.sire_id.ShouldBe(ratId),
            () => litter.date_of_birth.ShouldBeNull());
    }

    [Fact]
    public async Task Add_litter_sets_all_optional_values()
    {
        var dam = this.faker.Rat(sex: Sex.Doe);
        var damId = await this.fixture.Seed(dam);
        var sire = this.faker.Rat(sex: Sex.Buck);
        var sireId = await this.fixture.Seed(sire);
        var id = await this.repository.CreateLitter(damId: damId, sireId: sireId);
        
        using var db = this.fixture.GetConnection();
        var litter = await db.QuerySingleAsync<LitterRow>(
            @"SELECT * FROM litter WHERE id=@Id", new {Id = id});
        
        litter.ShouldSatisfyAllConditions(
            () => litter.id.ShouldBe(id),
            () => litter.dam_id.ShouldBe(damId),
            () => litter.sire_id.ShouldBe(sireId),
            () => litter.date_of_birth.ShouldBeNull());
    }

    [Fact]
    public async Task Update_litter_updates()
    {
        var id = await this.repository.CreateLitter();

        var dam = this.faker.Rat(sex: Sex.Doe);
        var sire = this.faker.Rat(sex: Sex.Buck);
        var damId = await this.fixture.Seed(dam);
        var sireId = await this.fixture.Seed(sire);
        var dob = this.faker.Date.PastDateOnly();

        var updatedLitter = new Litter(id, dam: (damId, dam.Name), sire: (sireId, sire.Name), dob);
        await this.repository.UpdateLitter(updatedLitter);

        using var db = this.fixture.GetConnection(); 
        var row = await db.QuerySingleAsync<LitterRow>(
            @"SELECT * FROM litter WHERE id=@Id", new {Id = id});
        
        row.ShouldSatisfyAllConditions(
            () => row.id.ShouldBe(id),
            () => row.dam_id.ShouldBe(damId),
            () => row.sire_id.ShouldBe(sireId),
            () => row.date_of_birth.ShouldBe(dob.ToPersistedDateTime()));
    }
    
    [Fact]
    public async Task Add_offspring_adds_successfully()
    {
        var id = await this.repository.CreateLitter();

        var rat = this.faker.Rat();
        var ratId = await this.fixture.Seed(rat);

        var result = await this.repository.AddOffspring(id, ratId);
        result.ShouldBe(AddOffspringResult.Success);
        
        using var db = this.fixture.GetConnection();
        var litterKin = await db.QuerySingleAsync<LitterKinRow>(
            @"SELECT * FROM litter_kin WHERE litter_id=@Id", new {Id=id});
        
        litterKin.offspring_id.ShouldBe(ratId);
    }
    
    [Fact]
    public async Task Add_offspring_adds_only_once()
    {
        var id = await this.repository.CreateLitter();

        var rat = this.faker.Rat();
        var ratId = await this.fixture.Seed(rat);

        await this.repository.AddOffspring(id, ratId);
        await this.repository.AddOffspring(id, ratId);

        using var db = this.fixture.GetConnection();
        var litterKin = await db.QueryAsync<LitterKinRow>(
            @"SELECT * FROM litter_kin WHERE litter_id=@Id", new {Id=id});
        
        litterKin.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Add_offspring_returns_error_for_nonexistant_litter()
    {
        var rat = this.faker.Rat();
        var ratId = await this.fixture.Seed(rat);

        var result = await this.repository.AddOffspring(long.MaxValue, ratId);
        
        result.ShouldBe(AddOffspringResult.NonExistantRatOrLitter);
    }

    [Fact]
    public async Task Add_offspring_returns_error_for_nonexistant_rat()
    {
        var id = await this.repository.CreateLitter();

        var result = await this.repository.AddOffspring(id, long.MaxValue);
        
        result.ShouldBe(AddOffspringResult.NonExistantRatOrLitter);
    }
    
    [Fact]
    public async Task Remove_offspring_removes()
    {
        var id = await this.repository.CreateLitter();

        var rat = this.faker.Rat();
        var ratId = await this.fixture.Seed(rat);
        await this.repository.AddOffspring(id, ratId);

        await this.repository.RemoveOffspring(id, ratId);

        using var db = this.fixture.GetConnection();
        var litterKin = await db.QueryAsync<LitterKinRow>(
            @"SELECT * FROM litter_kin WHERE litter_id=@Id", new {Id=id});
        
        litterKin.ShouldBeEmpty();
    }
    
    [Fact]
    public async Task Remove_offspring_succeeds_if_link_does_not_exist()
    {
        var id = await this.repository.CreateLitter();
        var ratId = await this.fixture.Seed(this.faker.Rat());

        await this.repository.RemoveOffspring(id, ratId);
        
        using var db = this.fixture.GetConnection();
        var litterKin = await db.QueryAsync<LitterKinRow>(
            @"SELECT * FROM litter_kin WHERE litter_id=@Id", new {Id=id});
        
        litterKin.ShouldBeEmpty();
    }

    [Fact]
    public async Task Delete_litter_deletes()
    {
        var id = await this.repository.CreateLitter();
        await this.repository.AddOffspring(id, await this.fixture.Seed(this.faker.Rat()));
        await this.repository.AddOffspring(id, await this.fixture.Seed(this.faker.Rat()));
        
        await this.repository.DeleteLitter(id);
        
        using var db = this.fixture.GetConnection();
        var litter = await db.QuerySingleOrDefaultAsync<LitterRow>(
            @"SELECT * FROM litter WHERE id=@Id", new {Id = id});
        var litterKin = await db.QueryAsync<LitterKinRow>(
            @"SELECT * FROM litter_kin WHERE litter_id=@Id", new {Id = id});
        
        litter.ShouldBeNull();
        litterKin.ShouldBeEmpty();
    }

    [Fact]
    public async Task Get_litter_gets_litter_details_and_parents()
    {
        var id = await this.repository.CreateLitter();

        var dam = this.faker.Rat(sex: Sex.Doe);
        var sire = this.faker.Rat(sex: Sex.Buck);
        var damId = await this.fixture.Seed(dam);
        var sireId = await this.fixture.Seed(sire);
        var dob = this.faker.Date.PastDateOnly();

        var updatedLitter = new Litter(id, dam: (damId, dam.Name), sire: (sireId, sire.Name), dob);
        await this.repository.UpdateLitter(updatedLitter);

        var retrievedLitter = await this.repository.GetLitter(id);
        retrievedLitter.ShouldBeEquivalentTo(updatedLitter);
    }

    [Fact]
    public async Task Get_litter_gets_litter_offspring()
    {
        var id = await this.repository.CreateLitter();
        
        var rat1 = this.faker.Rat();
        var rat2 = this.faker.Rat();
        var rat1Id = await this.fixture.Seed(rat1);
        var rat2Id = await this.fixture.Seed(rat2);
        await this.repository.AddOffspring(id, rat1Id);
        await this.repository.AddOffspring(id, rat2Id);


        var litter = await this.repository.GetLitter(id);
        
        litter.Offspring.ShouldBeEquivalentTo(new List<Offspring>
        {
            new(rat1Id, rat1.Name),
            new(rat2Id, rat2.Name)
        });
    }
    
    // ReSharper disable InconsistentNaming
    private record LitterRow(long id, long? dam_id, long? sire_id, long? date_of_birth);
    private record LitterKinRow(long litter_id, long offspring_id);
    // ReSharper restore InconsistentNaming
}