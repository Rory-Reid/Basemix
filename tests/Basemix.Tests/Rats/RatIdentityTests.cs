using Basemix.Rats;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.Rats;

public class RatIdentityTests
{
    private readonly Faker faker = new();
    
    [Fact]
    public void Creates_from_nonzero_long()
    {
        var idNumber = this.faker.Id();
        var id = new RatIdentity(idNumber);
        id.Value.ShouldBe(idNumber);
    }

    [Fact]
    public void Disallows_zero()
    {
        var createWithZeroId = () => new RatIdentity(0);
        createWithZeroId.ShouldThrow<InvalidIdentityException>();
    }

    [Fact]
    public void Disallows_negative()
    {
        var createWithNegativeId = () => new RatIdentity(this.faker.Random.Long(long.MinValue, 0 - 1));
        createWithNegativeId.ShouldThrow<InvalidIdentityException>();
    }

    [Fact]
    public void Anonymous_id_value_cannot_be_retrieved()
    {
        var anonymous = RatIdentity.Anonymous;
        var getAnonymousValue = new Action(() => { var _ = anonymous.Value; });
        getAnonymousValue.ShouldThrow<NoIdentityException>();
    }
}