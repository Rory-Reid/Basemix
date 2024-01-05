using Basemix.Lib.Litters;
using Basemix.Lib.Persistence;
using Basemix.Lib.Rats;
using Basemix.Lib.Statistics;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.Integration.SqliteStatisticsRepositoryTests;

public class LitterStatsTests : StatsSqliteIntegration
{
    private readonly Faker faker = new();
    private readonly StatsSqliteFixture fixture;
    private readonly SqliteStatisticsRepository repository;

    public LitterStatsTests(StatsSqliteFixture fixture) : base(fixture)
    {
        this.fixture = fixture;
        this.repository = new SqliteStatisticsRepository(fixture.GetConnection);
    }

    [Fact]
    public async Task Returns_stats_for_new_db()
    {
        var statistics = await this.repository.GetStatisticsOverview();
        statistics.BredLitters.ShouldSatisfyAllConditions(
            stats => stats.TotalBredRats.ShouldBe(0),
            stats => stats.TotalBucks.ShouldBe(0),
            stats => stats.TotalDoes.ShouldBe(0),
            stats => stats.TotalNotRecordedDead.ShouldBe(0),
            stats => stats.LongestLife.ShouldBe(TimeSpan.Zero),
            stats => stats.AverageAge.ShouldBe(TimeSpan.Zero),
            stats => stats.AverageGestationDays.ShouldBe(TimeSpan.Zero),
            stats => stats.TotalRehomed.ShouldBe(0),
            stats => stats.SmallestLitter.ShouldBe(0),
            stats => stats.BiggestLitter.ShouldBe(0),
            stats => stats.AverageLitterSize.ShouldBe(0));
    }

    [Fact]
    public async Task Returns_stats_for_bucks_and_does()
    {
        var buckCount = this.faker.Random.Int(1, 10);
        var doeCount = this.faker.Random.Int(1, 10);
        var unknownCount = this.faker.Random.Int(1, 10);
        var totalNotBred = this.faker.Random.Int(1, 10);
        var bucks = this.faker.Make(buckCount, () => this.faker.Rat(sex: Sex.Buck));
        var does = this.faker.Make(doeCount, () => this.faker.Rat(sex: Sex.Doe));
        var unknown = this.faker.Make(unknownCount, () => this.faker.Rat(sexProbability: 0));
        var notBred = this.faker.Make(totalNotBred, () => this.faker.Rat(sexProbability: 0.5f));
        var bredRats = new List<Rat>();
        foreach (var rat in bucks.Concat(does).Concat(unknown))
        {
            bredRats.Add(await this.fixture.Seed(rat));
        }
        
        var notBredRats = new List<Rat>();
        foreach (var rat in notBred)
        {
            notBredRats.Add(await this.fixture.Seed(rat));
        }

        // Distribute rats across two litters to ensure stats are calculated using all litters
        var bredLitter1 = new Litter(await this.fixture.LittersRepository.CreateLitter());
        var bredLitter2 = new Litter(await this.fixture.LittersRepository.CreateLitter());
        var unbredLitter = new Litter(await this.fixture.LittersRepository.CreateLitter());

        bredLitter1.BredByMe = true;
        bredLitter2.BredByMe = true;
        unbredLitter.BredByMe = false;
        await this.fixture.LittersRepository.UpdateLitter(bredLitter1);
        await this.fixture.LittersRepository.UpdateLitter(bredLitter2);
        await this.fixture.LittersRepository.UpdateLitter(unbredLitter);

        var litter1Rats = this.faker.Random.Int(1, bredRats.Count);
        foreach (var rat in bredRats.Take(litter1Rats))
        {
            await bredLitter1.AddOffspring(this.fixture.LittersRepository, rat);
        }

        foreach (var rat in bredRats.Skip(litter1Rats))
        {
            await bredLitter2.AddOffspring(this.fixture.LittersRepository, rat);
        }
        
        foreach (var rat in notBredRats.Take(this.faker.Random.Int(1, notBredRats.Count))) // Seed some in an unbred litter, some not connected
        {
            await unbredLitter.AddOffspring(this.fixture.LittersRepository, rat);
        }
        
        var statistics = await this.repository.GetStatisticsOverview();
        statistics.BredLitters.ShouldSatisfyAllConditions(
            stats => stats.TotalBredRats.ShouldBe(buckCount + doeCount + unknownCount),
            stats => stats.TotalBucks.ShouldBe(buckCount),
            stats => stats.TotalDoes.ShouldBe(doeCount));
    }

    [Fact]
    public async Task Returns_stats_for_dead_and_alive_rats()
    {
        var getDeadRat = () =>
        {
            var deathDate = this.faker.Date.RecentDateOnly();
            return this.faker.Rat(
                dateOfDeath: deathDate,
                dateOfBirth: this.faker.Date.PastDateOnly(yearsToGoBack: this.faker.Random.Int(1, 2), refDate: deathDate));
        };
        var getOldRat = () => this.faker.Rat(dateOfBirth: this.faker.Date.PastDateOnly(this.faker.Random.Int(10, 30)));
        var getLivingRat = () => this.faker.Rat();
        var deadCount = this.faker.Random.Int(1, 10);
        var aliveCount = this.faker.Random.Int(1, 10);
        var extremelyOldCount = this.faker.Random.Int(1, 10); // Just want to ensure the system doesn't infer death
        var totalNotBred = this.faker.Random.Int(1, 10);
        var dead = this.faker.Make(deadCount, getDeadRat);
        var alive = this.faker.Make(aliveCount, getLivingRat);
        var extremelyOld = this.faker.Make(extremelyOldCount, getOldRat);
        var notBred = this.faker.Make(totalNotBred, () => this.faker.PickRandom(getDeadRat, getLivingRat, getOldRat).Invoke());
        var bredRats = new List<Rat>();
        foreach (var rat in dead.Concat(alive).Concat(extremelyOld))
        {
            bredRats.Add(await this.fixture.Seed(rat));
        }
        
        var notBredRats = new List<Rat>();
        foreach (var rat in notBred)
        {
            notBredRats.Add(await this.fixture.Seed(rat));
        }

        // Distribute rats across two litters to ensure stats are calculated using all litters
        var bredLitter1 = new Litter(await this.fixture.LittersRepository.CreateLitter());
        var bredLitter2 = new Litter(await this.fixture.LittersRepository.CreateLitter());
        var unbredLitter = new Litter(await this.fixture.LittersRepository.CreateLitter());

        bredLitter1.BredByMe = true;
        bredLitter2.BredByMe = true;
        unbredLitter.BredByMe = false;
        await this.fixture.LittersRepository.UpdateLitter(bredLitter1);
        await this.fixture.LittersRepository.UpdateLitter(bredLitter2);
        await this.fixture.LittersRepository.UpdateLitter(unbredLitter);

        var litter1Rats = this.faker.Random.Int(1, bredRats.Count);
        foreach (var rat in bredRats.Take(litter1Rats))
        {
            await bredLitter1.AddOffspring(this.fixture.LittersRepository, rat);
        }

        foreach (var rat in bredRats.Skip(litter1Rats))
        {
            await bredLitter2.AddOffspring(this.fixture.LittersRepository, rat);
        }
        
        foreach (var rat in notBredRats.Take(this.faker.Random.Int(1, notBredRats.Count))) // Seed some in an unbred litter, some not connected
        {
            await unbredLitter.AddOffspring(this.fixture.LittersRepository, rat);
        }

        var longestLife = dead.Max(rat => rat.DateOfDeath!.Value.ToPersistedDateTime() - rat.DateOfBirth!.Value.ToPersistedDateTime());
        var averageAge = dead.Average(rat => rat.DateOfDeath!.Value.ToPersistedDateTime() - rat.DateOfBirth!.Value.ToPersistedDateTime());
        var statistics = await this.repository.GetStatisticsOverview();
        statistics.BredLitters.ShouldSatisfyAllConditions(
            stats => stats.TotalBredRats.ShouldBe(deadCount + aliveCount + extremelyOldCount),
            stats => stats.TotalNotRecordedDead.ShouldBe(aliveCount + extremelyOldCount),
            stats => stats.LongestLife.ShouldBe(TimeSpan.FromSeconds(longestLife)),
            stats => stats.AverageAge.ShouldBe(TimeSpan.FromSeconds(averageAge)));
    }

    [Fact]
    public async Task Returns_stats_for_rehomed_rats()
    {
        var owner1 = await this.fixture.Seed(this.faker.Owner());
        var owner2 = await this.fixture.Seed(this.faker.Owner());
        
        var ownedCount = this.faker.Random.Int(1, 10);
        var unownedCount = this.faker.Random.Int(1, 10);
        var unownedWithOwnerCount = this.faker.Random.Int(1, 10);
        var owned = this.faker.Make(ownedCount, () => this.faker.Rat(owned: true));
        var unowned = this.faker.Make(unownedCount, () => this.faker.Rat(owned: false));
        var unownedWithOwner = this.faker.Make(unownedWithOwnerCount,
            () => this.faker.Rat(owned: false, owner: this.faker.PickRandom(owner1, owner2)));
        var bredRats = new List<Rat>();
        foreach (var rat in owned.Concat(unowned).Concat(unownedWithOwner))
        {
            bredRats.Add(await this.fixture.Seed(rat));
        }

        foreach (var rat in unowned)
        {
            rat.Owned = false; // TODO improve seeding so this isn't necessary
            await this.fixture.RatsRepository.UpdateRat(rat);
        }
        
        // Make a not-bred rat in each state
        var notBredRats = new List<Rat>();
        var notBredOwned = await this.fixture.Seed(this.faker.Rat(owned: true));
        notBredRats.Add(notBredOwned);
        var notBredUnowned = await this.fixture.Seed(this.faker.Rat(owned: false));
        notBredUnowned.Owned = false; // TODO improve seeding so this isn't necessary
        await this.fixture.RatsRepository.UpdateRat(notBredUnowned);
        notBredRats.Add(notBredUnowned);
        var notBredUnownedWithOwner = await this.fixture.Seed(this.faker.Rat(owned: false, owner: owner1));
        notBredRats.Add(notBredUnownedWithOwner);

        // Distribute rats across two litters to ensure stats are calculated using all litters
        var bredLitter1 = new Litter(await this.fixture.LittersRepository.CreateLitter());
        var bredLitter2 = new Litter(await this.fixture.LittersRepository.CreateLitter());
        var unbredLitter = new Litter(await this.fixture.LittersRepository.CreateLitter());

        bredLitter1.BredByMe = true;
        bredLitter2.BredByMe = true;
        unbredLitter.BredByMe = false;
        await this.fixture.LittersRepository.UpdateLitter(bredLitter1);
        await this.fixture.LittersRepository.UpdateLitter(bredLitter2);
        await this.fixture.LittersRepository.UpdateLitter(unbredLitter);

        var litter1Rats = this.faker.Random.Int(1, bredRats.Count);
        foreach (var rat in bredRats.Take(litter1Rats))
        {
            await bredLitter1.AddOffspring(this.fixture.LittersRepository, rat);
        }

        foreach (var rat in bredRats.Skip(litter1Rats))
        {
            await bredLitter2.AddOffspring(this.fixture.LittersRepository, rat);
        }
        
        foreach (var rat in notBredRats.Take(this.faker.Random.Int(1, notBredRats.Count))) // Seed some in an unbred litter, some not connected
        {
            await unbredLitter.AddOffspring(this.fixture.LittersRepository, rat);
        }
        
        var statistics = await this.repository.GetStatisticsOverview();
        statistics.BredLitters.TotalRehomed.ShouldBe(unownedCount + unownedWithOwnerCount);
    }

    [Fact]
    public async Task Returns_stats_for_litter_sizes()
    {
        var bred = this.faker.Make(50, () => this.faker.Rat(sex: Sex.Buck));
        var notBred = this.faker.Make(50, () => this.faker.Rat(sexProbability: 0.5f));
        var bredRats = new List<Rat>();
        foreach (var rat in bred)
        {
            bredRats.Add(await this.fixture.Seed(rat));
        }
        
        var notBredRats = new List<Rat>();
        foreach (var rat in notBred)
        {
            notBredRats.Add(await this.fixture.Seed(rat));
        }

        var litters = new List<Litter>();
        for (var i = 0; i < this.faker.Random.Int(1, 5); i++)
        {
            var litter = new Litter(await this.fixture.LittersRepository.CreateLitter())
            {
                BredByMe = true
            };
            await this.fixture.LittersRepository.UpdateLitter(litter);
            litters.Add(litter);
        }
        
        foreach (var rat in bredRats)
        {
            await this.faker.PickRandom(litters).AddOffspring(this.fixture.LittersRepository, rat);
        }
        
        var unbredLitter = new Litter(await this.fixture.LittersRepository.CreateLitter());
        unbredLitter.BredByMe = false;
        await this.fixture.LittersRepository.UpdateLitter(unbredLitter);
        foreach (var rat in notBredRats.Take(this.faker.Random.Int(1, notBredRats.Count))) // Seed some in an unbred litter, some not connected
        {
            await this.fixture.LittersRepository.AddOffspring(unbredLitter.Id, rat.Id);
        }
        
        var statistics = await this.repository.GetStatisticsOverview();
        statistics.BredLitters.ShouldSatisfyAllConditions(
            stats => stats.TotalBredRats.ShouldBe(bredRats.Count),
            stats => stats.SmallestLitter.ShouldBe(litters.Min(l => l.Offspring.Count)),
            stats => stats.BiggestLitter.ShouldBe(litters.Max(l => l.Offspring.Count)),
            stats => stats.AverageLitterSize.ShouldBe(litters.Average(l => l.Offspring.Count)));
    }

    [Fact]
    public async Task Calculates_average_gestation_days()
    {
        var litters = new List<Litter>();
        for (var i = 0; i < this.faker.Random.Int(1, 5); i++)
        {
            var litter = new Litter(await this.fixture.LittersRepository.CreateLitter())
            {
                DateOfBirth = this.faker.Date.RecentDateOnly(),
                DateOfPairing = this.faker.Date.RecentDateOnly(this.faker.Random.Int(20, 24)),
                BredByMe = true
            };
            await this.fixture.LittersRepository.UpdateLitter(litter);
            litters.Add(litter);
        }

        var unbredLitter = new Litter(await this.fixture.LittersRepository.CreateLitter())
        {
            DateOfBirth = this.faker.Date.RecentDateOnly(),
            DateOfPairing = this.faker.Date.RecentDateOnly(this.faker.Random.Int(20, 24)),
            BredByMe = false
        };
        await this.fixture.LittersRepository.UpdateLitter(unbredLitter);
        
        var statistics = await this.repository.GetStatisticsOverview();
        var averageGestation = litters.Average(l => l.DateOfBirth!.Value.ToPersistedDateTime() - l.DateOfPairing!.Value.ToPersistedDateTime());
        statistics.BredLitters.AverageGestationDays.ShouldBe(TimeSpan.FromSeconds(averageGestation));
    }
}