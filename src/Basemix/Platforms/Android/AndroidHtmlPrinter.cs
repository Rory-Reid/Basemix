using Android.Content;
using Android.Print;
using AWebView = Android.Webkit.WebView;
using AWebViewClient = Android.Webkit.WebViewClient;

namespace Basemix.Platforms.Android;

public class AndroidHtmlPrinter : IHtmlPrinter
{
    public Task PrintHtmlAsync(string html, string jobName)
    {
        var context = Platform.CurrentActivity
            ?? throw new InvalidOperationException("No current activity");

        var webView = new AWebView(context);
        webView.Settings.JavaScriptEnabled = false;

        var tcs = new TaskCompletionSource();

        webView.SetWebViewClient(new PrintWebViewClient(jobName, tcs));
        webView.LoadDataWithBaseURL(null, html, "text/html", "utf-8", null);

        return tcs.Task;
    }

    private class PrintWebViewClient(string jobName, TaskCompletionSource tcs) : AWebViewClient
    {
        public override void OnPageFinished(AWebView? view, string? url)
        {
            if (view == null) return;

            try
            {
                var printManager = (PrintManager?)Platform.CurrentActivity
                    ?.GetSystemService(Context.PrintService);

                if (printManager == null)
                {
                    tcs.TrySetException(new InvalidOperationException("PrintManager not available"));
                    return;
                }

                var adapter = view.CreatePrintDocumentAdapter(jobName);
                var attributes = new PrintAttributes.Builder()
                    .SetMediaSize(PrintAttributes.MediaSize.IsoA4!.AsLandscape())
                    .SetMinMargins(PrintAttributes.Margins.NoMargins!)
                    .Build();

                printManager.Print(jobName, adapter, attributes);
                tcs.TrySetResult();
            }
            catch (Exception e)
            {
                tcs.TrySetException(e);
            }
        }
    }
}
