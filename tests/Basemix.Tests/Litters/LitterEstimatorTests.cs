using Basemix.Lib.Litters;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.Litters;

public class LitterEstimatorTests
{
    private readonly Faker faker = new();
    private readonly EstimationParameters parameters;
    private readonly LitterEstimator estimator;

    public LitterEstimatorTests()
    {
        this.parameters = this.faker.EstimationParameters();
        this.estimator = new LitterEstimator(() => this.parameters);
    }

    [Fact]
    public void Estimations_should_be_null_for_no_date_of_pairing_or_birth()
    {
        var litter = new Litter {DateOfBirth = null, DateOfPairing = null};
        this.estimator.EstimateFor(litter).ShouldSatisfyAllConditions(
            estimate => estimate.EarliestDateOfBirth.ShouldBeNull(),
            estimate => estimate.LatestDateOfBirth.ShouldBeNull(),
            estimate => estimate.EarliestFullyWeanedDate.ShouldBeNull(),
            estimate => estimate.EarliestSeparateSexesDate.ShouldBeNull(),
            estimate => estimate.EarliestRehomeDate.ShouldBeNull());
    }

    [Fact]
    public void Estimation_should_have_all_values_when_date_of_pairing_set()
    {
        var litter = new Litter {DateOfBirth = null, DateOfPairing = this.faker.Date.PastDateOnly()};
        this.estimator.EstimateFor(litter).ShouldSatisfyAllConditions(
            estimate => estimate.EarliestDateOfBirth.ShouldNotBeNull(),
            estimate => estimate.LatestDateOfBirth.ShouldNotBeNull(),
            estimate => estimate.EarliestFullyWeanedDate.ShouldNotBeNull(),
            estimate => estimate.EarliestSeparateSexesDate.ShouldNotBeNull(),
            estimate => estimate.EarliestRehomeDate.ShouldNotBeNull());
    }
    
    [Fact]
    public void Estimation_should_only_have_values_after_birth_when_date_of_birth_set()
    {
        var litter = new Litter {DateOfBirth = this.faker.Date.PastDateOnly(), DateOfPairing = null};
        this.estimator.EstimateFor(litter).ShouldSatisfyAllConditions(
            estimate => estimate.EarliestDateOfBirth.ShouldBeNull(),
            estimate => estimate.LatestDateOfBirth.ShouldBeNull(),
            estimate => estimate.EarliestFullyWeanedDate.ShouldNotBeNull(),
            estimate => estimate.EarliestSeparateSexesDate.ShouldNotBeNull(),
            estimate => estimate.EarliestRehomeDate.ShouldNotBeNull());
    }
    
    [Fact]
    public void Earliest_date_of_birth_should_be_min_days_after_date_of_pairing()
    {
        var dateOfPairing = this.faker.Date.RecentDateOnly();
        var litter = new Litter {DateOfPairing = dateOfPairing};
        this.estimator.EstimateFor(litter)
            .EarliestDateOfBirth.ShouldBe(dateOfPairing.AddDays(this.parameters.MinBirthDaysAfterPairing));
    }
    
    [Fact]
    public void Latest_date_of_birth_should_be_max_days_after_date_of_pairing()
    {
        var dateOfPairing = this.faker.Date.RecentDateOnly();
        var litter = new Litter {DateOfPairing = dateOfPairing};
        this.estimator.EstimateFor(litter)
            .LatestDateOfBirth.ShouldBe(dateOfPairing.AddDays(this.parameters.MaxBirthDaysAfterPairing));
    }
    
    [Fact]
    public void Earliest_fully_weaned_date_should_be_min_days_after_date_of_birth()
    {
        var dateOfBirth = this.faker.Date.RecentDateOnly();
        var litter = new Litter {DateOfBirth = dateOfBirth};
        this.estimator.EstimateFor(litter)
            .EarliestFullyWeanedDate.ShouldBe(dateOfBirth.AddDays(this.parameters.MinWeaningDaysAfterBirth));
    }
    
    [Fact]
    public void Earliest_separation_date_should_be_min_days_after_date_of_birth()
    {
        var dateOfBirth = this.faker.Date.RecentDateOnly();
        var litter = new Litter {DateOfBirth = dateOfBirth};
        this.estimator.EstimateFor(litter)
            .EarliestSeparateSexesDate.ShouldBe(dateOfBirth.AddDays(this.parameters.MinSeparationDaysAfterBirth));
    }
    
    [Fact]
    public void Earliest_rehome_date_should_be_min_days_after_date_of_birth()
    {
        var dateOfBirth = this.faker.Date.RecentDateOnly();
        var litter = new Litter {DateOfBirth = dateOfBirth};
        this.estimator.EstimateFor(litter)
            .EarliestRehomeDate.ShouldBe(dateOfBirth.AddDays(this.parameters.MinRehomeDaysAfterBirth));
    }

    [Fact]
    public void Earliest_fully_weaned_date_should_use_midpoint_estimate_for_date_of_birth_then_add_minimum()
    {
        var midpoint = (this.parameters.MaxBirthDaysAfterPairing - this.parameters.MinBirthDaysAfterPairing) / 2;
        var dateOfPairing = this.faker.Date.RecentDateOnly();
        var litter = new Litter {DateOfPairing = dateOfPairing};
        this.estimator.EstimateFor(litter)
            .EarliestFullyWeanedDate.ShouldBe(
                dateOfPairing
                    .AddDays(this.parameters.MinBirthDaysAfterPairing)
                    .AddDays(midpoint)
                    .AddDays(this.parameters.MinWeaningDaysAfterBirth));
    }
    
    [Fact]
    public void Earliest_separation_date_should_use_midpoint_estimate_for_date_of_birth_then_add_minimum()
    {
        var midpoint = (this.parameters.MaxBirthDaysAfterPairing - this.parameters.MinBirthDaysAfterPairing) / 2;
        var dateOfPairing = this.faker.Date.RecentDateOnly();
        var litter = new Litter {DateOfPairing = dateOfPairing};
        this.estimator.EstimateFor(litter)
            .EarliestSeparateSexesDate.ShouldBe(
                dateOfPairing
                    .AddDays(this.parameters.MinBirthDaysAfterPairing)
                    .AddDays(midpoint)
                    .AddDays(this.parameters.MinSeparationDaysAfterBirth));
    }

    [Fact]
    public void Earliest_rehome_date_should_use_midpoint_estimate_for_date_of_birth_then_add_minimum()
    {
        var midpoint = (this.parameters.MaxBirthDaysAfterPairing - this.parameters.MinBirthDaysAfterPairing) / 2;
        var dateOfPairing = this.faker.Date.RecentDateOnly();
        var litter = new Litter {DateOfPairing = dateOfPairing};
        this.estimator.EstimateFor(litter)
            .EarliestRehomeDate.ShouldBe(
                dateOfPairing
                    .AddDays(this.parameters.MinBirthDaysAfterPairing)
                    .AddDays(midpoint)
                    .AddDays(this.parameters.MinRehomeDaysAfterBirth));
    }
}