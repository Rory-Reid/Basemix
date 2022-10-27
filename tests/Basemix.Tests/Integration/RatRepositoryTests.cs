using Basemix.Tests.sdk;
using Shouldly;

namespace Basemix.Tests.Integration;

public class RatRepositoryTests : SqliteIntegration
{
    private readonly SqliteFixture fixture;
    private readonly RatRepository repo;

    public RatRepositoryTests(SqliteFixture fixture) : base(fixture)
    {
        this.fixture = fixture;
        this.repo = new RatRepository(this.fixture.GetConnection);
    }
    
    [Fact]
    public async Task Can_set_and_get_rat()
    {
        var rat = new Rat
        {
            Name = "Otis",
            DateOfBirth = new DateTimeOffset(new DateTime(2017, 03, 20)).ToUnixTimeSeconds(),
            Sex = "buck"
        };

        var id = await this.repo.AddRat(rat);
        var storedRat = await this.repo.GetRat(id);
        
        storedRat.ShouldBeEquivalentTo(rat);
    }
}
