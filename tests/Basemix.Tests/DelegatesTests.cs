using Basemix.Lib;
using Bogus;
using Shouldly;

namespace Basemix.Tests;

public class DelegatesTests
{
    private Faker faker = new();
    
    public record DateSpanTestCase(DateOnly From, DateOnly To, string Expected);
    
    [Fact]
    public void Humanise_datespan_works()
    {
        var testCases = new List<DateSpanTestCase>
        {
            new(new DateOnly(2001, 1, 1), new DateOnly(2001, 1, 2), "1 day"),
            new(new DateOnly(2001, 1, 1), new DateOnly(2001, 1, 3), "2 days"),
            new(new DateOnly(2001, 1, 1), new DateOnly(2001, 2, 1), "1 month"),
            new(new DateOnly(2001, 1, 1), new DateOnly(2001, 2, 2), "1 month, 1 day"),
            new(new DateOnly(2001, 1, 1), new DateOnly(2001, 2, 3), "1 month, 2 days"),
            new(new DateOnly(2001, 1, 1), new DateOnly(2001, 3, 1), "2 months"),
            new(new DateOnly(2001, 1, 1), new DateOnly(2001, 3, 2), "2 months, 1 day"),
            new(new DateOnly(2001, 1, 1), new DateOnly(2001, 3, 3), "2 months, 2 days"),
            new(new DateOnly(2001, 1, 1), new DateOnly(2002, 1, 1), "1 year"),
            new(new DateOnly(2001, 1, 1), new DateOnly(2002, 1, 30), "1 year"), // Don't care about days when doing years
            new(new DateOnly(2001, 1, 1), new DateOnly(2002, 2, 1), "1 year, 1 month"),
            new(new DateOnly(2001, 1, 1), new DateOnly(2002, 3, 1), "1 year, 2 months"),
            new(new DateOnly(2001, 1, 1), new DateOnly(2003, 1, 1), "2 years"),
            new(new DateOnly(2001, 1, 1), new DateOnly(2003, 2, 1), "2 years, 1 month"),
            new(new DateOnly(2001, 1, 1), new DateOnly(2003, 3, 1), "2 years, 2 months"),
        };
        
        foreach (var testCase in testCases)
        {
            Delegates.HumaniseDateSpan(testCase.From, testCase.To).ShouldBe(testCase.Expected);
        }
    }
    
    [Fact]
    public void Humanise_datespan_works_for_same_day()
    {
        var date = this.faker.Date.RecentDateOnly();
        Delegates.HumaniseDateSpan(date, date).ShouldBe("0 days");
    }
    
    [Fact]
    public void Humanise_datespan_swaps_dates()
    {
        var from = this.faker.Date.PastDateOnly();
        var to = this.faker.Date.FutureDateOnly();
        
        Delegates.HumaniseDateSpan(from, to).ShouldBe(Delegates.HumaniseDateSpan(to, from));
    }
}