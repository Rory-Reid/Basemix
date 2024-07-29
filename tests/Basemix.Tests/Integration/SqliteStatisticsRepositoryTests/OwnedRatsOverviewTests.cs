using Basemix.Lib.Persistence;
using Basemix.Lib.Rats;
using Basemix.Lib.Statistics.Persistence;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.Integration.SqliteStatisticsRepositoryTests;

public class OwnedRatsOverviewTests : StatsSqliteIntegration
{
    private readonly Faker faker = new();
    private readonly StatsSqliteFixture fixture;
    private readonly SqliteStatisticsRepository repository;

    public OwnedRatsOverviewTests(StatsSqliteFixture fixture) : base(fixture)
    {
        this.fixture = fixture;
        this.repository = new SqliteStatisticsRepository(fixture.GetConnection);
    }

    [Fact]
    public async Task Returns_stats_for_new_db()
    {
        var statistics = await this.repository.GetStatisticsOverview();
        
        statistics.OwnedRats.ShouldSatisfyAllConditions(
            stats => stats.Total.ShouldBe(0),
            stats => stats.TotalBucks.ShouldBe(0),
            stats => stats.TotalDoes.ShouldBe(0),
            stats => stats.TotalNotRecordedDead.ShouldBe(0),
            stats => stats.LongestLife.ShouldBe(TimeSpan.Zero),
            stats => stats.AverageAge.ShouldBe(TimeSpan.Zero),
            stats => stats.MostCommonVariety.ShouldBeNullOrEmpty(),
            stats => stats.MostCommonVarietyCount.ShouldBe(0));
    }

    [Fact]
    public async Task Returns_stats_for_bucks_and_does()
    {
        var buckCount = this.faker.Random.Int(1, 10);
        var doeCount = this.faker.Random.Int(1, 10);
        var unknownCount = this.faker.Random.Int(1, 10);
        var notOwnedCount = this.faker.Random.Int(1, 10);
        var bucks = this.faker.Make(buckCount, () => this.faker.Rat(sex: Sex.Buck, owned: true));
        var does = this.faker.Make(doeCount, () => this.faker.Rat(sex: Sex.Doe, owned: true));
        var unknown = this.faker.Make(unknownCount, () => this.faker.Rat(sexProbability: 0, owned: true));
        var notOwned = this.faker.Make(notOwnedCount, () => this.faker.Rat(sexProbability: 0.5f, owned: false));
        var ownedRats = new List<Rat>();
        foreach (var rat in bucks.Concat(does).Concat(unknown))
        {
            ownedRats.Add(await this.fixture.Seed(rat));
        }
        
        foreach (var rat in notOwned)
        {
            await this.fixture.Seed(rat);
        }
        
        var statistics = await this.repository.GetStatisticsOverview();
        statistics.OwnedRats.ShouldSatisfyAllConditions(
            stats => stats.Total.ShouldBe(ownedRats.Count),
            stats => stats.TotalBucks.ShouldBe(buckCount),
            stats => stats.TotalDoes.ShouldBe(doeCount));
    }

    [Fact]
    public async Task Returns_stats_for_dead_and_alive_rats()
    {
        var getDeadRat = (bool owned) =>
        {
            var deathDate = this.faker.Date.RecentDateOnly();
            return this.faker.Rat(
                owned: owned,
                dateOfDeath: deathDate,
                dead: true,
                dateOfBirth: this.faker.Date.PastDateOnly(yearsToGoBack: this.faker.Random.Int(1, 2), refDate: deathDate));
        };
        var getOldRat = (bool owned) => this.faker.Rat(owned: owned, dateOfBirth: this.faker.Date.PastDateOnly(this.faker.Random.Int(10, 30)));
        var getLivingRat = (bool owned) => this.faker.Rat(owned: owned);
        var deadCount = this.faker.Random.Int(1, 10);
        var aliveCount = this.faker.Random.Int(1, 10);
        var extremelyOldCount = this.faker.Random.Int(1, 10); // Just want to ensure the system doesn't infer death
        var notOwnedCount = this.faker.Random.Int(1, 10);
        var dead = this.faker.Make(deadCount, () => getDeadRat(true));
        var alive = this.faker.Make(aliveCount, () => getLivingRat(true));
        var extremelyOld = this.faker.Make(extremelyOldCount, () => getOldRat(true));
        var notOwned = this.faker.Make(notOwnedCount, () => this.faker.PickRandom(getDeadRat(false), getLivingRat(false), getOldRat(false)));
        var ownedRats = new List<Rat>();
        foreach (var rat in dead.Concat(alive).Concat(extremelyOld))
        {
            ownedRats.Add(await this.fixture.Seed(rat));
        }
        
        foreach (var rat in notOwned)
        {
            await this.fixture.Seed(rat);
        }
        
        var longestLife = dead.Max(rat => rat.DateOfDeath!.Value.ToPersistedDateTime() - rat.DateOfBirth!.Value.ToPersistedDateTime());
        var averageAge = dead.Average(rat => rat.DateOfDeath!.Value.ToPersistedDateTime() - rat.DateOfBirth!.Value.ToPersistedDateTime());
        var statistics = await this.repository.GetStatisticsOverview();
        statistics.OwnedRats.ShouldSatisfyAllConditions(
            stats => stats.Total.ShouldBe(ownedRats.Count),
            stats => stats.TotalNotRecordedDead.ShouldBe(aliveCount + extremelyOldCount),
            stats => stats.LongestLife.ShouldBe(TimeSpan.FromSeconds(longestLife)),
            stats => stats.AverageAge.ShouldBe(TimeSpan.FromSeconds(averageAge)));
    }

    [Fact]
    public async Task Returns_stats_for_variety()
    {
        var varieties = this.faker.Make(this.faker.Random.Int(2, 5), () => this.faker.Variety());
        var owned = this.faker.Make(50, () => this.faker.Rat(owned: true));
        var notOwned = this.faker.Make(50, () => this.faker.Rat(owned: false));
        var ownedRats = new List<Rat>();
        foreach (var rat in owned)
        {
            var seededRat = await this.fixture.Seed(rat);
            seededRat.Variety = this.faker.PickRandom(varieties);
            await seededRat.Save(this.fixture.RatsRepository);
            ownedRats.Add(seededRat);
        }
        
        foreach (var rat in notOwned)
        {
            var seededRat = await this.fixture.Seed(rat);
            seededRat.Variety = this.faker.PickRandom(varieties);
            await seededRat.Save(this.fixture.RatsRepository);
        }
        
        var mostCommonVariety = ownedRats
            .GroupBy(rat => rat.Variety)
            .OrderByDescending(group => group.Count())
            .ThenByDescending(group => group.Key)
            .First()
            .Key;
        
        var statistics = await this.repository.GetStatisticsOverview();
        statistics.OwnedRats.ShouldSatisfyAllConditions(
            stats => stats.MostCommonVariety.ShouldBe(mostCommonVariety),
            stats => stats.MostCommonVarietyCount.ShouldBe(ownedRats.Count(rat => rat.Variety == mostCommonVariety)));
    }
}