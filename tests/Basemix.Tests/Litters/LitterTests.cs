using Basemix.Lib.Litters;
using Basemix.Lib.Rats;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.Litters;

public class LitterTests
{
    private readonly Faker faker = new();
    private readonly MemoryRatsRepository ratsRepository;
    private readonly MemoryLittersRepository littersRepository;
    
    public LitterTests()
    {
        var backplane = new MemoryPersistenceBackplane();
        this.ratsRepository = new MemoryRatsRepository(backplane);
        this.littersRepository = new MemoryLittersRepository(backplane);
    }

    [Fact]
    public void Constructs_anonymous_litter()
    {
        var litter = new Litter();
        
        litter.ShouldSatisfyAllConditions(
            () => litter.Id.ShouldNotBeNull(),
            () => litter.DamId.ShouldBeNull(),
            () => litter.DamName.ShouldBeNull(),
            () => litter.SireId.ShouldBeNull(),
            () => litter.SireName.ShouldBeNull(),
            () => litter.DateOfBirth.ShouldBeNull(),
            () => litter.Offspring.ShouldBeEmpty(),
            () => litter.Notes.ShouldBeNull());
    }

    [Fact]
    public async Task Create_saves_and_returns_litter_with_id()
    {
        var litter = await Litter.Create(this.littersRepository);
        
        litter.Id.Value.ShouldBePositive();
        
        this.littersRepository.Litters.ShouldContainKey(litter.Id);
    }

    [Fact]
    public async Task Create_sets_bred_by_me_to_true()
    {
        var litter = await Litter.Create(this.littersRepository);
        litter.BredByMe.ShouldBeTrue();
    }

    [Fact]
    public async Task Set_dam_updates_local_and_stored_litter_with_valid_dam()
    {
        var litter = await this.CreateLitter();

        var dam = await this.ratsRepository.Seed(this.faker.Rat(sex: Sex.Doe));

        var result = await litter.SetDam(this.littersRepository, dam);
        
        result.ShouldBe(LitterAddResult.Success);
        litter.ShouldSatisfyAllConditions(
            () => litter.DamId.ShouldBe(dam.Id),
            () => litter.DamName.ShouldBe(dam.Name));
        
        this.littersRepository.Litters[litter.Id].ShouldSatisfyAllConditions(
            storedLitter => storedLitter.DamId.ShouldBe(dam.Id),
            storedLitter => storedLitter.DamName.ShouldBe(dam.Name));
    }
    
    [Fact]
    public async Task Set_dam_is_idempotent()
    {
        var litter = await this.CreateLitter();

        var dam = await this.ratsRepository.Seed(this.faker.Rat(sex: Sex.Doe));

        await litter.SetDam(this.littersRepository, dam);
        var result = await litter.SetDam(this.littersRepository, dam);
        
        result.ShouldBe(LitterAddResult.Success);
        litter.ShouldSatisfyAllConditions(
            () => litter.DamId.ShouldBe(dam.Id),
            () => litter.DamName.ShouldBe(dam.Name));
        
        this.littersRepository.Litters[litter.Id].ShouldSatisfyAllConditions(
            storedLitter => storedLitter.DamId.ShouldBe(dam.Id),
            storedLitter => storedLitter.DamName.ShouldBe(dam.Name));
    }

    [Fact]
    public async Task Set_dam_returns_error_if_buck_used_and_doesnt_modify_local_or_persisted_litter()
    {
        var litter = await this.CreateLitter();

        var buck = await this.ratsRepository.Seed(this.faker.Rat(sex: Sex.Buck));

        var result = await litter.SetDam(this.littersRepository, buck);
        
        result.ShouldBe(LitterAddResult.WrongSex);
        litter.ShouldSatisfyAllConditions(
            () => litter.DamId.ShouldBeNull(),
            () => litter.DamName.ShouldBeNull());
        
        this.littersRepository.Litters[litter.Id].ShouldSatisfyAllConditions(
            storedLitter => storedLitter.DamId.ShouldBeNull(),
            storedLitter => storedLitter.DamName.ShouldBeNull());
    }
    
    [Fact]
    public async Task Set_sire_updates_local_and_stored_litter_with_valid_sire()
    {
        var litter = await this.CreateLitter();

        var sire = await this.ratsRepository.Seed(this.faker.Rat(sex: Sex.Buck));

        var result = await litter.SetSire(this.littersRepository, sire);
        
        result.ShouldBe(LitterAddResult.Success);
        litter.ShouldSatisfyAllConditions(
            () => litter.SireId.ShouldBe(sire.Id),
            () => litter.SireName.ShouldBe(sire.Name));
        
        this.littersRepository.Litters[litter.Id].ShouldSatisfyAllConditions(
            storedLitter => storedLitter.SireId.ShouldBe(sire.Id),
            storedLitter => storedLitter.SireName.ShouldBe(sire.Name));
    }
    
    [Fact]
    public async Task Set_sire_is_idempotent()
    {
        var litter = await this.CreateLitter();

        var sire = await this.ratsRepository.Seed(this.faker.Rat(sex: Sex.Buck));

        await litter.SetDam(this.littersRepository, sire);
        var result = await litter.SetSire(this.littersRepository, sire);
        
        result.ShouldBe(LitterAddResult.Success);
        litter.ShouldSatisfyAllConditions(
            () => litter.SireId.ShouldBe(sire.Id),
            () => litter.SireName.ShouldBe(sire.Name));
        
        this.littersRepository.Litters[litter.Id].ShouldSatisfyAllConditions(
            storedLitter => storedLitter.SireId.ShouldBe(sire.Id),
            storedLitter => storedLitter.SireName.ShouldBe(sire.Name));
    }

    [Fact]
    public async Task Set_sire_returns_error_if_doe_used_and_doesnt_modify_local_or_persisted_litter()
    {
        var litter = await this.CreateLitter();

        var buck = await this.ratsRepository.Seed(this.faker.Rat(sex: Sex.Doe));

        var result = await litter.SetSire(this.littersRepository, buck);
        
        result.ShouldBe(LitterAddResult.WrongSex);
        litter.ShouldSatisfyAllConditions(
            () => litter.SireId.ShouldBeNull(),
            () => litter.SireName.ShouldBeNull());
        
        this.littersRepository.Litters[litter.Id].ShouldSatisfyAllConditions(
            storedLitter => storedLitter.SireId.ShouldBeNull(),
            storedLitter => storedLitter.SireName.ShouldBeNull());
    }

    [Fact]
    public async Task Add_offspring_stores_in_litter()
    {
        var litter = await this.CreateLitter();

        var rat = await this.ratsRepository.Seed(this.faker.Rat());
        await litter.AddOffspring(this.littersRepository, rat);
        
        litter.Offspring.ShouldContain(x => x.Id == rat.Id && x.Name == rat.Name);
    }

    [Fact]
    public async Task Add_offspring_stores_in_repository()
    {
        var litter = await this.CreateLitter();

        var rat = await this.ratsRepository.Seed(this.faker.Rat());
        await litter.AddOffspring(this.littersRepository, rat);
        
        this.littersRepository.Litters[litter.Id].Offspring.ShouldContain(x => x.Id == rat.Id && x.Name == rat.Name);
    }

    [Fact]
    public async Task Add_offspring_with_unsaved_rat_does_not_add()
    {
        var litter = await this.CreateLitter();

        await litter.AddOffspring(this.littersRepository, this.faker.Rat());
        
        litter.Offspring.ShouldBeEmpty();
    }

    [Fact]
    public async Task Remove_offspring_removes_from_litter()
    {
        var litter = await this.CreateLitter();
        var rat = await this.ratsRepository.Seed(this.faker.Rat());
        await litter.AddOffspring(this.littersRepository, rat);

        await litter.RemoveOffspring(this.littersRepository, rat.Id);
        
        litter.Offspring.ShouldBeEmpty();
    }

    [Fact]
    public async Task Remove_offspring_removes_from_repository()
    {
        var litter = await this.CreateLitter();
        var rat = await this.ratsRepository.Seed(this.faker.Rat());
        await litter.AddOffspring(this.littersRepository, rat);

        await litter.RemoveOffspring(this.littersRepository, rat.Id);
        
        this.littersRepository.Litters[litter.Id].Offspring.ShouldBeEmpty();
    }

    [Fact]
    public async Task Remove_offspring_with_rat_not_in_litter_does_nothing_and_does_not_error()
    {
        var litter = await this.CreateLitter();
        var rat = await this.ratsRepository.Seed(this.faker.Rat());
        await litter.AddOffspring(this.littersRepository, rat);

        var otherRat = await this.ratsRepository.Seed(this.faker.Rat());
        await litter.RemoveOffspring(this.littersRepository, otherRat.Id);

        litter.Offspring.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Properties_do_not_persist_on_set()
    {
        var litter = await this.CreateLitter();
        litter.DateOfBirth = this.faker.Date.PastDateOnly();
        litter.Notes = this.faker.Lorem.Paragraphs();
        
        (await this.littersRepository.GetLitter(litter.Id)).ShouldNotBeNull().ShouldSatisfyAllConditions(
            storedLitter => storedLitter.DateOfBirth.ShouldBeNull(),
            storedLitter => storedLitter.Notes.ShouldBeNull());
    }

    [Fact]
    public async Task Properties_persist_on_save()
    {
        var litter = await this.CreateLitter();
        litter.DateOfBirth = this.faker.Date.PastDateOnly();
        litter.Notes = this.faker.Lorem.Paragraphs();

        await litter.Save(this.littersRepository);
        
        (await this.littersRepository.GetLitter(litter.Id)).ShouldNotBeNull().ShouldSatisfyAllConditions(
            storedLitter => storedLitter.DateOfBirth.ShouldBe(litter.DateOfBirth),
            storedLitter => storedLitter.Notes.ShouldBe(litter.Notes));
    }
    
    [Fact]
    public async Task Setting_date_of_birth_only_persists_on_save()
    {
        var litter = await this.CreateLitter();
        litter.DateOfBirth = this.faker.Date.PastDateOnly();
        
        (await this.littersRepository.GetLitter(litter.Id))!.DateOfBirth.ShouldNotBe(litter.DateOfBirth);

        await litter.Save(this.littersRepository);
        
        (await this.littersRepository.GetLitter(litter.Id))!.DateOfBirth.ShouldBe(litter.DateOfBirth);
    }

    private async Task<Litter> CreateLitter() => await Litter.Create(this.littersRepository);
}