using Microsoft.AspNetCore.Components;

namespace Basemix.Tests.sdk;

internal class TestNavigationManager : NavigationManager
{
    public TestNavigationManager() => this.Initialize("/", "/");
    public string CurrentUri { get; private set; } = "";
    protected override void NavigateToCore(string uri, bool forceLoad) => this.CurrentUri = uri;
    protected override void NavigateToCore(string uri, NavigationOptions options) => this.CurrentUri = uri;
}