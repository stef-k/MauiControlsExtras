using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DemoApp.ViewModels;

public partial class RichTextEditorDemoViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _htmlContent = string.Empty;

    [ObservableProperty]
    private string _plainText = string.Empty;

    [ObservableProperty]
    private bool _isReadOnly;

    [ObservableProperty]
    private bool _showToolbar = true;

    [ObservableProperty]
    private bool _isDarkTheme;

    public RichTextEditorDemoViewModel()
    {
        Title = "RichTextEditor Demo";
        HtmlContent = @"<h1>Welcome to RichTextEditor</h1>
<p>This is a <strong>rich text editor</strong> control built with <em>Quill.js</em>.</p>
<h2>Features</h2>
<ul>
    <li>Bold, italic, underline formatting</li>
    <li>Headers and paragraphs</li>
    <li>Ordered and unordered lists</li>
    <li>Links and images</li>
    <li>Code blocks</li>
</ul>
<p>Try editing this content!</p>";
    }

    partial void OnHtmlContentChanged(string value)
    {
        UpdateStatus($"Content length: {value?.Length ?? 0} characters");
    }

    partial void OnIsReadOnlyChanged(bool value)
    {
        UpdateStatus($"Read-only: {(value ? "On" : "Off")}");
    }

    partial void OnShowToolbarChanged(bool value)
    {
        UpdateStatus($"Toolbar: {(value ? "Visible" : "Hidden")}");
    }

    partial void OnIsDarkThemeChanged(bool value)
    {
        UpdateStatus($"Theme: {(value ? "Dark" : "Light")}");
    }

    [RelayCommand]
    private void GetHtml()
    {
        UpdateStatus($"HTML content retrieved ({HtmlContent?.Length ?? 0} chars)");
    }

    [RelayCommand]
    private void ClearContent()
    {
        HtmlContent = string.Empty;
        UpdateStatus("Content cleared");
    }

    [RelayCommand]
    private void LoadSampleContent()
    {
        HtmlContent = @"<h1>Sample Document</h1>
<p>This is a sample document with various formatting options.</p>
<h2>Code Example</h2>
<pre><code>public class HelloWorld
{
    public static void Main()
    {
        Console.WriteLine(""Hello, World!"");
    }
}</code></pre>
<h2>Quote</h2>
<blockquote>The best way to predict the future is to invent it. - Alan Kay</blockquote>
<p>Thank you for using <a href=""https://github.com"">RichTextEditor</a>!</p>";
        UpdateStatus("Sample content loaded");
    }
}
