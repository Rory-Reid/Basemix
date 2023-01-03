namespace Basemix.Persistence;

public static class TypeExtensions
{
    public static long AsPersistedDateTime(this DateTime date) =>
        new DateTimeOffset(date).ToUnixTimeSeconds();

    public static long AsPersistedDateTime(this DateOnly date) =>
        new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToUnixTimeSeconds();

    public static DateTime AsDateTime(this long date) =>
        DateTimeOffset.FromUnixTimeSeconds(date).DateTime;

    public static DateOnly AsDateOnly(this long date) =>
        DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(date).Date);
}