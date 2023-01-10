using Microsoft.JSInterop;

namespace Basemix.UI;

public class JsInteropExports
{
    private readonly IJSRuntime runtime;

    public JsInteropExports(IJSRuntime runtime)
    {
        this.runtime = runtime;
    }

    public async Task<object> HistoryBack() =>
        await this.runtime.InvokeAsync<object>("history.back");
}

public delegate Task HistoryBack();