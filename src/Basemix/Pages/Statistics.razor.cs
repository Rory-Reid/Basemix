using Basemix.Lib.Statistics;
using Basemix.Lib.Statistics.Persistence;
using Microsoft.AspNetCore.Components;

namespace Basemix.Pages;

public partial class Statistics
{
    [Inject] public IStatisticsRepository StatisticsRepository { get; set; } = null!;

    public StatisticsOverview Stats { get; set; } = new();

    protected override async Task OnParametersSetAsync()
    {
        this.Stats = await this.StatisticsRepository.GetStatisticsOverview();
    }

    public static double Yearify(TimeSpan timeSpan) =>
        Math.Round(timeSpan.TotalDays / 365, 2);
}