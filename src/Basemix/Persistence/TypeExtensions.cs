namespace Basemix.Persistence;

public static class TypeExtensions
{
    public static long ToPersistedDateTime(this DateTime date) =>
        new DateTimeOffset(date).ToUnixTimeSeconds();

    public static long ToPersistedDateTime(this DateOnly date) =>
        new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToUnixTimeSeconds();

    public static DateTime ToDateTime(this long date) =>
        DateTimeOffset.FromUnixTimeSeconds(date).DateTime;

    public static DateOnly ToDateOnly(this long date) =>
        DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(date).Date);
}