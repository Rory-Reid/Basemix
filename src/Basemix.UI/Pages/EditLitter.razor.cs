using Basemix.Litters;
using Basemix.Litters.Persistence;
using Basemix.Rats;
using Basemix.Rats.Persistence;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Basemix.UI.Pages;

public partial class EditLitter
{
    [Inject] public ILittersRepository Repository { get; set; } = null!;
    [Inject] public IRatsRepository RatsRepository { get; set; } = null!;
    [Inject] public NavigationManager Nav { get; set; } = null!;
    [Inject] public IJSRuntime JsRuntime { get; set; } = null!;

    [Parameter] public long Id { get; set; }
    
    public bool LitterLoaded { get; private set; }
    public Litter Litter { get; set; } = null!;
    
    public bool ShowRatSearch { get; set; }
    public Sex? RatSearchSex { get; set; }
    public string RatSearchTerm { get; set; } = string.Empty;
    public List<RatSearchResult> RatSearchResults { get; set; } = new();
    public Func<RatSearchResult, Task> SetResult { get; set; } = _ => Task.CompletedTask;

    public bool HasDam => this.Litter.DamId != null;
    public bool HasDamName => !string.IsNullOrEmpty(this.Litter.DamName);
    public bool HasSire => this.Litter.SireId != null;
    public bool HasSireName => !string.IsNullOrEmpty(this.Litter.SireName);

    protected override async Task OnParametersSetAsync()
    {
        var litter = await this.Repository.GetLitter(this.Id);
        if (litter == null)
        {
            return;
        }

        this.LitterLoaded = true;
        this.Litter = litter;
    }
    
    public string LitterName()
    {
        if (this.HasDamName && this.HasSireName)
        {
            return $"{this.Litter.DamName} & {this.Litter.SireName}'s litter";
        }
        
        if (this.HasDamName)
        {
            return $"{this.Litter.DamName}'s litter";
        }

        if (this.HasSireName)
        {
            return $"{this.Litter.SireName}'s litter";
        }
        
        return "Anonymous litter";
    }
    
    public async Task AddParent(Sex sex)
    {
        var rat = await Rat.Create(this.RatsRepository);
        rat.Sex = sex;
        await rat.Save(this.RatsRepository);

        await (sex switch
        {
            Sex.Buck => this.Litter.SetSire(this.Repository, rat),
            Sex.Doe => this.Litter.SetDam(this.Repository, rat),
            _ => throw new ArgumentOutOfRangeException(nameof(sex))
        });
        
        this.Nav.NavigateTo($"/rats/{rat.Id.Value}/edit");
    }

    public async Task RemoveParent(Sex sex)
    {
        await (sex switch
        {
            Sex.Buck => this.Litter.RemoveSire(this.Repository),
            Sex.Doe => this.Litter.RemoveDam(this.Repository),
            _ => throw new ArgumentOutOfRangeException(nameof(sex))
        });
    }
    
    public async Task AddOffspring()
    {
        var rat = await Rat.Create(this.RatsRepository);
        if (this.Litter.DateOfBirth != null)
        {
            rat.DateOfBirth = this.Litter.DateOfBirth;
            await rat.Save(this.RatsRepository);
        }

        await this.Litter.AddOffspring(this.Repository, rat);
        this.Nav.NavigateTo($"/rats/{rat.Id.Value}/edit");
    }
    
    public async Task Search()
    {
        this.RatSearchResults = await this.RatsRepository.SearchRat(this.RatSearchTerm);
    }

    public void OpenDamSearch()
    {
        this.RatSearchResults.Clear();
        this.RatSearchTerm = string.Empty;
        this.RatSearchSex = Sex.Doe;
        this.SetResult = async (rat) =>
        {
            await this.Litter.SetDam(this.Repository, rat);
            this.ShowRatSearch = false;
        };
        this.ShowRatSearch = true;
    }
    
    public void OpenSireSearch()
    {
        this.RatSearchResults.Clear();
        this.RatSearchTerm = string.Empty;
        this.RatSearchSex = Sex.Buck;
        this.SetResult = async (rat) =>
        {
            await this.Litter.SetSire(this.Repository, rat);
            this.ShowRatSearch = false;
        };
        this.ShowRatSearch = true;
    }
    
    public void OpenOffspringSearch()
    {
        this.RatSearchResults.Clear();
        this.RatSearchTerm = string.Empty;
        this.RatSearchSex = null;
        this.SetResult = async (rat) =>
        {
            await this.Litter.AddOffspring(this.Repository, rat);
            this.ShowRatSearch = false;
        };
        this.ShowRatSearch = true;
    }

    public async Task Save()
    {
        await this.Litter.Save(this.Repository);
        await this.JsRuntime.InvokeAsync<object>("history.back");
    }

    public async Task Delete()
    {
        await this.Repository.DeleteLitter(this.Litter.Id);
        this.Nav.NavigateTo("/litters");
    }

    public async Task RemoveOffspring(Offspring rat)
    {
        await this.Litter.RemoveOffspring(this.Repository, rat.Id);
    }
}