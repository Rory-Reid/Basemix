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
    
    public List<string> ErrorMessages { get; } = new();
    public bool ShowError => this.ErrorMessages.Any();

    public Profile Profile { get; private set; } = null!;

    protected override async Task OnParametersSetAsync()
    {
        this.Profile = await this.Repository.GetDefaultProfile();
        this.SettingsLoaded = true;
    }

    public async Task SaveAndGoBack()
    {
        if (this.ValidateSettings())
        {
            await this.Repository.UpdateProfileSettings(this.Profile);
            await this.JsRuntime.InvokeAsync<object>("history.back");
        }
    }

    public bool ValidateSettings()
    {
        this.ErrorMessages.Clear();
        var litterEstimation = this.Profile.LitterEstimation;
        if (litterEstimation.MinBirthDaysAfterPairing > litterEstimation.MaxBirthDaysAfterPairing)
        {
            this.ErrorMessages.Add("Minimum DOB after pairing must be before maximum.");
        }

        if (litterEstimation.MinBirthDaysAfterPairing is < 19 or > 23 ||
            litterEstimation.MaxBirthDaysAfterPairing is < 21 or > 25)
        {
            this.ErrorMessages.Add("Gestation is typically 21-23 days.");
        }
        
        if (litterEstimation.MinWeaningDaysAfterBirth > litterEstimation.MinSeparationDaysAfterBirth)
        {
            this.ErrorMessages.Add("Rats should be weaned before they are separated.");
        }

        if (litterEstimation.MinWeaningDaysAfterBirth is < 7 * 3 or > (7 * 4) + 4)
        {
            this.ErrorMessages.Add("Rats should be fully weaned around 3.5-4 weeks after birth.");
        }

        if (litterEstimation.MinSeparationDaysAfterBirth is < (7 * 4) - 1 or > (7 * 5) + 1)
        {
            this.ErrorMessages.Add("Rats should be separated around 4-5 weeks after birth.");
        }

        if (litterEstimation.MinRehomeDaysAfterBirth < 7 * 6)
        {
            this.ErrorMessages.Add("Rehoming should be no sooner than 6 weeks after birth.");
        }

        return !this.ShowError;
    }
}