using Basemix.Lib.Owners;
using Basemix.Lib.Rats;

namespace Basemix.Tests.sdk;

public static class SqliteFixtureExtensions
{
    public static async Task<Rat> Seed(this SqliteFixture fixture, Rat rat, Owner? owner = null)
    {
        var id = await fixture.RatsRepository.AddRat(rat);
        var seededRat = CopyOf(rat, id);
        if (owner != null)
        {
            seededRat.Owned = false;
            await seededRat.SetOwner(fixture.RatsRepository, owner);
        }
        
        return seededRat;
    }

    private static Rat CopyOf(Rat rat, RatIdentity? id)
    {
        return new Rat(
            id: id ?? rat.Id,
            name: rat.Name,
            sex: rat.Sex,
            variety: rat.Variety,
            dateOfBirth: rat.DateOfBirth,
            litters: rat.Litters,
            ownerId: rat.OwnerId,
            ownerName: rat.OwnerName)
        {
            Notes = rat.Notes
        };
    }
}