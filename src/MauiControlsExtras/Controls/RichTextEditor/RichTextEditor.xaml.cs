using System.Text.Json;
using System.Windows.Input;
using MauiControlsExtras.Base;
using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Controls;

/// <summary>
/// A rich text editor control that supports HTML and Markdown content with a customizable toolbar.
/// Uses WebView with Quill.js for cross-platform rich text editing.
/// </summary>
public partial class RichTextEditor : TextStyledControlBase, IKeyboardNavigable, IClipboardSupport, IUndoRedo
{
    #region Constants

    // CDN URLs kept as fallback
    private const string QuillCdnBase = "https://cdn.quilljs.com/1.3.7";
    private const string TurndownCdnUrl = "https://unpkg.com/turndown@7.1.2/dist/turndown.js";
    private const string MarkedCdnUrl = "https://unpkg.com/marked@9.1.6/marked.min.js";

    #endregion

    #region Private Fields

    private bool _isInitialized;
    private bool _isUpdatingContent;
    private string? _pendingContent;
    private bool _hasKeyboardFocus;
    private bool _currentThemeIsDark;

    // Undo/Redo tracking (delegated to Quill's history module)
    private bool _canUndo;
    private bool _canRedo;

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="HtmlContent"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HtmlContentProperty = BindableProperty.Create(
        nameof(HtmlContent),
        typeof(string),
        typeof(RichTextEditor),
        null,
        BindingMode.TwoWay,
        propertyChanged: OnHtmlContentChanged);

    /// <summary>
    /// Identifies the <see cref="MarkdownContent"/> bindable property.
    /// </summary>
    public static readonly BindableProperty MarkdownContentProperty = BindableProperty.Create(
        nameof(MarkdownContent),
        typeof(string),
        typeof(RichTextEditor),
        null,
        BindingMode.TwoWay,
        propertyChanged: OnMarkdownContentChanged);

    /// <summary>
    /// Identifies the <see cref="ContentFormat"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ContentFormatProperty = BindableProperty.Create(
        nameof(ContentFormat),
        typeof(ContentFormat),
        typeof(RichTextEditor),
        ContentFormat.Html);

    /// <summary>
    /// Identifies the <see cref="Placeholder"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder),
        typeof(string),
        typeof(RichTextEditor),
        null,
        propertyChanged: OnPlaceholderChanged);

    /// <summary>
    /// Identifies the <see cref="IsReadOnly"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(
        nameof(IsReadOnly),
        typeof(bool),
        typeof(RichTextEditor),
        false,
        propertyChanged: OnIsReadOnlyChanged);

    /// <summary>
    /// Identifies the <see cref="ToolbarItems"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ToolbarItemsProperty = BindableProperty.Create(
        nameof(ToolbarItems),
        typeof(ToolbarConfig),
        typeof(RichTextEditor),
        null,
        propertyChanged: OnToolbarItemsChanged);

    /// <summary>
    /// Identifies the <see cref="ToolbarPosition"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ToolbarPositionProperty = BindableProperty.Create(
        nameof(ToolbarPosition),
        typeof(ToolbarPosition),
        typeof(RichTextEditor),
        ToolbarPosition.Top,
        propertyChanged: OnToolbarPositionChanged);

    /// <summary>
    /// Identifies the <see cref="MinHeight"/> bindable property.
    /// </summary>
    public static readonly BindableProperty MinHeightProperty = BindableProperty.Create(
        nameof(MinHeight),
        typeof(double),
        typeof(RichTextEditor),
        150.0);

    /// <summary>
    /// Identifies the <see cref="MaxHeight"/> bindable property.
    /// </summary>
    public static readonly BindableProperty MaxHeightProperty = BindableProperty.Create(
        nameof(MaxHeight),
        typeof(double),
        typeof(RichTextEditor),
        double.PositiveInfinity);

    /// <summary>
    /// Identifies the <see cref="EditorBackground"/> bindable property.
    /// </summary>
    public static readonly BindableProperty EditorBackgroundProperty = BindableProperty.Create(
        nameof(EditorBackground),
        typeof(Color),
        typeof(RichTextEditor),
        null,
        propertyChanged: OnEditorBackgroundChanged);

    /// <summary>
    /// Identifies the <see cref="IsLoading"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsLoadingProperty = BindableProperty.Create(
        nameof(IsLoading),
        typeof(bool),
        typeof(RichTextEditor),
        true);

    /// <summary>
    /// Identifies the <see cref="IsKeyboardNavigationEnabled"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsKeyboardNavigationEnabledProperty = BindableProperty.Create(
        nameof(IsKeyboardNavigationEnabled),
        typeof(bool),
        typeof(RichTextEditor),
        true);

    /// <summary>
    /// Identifies the <see cref="QuillSource"/> bindable property.
    /// </summary>
    public static readonly BindableProperty QuillSourceProperty = BindableProperty.Create(
        nameof(QuillSource),
        typeof(QuillJsSource),
        typeof(RichTextEditor),
        QuillJsSource.Bundled);

    /// <summary>
    /// Identifies the <see cref="CustomQuillCssUrl"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CustomQuillCssUrlProperty = BindableProperty.Create(
        nameof(CustomQuillCssUrl),
        typeof(string),
        typeof(RichTextEditor),
        null);

    /// <summary>
    /// Identifies the <see cref="CustomQuillJsUrl"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CustomQuillJsUrlProperty = BindableProperty.Create(
        nameof(CustomQuillJsUrl),
        typeof(string),
        typeof(RichTextEditor),
        null);

    /// <summary>
    /// Identifies the <see cref="CustomTurndownJsUrl"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CustomTurndownJsUrlProperty = BindableProperty.Create(
        nameof(CustomTurndownJsUrl),
        typeof(string),
        typeof(RichTextEditor),
        null);

    /// <summary>
    /// Identifies the <see cref="CustomMarkedJsUrl"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CustomMarkedJsUrlProperty = BindableProperty.Create(
        nameof(CustomMarkedJsUrl),
        typeof(string),
        typeof(RichTextEditor),
        null);

    /// <summary>
    /// Identifies the <see cref="ThemeMode"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ThemeModeProperty = BindableProperty.Create(
        nameof(ThemeMode),
        typeof(EditorThemeMode),
        typeof(RichTextEditor),
        EditorThemeMode.Auto,
        propertyChanged: OnThemeModeChanged);

    #endregion

    #region Command Properties

    /// <summary>
    /// Identifies the <see cref="ContentChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ContentChangedCommandProperty = BindableProperty.Create(
        nameof(ContentChangedCommand),
        typeof(ICommand),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="ContentChangedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ContentChangedCommandParameterProperty = BindableProperty.Create(
        nameof(ContentChangedCommandParameter),
        typeof(object),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="SelectionChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectionChangedCommandProperty = BindableProperty.Create(
        nameof(SelectionChangedCommand),
        typeof(ICommand),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="SelectionChangedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectionChangedCommandParameterProperty = BindableProperty.Create(
        nameof(SelectionChangedCommandParameter),
        typeof(object),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="LinkTappedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LinkTappedCommandProperty = BindableProperty.Create(
        nameof(LinkTappedCommand),
        typeof(ICommand),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="LinkTappedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LinkTappedCommandParameterProperty = BindableProperty.Create(
        nameof(LinkTappedCommandParameter),
        typeof(object),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="FocusChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FocusChangedCommandProperty = BindableProperty.Create(
        nameof(FocusChangedCommand),
        typeof(ICommand),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="FocusChangedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FocusChangedCommandParameterProperty = BindableProperty.Create(
        nameof(FocusChangedCommandParameter),
        typeof(object),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="CopyCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CopyCommandProperty = BindableProperty.Create(
        nameof(CopyCommand),
        typeof(ICommand),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="CopyCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CopyCommandParameterProperty = BindableProperty.Create(
        nameof(CopyCommandParameter),
        typeof(object),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="CutCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CutCommandProperty = BindableProperty.Create(
        nameof(CutCommand),
        typeof(ICommand),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="CutCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CutCommandParameterProperty = BindableProperty.Create(
        nameof(CutCommandParameter),
        typeof(object),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="PasteCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PasteCommandProperty = BindableProperty.Create(
        nameof(PasteCommand),
        typeof(ICommand),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="PasteCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PasteCommandParameterProperty = BindableProperty.Create(
        nameof(PasteCommandParameter),
        typeof(object),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="UndoCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty UndoCommandProperty = BindableProperty.Create(
        nameof(UndoCommand),
        typeof(ICommand),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="UndoCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty UndoCommandParameterProperty = BindableProperty.Create(
        nameof(UndoCommandParameter),
        typeof(object),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="RedoCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RedoCommandProperty = BindableProperty.Create(
        nameof(RedoCommand),
        typeof(ICommand),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="RedoCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RedoCommandParameterProperty = BindableProperty.Create(
        nameof(RedoCommandParameter),
        typeof(object),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="GotFocusCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty GotFocusCommandProperty = BindableProperty.Create(
        nameof(GotFocusCommand),
        typeof(ICommand),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="GotFocusCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty GotFocusCommandParameterProperty = BindableProperty.Create(
        nameof(GotFocusCommandParameter),
        typeof(object),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="LostFocusCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandProperty = BindableProperty.Create(
        nameof(LostFocusCommand),
        typeof(ICommand),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="LostFocusCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandParameterProperty = BindableProperty.Create(
        nameof(LostFocusCommandParameter),
        typeof(object),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="KeyPressCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandProperty = BindableProperty.Create(
        nameof(KeyPressCommand),
        typeof(ICommand),
        typeof(RichTextEditor));

    /// <summary>
    /// Identifies the <see cref="KeyPressCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandParameterProperty = BindableProperty.Create(
        nameof(KeyPressCommandParameter),
        typeof(object),
        typeof(RichTextEditor));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the HTML content of the editor.
    /// </summary>
    public string? HtmlContent
    {
        get => (string?)GetValue(HtmlContentProperty);
        set => SetValue(HtmlContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the Markdown content of the editor.
    /// </summary>
    public string? MarkdownContent
    {
        get => (string?)GetValue(MarkdownContentProperty);
        set => SetValue(MarkdownContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the primary content format (HTML or Markdown).
    /// </summary>
    public ContentFormat ContentFormat
    {
        get => (ContentFormat)GetValue(ContentFormatProperty);
        set => SetValue(ContentFormatProperty, value);
    }

    /// <summary>
    /// Gets or sets the placeholder text shown when the editor is empty.
    /// </summary>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the editor is read-only.
    /// </summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// Gets or sets the toolbar configuration.
    /// </summary>
    public ToolbarConfig ToolbarItems
    {
        get => (ToolbarConfig?)GetValue(ToolbarItemsProperty) ?? ToolbarConfig.Standard;
        set => SetValue(ToolbarItemsProperty, value);
    }

    /// <summary>
    /// Gets or sets the toolbar position.
    /// </summary>
    public ToolbarPosition ToolbarPosition
    {
        get => (ToolbarPosition)GetValue(ToolbarPositionProperty);
        set => SetValue(ToolbarPositionProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum height of the editor.
    /// </summary>
    public double MinHeight
    {
        get => (double)GetValue(MinHeightProperty);
        set => SetValue(MinHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum height of the editor.
    /// </summary>
    public double MaxHeight
    {
        get => (double)GetValue(MaxHeightProperty);
        set => SetValue(MaxHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the editor background color.
    /// </summary>
    public Color? EditorBackground
    {
        get => (Color?)GetValue(EditorBackgroundProperty);
        set => SetValue(EditorBackgroundProperty, value);
    }

    /// <summary>
    /// Gets whether the editor is loading.
    /// </summary>
    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        private set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>
    /// Gets or sets the source for Quill.js and related libraries.
    /// Default is Bundled for offline support.
    /// </summary>
    public QuillJsSource QuillSource
    {
        get => (QuillJsSource)GetValue(QuillSourceProperty);
        set => SetValue(QuillSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the custom Quill CSS URL when QuillSource is Custom.
    /// </summary>
    public string? CustomQuillCssUrl
    {
        get => (string?)GetValue(CustomQuillCssUrlProperty);
        set => SetValue(CustomQuillCssUrlProperty, value);
    }

    /// <summary>
    /// Gets or sets the custom Quill JS URL when QuillSource is Custom.
    /// </summary>
    public string? CustomQuillJsUrl
    {
        get => (string?)GetValue(CustomQuillJsUrlProperty);
        set => SetValue(CustomQuillJsUrlProperty, value);
    }

    /// <summary>
    /// Gets or sets the custom Turndown JS URL when QuillSource is Custom.
    /// </summary>
    public string? CustomTurndownJsUrl
    {
        get => (string?)GetValue(CustomTurndownJsUrlProperty);
        set => SetValue(CustomTurndownJsUrlProperty, value);
    }

    /// <summary>
    /// Gets or sets the custom Marked JS URL when QuillSource is Custom.
    /// </summary>
    public string? CustomMarkedJsUrl
    {
        get => (string?)GetValue(CustomMarkedJsUrlProperty);
        set => SetValue(CustomMarkedJsUrlProperty, value);
    }

    /// <summary>
    /// Gets or sets the theme mode for the editor.
    /// When set to Auto, the editor follows the system/app theme.
    /// </summary>
    public EditorThemeMode ThemeMode
    {
        get => (EditorThemeMode)GetValue(ThemeModeProperty);
        set => SetValue(ThemeModeProperty, value);
    }

    /// <summary>
    /// Gets the effective editor background color.
    /// </summary>
    public Color EffectiveEditorBackground =>
        EditorBackground ?? MauiControlsExtrasTheme.GetSurfaceColor();

    /// <summary>
    /// Gets whether the current effective theme is dark.
    /// </summary>
    public bool IsDarkTheme => GetEffectiveThemeIsDark();

    /// <summary>
    /// Gets whether the top toolbar should be visible.
    /// </summary>
    public bool ShowTopToolbar => ToolbarPosition == ToolbarPosition.Top && !IsReadOnly;

    /// <summary>
    /// Gets whether the bottom toolbar should be visible.
    /// </summary>
    public bool ShowBottomToolbar => ToolbarPosition == ToolbarPosition.Bottom && !IsReadOnly;

    #endregion

    #region Command Properties Implementation

    /// <summary>
    /// Gets or sets the command executed when content changes.
    /// </summary>
    public ICommand? ContentChangedCommand
    {
        get => (ICommand?)GetValue(ContentChangedCommandProperty);
        set => SetValue(ContentChangedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="ContentChangedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? ContentChangedCommandParameter
    {
        get => GetValue(ContentChangedCommandParameterProperty);
        set => SetValue(ContentChangedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when selection changes.
    /// </summary>
    public ICommand? SelectionChangedCommand
    {
        get => (ICommand?)GetValue(SelectionChangedCommandProperty);
        set => SetValue(SelectionChangedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="SelectionChangedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? SelectionChangedCommandParameter
    {
        get => GetValue(SelectionChangedCommandParameterProperty);
        set => SetValue(SelectionChangedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when a link is tapped.
    /// </summary>
    public ICommand? LinkTappedCommand
    {
        get => (ICommand?)GetValue(LinkTappedCommandProperty);
        set => SetValue(LinkTappedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="LinkTappedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? LinkTappedCommandParameter
    {
        get => GetValue(LinkTappedCommandParameterProperty);
        set => SetValue(LinkTappedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when focus changes.
    /// </summary>
    public ICommand? FocusChangedCommand
    {
        get => (ICommand?)GetValue(FocusChangedCommandProperty);
        set => SetValue(FocusChangedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="FocusChangedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? FocusChangedCommandParameter
    {
        get => GetValue(FocusChangedCommandParameterProperty);
        set => SetValue(FocusChangedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the copy command.
    /// </summary>
    public ICommand? CopyCommand
    {
        get => (ICommand?)GetValue(CopyCommandProperty);
        set => SetValue(CopyCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="CopyCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? CopyCommandParameter
    {
        get => GetValue(CopyCommandParameterProperty);
        set => SetValue(CopyCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the cut command.
    /// </summary>
    public ICommand? CutCommand
    {
        get => (ICommand?)GetValue(CutCommandProperty);
        set => SetValue(CutCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="CutCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? CutCommandParameter
    {
        get => GetValue(CutCommandParameterProperty);
        set => SetValue(CutCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the paste command.
    /// </summary>
    public ICommand? PasteCommand
    {
        get => (ICommand?)GetValue(PasteCommandProperty);
        set => SetValue(PasteCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="PasteCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? PasteCommandParameter
    {
        get => GetValue(PasteCommandParameterProperty);
        set => SetValue(PasteCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the undo command.
    /// </summary>
    public ICommand? UndoCommand
    {
        get => (ICommand?)GetValue(UndoCommandProperty);
        set => SetValue(UndoCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="UndoCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? UndoCommandParameter
    {
        get => GetValue(UndoCommandParameterProperty);
        set => SetValue(UndoCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the redo command.
    /// </summary>
    public ICommand? RedoCommand
    {
        get => (ICommand?)GetValue(RedoCommandProperty);
        set => SetValue(RedoCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="RedoCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? RedoCommandParameter
    {
        get => GetValue(RedoCommandParameterProperty);
        set => SetValue(RedoCommandParameterProperty, value);
    }

    /// <inheritdoc/>
    public ICommand? GotFocusCommand
    {
        get => (ICommand?)GetValue(GotFocusCommandProperty);
        set => SetValue(GotFocusCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="GotFocusCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? GotFocusCommandParameter
    {
        get => GetValue(GotFocusCommandParameterProperty);
        set => SetValue(GotFocusCommandParameterProperty, value);
    }

    /// <inheritdoc/>
    public ICommand? LostFocusCommand
    {
        get => (ICommand?)GetValue(LostFocusCommandProperty);
        set => SetValue(LostFocusCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="LostFocusCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? LostFocusCommandParameter
    {
        get => GetValue(LostFocusCommandParameterProperty);
        set => SetValue(LostFocusCommandParameterProperty, value);
    }

    /// <inheritdoc/>
    public ICommand? KeyPressCommand
    {
        get => (ICommand?)GetValue(KeyPressCommandProperty);
        set => SetValue(KeyPressCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="KeyPressCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? KeyPressCommandParameter
    {
        get => GetValue(KeyPressCommandParameterProperty);
        set => SetValue(KeyPressCommandParameterProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the content changes.
    /// </summary>
    public event EventHandler<RichTextContentChangedEventArgs>? ContentChanged;

    /// <summary>
    /// Occurs when the selection changes.
    /// </summary>
    public event EventHandler<RichTextSelectionChangedEventArgs>? SelectionChanged;

    /// <summary>
    /// Occurs when a link is tapped.
    /// </summary>
    public event EventHandler<RichTextLinkTappedEventArgs>? LinkTapped;

    /// <summary>
    /// Occurs when an image insert is requested.
    /// </summary>
    public event EventHandler<RichTextImageRequestedEventArgs>? ImageRequested;

    /// <summary>
    /// Occurs when the editor focus changes.
    /// </summary>
    public event EventHandler<RichTextFocusChangedEventArgs>? FocusChanged;

    /// <inheritdoc/>
    public event EventHandler<KeyboardFocusEventArgs>? KeyboardFocusGained;

    /// <inheritdoc/>
    public event EventHandler<KeyboardFocusEventArgs>? KeyboardFocusLost;

    /// <inheritdoc/>
    public event EventHandler<KeyEventArgs>? KeyPressed;

    /// <inheritdoc/>
#pragma warning disable CS0067 // Event is required by IKeyboardNavigable interface
    public event EventHandler<KeyEventArgs>? KeyReleased;
#pragma warning restore CS0067

    #endregion

    #region IKeyboardNavigable Implementation

    /// <inheritdoc/>
    public bool CanReceiveFocus => IsEnabled && IsVisible;

    /// <inheritdoc/>
    public bool IsKeyboardNavigationEnabled
    {
        get => (bool)GetValue(IsKeyboardNavigationEnabledProperty);
        set => SetValue(IsKeyboardNavigationEnabledProperty, value);
    }

    /// <inheritdoc/>
    public bool HasKeyboardFocus => _hasKeyboardFocus;

    /// <inheritdoc/>
    public bool HandleKeyPress(KeyEventArgs e)
    {
        if (!IsKeyboardNavigationEnabled || IsReadOnly) return false;

        // Fire the KeyPressed event
        KeyPressed?.Invoke(this, e);
        if (e.Handled) return true;

        // Execute KeyPressCommand
        if (KeyPressCommand?.CanExecute(e) == true)
        {
            KeyPressCommand.Execute(KeyPressCommandParameter ?? e);
            if (e.Handled) return true;
        }

        // Handle Ctrl+key shortcuts
        if (e.Modifiers.HasFlag(KeyModifiers.Control))
        {
            switch (e.Key)
            {
                case "B":
                    _ = FormatAsync(FormatType.Bold);
                    return true;
                case "I":
                    _ = FormatAsync(FormatType.Italic);
                    return true;
                case "U":
                    _ = FormatAsync(FormatType.Underline);
                    return true;
                case "C":
                    Copy();
                    return true;
                case "X":
                    Cut();
                    return true;
                case "V":
                    Paste();
                    return true;
                case "Z":
                    if (e.Modifiers.HasFlag(KeyModifiers.Shift))
                        Redo();
                    else
                        Undo();
                    return true;
                case "Y":
                    Redo();
                    return true;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public IReadOnlyList<KeyboardShortcut> GetKeyboardShortcuts()
    {
        return new List<KeyboardShortcut>
        {
            new() { Key = "B", Modifiers = "Ctrl", Description = "Bold", Category = "Formatting" },
            new() { Key = "I", Modifiers = "Ctrl", Description = "Italic", Category = "Formatting" },
            new() { Key = "U", Modifiers = "Ctrl", Description = "Underline", Category = "Formatting" },
            new() { Key = "C", Modifiers = "Ctrl", Description = "Copy", Category = "Clipboard" },
            new() { Key = "X", Modifiers = "Ctrl", Description = "Cut", Category = "Clipboard" },
            new() { Key = "V", Modifiers = "Ctrl", Description = "Paste", Category = "Clipboard" },
            new() { Key = "Z", Modifiers = "Ctrl", Description = "Undo", Category = "Editing" },
            new() { Key = "Y", Modifiers = "Ctrl", Description = "Redo", Category = "Editing" },
            new() { Key = "Z", Modifiers = "Ctrl+Shift", Description = "Redo", Category = "Editing" }
        };
    }

    /// <inheritdoc/>
    public new bool Focus()
    {
        if (!CanReceiveFocus) return false;
        _ = FocusEditorAsync();
        return true;
    }

    #endregion

    #region IClipboardSupport Implementation

    /// <inheritdoc/>
    public bool CanCopy => _hasKeyboardFocus;

    /// <inheritdoc/>
    public bool CanCut => _hasKeyboardFocus && !IsReadOnly;

    /// <inheritdoc/>
    public bool CanPaste => !IsReadOnly;

    /// <inheritdoc/>
    public void Copy()
    {
        _ = ExecuteJavaScriptAsync("document.execCommand('copy')");
        CopyCommand?.Execute(CopyCommandParameter);
    }

    /// <inheritdoc/>
    public void Cut()
    {
        if (IsReadOnly) return;
        _ = ExecuteJavaScriptAsync("document.execCommand('cut')");
        CutCommand?.Execute(CutCommandParameter);
    }

    /// <inheritdoc/>
    public void Paste()
    {
        if (IsReadOnly) return;
        _ = ExecuteJavaScriptAsync("document.execCommand('paste')");
        PasteCommand?.Execute(PasteCommandParameter);
    }

    /// <inheritdoc/>
    public object? GetClipboardContent() => null; // Clipboard content is managed by the browser

    #endregion

    #region IUndoRedo Implementation

    /// <inheritdoc/>
    public bool CanUndo => _canUndo;

    /// <inheritdoc/>
    public bool CanRedo => _canRedo;

    /// <inheritdoc/>
    public int UndoCount => 0; // Managed by Quill

    /// <inheritdoc/>
    public int RedoCount => 0; // Managed by Quill

    /// <inheritdoc/>
    public int UndoLimit { get; set; } = 100;

    /// <inheritdoc/>
    public bool Undo()
    {
        if (!CanUndo || IsReadOnly) return false;
        _ = ExecuteJavaScriptAsync("quill.history.undo()");
        UndoCommand?.Execute(UndoCommandParameter);
        return true;
    }

    /// <inheritdoc/>
    public bool Redo()
    {
        if (!CanRedo || IsReadOnly) return false;
        _ = ExecuteJavaScriptAsync("quill.history.redo()");
        RedoCommand?.Execute(RedoCommandParameter);
        return true;
    }

    /// <inheritdoc/>
    public void ClearUndoHistory()
    {
        _ = ExecuteJavaScriptAsync("quill.history.clear()");
    }

    /// <inheritdoc/>
    public string? GetUndoDescription() => null;

    /// <inheritdoc/>
    public string? GetRedoDescription() => null;

    /// <inheritdoc/>
    public void BeginBatchOperation(string? description = null)
    {
        // Not applicable for WebView-based editor
    }

    /// <inheritdoc/>
    public void EndBatchOperation()
    {
        // Not applicable for WebView-based editor
    }

    /// <inheritdoc/>
    public void CancelBatchOperation()
    {
        // Not applicable for WebView-based editor
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="RichTextEditor"/> class.
    /// </summary>
    public RichTextEditor()
    {
        InitializeComponent();

        // Set default toolbar
        if (GetValue(ToolbarItemsProperty) == null)
        {
            SetValue(ToolbarItemsProperty, ToolbarConfig.Standard);
        }

        // Initialize WebView when loaded
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        // Subscribe to app theme changes for auto mode
        if (Application.Current != null)
        {
            Application.Current.RequestedThemeChanged += OnAppThemeChanged;
        }
    }

    #endregion

    #region Initialization

    private async void OnLoaded(object? sender, EventArgs e)
    {
        await InitializeEditorAsync();
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        // Unsubscribe from theme changes to prevent memory leaks
        if (Application.Current != null)
        {
            Application.Current.RequestedThemeChanged -= OnAppThemeChanged;
        }
    }

    private async void OnAppThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        // Only respond if ThemeMode is Auto
        if (ThemeMode != EditorThemeMode.Auto) return;

        var newThemeIsDark = e.RequestedTheme == AppTheme.Dark;
        if (_currentThemeIsDark != newThemeIsDark)
        {
            await UpdateThemeAsync(newThemeIsDark);
        }
    }

    private static void OnThemeModeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is RichTextEditor editor && editor._isInitialized)
        {
            var isDark = editor.GetEffectiveThemeIsDark();
            if (editor._currentThemeIsDark != isDark)
            {
                _ = editor.UpdateThemeAsync(isDark);
            }
        }
    }

    private bool GetEffectiveThemeIsDark()
    {
        return ThemeMode switch
        {
            EditorThemeMode.Light => false,
            EditorThemeMode.Dark => true,
            _ => Application.Current?.RequestedTheme == AppTheme.Dark
        };
    }

    private async Task InitializeEditorAsync()
    {
        if (_isInitialized) return;

        // Set initial theme state
        _currentThemeIsDark = GetEffectiveThemeIsDark();

        var html = GenerateEditorHtml();
        editorWebView.Source = new HtmlWebViewSource { Html = html };

        // Wait for page load
        var tcs = new TaskCompletionSource<bool>();
        void handler(object? s, WebNavigatedEventArgs args)
        {
            editorWebView.Navigated -= handler;
            tcs.SetResult(true);
        }
        editorWebView.Navigated += handler;

        await tcs.Task;

        // Set initial content if pending
        if (!string.IsNullOrEmpty(_pendingContent))
        {
            await SetContentInternalAsync(_pendingContent);
            _pendingContent = null;
        }
        else if (!string.IsNullOrEmpty(HtmlContent))
        {
            await SetContentInternalAsync(HtmlContent);
        }

        _isInitialized = true;
        IsLoading = false;
    }

    private string GenerateEditorHtml()
    {
        var isDark = _currentThemeIsDark;
        var themeClass = isDark ? "dark-theme" : "light-theme";

        var urls = GetQuillUrls();
        var resourcesHtml = GenerateResourceIncludes(urls);

        return $@"
<!DOCTYPE html>
<html class=""{themeClass}"">
<head>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"">
    {resourcesHtml}
    <style>
        /* CSS Variables for theming */
        :root {{
            --editor-bg: #FFFFFF;
            --editor-text: #212121;
            --editor-placeholder: #9E9E9E;
            --editor-border: #CCCCCC;
            --editor-link: #06C;
            --editor-code-bg: #F0F0F0;
            --editor-code-text: #333333;
            --editor-blockquote-border: #CCCCCC;
            --editor-selection-bg: rgba(0, 123, 255, 0.2);
        }}

        html.dark-theme {{
            --editor-bg: #1E1E1E;
            --editor-text: #E0E0E0;
            --editor-placeholder: #808080;
            --editor-border: #424242;
            --editor-link: #6DB3F2;
            --editor-code-bg: #2D2D2D;
            --editor-code-text: #E0E0E0;
            --editor-blockquote-border: #616161;
            --editor-selection-bg: rgba(100, 150, 255, 0.3);
        }}

        * {{ margin: 0; padding: 0; box-sizing: border-box; }}

        body {{
            background: var(--editor-bg);
            color: var(--editor-text);
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            transition: background-color 0.2s ease, color 0.2s ease;
        }}

        /* Quill container */
        .ql-container {{
            border: none !important;
            font-size: {EffectiveFontSize}px;
            color: var(--editor-text);
        }}

        .ql-container.ql-snow {{
            border: none !important;
        }}

        /* Editor area */
        .ql-editor {{
            min-height: {MinHeight - 40}px;
            padding: 12px;
            color: var(--editor-text);
            background: var(--editor-bg);
        }}

        /* Placeholder */
        .ql-editor.ql-blank::before {{
            color: var(--editor-placeholder);
            font-style: italic;
        }}

        /* Links */
        .ql-editor a {{
            color: var(--editor-link);
        }}

        /* Code blocks */
        .ql-editor pre.ql-syntax,
        .ql-editor code,
        .ql-editor pre {{
            background-color: var(--editor-code-bg);
            color: var(--editor-code-text);
            border-radius: 4px;
        }}

        .ql-editor pre.ql-syntax {{
            padding: 12px;
            margin: 8px 0;
            overflow-x: auto;
        }}

        .ql-editor code {{
            padding: 2px 6px;
            font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
        }}

        /* Blockquote */
        .ql-editor blockquote {{
            border-left: 4px solid var(--editor-blockquote-border);
            padding-left: 16px;
            margin: 8px 0;
            color: var(--editor-text);
            opacity: 0.85;
        }}

        /* Selection highlight */
        .ql-editor ::selection {{
            background: var(--editor-selection-bg);
        }}

        /* Hide default Quill toolbar - we use MAUI toolbar */
        .ql-toolbar {{
            display: none !important;
        }}

        /* Scrollbar styling for dark mode */
        html.dark-theme ::-webkit-scrollbar {{
            width: 8px;
            height: 8px;
        }}

        html.dark-theme ::-webkit-scrollbar-track {{
            background: var(--editor-bg);
        }}

        html.dark-theme ::-webkit-scrollbar-thumb {{
            background: #555;
            border-radius: 4px;
        }}

        html.dark-theme ::-webkit-scrollbar-thumb:hover {{
            background: #666;
        }}
    </style>
</head>
<body>
    <div id=""editor""></div>
    <script>
        var quill = new Quill('#editor', {{
            theme: 'snow',
            placeholder: '{EscapeJsString(Placeholder ?? "Start writing...")}',
            readOnly: {(IsReadOnly ? "true" : "false")},
            modules: {{
                history: {{
                    delay: 1000,
                    maxStack: {UndoLimit},
                    userOnly: true
                }}
            }}
        }});

        var turndownService = new TurndownService();

        // Bridge object for C# interop
        window.RichTextBridge = {{
            getContent: function() {{ return quill.root.innerHTML; }},
            getMarkdown: function() {{ return turndownService.turndown(quill.root.innerHTML); }},
            setContent: function(html) {{ quill.clipboard.dangerouslyPasteHTML(html || ''); }},
            setMarkdown: function(md) {{ quill.clipboard.dangerouslyPasteHTML(marked.parse(md || '')); }},
            getText: function() {{ return quill.getText(); }},
            getSelection: function() {{
                var range = quill.getSelection();
                return range ? JSON.stringify({{start: range.index, length: range.length}}) : null;
            }},
            format: function(type) {{
                var range = quill.getSelection();
                if (!range) return;
                switch(type) {{
                    case 'bold': quill.format('bold', !quill.getFormat().bold); break;
                    case 'italic': quill.format('italic', !quill.getFormat().italic); break;
                    case 'underline': quill.format('underline', !quill.getFormat().underline); break;
                    case 'strike': quill.format('strike', !quill.getFormat().strike); break;
                    case 'h1': quill.format('header', quill.getFormat().header === 1 ? false : 1); break;
                    case 'h2': quill.format('header', quill.getFormat().header === 2 ? false : 2); break;
                    case 'h3': quill.format('header', quill.getFormat().header === 3 ? false : 3); break;
                    case 'bullet': quill.format('list', quill.getFormat().list === 'bullet' ? false : 'bullet'); break;
                    case 'ordered': quill.format('list', quill.getFormat().list === 'ordered' ? false : 'ordered'); break;
                    case 'blockquote': quill.format('blockquote', !quill.getFormat().blockquote); break;
                    case 'code-block': quill.format('code-block', !quill.getFormat()['code-block']); break;
                }}
            }},
            insertLink: function(url, text) {{
                var range = quill.getSelection(true);
                if (text) {{
                    quill.insertText(range.index, text, 'link', url);
                }} else {{
                    quill.format('link', url);
                }}
            }},
            insertImage: function(url, alt) {{
                var range = quill.getSelection(true);
                quill.insertEmbed(range.index, 'image', url);
            }},
            clearFormat: function() {{
                var range = quill.getSelection();
                if (range) quill.removeFormat(range.index, range.length);
            }},
            focus: function() {{ quill.focus(); }},
            blur: function() {{ quill.blur(); }},
            setReadOnly: function(readOnly) {{ quill.enable(!readOnly); }},
            canUndo: function() {{ return quill.history.stack.undo.length > 0; }},
            canRedo: function() {{ return quill.history.stack.redo.length > 0; }},
            setTheme: function(isDark) {{
                var html = document.documentElement;
                if (isDark) {{
                    html.classList.remove('light-theme');
                    html.classList.add('dark-theme');
                }} else {{
                    html.classList.remove('dark-theme');
                    html.classList.add('light-theme');
                }}
            }},
            getTheme: function() {{
                return document.documentElement.classList.contains('dark-theme') ? 'dark' : 'light';
            }}
        }};

        // Event handlers
        quill.on('text-change', function(delta, oldDelta, source) {{
            if (source === 'user') {{
                var html = quill.root.innerHTML;
                window.location.href = 'rte://content-changed?html=' + encodeURIComponent(html);
            }}
        }});

        quill.on('selection-change', function(range, oldRange, source) {{
            if (range) {{
                var text = quill.getText(range.index, range.length);
                window.location.href = 'rte://selection-changed?start=' + range.index + '&length=' + range.length + '&text=' + encodeURIComponent(text);
            }}
            // Focus change
            var focused = range !== null;
            window.location.href = 'rte://focus-changed?focused=' + focused;
        }});
    </script>
</body>
</html>";
    }

    private static string EscapeJsString(string? value)
    {
        if (value == null) return "";
        return value.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n").Replace("\r", "\\r");
    }

    /// <summary>
    /// Updates the editor theme dynamically without reinitializing.
    /// </summary>
    /// <param name="isDark">True for dark theme, false for light theme.</param>
    public async Task UpdateThemeAsync(bool isDark)
    {
        if (!_isInitialized) return;

        _currentThemeIsDark = isDark;
        OnPropertyChanged(nameof(IsDarkTheme));

        var jsValue = isDark ? "true" : "false";
        await ExecuteJavaScriptAsync($"RichTextBridge.setTheme({jsValue})");
    }

    /// <summary>
    /// Refreshes the editor theme based on current ThemeMode setting.
    /// </summary>
    public Task RefreshThemeAsync()
    {
        var isDark = GetEffectiveThemeIsDark();
        return UpdateThemeAsync(isDark);
    }

    private QuillJsUrls GetQuillUrls()
    {
        return QuillSource switch
        {
            QuillJsSource.Cdn => QuillJsResources.CdnUrls,
            QuillJsSource.Bundled => GetBundledOrFallbackUrls(),
            QuillJsSource.Custom => new QuillJsUrls
            {
                QuillCss = CustomQuillCssUrl ?? QuillJsResources.CdnUrls.QuillCss,
                QuillJs = CustomQuillJsUrl ?? QuillJsResources.CdnUrls.QuillJs,
                TurndownJs = CustomTurndownJsUrl ?? QuillJsResources.CdnUrls.TurndownJs,
                MarkedJs = CustomMarkedJsUrl ?? QuillJsResources.CdnUrls.MarkedJs
            },
            _ => QuillJsResources.CdnUrls
        };
    }

    private static QuillJsUrls GetBundledOrFallbackUrls()
    {
        // Try local files first, fallback to CDN
        try
        {
            if (QuillJsResources.AreLocalResourcesAvailable())
            {
                return QuillJsResources.GetLocalUrls();
            }

            // Try to extract bundled resources
            QuillJsResources.EnsureResourcesExtracted();
            if (QuillJsResources.AreLocalResourcesAvailable())
            {
                return QuillJsResources.GetLocalUrls();
            }
        }
        catch
        {
            // Extraction failed, fall through to CDN
        }

        // Fallback to CDN if local resources aren't available
        return QuillJsResources.CdnUrls;
    }

    private static string GenerateResourceIncludes(QuillJsUrls urls)
    {
        if (urls.IsInline)
        {
            // Inline CSS/JS (not currently used, but supported)
            return $@"
    <style>{urls.QuillCss}</style>
    <script>{urls.QuillJs}</script>
    <script>{urls.TurndownJs}</script>
    <script>{urls.MarkedJs}</script>";
        }

        // External URLs (file:// or https://)
        return $@"
    <link href=""{urls.QuillCss}"" rel=""stylesheet"">
    <script src=""{urls.QuillJs}""></script>
    <script src=""{urls.TurndownJs}""></script>
    <script src=""{urls.MarkedJs}""></script>";
    }

    #endregion

    #region WebView Event Handlers

    private void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (e.Url.StartsWith("rte://"))
        {
            e.Cancel = true;
            HandleBridgeMessage(e.Url);
        }
        else if (e.Url.StartsWith("http://") || e.Url.StartsWith("https://"))
        {
            // External link clicked
            e.Cancel = true;
            var args = new RichTextLinkTappedEventArgs(e.Url, null);
            LinkTapped?.Invoke(this, args);
            LinkTappedCommand?.Execute(LinkTappedCommandParameter ?? args);

            if (!args.Handled)
            {
                // Default: open in browser
                _ = Launcher.OpenAsync(e.Url);
            }
        }
    }

    private void HandleBridgeMessage(string url)
    {
        var uri = new Uri(url);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

        switch (uri.Host)
        {
            case "content-changed":
                if (!_isUpdatingContent)
                {
                    var html = Uri.UnescapeDataString(query["html"] ?? "");
                    var oldContent = HtmlContent;
                    _isUpdatingContent = true;
                    HtmlContent = html;
                    _isUpdatingContent = false;

                    var args = new RichTextContentChangedEventArgs(oldContent, html, ContentFormat.Html);
                    ContentChanged?.Invoke(this, args);
                    ContentChangedCommand?.Execute(ContentChangedCommandParameter ?? args);

                    // Update undo/redo state
                    _ = UpdateUndoRedoStateAsync();
                }
                break;

            case "selection-changed":
                var start = int.Parse(query["start"] ?? "0");
                var length = int.Parse(query["length"] ?? "0");
                var text = Uri.UnescapeDataString(query["text"] ?? "");
                var selArgs = new RichTextSelectionChangedEventArgs(start, length, text);
                SelectionChanged?.Invoke(this, selArgs);
                SelectionChangedCommand?.Execute(SelectionChangedCommandParameter ?? selArgs);
                break;

            case "focus-changed":
                var focused = query["focused"] == "true";
                if (_hasKeyboardFocus != focused)
                {
                    _hasKeyboardFocus = focused;
                    OnPropertyChanged(nameof(HasKeyboardFocus));
                    OnPropertyChanged(nameof(CanCopy));
                    OnPropertyChanged(nameof(CanCut));

                    // Fire keyboard focus events
                    var focusEventArgs = new KeyboardFocusEventArgs(focused);
                    if (focused)
                    {
                        KeyboardFocusGained?.Invoke(this, focusEventArgs);
                        GotFocusCommand?.Execute(GotFocusCommandParameter ?? this);
                    }
                    else
                    {
                        KeyboardFocusLost?.Invoke(this, focusEventArgs);
                        LostFocusCommand?.Execute(LostFocusCommandParameter ?? this);
                    }

                    var focusArgs = new RichTextFocusChangedEventArgs(focused);
                    FocusChanged?.Invoke(this, focusArgs);
                    FocusChangedCommand?.Execute(FocusChangedCommandParameter ?? focusArgs);
                }
                break;
        }
    }

    private async Task UpdateUndoRedoStateAsync()
    {
        try
        {
            var canUndoResult = await editorWebView.EvaluateJavaScriptAsync("RichTextBridge.canUndo()");
            var canRedoResult = await editorWebView.EvaluateJavaScriptAsync("RichTextBridge.canRedo()");

            _canUndo = canUndoResult == "true";
            _canRedo = canRedoResult == "true";

            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
        }
        catch
        {
            // Ignore JS errors
        }
    }

    #endregion

    #region Toolbar Event Handlers

    private void OnBoldClicked(object? sender, EventArgs e) => _ = FormatAsync(FormatType.Bold);
    private void OnItalicClicked(object? sender, EventArgs e) => _ = FormatAsync(FormatType.Italic);
    private void OnUnderlineClicked(object? sender, EventArgs e) => _ = FormatAsync(FormatType.Underline);
    private void OnStrikethroughClicked(object? sender, EventArgs e) => _ = FormatAsync(FormatType.Strikethrough);
    private void OnBulletListClicked(object? sender, EventArgs e) => _ = FormatAsync(FormatType.BulletList);
    private void OnNumberedListClicked(object? sender, EventArgs e) => _ = FormatAsync(FormatType.NumberedList);
    private void OnH1Clicked(object? sender, EventArgs e) => _ = FormatAsync(FormatType.Heading1);
    private void OnH2Clicked(object? sender, EventArgs e) => _ = FormatAsync(FormatType.Heading2);
    private void OnQuoteClicked(object? sender, EventArgs e) => _ = FormatAsync(FormatType.Quote);
    private void OnCodeBlockClicked(object? sender, EventArgs e) => _ = FormatAsync(FormatType.CodeBlock);
    private void OnUndoClicked(object? sender, EventArgs e) => Undo();
    private void OnRedoClicked(object? sender, EventArgs e) => Redo();
    private void OnClearFormattingClicked(object? sender, EventArgs e) => _ = ClearFormattingAsync();

    private async void OnLinkClicked(object? sender, EventArgs e)
    {
        var page = GetCurrentPage();
        if (page == null) return;

        var url = await page.DisplayPromptAsync(
            "Insert Link",
            "Enter the URL:",
            placeholder: "https://example.com");

        if (!string.IsNullOrEmpty(url))
        {
            await InsertLinkAsync(url, null);
        }
    }

    private async void OnImageClicked(object? sender, EventArgs e)
    {
        var args = new RichTextImageRequestedEventArgs();
        ImageRequested?.Invoke(this, args);

        if (args.Handled && !string.IsNullOrEmpty(args.ImageUrl))
        {
            await InsertImageAsync(args.ImageUrl, args.AltText);
        }
        else
        {
            var page = GetCurrentPage();
            if (page == null) return;

            // Default: prompt for URL
            var url = await page.DisplayPromptAsync(
                "Insert Image",
                "Enter the image URL:",
                placeholder: "https://example.com/image.png");

            if (!string.IsNullOrEmpty(url))
            {
                await InsertImageAsync(url, null);
            }
        }
    }

    private Page? GetCurrentPage()
    {
        return Window?.Page ?? Application.Current?.Windows.FirstOrDefault()?.Page;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Inserts text at the current cursor position.
    /// </summary>
    public Task InsertTextAsync(string text)
    {
        var escaped = JsonSerializer.Serialize(text);
        return ExecuteJavaScriptAsync($"quill.insertText(quill.getSelection(true).index, {escaped})");
    }

    /// <summary>
    /// Inserts HTML at the current cursor position.
    /// </summary>
    public Task InsertHtmlAsync(string html)
    {
        var escaped = JsonSerializer.Serialize(html);
        return ExecuteJavaScriptAsync($"quill.clipboard.dangerouslyPasteHTML(quill.getSelection(true).index, {escaped})");
    }

    /// <summary>
    /// Inserts an image at the current cursor position.
    /// </summary>
    public Task InsertImageAsync(string url, string? altText = null)
    {
        var escapedUrl = JsonSerializer.Serialize(url);
        var escapedAlt = JsonSerializer.Serialize(altText ?? "");
        return ExecuteJavaScriptAsync($"RichTextBridge.insertImage({escapedUrl}, {escapedAlt})");
    }

    /// <summary>
    /// Inserts a link at the current cursor position.
    /// </summary>
    public Task InsertLinkAsync(string url, string? text = null)
    {
        var escapedUrl = JsonSerializer.Serialize(url);
        var escapedText = text != null ? JsonSerializer.Serialize(text) : "null";
        return ExecuteJavaScriptAsync($"RichTextBridge.insertLink({escapedUrl}, {escapedText})");
    }

    /// <summary>
    /// Gets the currently selected text.
    /// </summary>
    public async Task<string?> GetSelectedTextAsync()
    {
        try
        {
            var json = await editorWebView.EvaluateJavaScriptAsync("RichTextBridge.getSelection()");
            if (string.IsNullOrEmpty(json) || json == "null") return null;

            // Parse and get text from selection range
            var text = await editorWebView.EvaluateJavaScriptAsync(
                "quill.getText(quill.getSelection().index, quill.getSelection().length)");
            return text;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Applies formatting to the current selection.
    /// </summary>
    public Task FormatAsync(FormatType format)
    {
        var formatName = format switch
        {
            FormatType.Bold => "bold",
            FormatType.Italic => "italic",
            FormatType.Underline => "underline",
            FormatType.Strikethrough => "strike",
            FormatType.Heading1 => "h1",
            FormatType.Heading2 => "h2",
            FormatType.Heading3 => "h3",
            FormatType.BulletList => "bullet",
            FormatType.NumberedList => "ordered",
            FormatType.Quote => "blockquote",
            FormatType.CodeBlock => "code-block",
            FormatType.Code => "code",
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

        return ExecuteJavaScriptAsync($"RichTextBridge.format('{formatName}')");
    }

    /// <summary>
    /// Clears all formatting from the current selection.
    /// </summary>
    public Task ClearFormattingAsync()
    {
        return ExecuteJavaScriptAsync("RichTextBridge.clearFormat()");
    }

    /// <summary>
    /// Focuses the editor.
    /// </summary>
    public Task FocusEditorAsync()
    {
        return ExecuteJavaScriptAsync("RichTextBridge.focus()");
    }

    /// <summary>
    /// Removes focus from the editor.
    /// </summary>
    public Task BlurAsync()
    {
        return ExecuteJavaScriptAsync("RichTextBridge.blur()");
    }

    /// <summary>
    /// Gets the plain text content of the editor.
    /// </summary>
    public async Task<string?> GetTextAsync()
    {
        try
        {
            return await editorWebView.EvaluateJavaScriptAsync("RichTextBridge.getText()");
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the content as HTML.
    /// </summary>
    public async Task<string?> GetHtmlAsync()
    {
        try
        {
            return await editorWebView.EvaluateJavaScriptAsync("RichTextBridge.getContent()");
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the content as Markdown.
    /// </summary>
    public async Task<string?> GetMarkdownAsync()
    {
        try
        {
            return await editorWebView.EvaluateJavaScriptAsync("RichTextBridge.getMarkdown()");
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region Property Changed Handlers

    private static void OnHtmlContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is RichTextEditor editor && !editor._isUpdatingContent)
        {
            _ = editor.SetContentInternalAsync((string?)newValue);
        }
    }

    private static void OnMarkdownContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is RichTextEditor editor && !editor._isUpdatingContent)
        {
            _ = editor.SetMarkdownInternalAsync((string?)newValue);
        }
    }

    private static void OnPlaceholderChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is RichTextEditor editor && editor._isInitialized)
        {
            var escaped = JsonSerializer.Serialize(newValue?.ToString() ?? "");
            _ = editor.ExecuteJavaScriptAsync($"quill.root.dataset.placeholder = {escaped}");
        }
    }

    private static void OnIsReadOnlyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is RichTextEditor editor)
        {
            editor.OnPropertyChanged(nameof(ShowTopToolbar));
            editor.OnPropertyChanged(nameof(ShowBottomToolbar));
            editor.OnPropertyChanged(nameof(CanCut));
            editor.OnPropertyChanged(nameof(CanPaste));

            if (editor._isInitialized)
            {
                var readOnly = (bool)newValue ? "true" : "false";
                _ = editor.ExecuteJavaScriptAsync($"RichTextBridge.setReadOnly({readOnly})");
            }
        }
    }

    private static void OnToolbarItemsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        // Toolbar visibility is bound in XAML
    }

    private static void OnToolbarPositionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is RichTextEditor editor)
        {
            editor.OnPropertyChanged(nameof(ShowTopToolbar));
            editor.OnPropertyChanged(nameof(ShowBottomToolbar));
        }
    }

    private static void OnEditorBackgroundChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is RichTextEditor editor)
        {
            editor.OnPropertyChanged(nameof(EffectiveEditorBackground));
        }
    }

    #endregion

    #region Private Methods

    private async Task SetContentInternalAsync(string? html)
    {
        if (!_isInitialized)
        {
            _pendingContent = html;
            return;
        }

        var escaped = JsonSerializer.Serialize(html ?? "");
        await ExecuteJavaScriptAsync($"RichTextBridge.setContent({escaped})");
    }

    private async Task SetMarkdownInternalAsync(string? markdown)
    {
        if (!_isInitialized)
        {
            // Convert markdown to HTML and store
            _pendingContent = markdown; // Will be converted by Quill
            return;
        }

        var escaped = JsonSerializer.Serialize(markdown ?? "");
        await ExecuteJavaScriptAsync($"RichTextBridge.setMarkdown({escaped})");
    }

    private async Task ExecuteJavaScriptAsync(string script)
    {
        if (!_isInitialized) return;

        try
        {
            await editorWebView.EvaluateJavaScriptAsync(script);
        }
        catch
        {
            // Ignore JavaScript errors
        }
    }

    #endregion
}
