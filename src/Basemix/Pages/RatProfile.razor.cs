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
    
    [Parameter] public long Id { get; set; }

    public bool RatLoaded { get; private set; }
    public bool PedigreeLoaded { get; private set; }
    public Rat Rat { get; private set; } = null!;
    public Node Pedigree { get; private set; } = null!;

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