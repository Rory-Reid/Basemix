using Basemix.Rats;
using Basemix.Rats.Persistence;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Basemix.UI.Pages;

public partial class Rats
{
    [Inject] public IRatsRepository Repository { get; set; } = null!;
    [Inject] public NavigationManager Nav { get; set; } = null!;
    [Inject] public IJSRuntime JsRuntime { get; set; } = null!;
    
    public List<RatSearchResult> RatsList { get; private set; } = new();

    public string? SearchTerm { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var allRats = await this.Repository.GetAll();
        this.RatsList = allRats.Select(x => x.ToSearchResult()).ToList();
    }

    public void OpenRatProfile(long ratId)
    {
        this.Nav.NavigateTo($"/rats/{ratId}");
    }

    public async Task GoBack()
    {
        await this.JsRuntime.InvokeAsync<object>("history.back");
    }

    public async Task CreateRat()
    {
        var rat = await Rat.Create(this.Repository);
        this.Nav.NavigateTo($"/rats/{rat.Id.Value}/edit");
    }

    public async Task Search()
    {
        if (string.IsNullOrEmpty(this.SearchTerm))
        {
            var allRats = await this.Repository.GetAll();
            this.RatsList = allRats.Select(x => x.ToSearchResult()).ToList();
        }
        else
        {
            this.RatsList = await this.Repository.SearchRat(this.SearchTerm);
        }
    }
}