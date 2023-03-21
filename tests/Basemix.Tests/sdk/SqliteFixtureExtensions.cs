using Basemix.Lib.Rats;

namespace Basemix.Tests.sdk;

public static class SqliteFixtureExtensions
{
    public static async Task<Rat> Seed(this SqliteFixture fixture, Rat rat)
    {
        var id = await fixture.RatsRepository.AddRat(rat);
        return CopyOf(rat, id);
    }

    private static Rat CopyOf(Rat rat, RatIdentity? id)
    {
        return new Rat(
            id: id ?? rat.Id,
            name: rat.Name,
            sex: rat.Sex,
            variety: rat.Variety,
            dateOfBirth: rat.DateOfBirth,
            litters: rat.Litters)
        {
            Notes = rat.Notes
        };
    }
}