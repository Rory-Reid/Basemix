using Basemix.Lib.Litters;
using Bogus;
using Shouldly;

namespace Basemix.Tests.Litters;

/// <summary>
/// These tests use data I've mostly pulled from the rat community
/// </summary>
public class StandardParameterLitterEstimatorTests
{
    private readonly Faker faker = new();
    private readonly LitterEstimator estimator = new(() => EstimationParameters.Standard);

    [Fact]
    public async Task Minimum_date_of_birth_should_be_21_days_after_date_of_pairing()
    {
        var dateOfPairing = this.faker.Date.RecentDateOnly();
        var litter = new Litter {DateOfPairing = dateOfPairing};
        (await this.estimator.EstimateFor(litter)).EarliestDateOfBirth.ShouldBe(dateOfPairing.AddDays(21));
    }

    [Fact]
    public async Task Maximum_date_of_birth_should_be_23_days_after_date_of_pairing()
    {
        var dateOfPairing = this.faker.Date.RecentDateOnly();
        var litter = new Litter {DateOfPairing = dateOfPairing};
        (await this.estimator.EstimateFor(litter)).LatestDateOfBirth.ShouldBe(dateOfPairing.AddDays(23));
    }
    
    [Fact]
    public async Task Earliest_fully_weaned_date_should_be_3_weeks_4_days_after_date_of_birth()
    {
        var dateOfBirth = this.faker.Date.RecentDateOnly();
        var litter = new Litter {DateOfBirth = dateOfBirth};
        (await this.estimator.EstimateFor(litter)).EarliestFullyWeanedDate.ShouldBe(dateOfBirth.AddDays(3 * 7).AddDays(4));
    }

    [Fact]
    public async Task Earliest_separate_sexes_date_should_be_4_weeks_3_days_after_date_of_birth()
    {
        var dateOfBirth = this.faker.Date.RecentDateOnly();
        var litter = new Litter {DateOfBirth = dateOfBirth};
        (await this.estimator.EstimateFor(litter)).EarliestSeparateSexesDate.ShouldBe(dateOfBirth.AddDays(4 * 7).AddDays(3));
    }
    
    [Fact]
    public async Task Earliest_rehome_date_should_be_6_weeks_after_date_of_birth()
    {
        var dateOfBirth = this.faker.Date.RecentDateOnly();
        var litter = new Litter {DateOfBirth = dateOfBirth};
        (await this.estimator.EstimateFor(litter)).EarliestRehomeDate.ShouldBe(dateOfBirth.AddDays(6 * 7));
    }
}