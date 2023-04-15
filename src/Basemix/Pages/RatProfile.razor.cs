using Basemix.Lib;
using Basemix.Lib.Litters;
using Basemix.Lib.Litters.Persistence;
using Basemix.Lib.Pedigrees;
using Basemix.Lib.Pedigrees.Persistence;
using Basemix.Lib.Rats;
using Basemix.Lib.Rats.Persistence;
using Microsoft.AspNetCore.Components;

namespace Basemix.Pages;

public partial class RatProfile
{
    [Inject] public IRatsRepository Repository { get; set; } = null!;
    [Inject] public ILittersRepository LittersRepository { get; set; } = null!;
    [Inject] public IPedigreeRepository PedigreeRepository { get; set; } = null!;
    [Inject] public NavigationManager Nav { get; set; } = null!;
    [Inject] public DateSpanToString DateSpanToString { get; set; } = null!;
    [Inject] public NowDateOnly NowDateOnly { get; set; } = null!;
    
    [Parameter] public long Id { get; set; }

    public bool RatLoaded { get; private set; }
    public bool PedigreeLoaded { get; private set; }
    public Rat Rat { get; private set; } = null!;
    public Node Pedigree { get; private set; } = null!;

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
}