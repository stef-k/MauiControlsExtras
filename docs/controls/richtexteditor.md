# RichTextEditor

A rich text editor control with HTML and Markdown support, powered by Quill.js.

## Features

- **HTML & Markdown** - Two-way binding for both formats
- **Formatting** - Bold, italic, underline, strikethrough, headings, lists
- **Customizable Toolbar** - Configure toolbar buttons or hide toolbar entirely
- **Offline Support** - Bundled Quill.js for offline operation
- **Dark Theme** - Dynamic light/dark theme switching
- **Undo/Redo** - Full undo/redo support with history limit
- **Clipboard** - Copy, cut, paste support
- **Links & Images** - Insert hyperlinks and images
- **Keyboard Navigation** - Standard formatting shortcuts

## Basic Usage

```xml
<extras:RichTextEditor
    HtmlContent="{Binding Content, Mode=TwoWay}"
    Placeholder="Start typing..."
    MinHeight="200" />
```

## Markdown Mode

```xml
<extras:RichTextEditor
    MarkdownContent="{Binding MarkdownContent, Mode=TwoWay}"
    ContentFormat="Markdown" />
```

## Toolbar Configuration

```xml
<!-- Standard toolbar (default) -->
<extras:RichTextEditor ToolbarItems="{x:Static extras:ToolbarConfig.Standard}" />

<!-- Minimal toolbar -->
<extras:RichTextEditor ToolbarItems="{x:Static extras:ToolbarConfig.Minimal}" />

<!-- Custom toolbar -->
<extras:RichTextEditor>
    <extras:RichTextEditor.ToolbarItems>
        <extras:ToolbarConfig Bold="True"
                             Italic="True"
                             BulletList="True"
                             Link="True" />
    </extras:RichTextEditor.ToolbarItems>
</extras:RichTextEditor>

<!-- No toolbar -->
<extras:RichTextEditor ToolbarPosition="None" />
```

## Theme Support

```xml
<!-- Auto theme (follows system) - default -->
<extras:RichTextEditor ThemeMode="Auto" />

<!-- Force dark theme -->
<extras:RichTextEditor ThemeMode="Dark" />

<!-- Force light theme -->
<extras:RichTextEditor ThemeMode="Light" />
```

## Quill.js Source Options

```xml
<!-- Bundled (offline) - default -->
<extras:RichTextEditor QuillSource="Bundled" />

<!-- CDN (requires internet) -->
<extras:RichTextEditor QuillSource="Cdn" />

<!-- Custom URLs -->
<extras:RichTextEditor
    QuillSource="Custom"
    CustomQuillCssUrl="https://mycdn.com/quill.css"
    CustomQuillJsUrl="https://mycdn.com/quill.js" />
```

## Read-Only Mode

```xml
<extras:RichTextEditor
    HtmlContent="{Binding Content}"
    IsReadOnly="True" />
```

## Code-Behind Operations

```csharp
// Insert text
await editor.InsertTextAsync("Hello World");

// Insert HTML
await editor.InsertHtmlAsync("<p>Hello <strong>World</strong></p>");

// Apply formatting
await editor.ApplyFormatAsync(FormatType.Bold);
await editor.ApplyFormatAsync(FormatType.Heading1);

// Insert link
await editor.InsertLinkAsync("https://example.com", "Click here");

// Clear formatting
await editor.ClearFormatAsync();

// Focus/blur
await editor.FocusAsync();
await editor.BlurAsync();

// Undo/Redo
editor.Undo();
editor.Redo();

// Get plain text
string text = await editor.GetPlainTextAsync();
```

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| Ctrl+B | Bold |
| Ctrl+I | Italic |
| Ctrl+U | Underline |
| Ctrl+C | Copy |
| Ctrl+X | Cut |
| Ctrl+V | Paste |
| Ctrl+Z | Undo |
| Ctrl+Y | Redo |
| Ctrl+Shift+Z | Redo |

## Events

| Event | Description |
|-------|-------------|
| ContentChanged | Content has changed |
| SelectionChanged | Text selection changed |
| FocusChanged | Editor gained/lost focus |
| LinkTapped | User tapped a link |

## Commands

| Command | Description |
|---------|-------------|
| ContentChangedCommand | Execute when content changes |
| SelectionChangedCommand | Execute when selection changes |
| FocusChangedCommand | Execute when focus changes |
| LinkTappedCommand | Execute when link is tapped |
| CopyCommand | Execute copy operation |
| CutCommand | Execute cut operation |
| PasteCommand | Execute paste operation |
| UndoCommand | Execute undo operation |
| RedoCommand | Execute redo operation |

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| HtmlContent | string | null | HTML content (two-way binding) |
| MarkdownContent | string | null | Markdown content (two-way binding) |
| ContentFormat | ContentFormat | Html | Primary content format |
| Placeholder | string | null | Placeholder text |
| IsReadOnly | bool | false | Read-only mode |
| ToolbarItems | ToolbarConfig | Standard | Toolbar configuration |
| ToolbarPosition | ToolbarPosition | Top | Top, Bottom, or None |
| MinHeight | double | 150 | Minimum editor height |
| MaxHeight | double | Infinity | Maximum editor height |
| ThemeMode | EditorThemeMode | Auto | Auto, Light, or Dark |
| QuillSource | QuillJsSource | Bundled | Cdn, Bundled, or Custom |
| UndoLimit | int | 100 | Maximum undo history size |
| IsLoading | bool | true | Whether editor is loading |
| IsDarkTheme | bool | (computed) | Current effective theme |
