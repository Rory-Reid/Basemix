using Basemix.Lib.Litters;
using Basemix.Lib.Rats;
using Bogus;
using Bogus.DataSets;

namespace Basemix.Tests.sdk;

/// <summary>
/// Until we get too many, they all go here
/// </summary>
public static class FakerExtensions
{
    public static long Id(this Faker faker) => faker.Random.Long(1);

    public static T PickNonDefault<T>(this Faker faker) where T : struct, Enum =>
        faker.PickRandom(Enum.GetValues<T>().Except(new[] {default(T)}));

    private static string[] Varieties =
    {
        // TODO build a better "Variety builder" - these are more categories than exhaustive varieties
        // Self
        "Pink eyed white", "Champagne", "Buff", "Platinum", "Quicksilver", "British Blue", "Black", "Chocolate", "Mink",
        "Ivory",
        // Marked
        "Berkshire", "Badger", "Irish", "Hooded", "Variegated", "Capped", "Essex", "Blazed Essex", "Chinchilla",
        "Squirrel", "Roan", "Striped Roan",
        // Russian
        "Russian Blue", "Russian Dove", "Russian Blue Agouti", "Russian Topaz",
        // Shaded
        "Argente Crème", "Himalayan", "Siamese", "Blue Point Siamese", "Burmese", "Wheaten Burmese", "Golden Himalayan",
        "Marten", "Silver Agouti",
        // AOV
        "Cream", "Topaz", "Silver Fawn", "Silver", "Agouti", "Cinnamon", "British Blue Agouti", "Lilac Agouti", "Pearl",
        "Cinnamon Pearl", "Platinum Agouti",
        // Rex/Dumbo
        "Rex", "Dumbo",
        // Guide standard
        "Cream Agouti", "Golden Siamese", "Lilac", "Russian Dove", "Russian Silver", "Russian Silver Agouti",
        "Sable Burmese", "Spotted Downunder",
        // Provisional
        "Bareback", "Blue Point Himalayan", "Cinnamon Chinchilla", "Copper", "Dwarf", "Havana", "Havana Agouti",
        "Merle", "Powder Blue", "Pink Eyed Ivory", "Russian Buff", "Russian Burmese", "Russian Pearl", "Satin",
        "Silken", "Variegated Downunder",
    };
    
    public static string Variety(this Faker faker) =>
        faker.PickRandom(Varieties);

    public static Rat Rat(this Faker faker, RatIdentity? id = null, string? name = null, Sex? sex = null,
        DateOnly? dateOfBirth = null, DateOnly? dateOfDeath = null, bool? owned = true)
    {
        var ratSex = sex ?? faker.PickNonDefault<Sex>();
        var ratName = name ?? ratSex switch
        {
            Sex.Buck => faker.Name.FirstName(Name.Gender.Male),
            Sex.Doe => faker.Name.FirstName(Name.Gender.Female),
            _ => throw new ArgumentOutOfRangeException()
        };

        return new Rat(id: id, name: ratName, sex: ratSex, variety: faker.Variety(),
            dateOfBirth: dateOfBirth ?? faker.Date.PastDateOnly(1))
        {
            Notes = faker.PickRandom(null, faker.Lorem.Paragraphs()),
            DateOfDeath = dateOfDeath,
            Owned = owned ?? faker.Random.Bool()
        };
    }

    public static Litter BlankLitter(this Faker faker, LitterIdentity? id = null) =>
        faker.Litter(id: id, null, null, null, 0, 0, 0, 0);

    public static Litter Litter(this Faker faker, LitterIdentity? id = null,
        (RatIdentity, string?)? dam = null, (RatIdentity, string?)? sire = null,
        List<Rat>? offspring = null,
        float damProbability = 0.5f, float sireProbability = 0.5f,
        int minimumOffspring = 0, int maximumOffspring = 12)
    {
        var hasDam = faker.Random.Bool(damProbability);
        var hasSire = faker.Random.Bool(sireProbability);
        return new Litter(
            identity: id,
            dam: dam ?? (hasDam ? new(faker.Id(), faker.Name.FirstName(Name.Gender.Female)) : null),
            sire: sire ?? (hasSire ? new(faker.Id(), faker.Name.FirstName(Name.Gender.Male)) : null),
            dateOfBirth: faker.Date.PastDateOnly(),
            offspring: offspring?.Select(x => new Offspring(x.Id, x.Name)).ToList() ?? faker.Make(faker.Random.Int(minimumOffspring, maximumOffspring),
                _ => new Offspring(faker.Id(), faker.Name.FirstName())).ToList());
    }
}