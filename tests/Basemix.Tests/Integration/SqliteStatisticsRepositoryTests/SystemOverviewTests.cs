using Basemix.Lib.Litters;
using Basemix.Lib.Statistics.Persistence;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.Integration.SqliteStatisticsRepositoryTests;

public class SystemOverviewTests : StatsSqliteIntegration
{
    private readonly Faker faker = new();
    private readonly StatsSqliteFixture fixture;
    private readonly SqliteStatisticsRepository repository;
    
    public SystemOverviewTests(StatsSqliteFixture fixture) : base(fixture)
    {
        this.fixture = fixture;
        this.repository = new SqliteStatisticsRepository(fixture.GetConnection);
    }

    [Fact]
    public async Task Can_get_stats_for_new_database()
    {
        var statistics = await this.repository.GetStatisticsOverview();
        statistics.System.ShouldSatisfyAllConditions(
            stats => stats.DatabaseCreatedOn.Date.ShouldBe(DateTime.UtcNow.Date),
            stats => stats.TotalRats.ShouldBe(0),
            stats => stats.TotalOwnedRats.ShouldBe(0),
            stats => stats.TotalOwners.ShouldBe(0),
            stats => stats.TotalLitters.ShouldBe(0),
            stats => stats.TotalBredLitters.ShouldBe(0));
    }

    [Fact]
    public async Task Can_get_rat_count()
    {
        var ownedRatCount = this.faker.Random.Int(1, 10);
        var unownedRatCount = this.faker.Random.Int(1, 10);
        var makeRats = this.faker.Make(ownedRatCount, async () =>
        {
            var rat = await this.fixture.Seed(this.faker.Rat());
            rat.Owned = true;
            await this.fixture.RatsRepository.UpdateRat(rat); // TODO make seeing better so 'owned' works without update
        });
        var makeUnownedRats = this.faker.Make(unownedRatCount,  async () =>
        {
            var rat = await this.fixture.Seed(this.faker.Rat());
            rat.Owned = false;
            await this.fixture.RatsRepository.UpdateRat(rat); // TODO make seeing better so 'owned' works without update
        });
        await Task.WhenAll(makeRats);
        await Task.WhenAll(makeUnownedRats);
        
        var statistics = await this.repository.GetStatisticsOverview();
        statistics.System.ShouldSatisfyAllConditions(
            stats => stats.TotalRats.ShouldBe(ownedRatCount + unownedRatCount),
            stats => stats.TotalOwnedRats.ShouldBe(ownedRatCount));
    }
    
    [Fact]
    public async Task Can_get_owner_count()
    {
        var ownerCount = this.faker.Random.Int(1, 10);
        var makeOwners = this.faker.Make(ownerCount, () => this.fixture.Seed(this.faker.Owner()));
        await Task.WhenAll(makeOwners);
        
        var statistics = await this.repository.GetStatisticsOverview();
        statistics.System.TotalOwners.ShouldBe(ownerCount);
    }

    [Fact]
    public async Task Can_get_litter_count()
    {
        var bredLitterCount = this.faker.Random.Int(1, 10);
        var notBredLitterCount = this.faker.Random.Int(1, 10);
        
        var makeBredLitters = this.faker.Make(bredLitterCount,  async () =>
        {
            var litter = new Litter(await this.fixture.LittersRepository.CreateLitter()) {BredByMe = true};
            await this.fixture.LittersRepository.UpdateLitter(litter);
        });
        var makeNotBredLitters = this.faker.Make(notBredLitterCount,  async () =>
        {
            var litter = new Litter(await this.fixture.LittersRepository.CreateLitter()) {BredByMe = false};
            await this.fixture.LittersRepository.UpdateLitter(litter);
        });
        
        await Task.WhenAll(makeBredLitters);
        await Task.WhenAll(makeNotBredLitters);
        
        var statistics = await this.repository.GetStatisticsOverview();
        statistics.System.ShouldSatisfyAllConditions(
            stats => stats.TotalLitters.ShouldBe(bredLitterCount + notBredLitterCount),
            stats => stats.TotalBredLitters.ShouldBe(bredLitterCount));
    }
}