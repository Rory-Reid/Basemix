using Basemix.Identity;
using Basemix.Litters;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.Litters;

public class LitterIdentityTests
{
    private readonly Faker faker = new();
    
    [Fact]
    public void Creates_from_nonzero_long()
    {
        var idNumber = this.faker.Id();
        var id = new LitterIdentity(idNumber);
        id.Value.ShouldBe(idNumber);
    }

    [Fact]
    public void Disallows_zero()
    {
        var createWithZeroId = () => new LitterIdentity(0);
        createWithZeroId.ShouldThrow<InvalidIdentityException>();
    }

    [Fact]
    public void Disallows_negative()
    {
        var createWithNegativeId = () => new LitterIdentity(this.faker.Random.Long(long.MinValue, 0 - 1));
        createWithNegativeId.ShouldThrow<InvalidIdentityException>();
    }

    [Fact]
    public void Anonymous_id_value_cannot_be_retrieved()
    {
        var anonymous = LitterIdentity.Anonymous;
        var getAnonymousValue = new Action(() => { var _ = anonymous.Value; });
        getAnonymousValue.ShouldThrow<NoIdentityException>();
    }

    [Fact]
    public void Implicit_operator_allows_conversion_from_long()
    {
        var expectedId = this.faker.Id();
        LitterIdentity id = expectedId;
        id.Value.ShouldBe(expectedId);
    }

    [Fact]
    public void Implicit_operator_allows_conversion_to_long()
    {
        var expectedId = this.faker.Id();
        var id = new LitterIdentity(expectedId);
        long implicitlyConvertedId = id;
        
        implicitlyConvertedId.ShouldBe(expectedId);
    }

    [Fact]
    public void Implicit_operator_allows_conversion_from_null_long()
    {
        LitterIdentity? id = (long?)null;
        id.ShouldBeNull();
    }

    [Fact]
    public void Implicit_operator_allows_conversion_to_null_long()
    {
        long? id = (LitterIdentity?) null;
        id.ShouldBeNull();
    }
    
    [Fact]
    public void Litter_ids_are_equal()
    {
        var value = this.faker.Id();
        var id1 = new LitterIdentity(value);
        var id2 = new LitterIdentity(value);
        
        id1.GetHashCode().ShouldBe(id2.GetHashCode());
        id1.Equals(id2).ShouldBeTrue();
        (id1 == id2).ShouldBeTrue();
    }
}