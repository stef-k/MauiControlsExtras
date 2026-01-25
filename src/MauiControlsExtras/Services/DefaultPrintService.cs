namespace MauiControlsExtras.Services;

/// <summary>
/// Default cross-platform print service implementation.
/// Uses platform-specific APIs where available.
/// </summary>
public class DefaultPrintService : IPrintService
{
    /// <inheritdoc />
    public bool IsPrintingSupported
    {
        get
        {
#if WINDOWS || MACCATALYST || IOS
            return true;
#elif ANDROID
            return Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat;
#else
            return false;
#endif
        }
    }

    /// <inheritdoc />
    public async Task<bool> PrintAsync(string content, PrintOptions? options = null)
    {
        if (!IsPrintingSupported)
            return false;

        options ??= new PrintOptions();

        // Wrap plain text in basic HTML for consistent printing
        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>{options.Title ?? "Print"}</title>
    <style>
        body {{ font-family: sans-serif; white-space: pre-wrap; }}
    </style>
</head>
<body>
<pre>{System.Web.HttpUtility.HtmlEncode(content)}</pre>
</body>
</html>";

        return await PrintHtmlAsync(html, options);
    }

    /// <inheritdoc />
    public async Task<bool> PrintHtmlAsync(string html, PrintOptions? options = null)
    {
        if (!IsPrintingSupported)
            return false;

        options ??= new PrintOptions();

#if WINDOWS
        return await PrintOnWindowsAsync(html, options);
#elif MACCATALYST || IOS
        return await PrintOnAppleAsync(html, options);
#elif ANDROID
        return await PrintOnAndroidAsync(html, options);
#else
        await Task.CompletedTask;
        return false;
#endif
    }

#if WINDOWS
    private static async Task<bool> PrintOnWindowsAsync(string html, PrintOptions options)
    {
        try
        {
            // Create a temporary HTML file
            var tempPath = Path.Combine(Path.GetTempPath(), $"print_{Guid.NewGuid()}.html");
            await File.WriteAllTextAsync(tempPath, html);

            // Use the default browser to print
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                }
            };
            process.Start();

            // Clean up after a delay
            _ = Task.Delay(5000).ContinueWith(_ =>
            {
                try { File.Delete(tempPath); } catch { }
            });

            return true;
        }
        catch
        {
            return false;
        }
    }
#endif

#if MACCATALYST || IOS
    private static async Task<bool> PrintOnAppleAsync(string html, PrintOptions options)
    {
        try
        {
            var result = false;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                var printInfo = UIKit.UIPrintInfo.PrintInfo;
                printInfo.OutputType = UIKit.UIPrintInfoOutputType.General;
                printInfo.JobName = options.Title ?? "Print Job";
                printInfo.Orientation = options.Orientation == PageOrientation.Landscape
                    ? UIKit.UIPrintInfoOrientation.Landscape
                    : UIKit.UIPrintInfoOrientation.Portrait;

                var printController = UIKit.UIPrintInteractionController.SharedPrintController;
                printController.PrintInfo = printInfo;

                var formatter = new UIKit.UIMarkupTextPrintFormatter(html);
                printController.PrintFormatter = formatter;

                printController.Present(true, (controller, completed, error) =>
                {
                    result = completed;
                });
            });

            return result;
        }
        catch
        {
            return false;
        }
    }
#endif

#if ANDROID
    private static async Task<bool> PrintOnAndroidAsync(string html, PrintOptions options)
    {
        try
        {
            var activity = Platform.CurrentActivity;
            if (activity == null)
                return false;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                var webView = new Android.Webkit.WebView(activity);
                webView.LoadDataWithBaseURL(null, html, "text/html", "UTF-8", null);

                var printManager = (Android.Print.PrintManager?)activity.GetSystemService(Android.Content.Context.PrintService);
                if (printManager == null)
                    return;

                var jobName = options.Title ?? "Print Job";
                var printAdapter = webView.CreatePrintDocumentAdapter(jobName);

                var builder = new Android.Print.PrintAttributes.Builder();
                var mediaSize = options.Orientation == PageOrientation.Landscape
                    ? Android.Print.PrintAttributes.MediaSize.UnknownLandscape
                    : Android.Print.PrintAttributes.MediaSize.UnknownPortrait;
                if (mediaSize != null)
                {
                    builder.SetMediaSize(mediaSize);
                }

                printManager.Print(jobName, printAdapter, builder.Build());
            });

            return true;
        }
        catch
        {
            return false;
        }
    }
#endif
}
