using System.Reflection;

namespace MauiControlsExtras.Controls;

/// <summary>
/// Manages Quill.js resources for the RichTextEditor.
/// </summary>
public static class QuillJsResources
{
    private const string QuillVersion = "1.3.7";
    private const string TurndownVersion = "7.1.2";
    private const string MarkedVersion = "9.1.6";

    private static string? _localResourcePath;
    private static bool _resourcesExtracted;
    private static readonly object _lockObject = new();

    /// <summary>
    /// Gets the CDN URLs for Quill.js resources.
    /// </summary>
    public static QuillJsUrls CdnUrls { get; } = new()
    {
        QuillCss = $"https://cdn.quilljs.com/{QuillVersion}/quill.snow.css",
        QuillJs = $"https://cdn.quilljs.com/{QuillVersion}/quill.min.js",
        TurndownJs = $"https://unpkg.com/turndown@{TurndownVersion}/dist/turndown.js",
        MarkedJs = $"https://unpkg.com/marked@{MarkedVersion}/marked.min.js"
    };

    /// <summary>
    /// Gets or sets the path where local resources are stored.
    /// </summary>
    public static string LocalResourcePath
    {
        get => _localResourcePath ??= Path.Combine(FileSystem.AppDataDirectory, "QuillJs");
        set => _localResourcePath = value;
    }

    /// <summary>
    /// Gets the local file URLs for Quill.js resources.
    /// </summary>
    public static QuillJsUrls GetLocalUrls()
    {
        EnsureResourcesExtracted();

        // Use file:// URLs for local files
        var basePath = LocalResourcePath.Replace("\\", "/");

#if WINDOWS
        var filePrefix = "file:///";
#else
        var filePrefix = "file://";
#endif

        return new QuillJsUrls
        {
            QuillCss = $"{filePrefix}{basePath}/quill.snow.css",
            QuillJs = $"{filePrefix}{basePath}/quill.min.js",
            TurndownJs = $"{filePrefix}{basePath}/turndown.js",
            MarkedJs = $"{filePrefix}{basePath}/marked.min.js"
        };
    }

    /// <summary>
    /// Gets inline data URLs for Quill.js resources (fallback for bundled mode).
    /// </summary>
    public static QuillJsUrls GetInlineUrls()
    {
        return new QuillJsUrls
        {
            QuillCss = GetInlineCss(),
            QuillJs = GetInlineQuillJs(),
            TurndownJs = GetInlineTurndownJs(),
            MarkedJs = GetInlineMarkedJs(),
            IsInline = true
        };
    }

    /// <summary>
    /// Ensures bundled resources are extracted to local storage.
    /// </summary>
    public static void EnsureResourcesExtracted()
    {
        if (_resourcesExtracted) return;

        lock (_lockObject)
        {
            if (_resourcesExtracted) return;

            try
            {
                Directory.CreateDirectory(LocalResourcePath);

                // Extract embedded resources
                ExtractResourceIfMissing("quill.snow.css");
                ExtractResourceIfMissing("quill.min.js");
                ExtractResourceIfMissing("turndown.js");
                ExtractResourceIfMissing("marked.min.js");

                _resourcesExtracted = true;
            }
            catch
            {
                // If extraction fails, fallback to inline or CDN
                _resourcesExtracted = false;
            }
        }
    }

    /// <summary>
    /// Downloads Quill.js resources from CDN to local storage.
    /// Call this on app startup if you want to pre-cache for offline use.
    /// </summary>
    public static async Task DownloadResourcesAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(LocalResourcePath);

        var resources = new[]
        {
            (CdnUrls.QuillCss, "quill.snow.css"),
            (CdnUrls.QuillJs, "quill.min.js"),
            (CdnUrls.TurndownJs, "turndown.js"),
            (CdnUrls.MarkedJs, "marked.min.js")
        };

        using var httpClient = new HttpClient();
        var completed = 0;

        foreach (var (url, filename) in resources)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var localPath = Path.Combine(LocalResourcePath, filename);
            var content = await httpClient.GetStringAsync(url, cancellationToken);
            await File.WriteAllTextAsync(localPath, content, cancellationToken);

            completed++;
            progress?.Report((double)completed / resources.Length);
        }

        _resourcesExtracted = true;
    }

    /// <summary>
    /// Checks if local resources are available.
    /// </summary>
    public static bool AreLocalResourcesAvailable()
    {
        var files = new[] { "quill.snow.css", "quill.min.js", "turndown.js", "marked.min.js" };
        return files.All(f => File.Exists(Path.Combine(LocalResourcePath, f)));
    }

    /// <summary>
    /// Clears cached local resources.
    /// </summary>
    public static void ClearLocalResources()
    {
        if (Directory.Exists(LocalResourcePath))
        {
            Directory.Delete(LocalResourcePath, true);
        }
        _resourcesExtracted = false;
    }

    private static void ExtractResourceIfMissing(string filename)
    {
        var localPath = Path.Combine(LocalResourcePath, filename);
        if (File.Exists(localPath)) return;

        var assembly = typeof(QuillJsResources).Assembly;
        var resourceName = $"MauiControlsExtras.Controls.RichTextEditor.Resources.{filename}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            using var fileStream = File.Create(localPath);
            stream.CopyTo(fileStream);
        }
    }

    // Minimal inline CSS for Quill (essential styles only)
    private static string GetInlineCss()
    {
        return @"
.ql-container{box-sizing:border-box;font-family:Helvetica,Arial,sans-serif;font-size:13px;height:100%;margin:0;position:relative}
.ql-container.ql-disabled .ql-tooltip{visibility:hidden}
.ql-editor{box-sizing:border-box;line-height:1.42;height:100%;outline:none;overflow-y:auto;padding:12px 15px;tab-size:4;-moz-tab-size:4;text-align:left;white-space:pre-wrap;word-wrap:break-word}
.ql-editor>*{cursor:text}
.ql-editor p,.ql-editor ol,.ql-editor ul,.ql-editor pre,.ql-editor blockquote,.ql-editor h1,.ql-editor h2,.ql-editor h3,.ql-editor h4,.ql-editor h5,.ql-editor h6{margin:0;padding:0;counter-reset:list-1 list-2 list-3 list-4 list-5 list-6 list-7 list-8 list-9}
.ql-editor ol,.ql-editor ul{padding-left:1.5em}
.ql-editor ol>li,.ql-editor ul>li{list-style-type:none}
.ql-editor ul>li::before{content:'\2022'}
.ql-editor li::before{display:inline-block;white-space:nowrap;width:1.2em}
.ql-editor ol li{counter-reset:list-1 list-2 list-3 list-4 list-5 list-6 list-7 list-8 list-9;counter-increment:list-0}
.ql-editor ol li::before{content:counter(list-0,decimal) '. '}
.ql-editor .ql-indent-1{padding-left:3em}
.ql-editor .ql-indent-2{padding-left:6em}
.ql-editor li.ql-indent-1{padding-left:4.5em}
.ql-editor li.ql-indent-2{padding-left:7.5em}
.ql-editor blockquote{border-left:4px solid #ccc;margin-bottom:5px;margin-top:5px;padding-left:16px}
.ql-editor code,.ql-editor pre{background-color:#f0f0f0;border-radius:3px}
.ql-editor pre{white-space:pre-wrap;margin-bottom:5px;margin-top:5px;padding:5px 10px}
.ql-editor code{font-size:85%;padding:2px 4px}
.ql-editor h1{font-size:2em}
.ql-editor h2{font-size:1.5em}
.ql-editor h3{font-size:1.17em}
.ql-editor a{text-decoration:underline}
.ql-editor img{max-width:100%}
.ql-editor.ql-blank::before{color:rgba(0,0,0,0.6);content:attr(data-placeholder);font-style:italic;left:15px;pointer-events:none;position:absolute;right:15px}
.ql-snow .ql-hidden{display:none}
.ql-snow .ql-toolbar{border:1px solid #ccc;box-sizing:border-box;padding:8px}
.ql-snow .ql-container{border:1px solid #ccc}
";
    }

    // Note: Full Quill.js is too large to inline. Use CDN or download.
    private static string GetInlineQuillJs()
    {
        // Return empty - will fallback to CDN
        return "";
    }

    private static string GetInlineTurndownJs()
    {
        // Return empty - will fallback to CDN
        return "";
    }

    private static string GetInlineMarkedJs()
    {
        // Return empty - will fallback to CDN
        return "";
    }
}

/// <summary>
/// Contains URLs for Quill.js resources.
/// </summary>
public class QuillJsUrls
{
    /// <summary>
    /// Gets or sets the Quill CSS URL.
    /// </summary>
    public string QuillCss { get; set; } = "";

    /// <summary>
    /// Gets or sets the Quill JS URL.
    /// </summary>
    public string QuillJs { get; set; } = "";

    /// <summary>
    /// Gets or sets the Turndown JS URL.
    /// </summary>
    public string TurndownJs { get; set; } = "";

    /// <summary>
    /// Gets or sets the Marked JS URL.
    /// </summary>
    public string MarkedJs { get; set; } = "";

    /// <summary>
    /// Gets or sets whether these are inline styles/scripts (not URLs).
    /// </summary>
    public bool IsInline { get; set; }
}
