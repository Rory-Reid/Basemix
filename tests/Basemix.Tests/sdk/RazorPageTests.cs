using Microsoft.AspNetCore.Components;

namespace Basemix.Tests.sdk;

public abstract class RazorPageTests<T> : IAsyncLifetime where T : ComponentBase, new()
{
    protected T Page { get; private set; } = null!;
    protected abstract T CreatePage();

    public async Task InitializeAsync()
    {
        // https://learn.microsoft.com/en-us/aspnet/core/blazor/components/lifecycle?view=aspnetcore-6.0
        this.Page = this.CreatePage();
        await RazorEngine.InvokeOnInitializedAsync(this.Page);
        await RazorEngine.InvokeOnParametersSetAsync(this.Page);
    }
    
    public Task DisposeAsync() => Task.CompletedTask;
}