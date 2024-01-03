namespace Basemix.Lib.Statistics.Persistence;

public interface IStatisticsRepository
{
    Task<StatisticsOverview> GetStatisticsOverview();
}