using Basemix.Litters;
using Basemix.Rats;
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
            () => litter.Offspring.ShouldBeEmpty());
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

    private async Task<Litter> CreateLitter() =>
        (await this.littersRepository.GetLitter(await this.littersRepository.CreateLitter()))!;
}