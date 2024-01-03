using Basemix.Lib.Owners;
using Basemix.Lib.Rats;

namespace Basemix.Tests.sdk;

public static class SqliteFixtureExtensions
{
    public static async Task<Rat> Seed(this ISqliteFixture fixture, Rat rat, Owner? owner = null)
    {
        var id = await fixture.RatsRepository.AddRat(rat);
        var seededRat = CopyOf(rat, id);
        if (owner != null)
        {
            seededRat.Owned = false;
            await seededRat.SetOwner(fixture.RatsRepository, owner);
        }
        
        await seededRat.Save(fixture.RatsRepository);
        return seededRat;
    }

    public static async Task<Owner> Seed(this ISqliteFixture fixture, Owner owner)
    {
        var id = await fixture.OwnersRepository.CreateOwner();
        var seededOwner = CopyOf(owner, id);
        await seededOwner.Save(fixture.OwnersRepository);
        return seededOwner;
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
            Owned = rat.Owned,
            DateOfDeath = rat.DateOfDeath,
            Notes = rat.Notes
        };
    }

    private static Owner CopyOf(Owner owner, OwnerIdentity? id)
    {
        return new Owner(
            identity: id ?? owner.Id)
        {
            Name = owner.Name,
            Email = owner.Email,
            Phone = owner.Phone,
            Notes = owner.Notes
        };
    }
}