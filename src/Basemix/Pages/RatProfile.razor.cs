using Basemix.Lib;
using Basemix.Lib.Litters;
using Basemix.Lib.Litters.Persistence;
using Basemix.Lib.Pedigrees;
using Basemix.Lib.Pedigrees.Persistence;
using Basemix.Lib.Rats;
using Basemix.Lib.Rats.Persistence;
using Basemix.Lib.Settings.Persistence;
using CommunityToolkit.Maui.Storage;
using Microsoft.AspNetCore.Components;

namespace Basemix.Pages;

public partial class RatProfile
{
    [Inject] public IRatsRepository Repository { get; set; } = null!;
    [Inject] public ILittersRepository LittersRepository { get; set; } = null!;
    [Inject] public IPedigreeRepository PedigreeRepository { get; set; } = null!;
    [Inject] public IProfileRepository ProfileRepository { get; set; } = null!;
    [Inject] public NavigationManager Nav { get; set; } = null!;
    [Inject] public PdfGenerator PdfGenerator { get; set; } = null!;
    [Inject] public DateSpanToString DateSpanToString { get; set; } = null!;
    [Inject] public NowDateOnly NowDateOnly { get; set; } = null!;
    [Inject] public ErrorContext ErrorContext { get; set; } = null!;
    
    [Parameter] public long Id { get; set; }

    public bool RatLoaded { get; private set; }
    public bool PedigreeLoaded { get; private set; }
    public bool ShowPdfExport { get; private set; }
    
    public Rat Rat { get; private set; } = null!;
    public Node Pedigree { get; private set; } = null!;
    public PedigreeContext PedigreeContext { get; set; } = null!;

    public string? RatAge
    {
        get
        {
            if (this.Rat.DateOfBirth == null)
            {
                return null;
            }
            
            var ageTo = this.Rat.DateOfDeath ?? this.NowDateOnly();
            return this.DateSpanToString(this.Rat.DateOfBirth.Value, ageTo);
        }
    }

    /// <summary>
    /// Represents when a rat is considered senior in automatic calculations
    /// If this is true, it calls into question if the rat is still alive.
    /// </summary>
    public bool RatIsSenior
    {
        get
        {
            if (this.Rat.DateOfDeath != null)
            {
                return false;
            }

            var ratAge = this.Rat.Age(this.NowDateOnly);
            if (ratAge == null)
            {
                return false; // TODO test
            }
            
            return (ratAge >= TimeSpan.FromDays(365.25 * 3)) && (ratAge <= TimeSpan.FromDays(365.25 * 4));
        }
    }

    /// <summary>
    /// Represents when a rat is considered too old for automatic calculations.
    /// If this is true, it basically means we assume the rat is not actually the age
    /// we calculate and has likely deceased.
    /// </summary>
    public bool RatTooOld
    {
        get
        {
            if (this.Rat.DateOfDeath != null)
            {
                return false;
            }
            
            var ratAge = this.Rat.Age(this.NowDateOnly);
            if (ratAge == null)
            {
                return false; // TODO test
            }
            
            return (ratAge > TimeSpan.FromDays(365.25 * 4));
        }
    }
    
    protected override async Task OnParametersSetAsync()
    {
        var rat = await this.Repository.GetRat(this.Id);
        if (rat == null)
        {
            return;
        }

        this.RatLoaded = true;
        this.Rat = rat;
        
        var pedigree = await this.PedigreeRepository.GetPedigree(this.Id);
        if (pedigree == null)
        {
            return;
        }

        this.PedigreeLoaded = true;
        this.Pedigree = pedigree;

        if (!string.IsNullOrEmpty(this.Pedigree.LitterName))
        {
            this.LitterName = this.Pedigree.LitterName;
        }
        else if (!string.IsNullOrEmpty(this.Pedigree.Dam?.Name) && !string.IsNullOrEmpty(this.Pedigree.Sire?.Name))
        {
            this.LitterName = $"{this.Pedigree.Dam.Name} & {this.Pedigree.Sire.Name}'s litter";
        }
        else if (!string.IsNullOrEmpty(this.Pedigree.Dam?.Name))
        {
            this.LitterName = $"{this.Pedigree.Dam.Name} & Unknown Sire's litter";
        }
        else if (!string.IsNullOrEmpty(this.Pedigree.Sire?.Name))
        {
            this.LitterName = $"{this.Pedigree.Sire.Name} & Unknown Dam's litter";
        }

        var profile = await this.ProfileRepository.GetDefaultProfile();
        this.PedigreeContext = new PedigreeContext
        {
            RatteryName = profile.RatteryName,
            FooterText = profile.Pedigree.Footer,
            ShowSex = profile.Pedigree.ShowSex
        };
    }
    
    public async Task NewLitter()
    {
        var litter = await Litter.Create(this.LittersRepository);
        if (this.Rat.Sex == Sex.Buck)
        {
            await litter.SetSire(this.LittersRepository, this.Rat);
        }
        else if (this.Rat.Sex == Sex.Doe)
        {
            await litter.SetDam(this.LittersRepository, this.Rat);
        }

        this.Nav.NavigateTo($"/litters/{litter.Id.Value}/edit");
    }
    
    public void OpenLitterProfile(long litterId)
    {
        this.Nav.NavigateTo($"/litters/{litterId}");
    }
    
    public string? LitterName { get; set; }
    
    public bool? ExportSuccess { get; set; }

    public async Task ExportPdfPedigree()
    {
        var profile = await this.ProfileRepository.GetDefaultProfile();
        try
        {
            var pdf = this.PdfGenerator.CreateFromPedigree(
                this.Pedigree,
                this.Rat.Sex,
                this.Rat.DateOfBirth,
                this.PedigreeContext.RatteryName,
                this.LitterName,
                this.PedigreeContext.FooterText,
                this.PedigreeContext.ShowSex,
                profile.Pedigree.Pdf);

#if ANDROID
            var tempFilePath = Path.Combine(FileSystem.Current.CacheDirectory, $"{this.Rat.Name}.pdf");
            await using var tempFile = new FileStream(tempFilePath, FileMode.Create);
            this.PdfGenerator.WriteToStream(pdf, tempFile);
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Save pedigree",
                File = new ShareFile(tempFilePath)
            });
#else
            try
            {
                var stream = new MemoryStream();
                this.PdfGenerator.WriteToStream(pdf, stream);
                await FileSaver.Default.SaveAsync($"{this.Rat.Name}.pdf", stream, CancellationToken.None);
            }
            catch (FolderPickerException e) when (e.Message is "Operation cancelled.")
            {
            }
            catch (FileSaveException e) when (e.Message is "Path doesn't exist.")
            {
                // CommunityToolkit doesn't handle people "cancelling" the file dialog and instead throws this exception.
            }
            catch (IndexOutOfRangeException e) when (e.Message.Contains("Arg_IndexOutOfRangeException"))
            {
                // Potential bug in CommunityToolkit for android. Seems to be an issue with returning the filepath
                // and not with actually saving the file. I can safely ignore it for now.
            }
#endif
        }
        catch (Exception e)
        {
            this.ShowPdfExport = false;
            this.ExportSuccess = false;
            this.ErrorContext.LastError = e.ToString();
            return;
        }

        this.ShowPdfExport = false;
        this.ExportSuccess = true;
    }
}

public class PedigreeContext
{
    public string? RatteryName { get; set; }
    public string? FooterText { get; set; }
    public bool ShowSex { get; set; }
}