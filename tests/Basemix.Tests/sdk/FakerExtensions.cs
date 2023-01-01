using Bogus;

namespace Basemix.Tests.sdk;

/// <summary>
/// Until we get too many, they all go here
/// </summary>
public static class FakerExtensions
{
    public static long Id(this Faker faker) => faker.Random.Long(1);

    public static T PickNonDefault<T>(this Faker faker) where T : struct, Enum =>
        faker.PickRandom(Enum.GetValues<T>().Except(new [] {default(T)}));
}