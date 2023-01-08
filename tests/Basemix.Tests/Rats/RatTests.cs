using Basemix.Rats;
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
            () => rat.Notes.ShouldBeNull());
    }
    
    [Fact]
    public void Creates_rat_with_basic_construction()
    {
        var id = new RatIdentity(this.faker.Id());
        var name = this.faker.Person.FirstName;
        var sex = this.faker.PickNonDefault<Sex>();
        var dob = this.faker.Date.RecentDateOnly();
        
        var rat = new Rat(id, name, sex, dob);
        
        rat.ShouldSatisfyAllConditions(
            () => rat.Id.ShouldBe(id),
            () => rat.Name.ShouldBe(name),
            () => rat.Sex.ShouldBe(sex),
            () => rat.DateOfBirth.ShouldBe(dob),
            () => rat.Notes.ShouldBeNull());
    }

    [Fact]
    public async Task Create_saves_and_returns_rat_with_id()
    {
        var rat = await Rat.Create(this.repository);
        
        rat.Id.Value.ShouldBePositive();
        
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
        
        this.repository.Rats[rat.Id].ShouldSatisfyAllConditions(
            storedRat => storedRat.Name.ShouldBeNull(),
            storedRat => storedRat.DateOfBirth.ShouldBeNull(),
            storedRat => storedRat.Sex.ShouldBeNull(),
            storedRat => storedRat.Notes.ShouldBeNull());

        await rat.Save(this.repository);
        
        this.repository.Rats[rat.Id].ShouldSatisfyAllConditions(
            storedRat => storedRat.Name.ShouldBe(rat.Name),
            storedRat => storedRat.DateOfBirth.ShouldBe(rat.DateOfBirth),
            storedRat => storedRat.Sex.ShouldBe(rat.Sex),
            storedRat => storedRat.Notes.ShouldBe(rat.Notes));
    }
}