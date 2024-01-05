using Basemix.Lib.Rats;
using Basemix.Lib.Rats.Persistence;
using Microsoft.AspNetCore.Components;

namespace Basemix.Pages;

public partial class Rats
{
    [Inject] public IRatsRepository Repository { get; set; } = null!;
    [Inject] public NavigationManager Nav { get; set; } = null!;
    [Inject] public FilterContext Filter { get; set; } = null!;
    
    public List<RatSearchResult> RatsList { get; private set; } = new();

    public string? SearchTerm { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await this.Search();
    }

    public void OpenRatProfile(long ratId)
    {
        this.Nav.NavigateTo($"/rats/{ratId}");
    }

    public async Task CreateRat()
    {
        var rat = await Rat.Create(this.Repository);
        this.Nav.NavigateTo($"/rats/{rat.Id.Value}/edit");
    }

    public async Task Search()
    {
        this.RatsList = await this.Repository.SearchRat(
            this.SearchTerm,
            this.Filter.RatsHideDeceased ? false : null,
            this.Filter.RatsHideUnowned ? true : null);
    }
}