using Basemix.Lib.Owners;
using Basemix.Lib.Rats;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.Rats;

public class RatTests
{
    private readonly Faker faker = new();
    private readonly MemoryRatsRepository repository = new();
    
    [Fact]
    public void Creates_rat_with_default_construction()
    {
        var rat = new Rat();
        
        rat.ShouldSatisfyAllConditions(
            () => rat.Id.ShouldNotBeNull().IsAnonymous.ShouldBeTrue(),
            () => rat.Name.ShouldBeNull(),
            () => rat.Sex.ShouldBeNull(),
            () => rat.DateOfBirth.ShouldBeNull(),
            () => rat.Notes.ShouldBeNull(),
            () => rat.DateOfDeath.ShouldBeNull(),
            () => rat.Variety.ShouldBeNull(),
            () => rat.Owned.ShouldBeFalse(),
            () => rat.OwnerId.ShouldBeNull(),
            () => rat.OwnerName.ShouldBeNull());
    }
    
    [Fact]
    public void Creates_rat_with_basic_construction()
    {
        var id = new RatIdentity(this.faker.Id());
        var name = this.faker.Person.FirstName;
        var sex = this.faker.PickNonDefault<Sex>();
        var variety = this.faker.Variety();
        var dob = this.faker.Date.RecentDateOnly();
        var litters = this.faker.Make(this.faker.Random.Int(0, 3), () => this.faker.RatLitter()).ToList();
        var ownerId = new OwnerIdentity(this.faker.Id());
        var ownerName = this.faker.Person.FullName;

        var rat = new Rat(id, name, sex, variety, dob, litters, ownerId, ownerName);
        
        rat.ShouldSatisfyAllConditions(
            () => rat.Id.ShouldBe(id),
            () => rat.Name.ShouldBe(name),
            () => rat.Sex.ShouldBe(sex),
            () => rat.Variety.ShouldBe(variety),
            () => rat.DateOfBirth.ShouldBe(dob),
            () => rat.Litters.ShouldBe(litters),
            () => rat.OwnerId.ShouldBe(ownerId),
            () => rat.OwnerName.ShouldBe(ownerName));
    }

    [Fact]
    public async Task Create_saves_and_returns_owned_rat_with_id()
    {
        var rat = await Rat.Create(this.repository);
        
        rat.Id.Value.ShouldBePositive();
        rat.Owned.ShouldBeTrue();

        this.repository.Rats.ShouldContainKey(rat.Id);
    }

    [Fact]
    public async Task Save_updates_all_rat_details()
    {
        var rat = await Rat.Create(this.repository);

        rat.Name = this.faker.Name.FirstName();
        rat.DateOfBirth = this.faker.Date.PastDateOnly();
        rat.Sex = this.faker.PickNonDefault<Sex>();
        rat.Notes = this.faker.Lorem.Paragraphs();
        rat.Variety = this.faker.Variety();
        rat.Owned = this.faker.Random.Bool();
        
        this.repository.Rats[rat.Id].ShouldSatisfyAllConditions(
            storedRat => storedRat.Name.ShouldBeNull(),
            storedRat => storedRat.DateOfBirth.ShouldBeNull(),
            storedRat => storedRat.Sex.ShouldBeNull(),
            storedRat => storedRat.Notes.ShouldBeNull(),
            storedRat => storedRat.Variety.ShouldBeNull(),
            storedRat => storedRat.Owned.ShouldBeTrue());

        await rat.Save(this.repository);
        
        this.repository.Rats[rat.Id].ShouldSatisfyAllConditions(
            storedRat => storedRat.Name.ShouldBe(rat.Name),
            storedRat => storedRat.DateOfBirth.ShouldBe(rat.DateOfBirth),
            storedRat => storedRat.Sex.ShouldBe(rat.Sex),
            storedRat => storedRat.Notes.ShouldBe(rat.Notes),
            storedRat => storedRat.Variety.ShouldBe(rat.Variety),
            storedRat => storedRat.Owned.ShouldBe(rat.Owned));
    }

    [Fact]
    public void Rat_transforms_to_search_result()
    {
        var rat = this.faker.Rat();
        rat.ToSearchResult().ShouldSatisfyAllConditions(
            result => result.Id.ShouldBe(rat.Id),
            result => result.Name.ShouldBe(rat.Name),
            result => result.Sex.ShouldBe(rat.Sex),
            result => result.DateOfBirth.ShouldBe(rat.DateOfBirth));
    }

    [Fact]
    public void Rat_age_is_null_if_date_of_birth_not_set()
    {
        var rat = this.faker.Rat();
        rat.DateOfBirth = null;
        
        rat.Age(() => DateOnly.MinValue).ShouldBeNull();
    }

    [Fact]
    public void Rat_age_is_calculated_to_date_if_date_of_birth_set()
    {
        var rat = this.faker.Rat();
        var today = rat.DateOfBirth!.Value.AddYears(1).AddMonths(2).AddDays(3);

        var dateOfBirth = rat.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue);
        var todayDateTime = today.ToDateTime(TimeOnly.MinValue);
        rat.Age(() => today).ShouldBe(todayDateTime - dateOfBirth);
    }

    [Fact]
    public void Rat_age_is_calculated_to_date_of_death_if_set()
    {
        var rat = this.faker.Rat();
        rat.DateOfDeath = rat.DateOfBirth!.Value.AddYears(1).AddMonths(2).AddDays(3);

        var dateOfBirth = rat.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue);
        var dateOfDeath = rat.DateOfDeath.Value.ToDateTime(TimeOnly.MinValue);
        rat.Age(() => DateOnly.MaxValue).ShouldBe(dateOfDeath - dateOfBirth);
    }

    [Fact]
    public async Task Set_owner_sets_owner_id_and_name_if_unowned()
    {
        var rat = await Rat.Create(this.repository);
        var owner = this.faker.Owner();
        
        rat.Owned = false;
        var result = await rat.SetOwner(this.repository, owner);
        
        result.ShouldBe(OwnerAddResult.Success);
        rat.ShouldSatisfyAllConditions(
            () => rat.OwnerId.ShouldBe(owner.Id),
            () => rat.OwnerName.ShouldBe(owner.Name));
    }
    
    [Fact]
    public async Task Set_owner_saves_rat_with_owner_details_if_unowned()
    {
        var rat = await Rat.Create(this.repository);
        var owner = this.faker.Owner();
        
        rat.Owned = false;
        var result = await rat.SetOwner(this.repository, owner);
        
        result.ShouldBe(OwnerAddResult.Success);
        this.repository.Rats[rat.Id].OwnerId.ShouldBe(owner.Id);
    }

    [Fact]
    public async Task Set_owner_returns_owned_by_user_if_owned_and_doesnt_save()
    {
        var rat = await Rat.Create(this.repository);
        var owner = this.faker.Owner();
        
        rat.Owned = true;
        var result = await rat.SetOwner(this.repository, owner);
        
        result.ShouldBe(OwnerAddResult.OwnedByUser);
        rat.ShouldSatisfyAllConditions(
            () => rat.OwnerId.ShouldBeNull(),
            () => rat.OwnerName.ShouldBeNull());
        this.repository.Rats[rat.Id].OwnerId.ShouldBeNull();
    }

    [Fact]
    public async Task Remove_owner_removes_and_saves()
    {
        var rat = await Rat.Create(this.repository);
        var owner = this.faker.Owner();
        
        rat.Owned = false;
        await rat.SetOwner(this.repository, owner);
        await rat.RemoveOwner(this.repository);
        
        rat.ShouldSatisfyAllConditions(
            () => rat.OwnerId.ShouldBeNull(),
            () => rat.OwnerName.ShouldBeNull());
        this.repository.Rats[rat.Id].ShouldSatisfyAllConditions(
            storedRat => storedRat.OwnerId.ShouldBeNull(),
            storedRat => storedRat.OwnerName.ShouldBeNull());
    }
}