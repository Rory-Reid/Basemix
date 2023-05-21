using Basemix.Tests.sdk;
using Microsoft.JSInterop;

namespace Basemix.Tests.Integration;

public class CompositionTests
{
    [Fact]
    public void Services_build_and_validate()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IJSRuntime, NullJsRuntime>();
        serviceCollection.AddBasemix();

        _ = serviceCollection.BuildServiceProvider(
            new ServiceProviderOptions {ValidateOnBuild = true, ValidateScopes = true});
    }
}