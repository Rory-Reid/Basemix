using Basemix.Rats;

namespace Basemix.Tests.sdk;

public static class SqliteFixtureExtensions
{
    public static Task<long> Seed(this SqliteFixture fixture, Rat rat) =>
        fixture.RatsRepository.AddRat(rat);
}