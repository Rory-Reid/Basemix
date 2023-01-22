using System.Runtime.CompilerServices;
using Basemix.Litters;
using Basemix.Rats;
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
        faker.PickRandom(Enum.GetValues<T>().Except(new [] {default(T)}));

    public static Rat Rat(this Faker faker, RatIdentity? id = null, string? name = null, Sex? sex = null)
    {
        var ratSex = sex ?? faker.PickNonDefault<Sex>();
        var ratName = name ?? ratSex switch
        {
            Sex.Buck => faker.Name.FirstName(Name.Gender.Male),
            Sex.Doe => faker.Name.FirstName(Name.Gender.Female),
            _ => throw new ArgumentOutOfRangeException()
        };

        return new Rat(id: id, name: ratName, sex: ratSex, dateOfBirth: faker.Date.PastDateOnly(1))
        {
            Notes = faker.PickRandom(null, faker.Lorem.Paragraphs())
        };
    }

    public static Litter BlankLitter(this Faker faker, LitterIdentity? id = null) =>
        faker.Litter(id: id, null, null, 0, 0, 0, 0);
    
    public static Litter Litter(this Faker faker, LitterIdentity? id = null,
        (RatIdentity, string?)? dam = null, (RatIdentity, string?)? sire = null,
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
            offspring: faker.Make(faker.Random.Int(minimumOffspring, maximumOffspring), _ => new Offspring(faker.Id(), faker.Name.FirstName())).ToList());
    }
}