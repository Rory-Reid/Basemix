using Basemix.Lib.Litters;
using Basemix.Lib.Litters.Persistence;
using Basemix.Lib.Persistence;
using Basemix.Lib.Rats;
using Basemix.Tests.sdk;
using Bogus;
using Dapper;
using Shouldly;

namespace Basemix.Tests.Integration;

public class LittersRepositoryTests : SqliteIntegration
{
    private readonly Faker faker = new();
    private readonly SqliteFixture fixture;
    private readonly SqliteLittersRepository repository;
    
    public LittersRepositoryTests(SqliteFixture fixture) : base(fixture)
    {
        this.fixture = fixture;
        this.repository = new SqliteLittersRepository(this.fixture.GetConnection);
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
        rat = await this.fixture.Seed(rat);
        var id = await this.repository.CreateLitter(damId: rat.Id);
        
        using var db = this.fixture.GetConnection();
        var litter = await db.QuerySingleAsync<LitterRow>(
            @"SELECT * FROM litter WHERE id=@Id", new {Id = id});
        
        litter.ShouldSatisfyAllConditions(
            () => litter.id.ShouldBe(id),
            () => litter.dam_id.ShouldBe(rat.Id.Value),
            () => litter.sire_id.ShouldBeNull(),
            () => litter.date_of_birth.ShouldBeNull());
    }

    [Fact]
    public async Task Add_litter_optionally_sets_sire()
    {
        var rat = this.faker.Rat(sex: Sex.Buck);
        rat = await this.fixture.Seed(rat);
        var id = await this.repository.CreateLitter(sireId: rat.Id);
        
        using var db = this.fixture.GetConnection();
        var litter = await db.QuerySingleAsync<LitterRow>(
            @"SELECT * FROM litter WHERE id=@Id", new {Id = id});
        
        litter.ShouldSatisfyAllConditions(
            () => litter.id.ShouldBe(id),
            () => litter.dam_id.ShouldBeNull(),
            () => litter.sire_id.ShouldBe(rat.Id.Value),
            () => litter.date_of_birth.ShouldBeNull());
    }

    [Fact]
    public async Task Add_litter_sets_all_optional_values()
    {
        var dam = this.faker.Rat(sex: Sex.Doe);
        dam = await this.fixture.Seed(dam);
        var sire = this.faker.Rat(sex: Sex.Buck);
        sire = await this.fixture.Seed(sire);
        var id = await this.repository.CreateLitter(damId: dam.Id, sireId: sire.Id);
        
        using var db = this.fixture.GetConnection();
        var litter = await db.QuerySingleAsync<LitterRow>(
            @"SELECT * FROM litter WHERE id=@Id", new {Id = id});
        
        litter.ShouldSatisfyAllConditions(
            () => litter.id.ShouldBe(id),
            () => litter.dam_id.ShouldBe(dam.Id.Value),
            () => litter.sire_id.ShouldBe(sire.Id.Value),
            () => litter.date_of_birth.ShouldBeNull(),
            () => litter.date_of_pairing.ShouldBeNull(),
            () => litter.notes.ShouldBeNull());
    }

    [Fact]
    public async Task Update_litter_updates()
    {
        var id = await this.repository.CreateLitter();

        var dam = this.faker.Rat(sex: Sex.Doe);
        var sire = this.faker.Rat(sex: Sex.Buck);
        dam = await this.fixture.Seed(dam);
        sire = await this.fixture.Seed(sire);
        var dob = this.faker.Date.PastDateOnly();
        var dop = this.faker.Date.RecentDateOnly(refDate: dob);
        var notes = this.faker.Lorem.Paragraph();

        var updatedLitter = new Litter(id, dam: (dam.Id, dam.Name), sire: (sire.Id, sire.Name), dob)
        {
            DateOfPairing = dop,
            Notes = notes
        };
        await this.repository.UpdateLitter(updatedLitter);

        using var db = this.fixture.GetConnection(); 
        var row = await db.QuerySingleAsync<LitterRow>(
            @"SELECT * FROM litter WHERE id=@Id", new {Id = id});
        
        row.ShouldSatisfyAllConditions(
            () => row.id.ShouldBe(id),
            () => row.dam_id.ShouldBe(dam.Id.Value),
            () => row.sire_id.ShouldBe(sire.Id.Value),
            () => row.date_of_birth.ShouldBe(dob.ToPersistedDateTime()),
            () => row.date_of_pairing.ShouldBe(dop.ToPersistedDateTime()),
            () => row.notes.ShouldBe(notes));
    }
    
    [Fact]
    public async Task Add_offspring_adds_successfully()
    {
        var id = await this.repository.CreateLitter();

        var rat = await this.fixture.Seed(this.faker.Rat());

        var result = await this.repository.AddOffspring(id, rat.Id);
        result.ShouldBe(AddOffspringResult.Success);
        
        using var db = this.fixture.GetConnection();
        var offspringId = await db.QuerySingleAsync<long>(
            @"SELECT id FROM rat WHERE litter_id=@Id", new {Id=id});
        
        offspringId.ShouldBe(rat.Id.Value);
    }
    
    [Fact]
    public async Task Add_offspring_adds_only_once()
    {
        var id = await this.repository.CreateLitter();

        var rat = await this.fixture.Seed(this.faker.Rat());

        await this.repository.AddOffspring(id, rat.Id);
        await this.repository.AddOffspring(id, rat.Id);

        using var db = this.fixture.GetConnection();
        var litterKin = await db.QueryAsync<long>(
            @"SELECT id FROM rat WHERE litter_id=@Id", new {Id=id});
        
        litterKin.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Add_offspring_returns_error_for_nonexistant_litter()
    {
        var rat = await this.fixture.Seed(this.faker.Rat());

        var result = await this.repository.AddOffspring(long.MaxValue, rat.Id);
        
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

        var rat = await this.fixture.Seed(this.faker.Rat());
        await this.repository.AddOffspring(id, rat.Id);

        var result = await this.repository.RemoveOffspring(id, rat.Id);
        result.ShouldBe(RemoveOffspringResult.Success);
        
        using var db = this.fixture.GetConnection();
        var litterKin = await db.QueryAsync<long>(
            @"SELECT * FROM rat WHERE litter_id=@Id", new {Id=id});
        
        litterKin.ShouldBeEmpty();
    }
    
    [Fact]
    public async Task Remove_offspring_succeeds_if_link_does_not_exist()
    {
        var id = await this.repository.CreateLitter();
        var rat = await this.fixture.Seed(this.faker.Rat());

        var result = await this.repository.RemoveOffspring(id, rat.Id);
        result.ShouldBe(RemoveOffspringResult.NothingToRemove);
        
        using var db = this.fixture.GetConnection();
        var litterKin = await db.QueryAsync<long>(
            @"SELECT * FROM rat WHERE litter_id=@Id", new {Id=id});
        
        litterKin.ShouldBeEmpty();
    }

    [Fact]
    public async Task Delete_litter_deletes()
    {
        var id = await this.repository.CreateLitter();
        await this.repository.AddOffspring(id, (await this.fixture.Seed(this.faker.Rat())).Id);
        await this.repository.AddOffspring(id, (await this.fixture.Seed(this.faker.Rat())).Id);
        
        await this.repository.DeleteLitter(id);
        
        using var db = this.fixture.GetConnection();
        var litter = await db.QuerySingleOrDefaultAsync<LitterRow>(
            @"SELECT * FROM litter WHERE id=@Id", new {Id = id});
        var litterKin = await db.QueryAsync<long>(
            @"SELECT * FROM rat WHERE litter_id=@Id", new {Id = id});
        
        litter.ShouldBeNull();
        litterKin.ShouldBeEmpty();
    }

    [Fact]
    public async Task Get_litter_gets_litter_details_and_parents()
    {
        var id = await this.repository.CreateLitter();

        var dam = this.faker.Rat(sex: Sex.Doe);
        var sire = this.faker.Rat(sex: Sex.Buck);
        dam = await this.fixture.Seed(dam);
        sire = await this.fixture.Seed(sire);
        var dob = this.faker.Date.PastDateOnly();
        var dop = dob.AddDays(-21);

        var updatedLitter = new Litter(id, dam: (dam.Id, dam.Name), sire: (sire.Id, sire.Name), dob)
        {
            DateOfPairing = dop,
            Notes = this.faker.Lorem.Paragraph()
        };
        await this.repository.UpdateLitter(updatedLitter);

        var retrievedLitter = await this.repository.GetLitter(id);
        retrievedLitter.ShouldBeEquivalentTo(updatedLitter);
    }

    [Fact]
    public async Task Get_litter_gets_litter_offspring()
    {
        var id = await this.repository.CreateLitter();

        var owner1 = await this.fixture.Seed(this.faker.Owner());
        var rat1 = await this.fixture.Seed(this.faker.Rat(), owner1);
        var rat2 = await this.fixture.Seed(this.faker.Rat());
        await this.repository.AddOffspring(id, rat1.Id);
        await this.repository.AddOffspring(id, rat2.Id);

        var litter = await this.repository.GetLitter(id);
        
        litter.ShouldNotBeNull().Offspring.ShouldBeEquivalentTo(new List<Offspring>
        {
            new(rat1.Id, rat1.Name, owner1.Name),
            new(rat2.Id, rat2.Name)
        });
    }
    
    // ReSharper disable InconsistentNaming
    private record LitterRow(long id, long? dam_id, long? sire_id, long? date_of_birth, long? date_of_pairing, string? notes);
    // ReSharper restore InconsistentNaming
}