using Basemix.Litters;
using Basemix.Litters.Persistence;
using Basemix.Pedigrees;
using Basemix.Pedigrees.Persistence;
using Basemix.Rats;
using Basemix.Rats.Persistence;
using Microsoft.AspNetCore.Components;

namespace Basemix.UI.Pages;

public partial class RatProfile
{
    [Inject] public IRatsRepository Repository { get; set; }
    [Inject] public ILittersRepository LittersRepository { get; set; }
    [Inject] public IPedigreeRepository PedigreeRepository { get; set; }
    [Inject] public NavigationManager Nav { get; set; }
    
    [Parameter] public long Id { get; set; }

    public Rat Rat { get; private set; }
    public Node Pedigree { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        var storedRat = await this.Repository.GetRat(this.Id);
        if (storedRat == null)
        {
            return;
        }

        this.Rat = storedRat;
        this.Pedigree = (await this.PedigreeRepository.GetPedigree(this.Id))!;
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