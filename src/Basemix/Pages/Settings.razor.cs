using Basemix.Lib.Settings;
using Basemix.Lib.Settings.Persistence;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Basemix.Pages;

public partial class Settings
{
    [Inject] public IProfileRepository Repository { get; set; } = null!;
    [Inject] public IJSRuntime JsRuntime { get; set; } = null!;

    public bool SettingsLoaded { get; set; } = false;

    public Profile Profile { get; private set; } = null!;

    protected override async Task OnParametersSetAsync()
    {
        this.Profile = await this.Repository.GetDefaultProfile();
        this.SettingsLoaded = true;
    }

    public async Task SaveAndGoBack()
    {
        await this.Repository.UpdateProfileSettings(this.Profile);
        await this.JsRuntime.InvokeAsync<object>("history.back");
    }
}