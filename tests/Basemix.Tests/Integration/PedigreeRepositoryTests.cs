using Basemix.Lib.Litters;
using Basemix.Lib.Litters.Persistence;
using Basemix.Lib.Pedigrees.Persistence;
using Basemix.Lib.Rats;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.Integration;

public class PedigreeRepositoryTests : SqliteIntegration
{
    private readonly Faker faker = new();
    private readonly SqliteFixture fixture;
    private readonly SqlitePedigreeRepository repository;
    private readonly SqliteLittersRepository littersRepository;
    
    public PedigreeRepositoryTests(SqliteFixture fixture) : base(fixture)
    {
        this.fixture = fixture;
        this.repository = new SqlitePedigreeRepository(fixture.GetConnection);
        this.littersRepository = new SqliteLittersRepository(fixture.GetConnection);
    }

    [Fact]
    public async Task Returns_null_for_unknown_rat()
    {
        var pedigree = await this.repository.GetPedigree(long.MaxValue);
        pedigree.ShouldBeNull();
    }

    [Fact]
    public async Task Returns_shallowest_pedigree()
    {
        var rat = this.faker.Rat();
        rat = await this.fixture.Seed(rat);

        var pedigree = await this.repository.GetPedigree(rat.Id);
        
        pedigree.ShouldNotBeNull().ShouldSatisfyAllConditions(
            () => pedigree.Name.ShouldBe(rat.Name),
            () => pedigree.Variety.ShouldBe(rat.Variety),
            () => pedigree.Dam.ShouldBeNull(),
            () => pedigree.Sire.ShouldBeNull());
    }

    [Fact]
    public async Task Returns_deepest_pedigree()
    {
        var rat = this.faker.Rat();
        
        var dam = this.faker.Rat(sex: Sex.Doe);
        var damDam = this.faker.Rat(sex: Sex.Doe);
        var damDamDam = this.faker.Rat(sex: Sex.Doe);
        var damDamDamDam = this.faker.Rat(sex: Sex.Doe);
        var damDamDamSire = this.faker.Rat(sex: Sex.Buck);
        var damDamSire = this.faker.Rat(sex: Sex.Buck);
        var damDamSireDam = this.faker.Rat(sex: Sex.Doe);
        var damDamSireSire = this.faker.Rat(sex: Sex.Buck);
        var damSire = this.faker.Rat(sex: Sex.Buck);
        var damSireDam = this.faker.Rat(sex: Sex.Doe);
        var damSireDamDam = this.faker.Rat(sex: Sex.Doe);
        var damSireDamSire = this.faker.Rat(sex: Sex.Buck);
        var damSireSire = this.faker.Rat(sex: Sex.Buck);
        var damSireSireDam = this.faker.Rat(sex: Sex.Doe);
        var damSireSireSire = this.faker.Rat(sex: Sex.Buck);
        
        var sire = this.faker.Rat(sex: Sex.Buck);
        var sireDam = this.faker.Rat(sex: Sex.Doe);
        var sireDamDam = this.faker.Rat(sex: Sex.Doe);
        var sireDamDamDam = this.faker.Rat(sex: Sex.Doe);
        var sireDamDamSire = this.faker.Rat(sex: Sex.Buck);
        var sireDamSire = this.faker.Rat(sex: Sex.Buck);
        var sireDamSireDam = this.faker.Rat(sex: Sex.Doe);
        var sireDamSireSire = this.faker.Rat(sex: Sex.Buck);
        var sireSire = this.faker.Rat(sex: Sex.Buck);
        var sireSireDam = this.faker.Rat(sex: Sex.Doe);
        var sireSireDamDam = this.faker.Rat(sex: Sex.Doe);
        var sireSireDamSire = this.faker.Rat(sex: Sex.Buck);
        var sireSireSire = this.faker.Rat(sex: Sex.Buck);
        var sireSireSireDam = this.faker.Rat(sex: Sex.Doe);
        var sireSireSireSire = this.faker.Rat(sex: Sex.Buck);

        rat = await this.fixture.Seed(rat);
        dam = await this.fixture.Seed(dam);
        damDam = await this.fixture.Seed(damDam);
        damDamDam = await this.fixture.Seed(damDamDam);
        damDamDamDam = await this.fixture.Seed(damDamDamDam);
        damDamDamSire = await this.fixture.Seed(damDamDamSire);
        damDamSire = await this.fixture.Seed(damDamSire);
        damDamSireDam = await this.fixture.Seed(damDamSireDam);
        damDamSireSire = await this.fixture.Seed(damDamSireSire);
        damSire = await this.fixture.Seed(damSire);
        damSireDam = await this.fixture.Seed(damSireDam);
        damSireDamDam = await this.fixture.Seed(damSireDamDam);
        damSireDamSire = await this.fixture.Seed(damSireDamSire);
        damSireSire = await this.fixture.Seed(damSireSire);
        damSireSireDam = await this.fixture.Seed(damSireSireDam);
        damSireSireSire = await this.fixture.Seed(damSireSireSire);
        sire = await this.fixture.Seed(sire);
        sireDam = await this.fixture.Seed(sireDam);
        sireDamDam = await this.fixture.Seed(sireDamDam);
        sireDamDamDam = await this.fixture.Seed(sireDamDamDam);
        sireDamDamSire = await this.fixture.Seed(sireDamDamSire);
        sireDamSire = await this.fixture.Seed(sireDamSire);
        sireDamSireDam = await this.fixture.Seed(sireDamSireDam);
        sireDamSireSire = await this.fixture.Seed(sireDamSireSire);
        sireSire = await this.fixture.Seed(sireSire);
        sireSireDam = await this.fixture.Seed(sireSireDam);
        sireSireDamDam = await this.fixture.Seed(sireSireDamDam);
        sireSireDamSire = await this.fixture.Seed(sireSireDamSire);
        sireSireSire = await this.fixture.Seed(sireSireSire);
        sireSireSireDam = await this.fixture.Seed(sireSireSireDam);
        sireSireSireSire = await this.fixture.Seed(sireSireSireSire);
        
        await this.CreateLitter(dam, sire, rat);
        
        await this.CreateLitter(damDam, damSire, dam);
        await this.CreateLitter(damDamDam, damDamSire, damDam);
        await this.CreateLitter(damDamDamDam, damDamDamSire, damDamDam);
        await this.CreateLitter(damDamSireDam, damDamSireSire, damDamSire);
        await this.CreateLitter(damSireDam, damSireSire, damSire);
        await this.CreateLitter(damSireDamDam, damSireDamSire, damSireDam);
        await this.CreateLitter(damSireSireDam, damSireSireSire, damSireSire);
        
        await this.CreateLitter(sireDam, sireSire, sire);
        await this.CreateLitter(sireDamDam, sireDamSire, sireDam);
        await this.CreateLitter(sireDamDamDam, sireDamDamSire, sireDamDam);
        await this.CreateLitter(sireDamSireDam, sireDamSireSire, sireDamSire);
        await this.CreateLitter(sireSireDam, sireSireSire, sireSire);
        await this.CreateLitter(sireSireDamDam, sireSireDamSire, sireSireDam);
        await this.CreateLitter(sireSireSireDam, sireSireSireSire, sireSireSire);

        var pedigree = await this.repository.GetPedigree(rat.Id);

        pedigree.ShouldNotBeNull().ShouldSatisfyAllConditions(
            () => pedigree.Name.ShouldBe(rat.Name),
            () => pedigree.Variety.ShouldBe(rat.Variety),
            () => pedigree.Dam.ShouldNotBeNull().ShouldSatisfyAllConditions(
                d => d.Name.ShouldBe(dam.Name),
                d => d.Variety.ShouldBe(dam.Variety),
                d => d.Dam.ShouldNotBeNull().ShouldSatisfyAllConditions(
                    dd => dd.Name.ShouldBe(damDam.Name),
                    dd => dd.Variety.ShouldBe(damDam.Variety),
                    dd => dd.Dam.ShouldNotBeNull().ShouldSatisfyAllConditions(
                        ddd => ddd.Name.ShouldBe(damDamDam.Name),
                        ddd => ddd.Variety.ShouldBe(damDamDam.Variety),
                        ddd => ddd.Dam.ShouldNotBeNull().ShouldSatisfyAllConditions(
                            dddd => dddd.Name.ShouldBe(damDamDamDam.Name),
                            dddd => dddd.Variety.ShouldBe(damDamDamDam.Variety)),
                        ddd => ddd.Sire.ShouldNotBeNull().ShouldSatisfyAllConditions(
                            ddds => ddds.Name.ShouldBe(damDamDamSire.Name),
                            ddds => ddds.Variety.ShouldBe(damDamDamSire.Variety))),
                    dd => dd.Sire.ShouldNotBeNull().ShouldSatisfyAllConditions(
                        dds => dds.Name.ShouldBe(damDamSire.Name),
                        dds => dds.Variety.ShouldBe(damDamSire.Variety),
                        dds => dds.Dam.ShouldNotBeNull().ShouldSatisfyAllConditions(
                            ddsd => ddsd.Name.ShouldBe(damDamSireDam.Name),
                            ddsd => ddsd.Variety.ShouldBe(damDamSireDam.Variety)),
                        dds => dds.Sire.ShouldNotBeNull().ShouldSatisfyAllConditions(
                            ddss => ddss.Name.ShouldBe(damDamSireSire.Name),
                            ddss => ddss.Variety.ShouldBe(damDamSireSire.Variety)))),
                d => d.Sire.ShouldNotBeNull().ShouldSatisfyAllConditions(
                    ds => ds.Name.ShouldBe(damSire.Name),
                    ds => ds.Variety.ShouldBe(damSire.Variety),
                    ds => ds.Dam.ShouldNotBeNull().ShouldSatisfyAllConditions(
                        dsd => dsd.Name.ShouldBe(damSireDam.Name),
                        dsd => dsd.Variety.ShouldBe(damSireDam.Variety),
                        dsd => dsd.Dam.ShouldNotBeNull().ShouldSatisfyAllConditions(
                            dsdd => dsdd.Name.ShouldBe(damSireDamDam.Name),
                            dsdd => dsdd.Variety.ShouldBe(damSireDamDam.Variety)),
                        dsd => dsd.Sire.ShouldNotBeNull().ShouldSatisfyAllConditions(
                            dsds => dsds.Name.ShouldBe(damSireDamSire.Name),
                            dsds => dsds.Variety.ShouldBe(damSireDamSire.Variety))),
                    ds => ds.Sire.ShouldNotBeNull().ShouldSatisfyAllConditions(
                        dss => dss.Name.ShouldBe(damSireSire.Name),
                        dss => dss.Variety.ShouldBe(damSireSire.Variety),
                        dss => dss.Dam.ShouldNotBeNull().ShouldSatisfyAllConditions(
                            dssd => dssd.Name.ShouldBe(damSireSireDam.Name),
                            dssd => dssd.Variety.ShouldBe(damSireSireDam.Variety)),
                        dss => dss.Sire.ShouldNotBeNull().ShouldSatisfyAllConditions(
                            dsss => dsss.Name.ShouldBe(damSireSireSire.Name),
                            dsss => dsss.Variety.ShouldBe(damSireSireSire.Variety))))),
            () => pedigree.Sire.ShouldNotBeNull().ShouldSatisfyAllConditions(
                s => s.Name.ShouldBe(sire.Name),
                s => s.Variety.ShouldBe(sire.Variety),
                s => s.Dam.ShouldNotBeNull().ShouldSatisfyAllConditions(
                    sd => sd.Name.ShouldBe(sireDam.Name),
                    sd => sd.Variety.ShouldBe(sireDam.Variety),
                    sd => sd.Dam.ShouldNotBeNull().ShouldSatisfyAllConditions(
                        sdd => sdd.Name.ShouldBe(sireDamDam.Name),
                        sdd => sdd.Variety.ShouldBe(sireDamDam.Variety),
                        sdd => sdd.Dam.ShouldNotBeNull().ShouldSatisfyAllConditions(
                            sddd => sddd.Name.ShouldBe(sireDamDamDam.Name),
                            sddd => sddd.Variety.ShouldBe(sireDamDamDam.Variety)),
                        sdd => sdd.Sire.ShouldNotBeNull().ShouldSatisfyAllConditions(
                            sdds => sdds.Name.ShouldBe(sireDamDamSire.Name),
                            sdds => sdds.Variety.ShouldBe(sireDamDamSire.Variety))),
                    sd => sd.Sire.ShouldNotBeNull().ShouldSatisfyAllConditions(
                        sds => sds.Name.ShouldBe(sireDamSire.Name),
                        sds => sds.Variety.ShouldBe(sireDamSire.Variety),
                        sds => sds.Dam.ShouldNotBeNull().ShouldSatisfyAllConditions(
                            sdsd => sdsd.Name.ShouldBe(sireDamSireDam.Name),
                            sdsd => sdsd.Variety.ShouldBe(sireDamSireDam.Variety)),
                        sds => sds.Sire.ShouldNotBeNull().ShouldSatisfyAllConditions(
                            sdss => sdss.Name.ShouldBe(sireDamSireSire.Name),
                            sdss => sdss.Variety.ShouldBe(sireDamSireSire.Variety)))),
                s => s.Sire.ShouldNotBeNull().ShouldSatisfyAllConditions(
                    ss => ss.Name.ShouldBe(sireSire.Name),
                    ss => ss.Variety.ShouldBe(sireSire.Variety),
                    ss => ss.Dam.ShouldNotBeNull().ShouldSatisfyAllConditions(
                        ssd => ssd.Name.ShouldBe(sireSireDam.Name),
                        ssd => ssd.Variety.ShouldBe(sireSireDam.Variety),
                        ssd => ssd.Dam.ShouldNotBeNull().ShouldSatisfyAllConditions(
                            ssdd => ssdd.Name.ShouldBe(sireSireDamDam.Name),
                            ssdd => ssdd.Variety.ShouldBe(sireSireDamDam.Variety)),
                        ssd => ssd.Sire.ShouldNotBeNull().ShouldSatisfyAllConditions(
                            ssds => ssds.Name.ShouldBe(sireSireDamSire.Name),
                            ssds => ssds.Variety.ShouldBe(sireSireDamSire.Variety))),
                    ss => ss.Sire.ShouldNotBeNull().ShouldSatisfyAllConditions(
                        sss => sss.Name.ShouldBe(sireSireSire.Name),
                        sss => sss.Variety.ShouldBe(sireSireSire.Variety),
                        sss => sss.Dam.ShouldNotBeNull().ShouldSatisfyAllConditions(
                            sssd => sssd.Name.ShouldBe(sireSireSireDam.Name),
                            sssd => sssd.Variety.ShouldBe(sireSireSireDam.Variety)),
                        sss => sss.Sire.ShouldNotBeNull().ShouldSatisfyAllConditions(
                            ssss => ssss.Name.ShouldBe(sireSireSireSire.Name),
                            ssss => ssss.Variety.ShouldBe(sireSireSireSire.Variety))))));
    }

    private async Task CreateLitter(Rat dam, Rat sire, Rat rat)
    {
        var litter = await Litter.Create(this.littersRepository);
        await litter.AddOffspring(this.littersRepository, rat);
        await litter.SetDam(this.littersRepository, dam);
        await litter.SetSire(this.littersRepository, sire);
    }

    // private static void ShouldMatch(Node? node, Rat rat)
    // {
    //     node.ShouldNotBeNull().ShouldSatisfyAllConditions(
    //         () => node.Name.ShouldBe(rat.Name),
    //         () => node.Variety.ShouldBe(rat.Variety));
    // }
}