using Basemix.Rats.Persistence;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.Integration;

public class RatRepositoryTests : SqliteIntegration
{
    private readonly Faker faker = new();
    private readonly RatsRepository repository;

    public RatRepositoryTests(SqliteFixture fixture) : base(fixture)
    {
        this.repository = new RatsRepository(fixture.GetConnection);
    }
    
    [Fact]
    public async Task Can_set_and_get_rat()
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
}
