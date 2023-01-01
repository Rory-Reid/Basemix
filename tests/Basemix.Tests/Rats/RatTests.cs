using Basemix.Rats;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.Rats;

public class RatTests
{
    private readonly Faker faker = new();
    
    [Fact]
    public void Creates_rat_with_default_construction()
    {
        var name = this.faker.Person.FirstName;
        var sex = this.faker.PickNonDefault<Sex>();
        var dob = this.faker.Date.RecentDateOnly();
        
        var rat = new Rat(name, sex, dob);
        
        rat.ShouldSatisfyAllConditions(
            () => rat.Id.ShouldNotBeNull(),
            () => rat.Name.ShouldBe(name),
            () => rat.Sex.ShouldBe(sex),
            () => rat.DateOfBirth.ShouldBe(dob),
            () => rat.Notes.ShouldBeNull());
    }

    [Fact]
    public void Creates_rat_with_identity()
    {
        var name = this.faker.Person.FirstName;
        var sex = this.faker.PickNonDefault<Sex>();
        var dob = this.faker.Date.RecentDateOnly();
        var id = new RatIdentity(this.faker.Id());
        
        var rat = new Rat(name, sex, dob, id);
        
        rat.Id.Value.ShouldBe(id.Value);
    }
}