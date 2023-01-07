namespace Basemix.Tests.sdk;

public static class NullableExtensions
{
    public static T? AsNullable<T>(this T obj) => (T?)obj;
}