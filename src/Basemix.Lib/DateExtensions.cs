using System.Globalization;

namespace Basemix.Lib;

public static class DateExtensions
{
    public static string ToLocalizedString(this DateOnly? date)
    {
        return date?.ToString("d", CultureInfo.CurrentCulture) ?? "-";
    }

    public static string ToLocalizedString(this DateOnly date)
    {
        return date.ToString("d", CultureInfo.CurrentCulture);
    }

    public static string ToLocalizedString(this DateTime date)
    {
        return date.ToString("d", CultureInfo.CurrentCulture);
    }

    public static string ToLocalizedPdfString(this DateOnly? date)
    {
        return date?.ToString("d", CultureInfo.CurrentCulture) + "." ?? "";
    }
}