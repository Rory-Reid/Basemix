using Basemix.Lib.Identity;
using Basemix.Lib.Rats;
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

    [Fact]
    public void Implicit_operator_allows_conversion_from_long()
    {
        var expectedId = this.faker.Id();
        RatIdentity id = expectedId;
        id.Value.ShouldBe(expectedId);
    }

    [Fact]
    public void Implicit_operator_allows_conversion_to_long()
    {
        var expectedId = this.faker.Id();
        var id = new RatIdentity(expectedId);
        long implicitlyConvertedId = id;
        
        implicitlyConvertedId.ShouldBe(expectedId);
    }

    [Fact]
    public void Implicit_operator_allows_conversion_from_null_long()
    {
        RatIdentity? id = (long?)null;
        id.ShouldBeNull();
    }

    [Fact]
    public void Implicit_operator_allows_conversion_to_null_long()
    {
        long? id = (RatIdentity?) null;
        id.ShouldBeNull();
    }

    [Fact]
    public void Rat_ids_are_equal()
    {
        var value = this.faker.Id();
        var id1 = new RatIdentity(value);
        var id2 = new RatIdentity(value);
        
        id1.GetHashCode().ShouldBe(id2.GetHashCode());
        id1.Equals(id2).ShouldBeTrue();
        (id1 == id2).ShouldBeTrue();
    }
}