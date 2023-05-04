using Basemix.Lib.Owners;
using Basemix.Lib.Owners.Persistence;
using Microsoft.AspNetCore.Components;

namespace Basemix.Pages;

public partial class Owners
{
    [Inject] public IOwnersRepository Repository { get; set; } = null!;
    [Inject] public NavigationManager Nav { get; set; } = null!;

    public string? SearchTerm { get; set; }

    public List<OwnerSearchResult> OwnersList { get; private set; } = new();
    
    protected override async Task OnParametersSetAsync()
    {
        await this.Search();
    }
    
    public async Task CreateOwner()
    {
        var owner = await Owner.Create(this.Repository);
        this.Nav.NavigateTo($"/owners/{owner.Id.Value}/edit");
    }

    public async Task Search()
    {
        this.OwnersList = await this.Repository.SearchOwner(this.SearchTerm);
    }
    
    public void OpenOwnerProfile(long ownerId)
    {
        this.Nav.NavigateTo($"/owners/{ownerId}");
    }
}