using Microsoft.AspNetCore.Components;

namespace Basemix.Tests.sdk;

public class RazorPageTests<T> : IAsyncLifetime where T : ComponentBase, new()
{
    protected T Page { get; init; } = new();
    public Task InitializeAsync() => RazorEngine.InvokeOnInitializedAsync(this.Page);
    public Task DisposeAsync() => Task.CompletedTask;
}