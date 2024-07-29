using System.Text.Json;
using Dapper;

namespace Basemix.Lib.Statistics.Persistence;

public class SqliteStatisticsRepository : IStatisticsRepository
{
    private readonly GetDatabase getDatabase;

    public SqliteStatisticsRepository(GetDatabase getDatabase) => this.getDatabase = getDatabase;
    
    public async Task<StatisticsOverview> GetStatisticsOverview()
    {
        using var db = this.getDatabase();

        var statistics = await db.QuerySingleAsync<PersistedStatisticsOverview>( // TODO maybe break this down into views?
            """
            SELECT
              SUM(litter_stats.litter_size)                                  AS total_bred_rats,
              SUM(litter_stats.buck_count)                                   AS total_bucks,
              SUM(litter_stats.doe_count)                                    AS total_does,
              SUM(litter_stats.litter_size - litter_stats.dead_count)        AS total_not_recorded_dead,
              MAX(litter_stats.longest_life)                                 AS longest_life,
              (SUM(litter_stats.sum_age) / SUM(litter_stats.dead_count))     AS avg_age,
              AVG(litter_stats.gestation_period)                             AS avg_gestation,
              SUM(litter_stats.rehomed_count)                                AS total_rehomed,
              MIN(litter_stats.litter_size)                                  AS min_litter_size,
              MAX(litter_stats.litter_size)                                  AS max_litter_size,
              AVG(litter_stats.litter_size)                                  AS avg_litter_size,
              
              owned_rats.count                                               AS total_owned_rats,
              owned_rats.buck_count                                          AS total_owned_bucks,
              owned_rats.doe_count                                           AS total_owned_does,
              (owned_rats.count - owned_rats.dead_count)                     AS total_owned_not_dead,
              owned_rats.longest_life                                        AS longest_owned_life,
              owned_rats.sum_age                                             AS avg_owned_age,
              owned_rats.most_common_variety                                 AS most_common_owned_variety,
              owned_rats.most_common_variety_count                           AS most_common_owned_variety_count,
              
              (
                SELECT json_group_array(json_object('name', owner_name, 'ratCount', owner_rat_count))
                FROM
                (
                  SELECT COUNT(*) AS owner_rat_count, o.name AS owner_name
                  FROM owner o
                  JOIN rat r ON o.id = r.owner_id
                  JOIN litter l on r.litter_id = l.id
                  WHERE l.bred_by_me IS TRUE
                  GROUP BY o.id ORDER BY owner_rat_count DESC, o.id LIMIT 3
                )
              ) AS top_3_owners,
            
              (SELECT applied FROM schemaversions WHERE schemaversionid = 1) AS db_created_at,
              system_stats.rat_count                                         AS system_rat_count,
              system_stats.owned_rat_count                                   AS system_owned_rat_count,
              system_stats.litter_count                                      AS system_litter_count,
              system_stats.bred_litter_count                                 AS system_bred_litter_count,
              system_stats.owner_count                                       AS system_owner_count
            FROM (SELECT 'Statistics')
            LEFT JOIN
            (
              SELECT
                l.id,
                COUNT(*)                                                AS litter_size,
                COUNT(CASE WHEN r.sex = 'Buck' THEN 1 END)              AS buck_count,
                COUNT(CASE WHEN r.sex = 'Doe' THEN 1 END)               AS doe_count,
                COUNT(CASE WHEN r.owned IS FALSE THEN 1 END)            AS rehomed_count,
                COUNT(CASE WHEN r.dead IS TRUE THEN 1 END)              AS dead_count,
                SUM((r.date_of_death - r.date_of_birth))                AS sum_age,
                MAX((r.date_of_death - r.date_of_birth))                AS longest_life,
                l.date_of_birth - l.date_of_pairing                     AS gestation_period
              FROM litter l
              LEFT JOIN rat r on l.id = r.litter_id
              WHERE l.bred_by_me IS TRUE
              GROUP BY l.id
            ) litter_stats ON TRUE
            LEFT JOIN
            (
              SELECT
                COUNT(*) AS count,
                COUNT(CASE WHEN sex = 'Buck' THEN 1 END)              AS buck_count,
                COUNT(CASE WHEN sex = 'Doe' THEN 1 END)               AS doe_count,
                COUNT(CASE WHEN date_of_death IS NOT NULL THEN 1 END) AS dead_count,
                AVG(date_of_death - date_of_birth)                    AS sum_age,
                MAX(date_of_death - date_of_birth)                    AS longest_life,
                most_common_variety.name                              AS most_common_variety,
                most_common_variety.count                             AS most_common_variety_count
              FROM rat
              JOIN
              (
                SELECT COUNT(*) AS count, variety AS name
                FROM rat WHERE owned IS TRUE
                GROUP BY variety ORDER BY count DESC, variety LIMIT 1
              ) most_common_variety ON TRUE
              WHERE owned IS TRUE
            ) owned_rats ON TRUE
            LEFT JOIN
            (
              SELECT
                r.count       AS rat_count,
                r.owned_count AS owned_rat_count,
                l.count       AS litter_count,
                l.bred_count  AS bred_litter_count,
                o.count       AS owner_count
              FROM (SELECT COUNT(*) AS count, COUNT(CASE WHEN owned IS TRUE THEN 1 END) AS owned_count FROM rat) r
              JOIN (SELECT COUNT(*) AS count, COUNT(CASE WHEN bred_by_me IS TRUE THEN 1 END) AS bred_count FROM litter) l ON TRUE
              JOIN (SELECT COUNT(*) AS count FROM owner) o ON TRUE
            ) system_stats ON TRUE;
            """);

        return statistics.ToModelledOverview();
    }
    
    private class PersistedStatisticsOverview
    {
        public int TotalBredRats { get; init; }
        public int TotalBucks { get; init; }
        public int TotalDoes { get; init; }
        public int TotalNotRecordedDead { get; init; }
        public int LongestLife { get; init; }
        public int AvgAge { get; init; }
        public int AvgGestation { get; init; }
        public int TotalRehomed { get; init; }
        public int MinLitterSize { get; init; }
        public int MaxLitterSize { get; init; }
        public double AvgLitterSize { get; init; }
        
        public int TotalOwnedRats { get; init; }
        public int TotalOwnedBucks { get; init; }
        public int TotalOwnedDoes { get; init; }
        public int TotalOwnedNotDead { get; init; }
        public int LongestOwnedLife { get; init; }
        public int AvgOwnedAge { get; init; }
        public string MostCommonOwnedVariety { get; init; } = null!;
        public int MostCommonOwnedVarietyCount { get; init; }
        
        public string Top3Owners { get; init; } = null!;

        public string DbCreatedAt { get; init; } = null!;
        public int SystemRatCount { get; init; }
        public int SystemOwnedRatCount { get; init; }
        public int SystemLitterCount { get; init; }
        public int SystemBredLitterCount { get; init; }
        public int SystemOwnerCount { get; init; }

        public StatisticsOverview ToModelledOverview() =>
            new()
            {
                BredLitters = new()
                {
                    TotalBredRats = this.TotalBredRats,
                    TotalBucks = this.TotalBucks,
                    TotalDoes = this.TotalDoes,
                    TotalNotRecordedDead = this.TotalNotRecordedDead,
                    LongestLife = TimeSpan.FromSeconds(this.LongestLife),
                    AverageAge = TimeSpan.FromSeconds(this.AvgAge),
                    AverageGestationDays = TimeSpan.FromSeconds(this.AvgGestation),
                    TotalRehomed = this.TotalRehomed,
                    SmallestLitter = this.MinLitterSize,
                    BiggestLitter = this.MaxLitterSize,
                    AverageLitterSize = this.AvgLitterSize
                },
                OwnedRats = new()
                {
                    Total = this.TotalOwnedRats,
                    TotalBucks = this.TotalOwnedBucks,
                    TotalDoes = this.TotalOwnedDoes,
                    TotalNotRecordedDead = this.TotalOwnedNotDead,
                    LongestLife = TimeSpan.FromSeconds(this.LongestOwnedLife),
                    AverageAge = TimeSpan.FromSeconds(this.AvgOwnedAge),
                    MostCommonVariety = this.MostCommonOwnedVariety,
                    MostCommonVarietyCount = this.MostCommonOwnedVarietyCount
                },
                Owners = new()
                {
                    TopOwners =
                        JsonSerializer.Deserialize<List<StatisticsOverview.OwnersOverview.Owner>>(
                            this.Top3Owners,
                            new JsonSerializerOptions{PropertyNameCaseInsensitive = true}) 
                        ?? new List<StatisticsOverview.OwnersOverview.Owner>()
                },
                System = new()
                {
                    TotalRats = this.SystemRatCount,
                    TotalOwnedRats = this.SystemOwnedRatCount,
                    TotalLitters = this.SystemLitterCount,
                    TotalBredLitters = this.SystemBredLitterCount,
                    TotalOwners = this.SystemOwnerCount,
                    DatabaseCreatedOn = DateTime.Parse(this.DbCreatedAt)
                }
            };
    }
}