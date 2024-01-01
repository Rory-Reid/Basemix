using Basemix.Lib.Settings.Persistence;

namespace Basemix.Lib.Litters;

public class LitterEstimator
{
    public delegate Task<EstimationParameters> GetEstimationParameters();

    private readonly GetEstimationParameters getParameters;

    public LitterEstimator(GetEstimationParameters getParameters)
    {
        this.getParameters = getParameters;
    }
    
    public async Task<Estimation> EstimateFor(Litter litter)
    {
        var parameters = await this.getParameters();
        switch (litter)
        {
            case {DateOfPairing: not null, DateOfBirth: null}:
                var earliestBirth = litter.DateOfPairing.Value.AddDays(parameters.MinBirthDaysAfterPairing);
                var latestBirth = litter.DateOfPairing.Value.AddDays(parameters.MaxBirthDaysAfterPairing);
                var middle = (parameters.MaxBirthDaysAfterPairing - parameters.MinBirthDaysAfterPairing) / 2;
                var midpointBirth = earliestBirth.AddDays(middle);
                return new Estimation
                {
                    EarliestDateOfBirth = earliestBirth,
                    LatestDateOfBirth = latestBirth,
                    EarliestFullyWeanedDate = midpointBirth.AddDays(parameters.MinWeaningDaysAfterBirth),
                    EarliestSeparateSexesDate = midpointBirth.AddDays(parameters.MinSeparationDaysAfterBirth),
                    EarliestRehomeDate = midpointBirth.AddDays(parameters.MinRehomeDaysAfterBirth)
                };
            case {DateOfBirth: not null}:
                return new Estimation
                {
                    EarliestFullyWeanedDate =
                        litter.DateOfBirth.Value.AddDays(parameters.MinWeaningDaysAfterBirth),
                    EarliestSeparateSexesDate =
                        litter.DateOfBirth.Value.AddDays(parameters.MinSeparationDaysAfterBirth),
                    EarliestRehomeDate =
                        litter.DateOfBirth.Value.AddDays(parameters.MinRehomeDaysAfterBirth)
                };
            default:
                return new Estimation();
        }
    }
}

public record EstimationParameters(
    int MinBirthDaysAfterPairing,
    int MaxBirthDaysAfterPairing,
    int MinWeaningDaysAfterBirth,
    int MinSeparationDaysAfterBirth,
    int MinRehomeDaysAfterBirth)
{
    public static Task<EstimationParameters> Standard =>
        Task.FromResult(new EstimationParameters(
            MinBirthDaysAfterPairing: 21,
            MaxBirthDaysAfterPairing: 23,
            MinWeaningDaysAfterBirth: (3 * 7) + 4,
            MinSeparationDaysAfterBirth: (4 * 7) + 3,
            MinRehomeDaysAfterBirth: 6 * 7));

    public static async Task<EstimationParameters> FromSettings(IProfileRepository profileRepository)
    {
        var profile = await profileRepository.GetDefaultProfile();
        return new EstimationParameters(
            MinBirthDaysAfterPairing: profile.LitterEstimation.MinBirthDaysAfterPairing,
            MaxBirthDaysAfterPairing: profile.LitterEstimation.MaxBirthDaysAfterPairing,
            MinWeaningDaysAfterBirth: profile.LitterEstimation.MinWeaningDaysAfterBirth,
            MinSeparationDaysAfterBirth: profile.LitterEstimation.MinSeparationDaysAfterBirth,
            MinRehomeDaysAfterBirth: profile.LitterEstimation.MinRehomeDaysAfterBirth);
    }
}

public record Estimation
{
    public DateOnly? EarliestDateOfBirth { get; init; }
    public DateOnly? LatestDateOfBirth { get; init; }
    public DateOnly? EarliestFullyWeanedDate { get; init; }
    public DateOnly? EarliestSeparateSexesDate { get; init; }
    public DateOnly? EarliestRehomeDate { get; init; }

    public bool IsRelevant(NowDateOnly now) =>
        this.EarliestRehomeDate != null && now() <= this.EarliestRehomeDate.Value.AddDays(14);
    
    public bool IsEmpty =>
        this.EarliestDateOfBirth == null &&
        this.LatestDateOfBirth == null &&
        this.EarliestFullyWeanedDate == null &&
        this.EarliestSeparateSexesDate == null &&
        this.EarliestRehomeDate == null;
}