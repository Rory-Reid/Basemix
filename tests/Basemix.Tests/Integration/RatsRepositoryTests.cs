using Basemix.Rats;
using Basemix.Rats.Persistence;
using Basemix.Tests.sdk;
using Shouldly;

namespace Basemix.Tests.Integration;

public class RatRepositoryTests : SqliteIntegration
{
    private readonly RatsRepository repo;

    public RatRepositoryTests(SqliteFixture fixture) : base(fixture)
    {
        this.repo = new RatsRepository(fixture.GetConnection);
    }
    
    [Fact]
    public async Task Can_set_and_get_rat()
    {
        var rat = new Rat("Otis", Sex.Buck, new DateOnly(2017, 03, 20));

        var id = await this.repo.AddRat(rat);
        var storedRat = await this.repo.GetRat(id);
        
        storedRat.ShouldSatisfyAllConditions(
            () => storedRat.Id.Value.ShouldBe(id),
            () => storedRat.Name.ShouldBe(rat.Name),
            () => storedRat.DateOfBirth.ShouldBe(rat.DateOfBirth),
            () => storedRat.Sex.ShouldBe(rat.Sex));
    }
}
