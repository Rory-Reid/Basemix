using System.Data;

namespace Basemix.Lib;

public delegate IDbConnection GetDatabase();

public delegate string GetDatabasePath();

public delegate DateOnly NowDateOnly();

public delegate string DateSpanToString(DateOnly date1, DateOnly date2);

public static class Delegates
{
    public static string HumaniseDateSpan(DateOnly from, DateOnly to)
    {
        if (from == to)
        {
            return "0 days";
        }

        if (from > to)
        {
            (from, to) = (to, from);
        }
        
        var yearDiff = to.Year - from.Year;
        var monthDiff = to.Month - from.Month;
        var dayDiff = to.Day - from.Day;
        if (dayDiff < 0)
        {
            dayDiff += 30;
            monthDiff--;
        }
        
        if (monthDiff < 0)
        {
            monthDiff += 12;
            yearDiff--;
        }

        if (yearDiff > 0)
        {
            var years = $"{yearDiff} {Pluralise(yearDiff, "year", "years")}";
            if (monthDiff > 0)
            {
                
                return $"{years}, {monthDiff} {Pluralise(monthDiff, "month", "months")}";
            }

            return years;
        }
        
        if (monthDiff > 0)
        {
            var months = $"{monthDiff} {Pluralise(monthDiff, "month", "months")}";
            if (dayDiff > 0)
            {
                return $"{months}, {dayDiff} {Pluralise(dayDiff, "day", "days")}";
            }

            return months;
        }
        
        return $"{dayDiff} {Pluralise(dayDiff, "day", "days")}";
    }
    
    private static string Pluralise(int count, string singular, string plural) =>
        count == 1 ? singular : plural;
}
