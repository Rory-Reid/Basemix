using Basemix.Lib.Litters;
using Basemix.Lib.Litters.Persistence;
using Microsoft.AspNetCore.Components;

namespace Basemix.Pages;

public partial class Litters
{
    [Inject] public ILittersRepository Repository { get; set; } = null!;
    [Inject] public NavigationManager Nav { get; set; } = null!;
    [Inject] public FilterContext Filter { get; set; } = null!;
    
    public List<LitterOverview> LitterList { get; private set; } = new();

    protected override async Task OnParametersSetAsync()
    {
        await this.Search();
    }

    public void OpenLitterProfile(long litterId)
    {
        this.Nav.NavigateTo($"/litters/{litterId}");
    }

    public async Task CreateLitter()
    {
        var litter = await Litter.Create(this.Repository);
        this.Nav.NavigateTo($"/litters/{litter.Id.Value}/edit");
    }

    public async Task Search()
    {
        this.LitterList = await this.Repository.SearchLitters(
            this.Filter.LittersShowOnlyBredByMe ? true : null);
    }
}