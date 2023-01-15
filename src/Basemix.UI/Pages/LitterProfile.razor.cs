using Basemix.Litters;
using Basemix.Litters.Persistence;
using Basemix.Rats;
using Basemix.Rats.Persistence;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Basemix.UI.Pages;

public partial class LitterProfile
{
    [Inject] public ILittersRepository Repository { get; set; }
    [Inject] public IRatsRepository RatsRepository { get; set; }
    [Inject] public NavigationManager Nav { get; set; }
    [Inject] public IJSRuntime JsRuntime { get; set; }
    
    [Parameter] public long Id { get; set; }
    
    public Litter Litter { get; private set; }
    
    public bool ShowRatSearch { get; set; }
    public Sex? RatSearchSex { get; set; }
    public string RatSearchTerm { get; set; }
    public List<RatSearchResult> RatSearchResults { get; set; } = new();
    public Func<RatSearchResult, Task> SetResult { get; set; } = _ => Task.CompletedTask;

    public bool HasDam => this.Litter.DamId != null;
    public bool HasDamName => !string.IsNullOrEmpty(this.Litter.DamName);
    public bool HasSire => this.Litter.SireId != null;
    public bool HasSireName => !string.IsNullOrEmpty(this.Litter.SireName);

    protected override async Task OnInitializedAsync()
    {
        var storedLitter = (await this.Repository.GetLitter(this.Id));
        if (storedLitter == null)
        {
            await this.GoBack();
            return;
        }

        this.Litter = storedLitter;
    }
    
    public void OpenRatProfile(long ratId)
    {
        this.Nav.NavigateTo($"/rats/{ratId}");
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

    public void OpenDamSearch()
    {
        this.RatSearchResults.Clear();
        this.RatSearchTerm = string.Empty;
        this.RatSearchSex = Sex.Doe; // TODO implement
        this.SetResult = async (rat) =>
        {
            await this.Litter.SetDam(this.Repository, rat);
            this.ShowRatSearch = false;
            this.SetResult = _ => Task.CompletedTask;
        };
        this.ShowRatSearch = true;
    }
    
    public void OpenSireSearch()
    {
        this.RatSearchResults.Clear();
        this.RatSearchTerm = string.Empty;
        this.RatSearchSex = Sex.Buck; // TODO implement
        this.SetResult = async (rat) =>
        {
            await this.Litter.SetSire(this.Repository, rat);
            this.ShowRatSearch = false;
            this.SetResult = _ => Task.CompletedTask;
        };
        this.ShowRatSearch = true;
    }
    
    public void OpenOffspringSearch()
    {
        this.RatSearchResults.Clear();
        this.RatSearchTerm = string.Empty;
        this.RatSearchSex = null; // TODO implement
        this.SetResult = async (rat) =>
        {
            await this.Litter.AddOffspring(this.Repository, rat);
            this.ShowRatSearch = false;
            this.SetResult = _ => Task.CompletedTask;
        };
        this.ShowRatSearch = true;
    }
    
    public async Task Search()
    {
        this.RatSearchResults = await this.RatsRepository.SearchRat(this.RatSearchTerm);
    }
    
    private async Task GoBack()
    {
        await this.JsRuntime.InvokeAsync<object>("history.back");
    }
}