using Basemix.Lib.Owners;
using Basemix.Lib.Owners.Persistence;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Basemix.Pages;

public partial class EditOwner
{
    [Inject] public IOwnersRepository Repository { get; set; } = null!;
    [Inject] public IJSRuntime JsRuntime { get; set; } = null!;
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

    public async Task SaveAndGoBack()
    {
        await this.Owner.Save(this.Repository);
        await this.JsRuntime.InvokeAsync<object>("history.back");
    }

    public async Task Delete()
    {
        await this.Repository.DeleteOwner(this.Owner.Id);
        this.Nav.NavigateTo("/owners");
    }
}