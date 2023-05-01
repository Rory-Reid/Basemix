namespace Basemix.Lib.Ingestion.RatRecordsSpreadsheet;

public record LitterRow(
    string? LitterIdentifier,
    DateOnly? MatingDate,
    int? DayOfBirthCalculated,
    string? TimeOfBirth,
    DateOnly? DateOfBirth,
    double? CurrentAgeMonths,
    int? NumberOfDoes,
    int? NumberOfBucks,
    int? TotalInLitterCalculated,
    int? TotalInLitterIncludingStillbornCalculated,
    int? NumberOfStillborn,
    int? NumberOfInfantDeaths,
    int? CurrentlyAlive,
    int? PassedOn,
    double? PercentageLivingCalculated,
    double? MeanAverageAgeCalculated,
    double? MinimumAge,
    double? MaximumAge)
{
    public bool IsNullLitter =>
        this.LitterIdentifier is null;
};