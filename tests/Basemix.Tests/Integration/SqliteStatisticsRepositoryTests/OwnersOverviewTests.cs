using Basemix.Lib.Litters;
using Basemix.Lib.Owners;
using Basemix.Lib.Rats;
using Basemix.Lib.Statistics.Persistence;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.Integration.SqliteStatisticsRepositoryTests;

public class OwnersOverviewTests : StatsSqliteIntegration
{
    private readonly Faker faker = new();
    private readonly StatsSqliteFixture fixture;
    private readonly SqliteStatisticsRepository repository;

    public OwnersOverviewTests(StatsSqliteFixture fixture) : base(fixture)
    {
        this.fixture = fixture;
        this.repository = new SqliteStatisticsRepository(fixture.GetConnection);
    }

    [Fact]
    public async Task Returns_stats_for_new_db()
    {
        var statistics = await this.repository.GetStatisticsOverview();
        
        statistics.Owners.TopOwners.ShouldBeEmpty();
    }
    
    [Fact]
    public async Task Returns_top_owners_from_bred_litters()
    {
        var owners = new List<Owner>();
        for (var i = 0; i < 4; i++)
        {
            owners.Add(await this.fixture.Seed(this.faker.Owner()));
        }
        
        var ownedCount = this.faker.Random.Int(1, 10);
        var unownedCount = this.faker.Random.Int(1, 10);
        var unownedWithOwnerCount = this.faker.Random.Int(1, 10);
        var owned = this.faker.Make(ownedCount, () => this.faker.Rat(owned: true));
        var unowned = this.faker.Make(unownedCount, () => this.faker.Rat(owned: false));
        var unownedWithOwner = this.faker.Make(unownedWithOwnerCount,
            () => this.faker.Rat(owned: false, owner: this.faker.PickRandom(owners)));
        var bredRats = new List<Rat>();
        foreach (var rat in owned.Concat(unowned).Concat(unownedWithOwner))
        {
            bredRats.Add(await this.fixture.Seed(rat));
        }

        // Distribute rats across two litters to ensure stats are calculated using all litters
        var bredLitter1 = new Litter(await this.fixture.LittersRepository.CreateLitter());
        var bredLitter2 = new Litter(await this.fixture.LittersRepository.CreateLitter());

        bredLitter1.BredByMe = true;
        bredLitter2.BredByMe = true;
        await this.fixture.LittersRepository.UpdateLitter(bredLitter1);
        await this.fixture.LittersRepository.UpdateLitter(bredLitter2);

        var litter1Rats = this.faker.Random.Int(1, bredRats.Count);
        foreach (var rat in bredRats.Take(litter1Rats))
        {
            await bredLitter1.AddOffspring(this.fixture.LittersRepository, rat);
        }

        foreach (var rat in bredRats.Skip(litter1Rats))
        {
            await bredLitter2.AddOffspring(this.fixture.LittersRepository, rat);
        }

        var topOwners = unownedWithOwner
            .GroupBy(rat => rat.OwnerId)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key!.Value)
            .Take(3)
            .ToList();
        
        var statistics = await this.repository.GetStatisticsOverview();
        var owner1 = owners.First(o => o.Id == topOwners[0].Key);
        statistics.Owners.ShouldSatisfyAllConditions(
            stats => stats.TopOwners.Count.ShouldBe(topOwners.Count),
            stats => stats.TopOwners[0].Name.ShouldBe(owner1.Name),
            stats => stats.TopOwners[0].RatCount.ShouldBe(topOwners[0].Count()));
        
        if (topOwners.Count > 1)
        {
            var owner2 = owners.First(o => o.Id == topOwners[1].Key);
            statistics.Owners.ShouldSatisfyAllConditions(
                stats => stats.TopOwners[1].Name.ShouldBe(owner2.Name),
                stats => stats.TopOwners[1].RatCount.ShouldBe(topOwners[1].Count()));
        }
        
        if (topOwners.Count > 2)
        {
            var owner3 = owners.First(o => o.Id == topOwners[2].Key);
            statistics.Owners.ShouldSatisfyAllConditions(
                stats => stats.TopOwners[2].Name.ShouldBe(owner3.Name),
                stats => stats.TopOwners[2].RatCount.ShouldBe(topOwners[2].Count()));
        }
    }
    
    [Fact]
    public async Task Doesnt_count_non_bred_rats_in_top_owners()
    {
        var owner = await this.fixture.Seed(this.faker.Owner());

        var notBredRatsWithOwner = new List<Rat>();
        for (var i = 0; i < 50; i++)
        {
            notBredRatsWithOwner.Add(await this.fixture.Seed(this.faker.Rat(owned: false, owner: owner)));
        }
        
        var notBredLitter = new Litter(await this.fixture.LittersRepository.CreateLitter())
        {
            BredByMe = false
        };
        await this.fixture.LittersRepository.UpdateLitter(notBredLitter);
        
        
        foreach (var rat in notBredRatsWithOwner.Take(this.faker.Random.Int(1, notBredRatsWithOwner.Count))) // Seed some in a non-bred litter, some not connected
        {
            await notBredLitter.AddOffspring(this.fixture.LittersRepository, rat);
        }
        
        var statistics = await this.repository.GetStatisticsOverview();
        statistics.Owners.TopOwners.ShouldBeEmpty();
    }
}