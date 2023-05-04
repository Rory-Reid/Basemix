using Basemix.Lib.Owners;
using Basemix.Lib.Owners.Persistence;
using Microsoft.AspNetCore.Components;

namespace Basemix.Pages;

public partial class OwnerProfile
{
    [Inject] public IOwnersRepository Repository { get; set; } = null!;
    [Inject] public NavigationManager Nav { get; set; } = null!;
    
    [Parameter] public long Id { get; set; }
    
    public bool OwnerLoaded { get; private set; }
    
    public Owner Owner { get; private set; } = null!;
    
    protected override async Task OnParametersSetAsync()
    {
        var owner = await this.Repository.GetOwner(this.Id);
        if (owner == null)
        {
            return;
        }

        this.OwnerLoaded = true;
        this.Owner = owner;
    }

    public void Edit()
    {
        this.Nav.NavigateTo($"/owners/{this.Id}/edit");
    }
}