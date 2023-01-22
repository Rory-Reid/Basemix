using Basemix.Litters;
using Basemix.Litters.Persistence;
using Microsoft.AspNetCore.Components;

namespace Basemix.UI.Pages;

public partial class Litters
{
    [Inject] public ILittersRepository Repository { get; set; }
    [Inject] public NavigationManager Nav { get; set; }
    
    public List<LitterOverview> LitterList { get; private set; } = new();
    
    protected override async Task OnInitializedAsync()
    {
        this.LitterList = await this.Repository.GetAll();
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
}