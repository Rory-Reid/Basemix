using CoreGraphics;
using UIKit;
using WebKit;

namespace Basemix.Platforms.MacCatalyst;

public class AppleHtmlPrinter : IHtmlPrinter
{
    public Task PrintHtmlAsync(string html, string jobName)
    {
        var tcs = new TaskCompletionSource();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                var webView = new WKWebView(new CGRect(0, 0, 1, 1), new WKWebViewConfiguration());
                webView.NavigationDelegate = new PrintNavigationDelegate(jobName, tcs);
                webView.LoadHtmlString(html, null!);
            }
            catch (Exception e)
            {
                tcs.TrySetException(e);
            }
        });

        return tcs.Task;
    }

    private class PrintNavigationDelegate(string jobName, TaskCompletionSource tcs) : WKNavigationDelegate
    {
        public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            try
            {
                var printInfo = UIPrintInfo.PrintInfo;
                printInfo.JobName = jobName;
                printInfo.OutputType = UIPrintInfoOutputType.General;
                printInfo.Orientation = UIPrintInfoOrientation.Landscape;

                var formatter = webView.ViewPrintFormatter;

                var renderer = new SinglePageRenderer();
                renderer.AddPrintFormatter(formatter, 0);

                var controller = UIPrintInteractionController.SharedPrintController;
                controller.PrintInfo = printInfo;
                controller.PrintPageRenderer = renderer;

                controller.Present(true, (_, completed, error) =>
                {
                    if (error != null)
                        tcs.TrySetException(new Exception(error.LocalizedDescription));
                    else
                        tcs.TrySetResult();
                });
            }
            catch (Exception e)
            {
                tcs.TrySetException(e);
            }
        }
    }

    private class SinglePageRenderer : UIPrintPageRenderer
    {
        public override nint NumberOfPages => 1;
    }
}
