# RichTextEditor API Reference

Full API documentation for the `MauiControlsExtras.Controls.RichTextEditor` control.

## Namespace

```csharp
using MauiControlsExtras.Controls;
```

## Class Definition

```csharp
public partial class RichTextEditor : TextStyledControlBase, IKeyboardNavigable, IClipboardSupport, IUndoRedo
```

## Inheritance

Inherits from [TextStyledControlBase](base-classes.md#textstyledcontrolbase). See base class documentation for inherited styling and typography properties.

## Interfaces

- [IKeyboardNavigable](interfaces.md#ikeyboardnavigable) - Keyboard navigation support
- [IClipboardSupport](interfaces.md#iclipboardsupport) - Clipboard operations
- [IUndoRedo](interfaces.md#iundoredo) - Undo/Redo functionality

---

## Properties

### Core Properties

#### HtmlContent

Gets or sets the HTML content of the editor.

```csharp
public string? HtmlContent { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `string?` | `null` | Yes | TwoWay |

---

#### MarkdownContent

Gets or sets the Markdown content of the editor.

```csharp
public string? MarkdownContent { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `string?` | `null` | Yes | TwoWay |

---

#### ContentFormat

Gets or sets the primary content format (HTML or Markdown).

```csharp
public ContentFormat ContentFormat { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `ContentFormat` | `Html` | Yes |

---

#### Placeholder

Gets or sets the placeholder text shown when the editor is empty.

```csharp
public string? Placeholder { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string?` | `null` | Yes |

---

#### IsReadOnly

Gets or sets whether the editor is read-only.

```csharp
public bool IsReadOnly { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

### Toolbar Properties

#### ToolbarItems

Gets or sets the toolbar configuration.

```csharp
public ToolbarConfig ToolbarItems { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `ToolbarConfig` | `ToolbarConfig.Standard` | Yes |

---

#### ToolbarPosition

Gets or sets the toolbar position.

```csharp
public ToolbarPosition ToolbarPosition { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `ToolbarPosition` | `Top` | Yes |

---

### Size Properties

#### MinHeight

Gets or sets the minimum height of the editor.

```csharp
public double MinHeight { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `150` | Yes |

---

#### MaxHeight

Gets or sets the maximum height of the editor.

```csharp
public double MaxHeight { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `double.PositiveInfinity` | Yes |

---

### Appearance Properties

#### EditorBackground

Gets or sets the editor background color.

```csharp
public Color? EditorBackground { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` | Yes |

---

#### ThemeMode

Gets or sets the theme mode for the editor.

```csharp
public EditorThemeMode ThemeMode { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `EditorThemeMode` | `Auto` | Yes |

---

### Library Source Properties

#### QuillSource

Gets or sets the source for Quill.js and related libraries.

```csharp
public QuillJsSource QuillSource { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `QuillJsSource` | `Bundled` | Yes |

---

#### CustomQuillCssUrl

Gets or sets the custom Quill CSS URL when QuillSource is Custom.

```csharp
public string? CustomQuillCssUrl { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string?` | `null` | Yes |

---

#### CustomQuillJsUrl

Gets or sets the custom Quill JS URL when QuillSource is Custom.

```csharp
public string? CustomQuillJsUrl { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string?` | `null` | Yes |

---

### State Properties

#### IsLoading (Read-only)

Gets whether the editor is loading.

```csharp
public bool IsLoading { get; }
```

---

#### IsDarkTheme (Read-only)

Gets whether the current effective theme is dark.

```csharp
public bool IsDarkTheme { get; }
```

---

#### ShowTopToolbar (Read-only)

Gets whether the top toolbar should be visible.

```csharp
public bool ShowTopToolbar { get; }
```

---

#### ShowBottomToolbar (Read-only)

Gets whether the bottom toolbar should be visible.

```csharp
public bool ShowBottomToolbar { get; }
```

---

### IClipboardSupport Properties

#### CanCopy (Read-only)

Gets whether copy is available.

```csharp
public bool CanCopy { get; }
```

---

#### CanCut (Read-only)

Gets whether cut is available.

```csharp
public bool CanCut { get; }
```

---

#### CanPaste (Read-only)

Gets whether paste is available.

```csharp
public bool CanPaste { get; }
```

---

### IUndoRedo Properties

#### CanUndo (Read-only)

Gets whether undo is available.

```csharp
public bool CanUndo { get; }
```

---

#### CanRedo (Read-only)

Gets whether redo is available.

```csharp
public bool CanRedo { get; }
```

---

#### UndoLimit

Gets or sets the undo history limit.

```csharp
public int UndoLimit { get; set; }
```

| Type | Default |
|------|---------|
| `int` | `100` |

---

## Events

### ContentChanged

Occurs when the content changes.

```csharp
public event EventHandler<RichTextContentChangedEventArgs>? ContentChanged;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| OldContent | `string?` | Previous content |
| NewContent | `string?` | New content |
| Format | `ContentFormat` | Content format |

---

### SelectionChanged

Occurs when the selection changes.

```csharp
public event EventHandler<RichTextSelectionChangedEventArgs>? SelectionChanged;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| Start | `int` | Selection start position |
| Length | `int` | Selection length |
| SelectedText | `string` | Selected text |

---

### LinkTapped

Occurs when a link is tapped.

```csharp
public event EventHandler<RichTextLinkTappedEventArgs>? LinkTapped;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| Url | `string` | The link URL |
| Text | `string?` | The link text |
| Handled | `bool` | Set to true to prevent default handling |

---

### ImageRequested

Occurs when an image insert is requested.

```csharp
public event EventHandler<RichTextImageRequestedEventArgs>? ImageRequested;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| ImageUrl | `string?` | Set this with the image URL |
| AltText | `string?` | Set this with alt text |
| Handled | `bool` | Set to true when you've provided the URL |

---

### FocusChanged

Occurs when the editor focus changes.

```csharp
public event EventHandler<RichTextFocusChangedEventArgs>? FocusChanged;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| IsFocused | `bool` | Whether the editor has focus |

---

## Commands

### ContentChangedCommand

Executed when content changes.

```csharp
public ICommand? ContentChangedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Args | `RichTextContentChangedEventArgs` |

---

### SelectionChangedCommand

Executed when selection changes.

```csharp
public ICommand? SelectionChangedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Args | `RichTextSelectionChangedEventArgs` |

---

### LinkTappedCommand

Executed when a link is tapped.

```csharp
public ICommand? LinkTappedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Args | `RichTextLinkTappedEventArgs` |

---

### FocusChangedCommand

Executed when focus changes.

```csharp
public ICommand? FocusChangedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Args | `RichTextFocusChangedEventArgs` |

---

### CopyCommand

Executed when copy is triggered.

```csharp
public ICommand? CopyCommand { get; set; }
```

---

### CutCommand

Executed when cut is triggered.

```csharp
public ICommand? CutCommand { get; set; }
```

---

### PasteCommand

Executed when paste is triggered.

```csharp
public ICommand? PasteCommand { get; set; }
```

---

### UndoCommand

Executed when undo is triggered.

```csharp
public ICommand? UndoCommand { get; set; }
```

---

### RedoCommand

Executed when redo is triggered.

```csharp
public ICommand? RedoCommand { get; set; }
```

---

## Methods

### Content Methods

#### InsertTextAsync(string text)

Inserts text at the current cursor position.

```csharp
public Task InsertTextAsync(string text)
```

---

#### InsertHtmlAsync(string html)

Inserts HTML at the current cursor position.

```csharp
public Task InsertHtmlAsync(string html)
```

---

#### InsertImageAsync(string url, string? altText)

Inserts an image at the current cursor position.

```csharp
public Task InsertImageAsync(string url, string? altText = null)
```

---

#### InsertLinkAsync(string url, string? text)

Inserts a link at the current cursor position.

```csharp
public Task InsertLinkAsync(string url, string? text = null)
```

---

#### GetTextAsync()

Gets the plain text content of the editor.

```csharp
public Task<string?> GetTextAsync()
```

---

#### GetHtmlAsync()

Gets the content as HTML.

```csharp
public Task<string?> GetHtmlAsync()
```

---

#### GetMarkdownAsync()

Gets the content as Markdown.

```csharp
public Task<string?> GetMarkdownAsync()
```

---

#### GetSelectedTextAsync()

Gets the currently selected text.

```csharp
public Task<string?> GetSelectedTextAsync()
```

---

### Formatting Methods

#### FormatAsync(FormatType format)

Applies formatting to the current selection.

```csharp
public Task FormatAsync(FormatType format)
```

---

#### ClearFormattingAsync()

Clears all formatting from the current selection.

```csharp
public Task ClearFormattingAsync()
```

---

### Focus Methods

#### FocusEditorAsync()

Focuses the editor.

```csharp
public Task FocusEditorAsync()
```

---

#### BlurAsync()

Removes focus from the editor.

```csharp
public Task BlurAsync()
```

---

### Theme Methods

#### UpdateThemeAsync(bool isDark)

Updates the editor theme dynamically.

```csharp
public Task UpdateThemeAsync(bool isDark)
```

---

#### RefreshThemeAsync()

Refreshes the editor theme based on current ThemeMode setting.

```csharp
public Task RefreshThemeAsync()
```

---

### Clipboard Methods (IClipboardSupport)

#### Copy()

Copies the selection to clipboard.

```csharp
public void Copy()
```

---

#### Cut()

Cuts the selection to clipboard.

```csharp
public void Cut()
```

---

#### Paste()

Pastes from clipboard.

```csharp
public void Paste()
```

---

### Undo/Redo Methods (IUndoRedo)

#### Undo()

Undoes the last action.

```csharp
public bool Undo()
```

---

#### Redo()

Redoes the last undone action.

```csharp
public bool Redo()
```

---

#### ClearUndoHistory()

Clears the undo history.

```csharp
public void ClearUndoHistory()
```

---

## Enumerations

### ContentFormat

```csharp
public enum ContentFormat
{
    Html,       // HTML content
    Markdown    // Markdown content
}
```

### ToolbarPosition

```csharp
public enum ToolbarPosition
{
    Top,      // Toolbar at the top
    Bottom,   // Toolbar at the bottom
    None      // No toolbar
}
```

### EditorThemeMode

```csharp
public enum EditorThemeMode
{
    Auto,   // Follow system/app theme
    Light,  // Always light theme
    Dark    // Always dark theme
}
```

### QuillJsSource

```csharp
public enum QuillJsSource
{
    Bundled,   // Use bundled offline resources
    Cdn,       // Use CDN resources
    Custom     // Use custom URLs
}
```

### FormatType

```csharp
public enum FormatType
{
    Bold,
    Italic,
    Underline,
    Strikethrough,
    Heading1,
    Heading2,
    Heading3,
    BulletList,
    NumberedList,
    Quote,
    CodeBlock,
    Code
}
```

---

## Supporting Types

### ToolbarConfig

Configuration for toolbar items.

```csharp
public class ToolbarConfig
{
    public bool ShowBold { get; set; }
    public bool ShowItalic { get; set; }
    public bool ShowUnderline { get; set; }
    public bool ShowStrikethrough { get; set; }
    public bool ShowHeading1 { get; set; }
    public bool ShowHeading2 { get; set; }
    public bool ShowBulletList { get; set; }
    public bool ShowNumberedList { get; set; }
    public bool ShowQuote { get; set; }
    public bool ShowCodeBlock { get; set; }
    public bool ShowLink { get; set; }
    public bool ShowImage { get; set; }
    public bool ShowUndo { get; set; }
    public bool ShowRedo { get; set; }
    public bool ShowClearFormatting { get; set; }

    // Presets
    public static ToolbarConfig Standard { get; }
    public static ToolbarConfig Minimal { get; }
    public static ToolbarConfig Full { get; }
}
```

---

## Keyboard Shortcuts

| Key | Description |
|-----|-------------|
| Ctrl+B | Bold |
| Ctrl+I | Italic |
| Ctrl+U | Underline |
| Ctrl+C | Copy |
| Ctrl+X | Cut |
| Ctrl+V | Paste |
| Ctrl+Z | Undo |
| Ctrl+Y | Redo |
| Ctrl+Shift+Z | Redo |

---

## Usage Examples

### Basic Editor

```xml
<extras:RichTextEditor HtmlContent="{Binding Content}"
                       Placeholder="Start writing..."
                       ContentChangedCommand="{Binding OnContentChangedCommand}" />
```

### Markdown Mode

```xml
<extras:RichTextEditor MarkdownContent="{Binding MarkdownContent}"
                       ContentFormat="Markdown"
                       Placeholder="Write in Markdown..." />
```

### Minimal Toolbar

```xml
<extras:RichTextEditor HtmlContent="{Binding Content}"
                       ToolbarItems="{x:Static extras:ToolbarConfig.Minimal}"
                       ToolbarPosition="Top" />
```

### Full Toolbar

```xml
<extras:RichTextEditor HtmlContent="{Binding Content}"
                       ToolbarItems="{x:Static extras:ToolbarConfig.Full}"
                       ToolbarPosition="Bottom" />
```

### Read-Only Mode

```xml
<extras:RichTextEditor HtmlContent="{Binding ArticleContent}"
                       IsReadOnly="True"
                       ToolbarPosition="None" />
```

### Custom Theme

```xml
<extras:RichTextEditor HtmlContent="{Binding Content}"
                       ThemeMode="Dark"
                       EditorBackground="#1E1E1E"
                       AccentColor="#2196F3" />
```

### With Image Handling

```xml
<extras:RichTextEditor HtmlContent="{Binding Content}"
                       ImageRequested="OnImageRequested" />
```

```csharp
private async void OnImageRequested(object sender, RichTextImageRequestedEventArgs e)
{
    // Show image picker
    var result = await FilePicker.PickAsync(new PickOptions
    {
        FileTypes = FilePickerFileType.Images
    });

    if (result != null)
    {
        // Upload and get URL
        var url = await _imageService.UploadAsync(result);
        e.ImageUrl = url;
        e.AltText = result.FileName;
        e.Handled = true;
    }
}
```

### With Link Handling

```xml
<extras:RichTextEditor HtmlContent="{Binding Content}"
                       LinkTappedCommand="{Binding OnLinkTappedCommand}" />
```

```csharp
[RelayCommand]
private async Task OnLinkTapped(RichTextLinkTappedEventArgs args)
{
    // Custom link handling
    if (args.Url.StartsWith("internal://"))
    {
        await NavigateToInternal(args.Url);
        args.Handled = true;
    }
    // Default: opens in browser
}
```

### Size Constraints

```xml
<extras:RichTextEditor HtmlContent="{Binding Content}"
                       MinHeight="200"
                       MaxHeight="500" />
```

### Custom CDN Source

```xml
<extras:RichTextEditor HtmlContent="{Binding Content}"
                       QuillSource="Cdn" />
```

### Custom Library URLs

```xml
<extras:RichTextEditor HtmlContent="{Binding Content}"
                       QuillSource="Custom"
                       CustomQuillJsUrl="https://mycdn.com/quill.js"
                       CustomQuillCssUrl="https://mycdn.com/quill.snow.css" />
```

### Code-Behind

```csharp
// Create rich text editor
var editor = new RichTextEditor
{
    Placeholder = "Write something...",
    ToolbarItems = ToolbarConfig.Standard,
    ToolbarPosition = ToolbarPosition.Top,
    ThemeMode = EditorThemeMode.Auto,
    MinHeight = 200
};

// Handle content changes
editor.ContentChanged += (sender, args) =>
{
    Console.WriteLine($"Content changed: {args.NewContent}");
};

// Handle selection changes
editor.SelectionChanged += (sender, args) =>
{
    Console.WriteLine($"Selected: {args.SelectedText}");
};

// Handle link taps
editor.LinkTapped += (sender, args) =>
{
    if (args.Url.Contains("dangerous"))
    {
        args.Handled = true;
        ShowWarning("Blocked link");
    }
};

// Handle image requests
editor.ImageRequested += async (sender, args) =>
{
    var url = await PickAndUploadImage();
    if (!string.IsNullOrEmpty(url))
    {
        args.ImageUrl = url;
        args.Handled = true;
    }
};

// Programmatic formatting
await editor.FormatAsync(FormatType.Bold);
await editor.FormatAsync(FormatType.Italic);
await editor.InsertLinkAsync("https://example.com", "Click here");
await editor.InsertImageAsync("https://example.com/image.png", "Alt text");

// Get content
var html = await editor.GetHtmlAsync();
var markdown = await editor.GetMarkdownAsync();
var text = await editor.GetTextAsync();

// Clipboard operations
editor.Copy();
editor.Cut();
editor.Paste();

// Undo/Redo
editor.Undo();
editor.Redo();
editor.ClearUndoHistory();

// Focus control
await editor.FocusEditorAsync();
await editor.BlurAsync();

// Theme control
await editor.UpdateThemeAsync(isDark: true);
await editor.RefreshThemeAsync();
```

### MVVM Pattern

```csharp
// ViewModel
public class ArticleEditorViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _htmlContent;

    [ObservableProperty]
    private bool _isDirty;

    [RelayCommand]
    private void OnContentChanged(RichTextContentChangedEventArgs args)
    {
        IsDirty = true;
        AutoSave();
    }

    [RelayCommand]
    private async Task OnLinkTapped(RichTextLinkTappedEventArgs args)
    {
        // Handle internal links
        if (args.Url.StartsWith("article://"))
        {
            var articleId = args.Url.Replace("article://", "");
            await _navigationService.NavigateToArticle(articleId);
            args.Handled = true;
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        await _articleService.SaveAsync(HtmlContent);
        IsDirty = false;
    }

    private void AutoSave()
    {
        _autoSaveService.QueueSave(HtmlContent);
    }
}
```

