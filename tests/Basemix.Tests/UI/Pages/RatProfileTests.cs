using System.Diagnostics.CodeAnalysis;
using Basemix.Lib;
using Basemix.Lib.Pedigrees;
using Basemix.Lib.Rats;
using Basemix.Pages;
using Basemix.Tests.sdk;
using Bogus;
using Bogus.DataSets;
using Shouldly;

namespace Basemix.Tests.UI.Pages;

public class RatProfileTests : RazorPageTests<RatProfile>
{
    private readonly Faker faker = new();
    private readonly TestNavigationManager nav = new();
    private MemoryRatsRepository repository = null!;
    private MemoryLittersRepository littersRepository = null!;
    private MemoryPedigreesRepository pedigreesRepository = null!;
    private MemoryProfileRepository profileRepository = null!;

    [SuppressMessage("Usage", "BL0005:Component parameter should not be set outside of its component.")]
    protected override RatProfile CreatePage()
    {
        var backplane = new MemoryPersistenceBackplane();
        this.repository = new MemoryRatsRepository(backplane);
        this.littersRepository = new MemoryLittersRepository(backplane);
        this.pedigreesRepository = new MemoryPedigreesRepository(backplane);
        this.profileRepository = new MemoryProfileRepository(backplane);
        return new()
        {
            Id = this.faker.Id(),
            Repository = this.repository,
            LittersRepository = this.littersRepository,
            PedigreeRepository = this.pedigreesRepository,
            ProfileRepository = this.profileRepository,
            Nav = this.nav,
            DateSpanToString = Delegates.HumaniseDateSpan,
            NowDateOnly = () => this.Now,
        };
    }

    public DateOnly Now { get; set; } = DateOnly.MaxValue;
    
    [Fact]
    public async Task Loads_rat_from_repository_on_parameters_set()
    {
        var rat = this.faker.Rat(id: this.Page.Id);
        this.repository.Rats[this.Page.Id] = rat;

        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        
        this.Page.Rat.ShouldBe(rat);
    }

    [Fact]
    public async Task New_litter_button_creates_new_litter_sets_dam_and_navigates_to_edit_litter()
    {
        var rat = this.faker.Rat(id: this.Page.Id, sex: Sex.Doe);
        this.repository.Rats[this.Page.Id] = rat;
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);

        await this.Page.NewLitter();
        
        var litter = this.littersRepository.Litters.ShouldHaveSingleItem().Value;
        litter.ShouldSatisfyAllConditions(
                () => litter.DamId.ShouldBe(rat.Id),
                () => litter.DamName.ShouldBe(rat.Name));
        this.nav.CurrentUri.ShouldBe($"/litters/{litter.Id.Value}/edit");
    }

    [Fact]
    public async Task New_litter_button_creates_new_litter_sets_sire_and_navigates_to_edit_litter()
    {
        var rat = this.faker.Rat(id: this.Page.Id, sex: Sex.Buck);
        this.repository.Rats[this.Page.Id] = rat;
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);

        await this.Page.NewLitter();
        
        var litter = this.littersRepository.Litters.ShouldHaveSingleItem().Value;
        litter.ShouldSatisfyAllConditions(
            () => litter.SireId.ShouldBe(rat.Id),
            () => litter.SireName.ShouldBe(rat.Name));
        this.nav.CurrentUri.ShouldBe($"/litters/{litter.Id.Value}/edit");
    }

    [Fact]
    public void Open_litter_profile_navigates_to_litter()
    {
        var litterId = this.faker.Id();
        
        this.Page.OpenLitterProfile(litterId);
        
        this.nav.CurrentUri.ShouldBe($"/litters/{litterId}");
    }

    [Fact]
    public async Task Rat_age_displays_stringified_age_to_date()
    {
        var dob = new DateOnly(2000, 1, 1);
        this.Now = new DateOnly(2001, 2, 2);
        var rat = this.faker.Rat(id: this.Page.Id, dateOfBirth: dob);
        this.repository.Rats[this.Page.Id] = rat;
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);

        this.Page.RatAge.ShouldBe(Delegates.HumaniseDateSpan(dob, this.Now));
    }

    [Fact]
    public async Task Rat_age_uses_date_of_death_if_set_instead_of_now()
    {
        var dob = new DateOnly(2000, 1, 1);
        var dod = new DateOnly(2001, 2, 2);
        this.Now = new DateOnly(2023, 4, 8);
        var rat = this.faker.Rat(id: this.Page.Id, dateOfBirth: dob, dateOfDeath: dod);
        this.repository.Rats[this.Page.Id] = rat;
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);

        this.Page.RatAge.ShouldBe(Delegates.HumaniseDateSpan(dob, dod));
    }

    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    public async Task Rat_is_senior_if_rat_is_between_3_and_4_years_old(int years)
    {
        var dob = this.faker.Date.PastDateOnly();
        var rat = this.faker.Rat(id: this.Page.Id, dateOfBirth: dob);
        this.repository.Rats[this.Page.Id] = rat;
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);

        this.Now = dob.AddYears(years);
        this.Page.RatIsSenior.ShouldBeTrue($"Rat dob: {dob}, now: {this.Now}");
    }

    [Fact]
    public async Task Rat_is_not_senior_if_under_3_years_old()
    {
        var dob = this.faker.Date.PastDateOnly();
        var rat = this.faker.Rat(id: this.Page.Id, dateOfBirth: dob);
        this.repository.Rats[this.Page.Id] = rat;
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);

        this.Now = dob.AddYears(3).AddDays(-1);
        this.Page.RatIsSenior.ShouldBeFalse();
    }
    
    /// <summary>
    /// This bool just serves as an indicator for _calculated age to date_ - if a user has told us the exact age of a
    /// rat at time of death then we don't want to display tooltips indicating the rat is getting questionably old.
    /// </summary>
    [Fact]
    public async Task Rat_is_not_senior_if_rat_is_between_3_and_4_at_time_of_death()
    {
        var dob = this.faker.Date.PastDateOnly();
        var rat = this.faker.Rat(id: this.Page.Id, dateOfBirth: dob, dateOfDeath: dob.AddYears(3));
        this.Now = dob.AddYears(3).AddMonths(6); // 6 months after death
        this.repository.Rats[this.Page.Id] = rat;
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
            
        this.Page.RatIsSenior.ShouldBeFalse();
    }

    [Fact]
    public async Task Rat_is_too_old_if_over_4_years()
    {
        var dob = this.faker.Date.PastDateOnly();
        var rat = this.faker.Rat(id: this.Page.Id, dateOfBirth: dob);
        this.repository.Rats[this.Page.Id] = rat;
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);

        this.Now = dob.AddYears(4).AddDays(1);
        this.Page.RatTooOld.ShouldBeTrue();
    }

    [Fact]
    public async Task Rat_is_not_too_old_if_4_years_or_under()
    {
        var dob = this.faker.Date.PastDateOnly();
        var rat = this.faker.Rat(id: this.Page.Id, dateOfBirth: dob);
        this.repository.Rats[this.Page.Id] = rat;
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);

        this.Now = dob.AddYears(4).AddDays(-1);
        this.Page.RatTooOld.ShouldBeFalse();
    }
    
    /// <summary>
    /// Again this is an indicator for _calculated age to date_ - if a user has told us the exact age of a rat by filling
    /// in their date of death, we accept that as fact. This bool only serves to inform the UI if we can trust the age
    /// when rendering things.
    /// </summary>
    [Fact]
    public async Task Rat_is_not_too_old_if_over_4_years_at_time_of_death()
    {
        var dob = this.faker.Date.PastDateOnly();
        this.Now = dob.AddYears(10);
        var rat = this.faker.Rat(id: this.Page.Id, dateOfBirth: dob, dateOfDeath: dob.AddYears(4).AddDays(1));
        this.repository.Rats[this.Page.Id] = rat;
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);

        this.Page.RatTooOld.ShouldBeFalse();
    }

    [Fact]
    public async Task Litter_name_is_pulled_from_named_litter_in_pedigree()
    {
        var litterName = $"{this.faker.Hacker.Phrase()} Litter";
        var rat = this.faker.Rat(id: this.Page.Id);
        this.repository.Rats[this.Page.Id] = rat;
        this.pedigreesRepository.Pedigrees[this.Page.Id] = new Node
        {
            LitterName = litterName,
        };
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        
        this.Page.LitterName.ShouldBe(litterName);
    }

    [Fact]
    public async Task Litter_name_generates_from_parents_if_rat_is_not_from_named_litter()
    {
        var rat = this.faker.Rat(id: this.Page.Id);
        this.repository.Rats[this.Page.Id] = rat;
        var familyTree = new Node
        {
            Dam = new Node
            {
                Name = this.faker.Name.FirstName(Name.Gender.Female)
            },
            Sire = new Node
            {
                Name = this.faker.Name.FirstName(Name.Gender.Male)
            }
        };
        this.pedigreesRepository.Pedigrees[this.Page.Id] = familyTree;
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        
        this.Page.LitterName.ShouldBe($"{familyTree.Dam.Name} & {familyTree.Sire.Name}'s litter");
    }
    
    [Theory]
    [InlineData(true)] // We've got a dad but they don't have a name
    [InlineData(false)] // No dad
    public async Task Litter_name_generates_from_dam_only_if_rat_is_not_from_named_litter(bool hasSire)
    {
        var rat = this.faker.Rat(id: this.Page.Id);
        this.repository.Rats[this.Page.Id] = rat;
        var familyTree = new Node
        {
            Dam = new Node
            {
                Name = this.faker.Name.FirstName(Name.Gender.Female)
            },
            Sire = hasSire ? new Node {Name = string.Empty} : null
        };
        this.pedigreesRepository.Pedigrees[this.Page.Id] = familyTree;
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        
        this.Page.LitterName.ShouldBe($"{familyTree.Dam.Name} & Unknown Sire's litter");
    }    
    
    [Theory]
    [InlineData(true)] // We've got a mum but they don't have a name
    [InlineData(false)] // No dad
    public async Task Litter_name_generates_from_sire_only_if_rat_is_not_from_named_litter(bool hasDam)
    {
        var rat = this.faker.Rat(id: this.Page.Id);
        this.repository.Rats[this.Page.Id] = rat;
        var familyTree = new Node
        {
            Sire = new Node
            {
                Name = this.faker.Name.FirstName(Name.Gender.Female)
            },
            Dam = hasDam ? new Node {Name = string.Empty} : null
        };
        this.pedigreesRepository.Pedigrees[this.Page.Id] = familyTree;
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        
        this.Page.LitterName.ShouldBe($"{familyTree.Sire.Name} & Unknown Dam's litter");
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public async Task Litter_name_is_empty_if_rat_has_no_named_parents_and_is_not_from_named_litter(bool hasDam, bool hasSire)
    {
        var rat = this.faker.Rat(id: this.Page.Id);
        this.repository.Rats[this.Page.Id] = rat;
        var familyTree = new Node
        {
            LitterName = this.faker.Random.Bool() ? string.Empty : null, 
            Dam = hasDam ? new Node {Name = string.Empty} : null,
            Sire = hasSire ? new Node {Name = string.Empty} : null
        };
        this.pedigreesRepository.Pedigrees[this.Page.Id] = familyTree;
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
        
        this.Page.LitterName.ShouldBeNull();
        
    }
}