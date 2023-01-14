using Microsoft.AspNetCore.Components;

namespace Basemix.Tests.sdk;

public abstract class RazorPageTests<T> : IAsyncLifetime where T : ComponentBase, new()
{
    protected RazorPageTests()
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        this.Page = this.CreatePage();
    }
    
    protected T Page { get; }
    protected abstract T CreatePage();

    public Task InitializeAsync() => RazorEngine.InvokeOnInitializedAsync(this.Page);
    public Task DisposeAsync() => Task.CompletedTask;
}