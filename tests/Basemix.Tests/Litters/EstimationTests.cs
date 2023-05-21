using Basemix.Lib.Litters;
using Bogus;
using Shouldly;

namespace Basemix.Tests.Litters;

public class EstimationTests
{
    private readonly Faker faker = new();
    
    [Fact]
    public void Is_empty_if_nothing_set()
    {
        new Estimation().IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void Is_not_empty_when_something_set()
    {
        new Estimation {EarliestDateOfBirth = this.faker.Date.FutureDateOnly()}.IsEmpty.ShouldBeFalse();
        new Estimation {LatestDateOfBirth = this.faker.Date.FutureDateOnly()}.IsEmpty.ShouldBeFalse();
        new Estimation {EarliestFullyWeanedDate = this.faker.Date.FutureDateOnly()}.IsEmpty.ShouldBeFalse();
        new Estimation {EarliestSeparateSexesDate = this.faker.Date.FutureDateOnly()}.IsEmpty.ShouldBeFalse();
        new Estimation {EarliestRehomeDate = this.faker.Date.FutureDateOnly()}.IsEmpty.ShouldBeFalse();
    }
    
    [Fact]
    public void Is_relevant_if_earliest_rehome_date_plus_2_weeks_is_today_or_in_the_future()
    {
        var now = this.faker.Date.RecentDateOnly();
        var daysAgo = this.faker.Random.Int(0, 14);
        var estimation = new Estimation {EarliestRehomeDate = now.AddDays(-daysAgo)};
        estimation.IsRelevant(() => now).ShouldBeTrue();
    }

    [Fact]
    public void Is_not_relevant_if_earliest_rehome_date_is_over_two_weeks_in_past()
    {
        var now = this.faker.Date.RecentDateOnly();
        var estimation = new Estimation {EarliestRehomeDate = now.AddDays(-15)};
        estimation.IsRelevant(() => now).ShouldBeFalse();
    }
}