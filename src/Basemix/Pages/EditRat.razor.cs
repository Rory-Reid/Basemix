using Basemix.Lib.Litters;
using Basemix.Lib.Litters.Persistence;
using Basemix.Lib.Media;
using Basemix.Lib.Media.Persistence;
using Basemix.Lib.Owners;
using Basemix.Lib.Owners.Persistence;
using Basemix.Lib.Rats;
using Basemix.Lib.Rats.Persistence;
using Basemix.Lib.Settings.Persistence;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Basemix.Pages;

public partial class EditRat
{
    [Inject] public IRatsRepository Repository { get; set; } = null!;
    [Inject] public ILittersRepository LittersRepository { get; set; } = null!;
    [Inject] public IOwnersRepository OwnersRepository { get; set; } = null!;
    [Inject] public IOptionsRepository OptionsRepository { get; set; } = null!;
    [Inject] public IProfileRepository ProfileRepository { get; set; } = null!;
    [Inject] public IMediaRepository MediaRepository { get; set; } = null!;
    [Inject] public IPhotoPicker PhotoPicker { get; set; } = null!;
    [Inject] public ErrorContext ErrorContext { get; set; } = null!;
    [Inject] public IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] public NavigationManager Nav { get; set; } = null!;

    [Parameter] public long Id { get; set; }

    public bool RatLoaded { get; private set; }
    public Rat Rat { get; private set; } = null!;
    public RatForm RatForm { get; private set; } = new();
    public RatPhoto? Photo { get; private set; }
    public bool PhotoMissing { get; private set; }

    public bool ShowOwnerSearch { get; set; }
    public string? OwnerSearchTerm { get; set; }
    public List<OwnerSearchResult> OwnerSearchResults { get; set; } = new();

    public bool DisableCreateLitter => !this.CanAddLitter();

    public List<DeathReason> DeathReasonOptions { get; set; } = new();

    protected override async Task OnParametersSetAsync()
    {
        var rat = await this.Repository.GetRat(this.Id);
        if (rat == null)
        {
            return;
        }

        this.RatLoaded = true;
        this.Rat = rat;
        this.RatForm = new()
        {
            Name = this.Rat.Name,
            DateOfBirth = this.Rat.DateOfBirth,
            Sex = this.Rat.Sex?.ToString(),
            Variety = this.Rat.Variety,
            Notes = this.Rat.Notes,
            Dead = this.Rat.Dead,
            DateOfDeath = this.Rat.DateOfDeath,
            DeathReasonId = this.Rat.DeathReason?.Id,
            Owned = this.Rat.Owned
        };

        if (rat.PhotoId != null)
        {
            this.Photo = await this.MediaRepository.GetPhoto(rat.PhotoId);
            this.PhotoMissing = this.Photo == null;
        }

        this.DeathReasonOptions = await this.OptionsRepository.GetDeathReasons();
    }

    private bool CanAddLitter() =>
        !string.IsNullOrEmpty(this.RatForm.Name) &&
        !string.IsNullOrEmpty(this.RatForm.Sex);

    public async Task NewLitter()
    {
        if (!this.CanAddLitter())
        {
            return;
        }

        await this.SaveRat();
        var litter = await Litter.Create(this.LittersRepository);
        switch (this.Rat.Sex)
        {
            case Sex.Buck:
                await litter.SetSire(this.LittersRepository, this.Rat);
                break;
            case Sex.Doe:
                await litter.SetDam(this.LittersRepository, this.Rat);
                break;
        }
        
        this.Nav.NavigateTo($"/litters/{litter.Id.Value}/edit");
    }

    public void EditLitter(long litterId)
    {
        this.Nav.NavigateTo($"/litters/{litterId}/edit");
    }

    public async Task SaveAndGoBack()
    {
        await this.SaveRat();
        await this.JsRuntime.InvokeAsync<object>("history.back");
    }

    public async Task DeleteRat()
    {
        await this.Repository.DeleteRat(this.Id);
        this.Nav.NavigateTo("/rats");
    }

    public async Task AddOwner()
    {
        await this.SaveRat();
        var owner = await Owner.Create(this.OwnersRepository);
        if (await this.Rat.SetOwner(this.Repository, owner) == OwnerAddResult.Success)
        {
            this.Nav.NavigateTo($"/owners/{owner.Id.Value}/edit");
        }
    }
    
    public void OpenOwnerSearch()
    {
        this.OwnerSearchResults.Clear();
        this.OwnerSearchTerm = string.Empty;
        this.ShowOwnerSearch = true;
    }

    public async Task SearchOwner()
    {
        this.OwnerSearchResults = await this.OwnersRepository.SearchOwner(this.OwnerSearchTerm);
    }

    public async Task SetResult(OwnerSearchResult result)
    {
        this.Rat.Owned = this.RatForm.Owned;
        await this.Rat.SetOwner(this.Repository, result);
        this.ShowOwnerSearch = false;
    }

    public async Task RemoveOwner()
    {
        await this.Rat.RemoveOwner(this.Repository);
    }
    
    public async Task UploadPhoto()
    {
        try
        {
            using var result = await this.PhotoPicker.PickPhotoAsync();
            if (result == null)
            {
                return;
            }

            var profile = await this.ProfileRepository.GetDefaultProfile();
            var photoId = MediaIds.RatProfilePhoto(this.Rat.Id);

            if (this.Rat.PhotoId != photoId)
            {
                this.Rat.PhotoId = photoId;
                await this.Rat.Save(this.Repository);
            }

            await this.MediaRepository.SavePhoto(
                photoId,
                result.Stream,
                result.FileName,
                profile.Photo.MaxResolution,
                profile.Photo.CompressionEnabled);

            this.Photo = await this.MediaRepository.GetPhoto(photoId);
            this.PhotoMissing = this.Photo == null;
        }
        catch (Exception e)
        {
            this.ErrorContext.LastError = e.ToString();
        }
    }

    public async Task DeletePhoto()
    {
        try
        {
            var oldPhotoId = this.Rat.PhotoId;
            if (oldPhotoId == null)
            {
                return;
            }

            await this.MediaRepository.DeletePhoto(oldPhotoId);

            this.Rat.PhotoId = null;
            await this.Rat.Save(this.Repository);

            this.Photo = null;
            this.PhotoMissing = false;
        }
        catch (Exception e)
        {
            this.ErrorContext.LastError = e.ToString();
        }
    }

    private async Task SaveRat()
    {
        Enum.TryParse<Sex>(this.RatForm.Sex, out var sex);

        this.Rat.Name = this.RatForm.Name;
        this.Rat.Sex = sex != Sex.Error ? sex : null;
        this.Rat.Variety = this.RatForm.Variety;
        this.Rat.DateOfBirth = this.RatForm.DateOfBirth;
        this.Rat.Notes = this.RatForm.Notes;
        this.Rat.DateOfDeath = this.RatForm.DateOfDeath;
        this.Rat.Owned = this.RatForm.Owned;
        this.Rat.Dead = this.RatForm.Dead;
        this.Rat.DeathReason = this.DeathReasonOptions.FirstOrDefault(x => x.Id == this.RatForm.DeathReasonId);

        await this.Rat.Save(this.Repository);
    }
}

public class RatForm
{
    public string? Name { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public bool Dead { get; set; }

    public DateOnly? DateOfDeath
    {
        get;
        set;
    }
    public long? DeathReasonId { get; set; }
    public string? Sex { get; set; }
    public string? Variety { get; set; }
    public string? Notes { get; set; }
    public bool Owned { get; set; } = true;
}