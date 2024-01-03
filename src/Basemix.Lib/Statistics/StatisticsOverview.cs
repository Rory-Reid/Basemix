namespace Basemix.Lib.Statistics;

public class StatisticsOverview
{
    public BredLittersOverview BredLitters { get; init; } = null!;
    public OwnedRatsOverview OwnedRats { get; init; } = null!;
    public OwnersOverview Owners { get; init; } = null!;
    public SystemOverview System { get; init; } = null!;
        
    public class BredLittersOverview
    {
        public int TotalBredRats { get; init; }
        public int TotalBucks { get; init; }
        public int TotalDoes { get; init; }
        public int TotalNotRecordedDead { get; init; }
        public TimeSpan LongestLife { get; init; }
        public TimeSpan AverageAge { get; init; }
        public TimeSpan AverageGestationDays { get; init; }
        public int TotalRehomed { get; init; }
        public int SmallestLitter { get; init; }
        public int BiggestLitter { get; init; }
        public double AverageLitterSize { get; init; }
    }

    public class OwnedRatsOverview
    {
        public int Total { get; init; }
        public int TotalBucks { get; init; }
        public int TotalDoes { get; init; }
        public int TotalNotRecordedDead { get; init; }
        public TimeSpan LongestLife { get; init; }
        public TimeSpan AverageAge { get; init; }
        public string MostCommonVariety { get; init; } = null!;
        public int MostCommonVarietyCount { get; init; }
    }

    public class OwnersOverview
    {
        public IReadOnlyList<Owner> TopOwners { get; init; } = null!;
        public record Owner(string Name, int RatCount);
    }

    public class SystemOverview
    {
        public int TotalRats { get; init; }
        public int TotalOwnedRats { get; init; }
        public int TotalLitters { get; init; }
        public int TotalBredLitters { get; init; }
        public int TotalOwners { get; init; }
        public DateTime DatabaseCreatedOn { get; init; }
    }
}