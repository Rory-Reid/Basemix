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

    public static Rat Rat(this Faker faker)
    {
        var sex = faker.PickNonDefault<Sex>();
        var name = sex switch
        {
            Sex.Buck => faker.Name.FirstName(Name.Gender.Male),
            Sex.Doe => faker.Name.FirstName(Name.Gender.Female),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        return new(name, sex, faker.Date.PastDateOnly(1))
        {
            Notes = faker.Lorem.Paragraphs()
        };
    }
}