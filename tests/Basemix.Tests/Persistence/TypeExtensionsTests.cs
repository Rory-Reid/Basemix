using Basemix.Persistence;
using Shouldly;

namespace Basemix.Tests.Persistence;

public class TypeExtensionsTests
{
    [Fact]
    public void AsDateTime_extension_converts_unix_seconds_to_DateTime()
    {
        var date = 1672519509L;
        date.AsDateTime().ShouldBe(new DateTime(2022, 12, 31, 20, 45, 09));
    }

    [Fact]
    public void AsDateOnly_extension_converts_unix_seconds_to_DateOnly()
    {
        var date = 1672519509L;
        date.AsDateOnly().ShouldBe(new DateOnly(2022, 12, 31));
    }

    [Fact]
    public void AsPersistedDateTime_extension_converts_DateTime_to_unix_seconds()
    {
        var date = new DateTime(2022, 12, 31, 20, 45, 09);
        date.AsPersistedDateTime().ShouldBe(1672519509L);
    }

    [Fact]
    public void AsPersistedDateTime_extension_converts_DateOnly_to_unix_seconds()
    {
        var date = new DateOnly(2022, 12, 31);
        date.AsPersistedDateTime().ShouldBe(1672444800L);
    }
}