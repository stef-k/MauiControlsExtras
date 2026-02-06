using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiControlsExtras.Base;
using MauiControlsExtras.Base.Validation;
using MauiControlsExtras.Converters;
using MauiControlsExtras.ContextMenu;
using MauiControlsExtras.Helpers;
using Microsoft.Maui.Controls.Shapes;

namespace MauiControlsExtras.Controls;

/// <summary>
/// A feature-rich ComboBox control for .NET MAUI with filtering, autocomplete, and image support.
/// </summary>
/// <remarks>
/// <para>
/// The ComboBox provides a dropdown selection experience similar to WinForms ComboBox,
/// with additional features for mobile and cross-platform scenarios:
/// </para>
/// <list type="bullet">
///   <item><description>Searchable/filterable dropdown list with debounced input</description></item>
///   <item><description>Support for complex objects via DisplayMemberPath and ValueMemberPath</description></item>
///   <item><description>Image/icon support via IconMemberPath</description></item>
///   <item><description>Two-way binding for SelectedItem and SelectedValue</description></item>
///   <item><description>Theme-aware styling (light/dark mode)</description></item>
///   <item><description>Customizable placeholder text</description></item>
///   <item><description>Clear selection button</description></item>
///   <item><description>Default value support</description></item>
///   <item><description>MVVM command support (SelectionChangedCommand, OpenedCommand, etc.)</description></item>
///   <item><description>Built-in validation with IsRequired support</description></item>
/// </list>
/// <example>
/// Basic usage:
/// <code>
/// &lt;extras:ComboBox ItemsSource="{Binding Countries}"
///                  SelectedItem="{Binding SelectedCountry, Mode=TwoWay}"
///                  DisplayMemberPath="Name"
///                  Placeholder="Select a country..." /&gt;
/// </code>
/// </example>
/// <example>
/// With icons:
/// <code>
/// &lt;extras:ComboBox ItemsSource="{Binding Icons}"
///                  SelectedItem="{Binding SelectedIcon, Mode=TwoWay}"
///                  DisplayMemberPath="DisplayName"
///                  IconMemberPath="ImagePath"
///                  Placeholder="Select an icon..." /&gt;
/// </code>
/// </example>
/// <example>
/// With MVVM commands:
/// <code>
/// &lt;extras:ComboBox ItemsSource="{Binding Countries}"
///                  SelectedItem="{Binding SelectedCountry, Mode=TwoWay}"
///                  SelectionChangedCommand="{Binding CountrySelectedCommand}"
///                  IsRequired="True"
///                  ValidateCommand="{Binding ValidateCommand}" /&gt;
/// </code>
/// </example>
/// </remarks>
public partial class ComboBox : TextStyledControlBase, IValidatable, Base.IKeyboardNavigable, Base.IClipboardSupport, Base.IContextMenuSupport
{
    private bool _isExpanded;
    private bool _isUpdatingFromSelection;
    private CancellationTokenSource? _debounceTokenSource;
    private List<string> _validationErrors = new();
    private int _highlightedIndex = -1;
    private bool _isKeyboardNavigationEnabled = true;
    private static readonly List<Base.KeyboardShortcut> _keyboardShortcuts = new();
    private readonly ContextMenuItemCollection _contextMenuItems = new();
    private CancellationTokenSource? _longPressCts;

    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="ItemsSource"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource),
        typeof(IEnumerable),
        typeof(ComboBox),
        default(IEnumerable),
        propertyChanged: OnItemsSourceChanged);

    /// <summary>
    /// Identifies the <see cref="SelectedItem"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem),
        typeof(object),
        typeof(ComboBox),
        default(object),
        BindingMode.TwoWay,
        propertyChanged: OnSelectedItemChanged);

    /// <summary>
    /// Identifies the <see cref="DisplayMemberPath"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DisplayMemberPathProperty = BindableProperty.Create(
        nameof(DisplayMemberPath),
        typeof(string),
        typeof(ComboBox),
        default(string),
        propertyChanged: OnDisplayMemberPathChanged);

    /// <summary>
    /// Identifies the <see cref="IconMemberPath"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IconMemberPathProperty = BindableProperty.Create(
        nameof(IconMemberPath),
        typeof(string),
        typeof(ComboBox),
        default(string),
        propertyChanged: OnIconMemberPathChanged);

    /// <summary>
    /// Identifies the <see cref="ValueMemberPath"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ValueMemberPathProperty = BindableProperty.Create(
        nameof(ValueMemberPath),
        typeof(string),
        typeof(ComboBox),
        default(string));

    /// <summary>
    /// Identifies the <see cref="SelectedValue"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectedValueProperty = BindableProperty.Create(
        nameof(SelectedValue),
        typeof(object),
        typeof(ComboBox),
        default(object),
        BindingMode.TwoWay,
        propertyChanged: OnSelectedValueChanged);

    /// <summary>
    /// Identifies the <see cref="Placeholder"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder),
        typeof(string),
        typeof(ComboBox),
        "Select an item");

    /// <summary>
    /// Identifies the <see cref="DefaultValue"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DefaultValueProperty = BindableProperty.Create(
        nameof(DefaultValue),
        typeof(object),
        typeof(ComboBox),
        default(object),
        propertyChanged: OnDefaultValueChanged);

    /// <summary>
    /// Identifies the <see cref="VisibleItemCount"/> bindable property.
    /// </summary>
    public static readonly BindableProperty VisibleItemCountProperty = BindableProperty.Create(
        nameof(VisibleItemCount),
        typeof(int),
        typeof(ComboBox),
        5);

    /// <summary>
    /// Identifies the <see cref="DisplayText"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DisplayTextProperty = BindableProperty.Create(
        nameof(DisplayText),
        typeof(string),
        typeof(ComboBox),
        string.Empty);

    /// <summary>
    /// Identifies the <see cref="DisplayTextColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DisplayTextColorProperty = BindableProperty.Create(
        nameof(DisplayTextColor),
        typeof(Color),
        typeof(ComboBox),
        Colors.Gray);

    /// <summary>
    /// Identifies the <see cref="HasSelection"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HasSelectionProperty = BindableProperty.Create(
        nameof(HasSelection),
        typeof(bool),
        typeof(ComboBox),
        false);

    /// <summary>
    /// Identifies the <see cref="FilteredItems"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FilteredItemsProperty = BindableProperty.Create(
        nameof(FilteredItems),
        typeof(ObservableCollection<object>),
        typeof(ComboBox),
        default(ObservableCollection<object>));

    /// <summary>
    /// Identifies the <see cref="ListMaxHeight"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ListMaxHeightProperty = BindableProperty.Create(
        nameof(ListMaxHeight),
        typeof(double),
        typeof(ComboBox),
        200.0);

    /// <summary>
    /// Identifies the <see cref="ItemTemplate"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
        nameof(ItemTemplate),
        typeof(DataTemplate),
        typeof(ComboBox),
        null,
        propertyChanged: OnItemTemplateChanged);

    /// <summary>
    /// Identifies the <see cref="SelectedIconSource"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectedIconSourceProperty = BindableProperty.Create(
        nameof(SelectedIconSource),
        typeof(string),
        typeof(ComboBox),
        default(string));

    /// <summary>
    /// Identifies the <see cref="HasSelectedIcon"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HasSelectedIconProperty = BindableProperty.Create(
        nameof(HasSelectedIcon),
        typeof(bool),
        typeof(ComboBox),
        false);

    /// <summary>
    /// Identifies the <see cref="PopupMode"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PopupModeProperty = BindableProperty.Create(
        nameof(PopupMode),
        typeof(bool),
        typeof(ComboBox),
        false);

    /// <summary>
    /// Identifies the <see cref="IsSearchVisible"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsSearchVisibleProperty = BindableProperty.Create(
        nameof(IsSearchVisible),
        typeof(bool),
        typeof(ComboBox),
        true);

    #endregion

    #region Command Bindable Properties

    /// <summary>
    /// Identifies the <see cref="SelectionChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectionChangedCommandProperty = BindableProperty.Create(
        nameof(SelectionChangedCommand),
        typeof(ICommand),
        typeof(ComboBox),
        default(ICommand));

    /// <summary>
    /// Identifies the <see cref="SelectionChangedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectionChangedCommandParameterProperty = BindableProperty.Create(
        nameof(SelectionChangedCommandParameter),
        typeof(object),
        typeof(ComboBox),
        default(object));

    /// <summary>
    /// Identifies the <see cref="OpenedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty OpenedCommandProperty = BindableProperty.Create(
        nameof(OpenedCommand),
        typeof(ICommand),
        typeof(ComboBox),
        default(ICommand));

    /// <summary>
    /// Identifies the <see cref="ClosedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ClosedCommandProperty = BindableProperty.Create(
        nameof(ClosedCommand),
        typeof(ICommand),
        typeof(ComboBox),
        default(ICommand));

    /// <summary>
    /// Identifies the <see cref="ClearCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ClearCommandProperty = BindableProperty.Create(
        nameof(ClearCommand),
        typeof(ICommand),
        typeof(ComboBox),
        default(ICommand));

    /// <summary>
    /// Identifies the <see cref="CopyCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CopyCommandProperty = BindableProperty.Create(
        nameof(CopyCommand),
        typeof(ICommand),
        typeof(ComboBox),
        default(ICommand));

    /// <summary>
    /// Identifies the <see cref="CutCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CutCommandProperty = BindableProperty.Create(
        nameof(CutCommand),
        typeof(ICommand),
        typeof(ComboBox),
        default(ICommand));

    /// <summary>
    /// Identifies the <see cref="PasteCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PasteCommandProperty = BindableProperty.Create(
        nameof(PasteCommand),
        typeof(ICommand),
        typeof(ComboBox),
        default(ICommand));

    /// <summary>
    /// Identifies the <see cref="ShowDefaultContextMenu"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowDefaultContextMenuProperty = BindableProperty.Create(
        nameof(ShowDefaultContextMenu),
        typeof(bool),
        typeof(ComboBox),
        true);

    /// <summary>
    /// Identifies the <see cref="ContextMenuOpeningCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ContextMenuOpeningCommandProperty = BindableProperty.Create(
        nameof(ContextMenuOpeningCommand),
        typeof(ICommand),
        typeof(ComboBox));

    #endregion

    #region Validation Bindable Properties

    /// <summary>
    /// Identifies the <see cref="IsRequired"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(
        nameof(IsRequired),
        typeof(bool),
        typeof(ComboBox),
        false);

    /// <summary>
    /// Identifies the <see cref="RequiredErrorMessage"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RequiredErrorMessageProperty = BindableProperty.Create(
        nameof(RequiredErrorMessage),
        typeof(string),
        typeof(ComboBox),
        "Selection is required");

    /// <summary>
    /// Identifies the <see cref="IsValid"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsValidProperty = BindableProperty.Create(
        nameof(IsValid),
        typeof(bool),
        typeof(ComboBox),
        true);

    /// <summary>
    /// Identifies the <see cref="ValidateCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ValidateCommandProperty = BindableProperty.Create(
        nameof(ValidateCommand),
        typeof(ICommand),
        typeof(ComboBox),
        default(ICommand));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the collection of items to display in the dropdown.
    /// </summary>
    /// <value>An IEnumerable containing the items to display.</value>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the currently selected item.
    /// </summary>
    /// <value>The selected item, or null if no item is selected.</value>
    /// <remarks>This property supports two-way binding.</remarks>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Gets or sets the property path to use for display text when items are complex objects.
    /// </summary>
    /// <value>The property name to display, or null to use ToString().</value>
    /// <example>
    /// For a list of Country objects with a Name property:
    /// <code>DisplayMemberPath="Name"</code>
    /// </example>
    public string? DisplayMemberPath
    {
        get => (string?)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    /// <summary>
    /// Gets or sets the property path to use for item icons/images.
    /// </summary>
    /// <value>The property name containing the image path, or null for no icons.</value>
    /// <remarks>
    /// The property should return a path to an image in Resources/Raw.
    /// Images are loaded asynchronously and cached for performance.
    /// </remarks>
    /// <example>
    /// For items with an IconPath property:
    /// <code>IconMemberPath="IconPath"</code>
    /// </example>
    public string? IconMemberPath
    {
        get => (string?)GetValue(IconMemberPathProperty);
        set => SetValue(IconMemberPathProperty, value);
    }

    /// <summary>
    /// Gets or sets the property path to use for the selected value.
    /// </summary>
    /// <value>The property name for the value, or null to use the item itself.</value>
    /// <remarks>
    /// When set, <see cref="SelectedValue"/> will contain the value of this property
    /// from the selected item, rather than the item itself.
    /// </remarks>
    public string? ValueMemberPath
    {
        get => (string?)GetValue(ValueMemberPathProperty);
        set => SetValue(ValueMemberPathProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected value based on <see cref="ValueMemberPath"/>.
    /// </summary>
    /// <value>The selected value, or null if no item is selected.</value>
    /// <remarks>This property supports two-way binding.</remarks>
    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    /// <summary>
    /// Gets or sets the placeholder text shown when no item is selected.
    /// </summary>
    /// <value>The placeholder text. Default is "Select an item".</value>
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>
    /// Gets or sets the default value to select when items are loaded.
    /// </summary>
    /// <value>The default value to match against ValueMemberPath, DisplayMemberPath, or the item itself.</value>
    public object? DefaultValue
    {
        get => GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of items visible in the dropdown without scrolling.
    /// </summary>
    /// <value>The number of visible items. Default is 5.</value>
    public int VisibleItemCount
    {
        get => (int)GetValue(VisibleItemCountProperty);
        set => SetValue(VisibleItemCountProperty, value);
    }

    /// <summary>
    /// Gets the display text shown in collapsed state.
    /// </summary>
    public string DisplayText
    {
        get => (string)GetValue(DisplayTextProperty);
        private set => SetValue(DisplayTextProperty, value);
    }

    /// <summary>
    /// Gets the display text color.
    /// </summary>
    public Color DisplayTextColor
    {
        get => (Color)GetValue(DisplayTextColorProperty);
        private set => SetValue(DisplayTextColorProperty, value);
    }

    /// <summary>
    /// Gets whether an item is currently selected.
    /// </summary>
    public bool HasSelection
    {
        get => (bool)GetValue(HasSelectionProperty);
        private set => SetValue(HasSelectionProperty, value);
    }

    /// <summary>
    /// Gets the filtered items collection for the dropdown.
    /// </summary>
    public ObservableCollection<object> FilteredItems
    {
        get => (ObservableCollection<object>)GetValue(FilteredItemsProperty);
        private set => SetValue(FilteredItemsProperty, value);
    }

    /// <summary>
    /// Gets the maximum height for the dropdown list.
    /// </summary>
    public double ListMaxHeight
    {
        get => (double)GetValue(ListMaxHeightProperty);
        private set => SetValue(ListMaxHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets a custom template for dropdown items.
    /// When set, overrides the default template created from DisplayMemberPath.
    /// </summary>
    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// Gets the selected item's icon source path.
    /// </summary>
    public string? SelectedIconSource
    {
        get => (string?)GetValue(SelectedIconSourceProperty);
        private set => SetValue(SelectedIconSourceProperty, value);
    }

    /// <summary>
    /// Gets whether the selected item has an icon to display.
    /// </summary>
    public bool HasSelectedIcon
    {
        get => (bool)GetValue(HasSelectedIconProperty);
        private set => SetValue(HasSelectedIconProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the ComboBox uses popup mode.
    /// When true, the dropdown raises PopupRequested instead of showing inline.
    /// Used for constrained containers like DataGrid cells.
    /// </summary>
    public bool PopupMode
    {
        get => (bool)GetValue(PopupModeProperty);
        set => SetValue(PopupModeProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the search input is visible in the dropdown.
    /// Set to false to hide the search UI for small item lists.
    /// </summary>
    public bool IsSearchVisible
    {
        get => (bool)GetValue(IsSearchVisibleProperty);
        set => SetValue(IsSearchVisibleProperty, value);
    }

    /// <summary>
    /// Gets whether the dropdown is currently expanded.
    /// </summary>
    public bool IsExpanded => _isExpanded;

    #endregion

    #region Command Properties

    /// <summary>
    /// Gets or sets the command to execute when the selection changes.
    /// The command parameter is the newly selected item (or <see cref="SelectionChangedCommandParameter"/> if set).
    /// </summary>
    public ICommand? SelectionChangedCommand
    {
        get => (ICommand?)GetValue(SelectionChangedCommandProperty);
        set => SetValue(SelectionChangedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="SelectionChangedCommand"/>.
    /// If not set, the selected item is used as the parameter.
    /// </summary>
    public object? SelectionChangedCommandParameter
    {
        get => GetValue(SelectionChangedCommandParameterProperty);
        set => SetValue(SelectionChangedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the dropdown is opened.
    /// </summary>
    public ICommand? OpenedCommand
    {
        get => (ICommand?)GetValue(OpenedCommandProperty);
        set => SetValue(OpenedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the dropdown is closed.
    /// </summary>
    public ICommand? ClosedCommand
    {
        get => (ICommand?)GetValue(ClosedCommandProperty);
        set => SetValue(ClosedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the selection is cleared.
    /// </summary>
    public ICommand? ClearCommand
    {
        get => (ICommand?)GetValue(ClearCommandProperty);
        set => SetValue(ClearCommandProperty, value);
    }

    /// <inheritdoc />
    public ICommand? CopyCommand
    {
        get => (ICommand?)GetValue(CopyCommandProperty);
        set => SetValue(CopyCommandProperty, value);
    }

    /// <inheritdoc />
    public ICommand? CutCommand
    {
        get => (ICommand?)GetValue(CutCommandProperty);
        set => SetValue(CutCommandProperty, value);
    }

    /// <inheritdoc />
    public ICommand? PasteCommand
    {
        get => (ICommand?)GetValue(PasteCommandProperty);
        set => SetValue(PasteCommandProperty, value);
    }

    #endregion

    #region IClipboardSupport

    /// <inheritdoc />
    public bool CanCopy => IsEnabled && searchEntry?.Text?.Length > 0;

    /// <inheritdoc />
    public bool CanCut => CanCopy;

    /// <inheritdoc />
    public bool CanPaste => IsEnabled;

    /// <inheritdoc />
    public void Copy()
    {
        if (!CanCopy) return;
        var content = GetClipboardContent();
        if (content is string text)
            Clipboard.Default.SetTextAsync(text).ConfigureAwait(false);
        CopyCommand?.Execute(content);
    }

    /// <inheritdoc />
    public void Cut()
    {
        if (!CanCut) return;
        var content = GetClipboardContent();
        if (content is string text)
            Clipboard.Default.SetTextAsync(text).ConfigureAwait(false);
        searchEntry.Text = string.Empty;
        CutCommand?.Execute(content);
    }

    /// <inheritdoc />
    public void Paste()
    {
        if (!CanPaste) return;
        var task = Clipboard.Default.GetTextAsync();
        task.ContinueWith(t =>
        {
            if (t.Result is string text)
                MainThread.BeginInvokeOnMainThread(() => searchEntry.Text = text);
        }, TaskScheduler.Default);
        PasteCommand?.Execute(null);
    }

    /// <inheritdoc />
    public object? GetClipboardContent() => searchEntry?.Text;

    #endregion

    #region IContextMenuSupport

    /// <inheritdoc />
    public ContextMenuItemCollection ContextMenuItems => _contextMenuItems;

    /// <summary>
    /// Gets or sets whether to show default context menu items (Copy, Cut, Paste, Select All, Clear).
    /// </summary>
    public bool ShowDefaultContextMenu
    {
        get => (bool)GetValue(ShowDefaultContextMenuProperty);
        set => SetValue(ShowDefaultContextMenuProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the context menu is opening.
    /// </summary>
    public ICommand? ContextMenuOpeningCommand
    {
        get => (ICommand?)GetValue(ContextMenuOpeningCommandProperty);
        set => SetValue(ContextMenuOpeningCommandProperty, value);
    }

    /// <inheritdoc />
    public event EventHandler<ContextMenuOpeningEventArgs>? ContextMenuOpening;

    /// <inheritdoc />
    public void ShowContextMenu(Point? position = null)
    {
        _ = ShowContextMenuAsync(position);
    }

    /// <summary>
    /// Asynchronously shows the context menu.
    /// </summary>
    /// <param name="position">The position to show the menu at.</param>
    public async Task ShowContextMenuAsync(Point? position = null)
    {
        var menuItems = new ContextMenuItemCollection();

        // Add custom items first
        foreach (var item in _contextMenuItems)
        {
            menuItems.Add(item);
        }

        // Add separator if we have custom items and will add default items
        if (_contextMenuItems.Count > 0 && ShowDefaultContextMenu)
        {
            menuItems.AddSeparator();
        }

        // Add default clipboard and editing items
        if (ShowDefaultContextMenu)
        {
            var copyItem = ContextMenuItem.Create(
                "Copy",
                Copy,
                "\uE8C8",
                GetPlatformShortcutText("C"));
            copyItem.IsEnabled = CanCopy;
            menuItems.Add(copyItem);

            var cutItem = ContextMenuItem.Create(
                "Cut",
                Cut,
                "\uE8C6",
                GetPlatformShortcutText("X"));
            cutItem.IsEnabled = CanCut;
            menuItems.Add(cutItem);

            var pasteItem = ContextMenuItem.Create(
                "Paste",
                Paste,
                "\uE77F",
                GetPlatformShortcutText("V"));
            pasteItem.IsEnabled = CanPaste;
            menuItems.Add(pasteItem);

            menuItems.AddSeparator();

            var selectAllItem = ContextMenuItem.Create(
                "Select All",
                SelectAllText,
                "\uE8B3",
                GetPlatformShortcutText("A"));
            selectAllItem.IsEnabled = searchEntry?.Text?.Length > 0;
            menuItems.Add(selectAllItem);

            menuItems.AddSeparator();

            var clearItem = ContextMenuItem.Create(
                "Clear",
                ClearSelection,
                "\uE74D");
            clearItem.IsEnabled = HasSelection;
            menuItems.Add(clearItem);
        }

        if (menuItems.Count == 0) return;

        // Raise opening event
        var eventArgs = new ContextMenuOpeningEventArgs(
            menuItems,
            position ?? Point.Zero,
            this);

        ContextMenuOpening?.Invoke(this, eventArgs);

        if (ContextMenuOpeningCommand?.CanExecute(eventArgs) == true)
        {
            ContextMenuOpeningCommand.Execute(eventArgs);
        }

        if (eventArgs.Cancel || eventArgs.Handled) return;

        await ContextMenuService.Current.ShowAsync(this, menuItems.ToList(), position);
    }

    private void SelectAllText()
    {
        if (searchEntry?.Text?.Length > 0)
        {
            searchEntry.CursorPosition = 0;
            searchEntry.SelectionLength = searchEntry.Text.Length;
        }
    }

    private static string GetPlatformShortcutText(string key)
    {
#if MACCATALYST || IOS
        return $"⌘{key}";
#else
        return $"Ctrl+{key}";
#endif
    }

    #endregion

    #region Validation Properties

    /// <summary>
    /// Gets or sets whether a selection is required for validation to pass.
    /// </summary>
    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    /// <summary>
    /// Gets or sets the error message shown when validation fails due to no selection.
    /// </summary>
    public string RequiredErrorMessage
    {
        get => (string)GetValue(RequiredErrorMessageProperty);
        set => SetValue(RequiredErrorMessageProperty, value);
    }

    /// <summary>
    /// Gets whether the current selection is valid according to validation rules.
    /// </summary>
    public bool IsValid
    {
        get => (bool)GetValue(IsValidProperty);
        private set => SetValue(IsValidProperty, value);
    }

    /// <summary>
    /// Gets the list of current validation errors.
    /// </summary>
    public IReadOnlyList<string> ValidationErrors => _validationErrors.AsReadOnly();

    /// <summary>
    /// Gets or sets the command to execute when validation is triggered.
    /// The command parameter is the <see cref="ValidationResult"/>.
    /// </summary>
    public ICommand? ValidateCommand
    {
        get => (ICommand?)GetValue(ValidateCommandProperty);
        set => SetValue(ValidateCommandProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the selected item changes.
    /// </summary>
    public event EventHandler<object?>? SelectionChanged;

    /// <summary>
    /// Occurs when the dropdown is opened.
    /// </summary>
    public event EventHandler? Opened;

    /// <summary>
    /// Occurs when the dropdown is closed.
    /// </summary>
    public event EventHandler? Closed;

    /// <summary>
    /// Occurs when PopupMode is true and the dropdown should be shown.
    /// The parent container should handle this event to show a popup overlay.
    /// </summary>
    public event EventHandler<ComboBoxPopupRequestEventArgs>? PopupRequested;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ComboBox"/> class.
    /// </summary>
    public ComboBox()
    {
        InitializeComponent();
        FilteredItems = new ObservableCollection<object>();
        SetupItemTemplate();
        UpdateDisplayState();
        UpdateListMaxHeight();

        // Wire up keyboard events from search entry
        searchEntry.Completed += OnSearchEntryCompleted;
        searchEntry.HandlerChanged += OnSearchEntryHandlerChanged;

        // Wire up keyboard events from hidden keyboard capture entry (for when search is hidden)
        keyboardCaptureEntry.HandlerChanged += OnKeyboardCaptureEntryHandlerChanged;

        SetupContextMenuGestures();
    }

    private void OnSearchEntryHandlerChanged(object? sender, EventArgs e)
    {
        if (searchEntry.Handler?.PlatformView == null) return;

#if WINDOWS
        if (searchEntry.Handler.PlatformView is Microsoft.UI.Xaml.Controls.TextBox textBox)
        {
            textBox.KeyDown += OnWindowsTextBoxKeyDown;
            textBox.PreviewKeyDown += OnWindowsTextBoxPreviewKeyDown;
        }
#endif

        MobileClipboardBridge.Setup(searchEntry, this);
    }

    private void OnKeyboardCaptureEntryHandlerChanged(object? sender, EventArgs e)
    {
        if (keyboardCaptureEntry.Handler?.PlatformView == null) return;

#if WINDOWS
        if (keyboardCaptureEntry.Handler.PlatformView is Microsoft.UI.Xaml.Controls.TextBox textBox)
        {
            textBox.KeyDown += OnWindowsKeyboardCaptureKeyDown;
            textBox.PreviewKeyDown += OnWindowsKeyboardCapturePreviewKeyDown;
        }
#endif
    }

#if WINDOWS
    private void OnWindowsTextBoxPreviewKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        // Handle Tab key for autocomplete
        if (e.Key == Windows.System.VirtualKey.Tab && _isExpanded)
        {
            // If single filtered result or highlighted item, select it
            if (FilteredItems.Count == 1)
            {
                SelectItem(FilteredItems[0]);
                e.Handled = true;
            }
            else if (_highlightedIndex >= 0 && _highlightedIndex < FilteredItems.Count)
            {
                SelectItem(FilteredItems[_highlightedIndex]);
                e.Handled = true;
            }
        }
    }

    private void OnWindowsTextBoxKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        // Handle Alt+Down/Alt+Up regardless of expanded state (standard Windows accessibility pattern)
        if (HandleAltKeyCombo(e)) return;

        if (!_isExpanded) return;
        HandleWindowsKeyDown(e);
    }

    private void OnWindowsKeyboardCapturePreviewKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        // Handle Tab key for autocomplete when search is hidden
        if (e.Key == Windows.System.VirtualKey.Tab && _isExpanded)
        {
            if (FilteredItems.Count == 1)
            {
                SelectItem(FilteredItems[0]);
                e.Handled = true;
            }
            else if (_highlightedIndex >= 0 && _highlightedIndex < FilteredItems.Count)
            {
                SelectItem(FilteredItems[_highlightedIndex]);
                e.Handled = true;
            }
        }
    }

    private void OnWindowsKeyboardCaptureKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        // Handle Alt+Down/Alt+Up regardless of expanded state (standard Windows accessibility pattern)
        if (HandleAltKeyCombo(e)) return;

        if (!_isExpanded) return;
        HandleWindowsKeyDown(e);
    }

    /// <summary>
    /// Handles Alt+Down (open dropdown) and Alt+Up (close dropdown) keyboard shortcuts.
    /// This is a standard Windows accessibility pattern for combo boxes.
    /// </summary>
    private bool HandleAltKeyCombo(Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        var isAltDown = Microsoft.UI.Input.InputKeyboardSource
            .GetKeyStateForCurrentThread(Windows.System.VirtualKey.Menu)
            .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

        if (!isAltDown) return false;

        if (e.Key == Windows.System.VirtualKey.Down)
        {
            if (!_isExpanded)
            {
                Open();
                e.Handled = true;
                return true;
            }
        }
        else if (e.Key == Windows.System.VirtualKey.Up)
        {
            if (_isExpanded)
            {
                Close();
                e.Handled = true;
                return true;
            }
        }

        return false;
    }

    private void HandleWindowsKeyDown(Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case Windows.System.VirtualKey.Down:
                if (FilteredItems.Count > 0)
                {
                    _highlightedIndex = (_highlightedIndex + 1) % FilteredItems.Count;
                    UpdateHighlightVisual();
                    e.Handled = true;
                }
                break;

            case Windows.System.VirtualKey.Up:
                if (FilteredItems.Count > 0)
                {
                    _highlightedIndex = _highlightedIndex <= 0 ? FilteredItems.Count - 1 : _highlightedIndex - 1;
                    UpdateHighlightVisual();
                    e.Handled = true;
                }
                break;

            case Windows.System.VirtualKey.Escape:
                Close();
                e.Handled = true;
                break;

            case Windows.System.VirtualKey.Enter:
                // Select highlighted item
                if (_highlightedIndex >= 0 && _highlightedIndex < FilteredItems.Count)
                {
                    SelectItem(FilteredItems[_highlightedIndex]);
                    e.Handled = true;
                }
                else if (FilteredItems.Count == 1)
                {
                    SelectItem(FilteredItems[0]);
                    e.Handled = true;
                }
                break;

            case Windows.System.VirtualKey.Home:
                if (FilteredItems.Count > 0)
                {
                    _highlightedIndex = 0;
                    UpdateHighlightVisual();
                    e.Handled = true;
                }
                break;

            case Windows.System.VirtualKey.End:
                if (FilteredItems.Count > 0)
                {
                    _highlightedIndex = FilteredItems.Count - 1;
                    UpdateHighlightVisual();
                    e.Handled = true;
                }
                break;
        }
    }
#endif

    private void OnSearchEntryCompleted(object? sender, EventArgs e)
    {
        // Enter pressed in search entry - select highlighted item or single filtered item
        if (FilteredItems.Count == 1)
        {
            SelectItem(FilteredItems[0]);
        }
        else if (_highlightedIndex >= 0 && _highlightedIndex < FilteredItems.Count)
        {
            SelectItem(FilteredItems[_highlightedIndex]);
        }
    }

    #endregion

    #region Property Changed Handlers

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ComboBox comboBox)
        {
            comboBox.UpdateFilteredItems(string.Empty);
            comboBox.ApplyDefaultValue();
        }
    }

    private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ComboBox comboBox)
        {
            comboBox.OnSelectedItemChangedInternal(newValue);
        }
    }

    private static void OnSelectedValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ComboBox comboBox && !comboBox._isUpdatingFromSelection)
        {
            comboBox.SelectItemByValue(newValue);
        }
    }

    private static void OnDefaultValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ComboBox comboBox)
        {
            comboBox.ApplyDefaultValue();
        }
    }

    private static void OnDisplayMemberPathChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ComboBox comboBox)
        {
            comboBox.SetupItemTemplate();
        }
    }

    private static void OnIconMemberPathChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ComboBox comboBox)
        {
            comboBox.SetupItemTemplate();
        }
    }

    private static void OnItemTemplateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ComboBox comboBox)
        {
            comboBox.SetupItemTemplate();
        }
    }

    private void OnSelectedItemChangedInternal(object? newValue)
    {
        if (_isUpdatingFromSelection)
            return;

        _isUpdatingFromSelection = true;
        try
        {
            if (!string.IsNullOrEmpty(ValueMemberPath) && newValue != null)
            {
                SelectedValue = GetPropertyValue(newValue, ValueMemberPath);
            }
            else if (newValue == null)
            {
                SelectedValue = null;
            }

            UpdateDisplayState();
            RaiseSelectionChanged(newValue);
        }
        finally
        {
            _isUpdatingFromSelection = false;
        }
    }

    #endregion

    #region Event Handlers

    private void OnCollapsedTapped(object? sender, TappedEventArgs e)
    {
        ToggleDropdown();
    }

    private void OnClearTapped(object? sender, TappedEventArgs e)
    {
        ClearSelection();
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        clearSearchButton.IsVisible = !string.IsNullOrEmpty(e.NewTextValue);

        _debounceTokenSource?.Cancel();
        _debounceTokenSource = new CancellationTokenSource();

        var token = _debounceTokenSource.Token;
        Task.Delay(100, token).ContinueWith(_ =>
        {
            if (!token.IsCancellationRequested)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdateFilteredItems(e.NewTextValue);

                    // Auto-highlight single result for easy Enter/Tab selection
                    if (FilteredItems.Count == 1)
                    {
                        _highlightedIndex = 0;
                    }
                    else if (FilteredItems.Count > 0)
                    {
                        _highlightedIndex = 0; // Highlight first item by default
                    }
                    else
                    {
                        _highlightedIndex = -1;
                    }
                    UpdateHighlightVisual();
                });
            }
        }, TaskContinuationOptions.OnlyOnRanToCompletion);
    }

    private void OnClearSearchTapped(object? sender, TappedEventArgs e)
    {
        searchEntry.Text = string.Empty;
        UpdateFilteredItems(string.Empty);
    }

    private void OnItemTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Grid grid && grid.BindingContext is { } item)
        {
            SelectItem(item);
        }
    }

    private bool _isUpdatingHighlight;

    private void OnItemsListSelectionChanged(object? sender, Microsoft.Maui.Controls.SelectionChangedEventArgs e)
    {
        // Ignore programmatic selection changes for highlighting
        if (_isUpdatingHighlight) return;

        // When user clicks an item in the list, select it
        if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is { } item)
        {
            SelectItem(item);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Programmatically opens the dropdown.
    /// </summary>
    public void Open()
    {
        if (!_isExpanded)
        {
            ToggleDropdown();
        }
    }

    /// <summary>
    /// Programmatically closes the dropdown.
    /// </summary>
    public void Close()
    {
        if (_isExpanded)
        {
            ToggleDropdown();
        }
    }

    /// <summary>
    /// Clears the current selection.
    /// </summary>
    public void ClearSelection()
    {
        _isUpdatingFromSelection = true;
        try
        {
            SelectedItem = null;
            SelectedValue = null;
            UpdateDisplayState();
            RaiseSelectionChanged(null);
        }
        finally
        {
            _isUpdatingFromSelection = false;
        }

        // Execute clear command
        if (ClearCommand?.CanExecute(null) == true)
        {
            ClearCommand.Execute(null);
        }

        if (_isExpanded)
        {
            Close();
        }
    }

    /// <summary>
    /// Refreshes the dropdown items from the current ItemsSource.
    /// </summary>
    public void RefreshItems()
    {
        UpdateFilteredItems(searchEntry?.Text ?? string.Empty);
    }

    /// <summary>
    /// Sets the selected item from an external source (e.g., popup overlay).
    /// Used in PopupMode when the selection is made in the parent's popup.
    /// </summary>
    public void SetSelectedItemFromPopup(object? item)
    {
        SelectItem(item!);
    }

    /// <summary>
    /// Performs validation and returns the result.
    /// </summary>
    /// <returns>The validation result.</returns>
    public ValidationResult Validate()
    {
        _validationErrors.Clear();

        if (IsRequired && SelectedItem == null)
        {
            _validationErrors.Add(RequiredErrorMessage);
        }

        var result = _validationErrors.Count == 0
            ? ValidationResult.Success
            : ValidationResult.Failure(_validationErrors);

        IsValid = result.IsValid;
        OnPropertyChanged(nameof(ValidationErrors));

        // Execute validate command with result
        if (ValidateCommand?.CanExecute(result) == true)
        {
            ValidateCommand.Execute(result);
        }

        return result;
    }

    #endregion

    #region Private Methods

    private void RaiseSelectionChanged(object? newValue)
    {
        // Raise event
        SelectionChanged?.Invoke(this, newValue);

        // Execute command
        if (SelectionChangedCommand != null)
        {
            var parameter = SelectionChangedCommandParameter ?? newValue;
            if (SelectionChangedCommand.CanExecute(parameter))
            {
                SelectionChangedCommand.Execute(parameter);
            }
        }

        // Trigger validation if required
        if (IsRequired)
        {
            Validate();
        }
    }

    private void RaiseOpened()
    {
        Opened?.Invoke(this, EventArgs.Empty);

        if (OpenedCommand?.CanExecute(null) == true)
        {
            OpenedCommand.Execute(null);
        }
    }

    private void RaiseClosed()
    {
        Closed?.Invoke(this, EventArgs.Empty);

        if (ClosedCommand?.CanExecute(null) == true)
        {
            ClosedCommand.Execute(null);
        }
    }

    private void SetupItemTemplate()
    {
        // If a custom ItemTemplate is provided, use it directly
        if (ItemTemplate != null)
        {
            itemsList.ItemTemplate = ItemTemplate;
            return;
        }

        // Otherwise, create default template based on DisplayMemberPath/IconMemberPath
        var displayMemberPath = DisplayMemberPath;
        var iconMemberPath = IconMemberPath;
        var hasIcon = !string.IsNullOrEmpty(iconMemberPath);

        itemsList.ItemTemplate = new DataTemplate(() =>
        {
            var grid = new Grid
            {
                Padding = new Thickness(12, 10),
                BackgroundColor = Colors.Transparent,
                ColumnSpacing = 10
            };

            if (hasIcon)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(28)));
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            }

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnItemTapped;
            grid.GestureRecognizers.Add(tapGesture);

            if (hasIcon)
            {
                var image = new Image
                {
                    WidthRequest = 28,
                    HeightRequest = 28,
                    VerticalOptions = LayoutOptions.Center,
                    Aspect = Aspect.AspectFit
                };
                image.SetBinding(Image.SourceProperty, new Binding(iconMemberPath, converter: new MauiAssetImageConverter()));
                Grid.SetColumn(image, 0);
                grid.Add(image);
            }

            var label = new Label
            {
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center
            };
            label.SetAppThemeColor(Label.TextColorProperty,
                Color.FromArgb("#212121"),
                Colors.White);

            if (!string.IsNullOrEmpty(displayMemberPath))
            {
                label.SetBinding(Label.TextProperty, new Binding(displayMemberPath));
            }
            else
            {
                label.SetBinding(Label.TextProperty, new Binding("."));
            }

            if (hasIcon)
            {
                Grid.SetColumn(label, 1);
            }
            grid.Add(label);

            VisualStateManager.SetVisualStateGroups(grid, new VisualStateGroupList
            {
                new VisualStateGroup
                {
                    Name = "CommonStates",
                    States =
                    {
                        new VisualState
                        {
                            Name = "Normal",
                            Setters = { new Setter { Property = Grid.BackgroundColorProperty, Value = Colors.Transparent } }
                        },
                        new VisualState
                        {
                            Name = "PointerOver",
                            Setters =
                            {
                                new Setter
                                {
                                    Property = Grid.BackgroundColorProperty,
                                    Value = MauiControlsExtras.Theming.MauiControlsExtrasTheme.Current.HoverColor
                                }
                            }
                        },
                        new VisualState
                        {
                            Name = "Selected",
                            Setters =
                            {
                                new Setter
                                {
                                    Property = Grid.BackgroundColorProperty,
                                    Value = MauiControlsExtras.Theming.MauiControlsExtrasTheme.Current.SelectedBackgroundColor
                                }
                            }
                        }
                    }
                }
            });

            return grid;
        });
    }

    private void ToggleDropdown()
    {
        // In popup mode, don't show inline dropdown - raise event for parent to handle
        if (PopupMode && !_isExpanded)
        {
            var bounds = GetBoundsRelativeToPage();
            var args = new ComboBoxPopupRequestEventArgs(
                this,
                bounds,
                ItemsSource,
                DisplayMemberPath,
                SelectedItem,
                Placeholder,
                IsSearchVisible);
            PopupRequested?.Invoke(this, args);
            return;
        }

        _isExpanded = !_isExpanded;

        var accentColor = AccentColor;
        var defaultStroke = EffectiveBorderColor;

        if (_isExpanded)
        {
            UpdateFilteredItems(string.Empty);
            expandedBorder.IsVisible = true;
            dropdownArrow.Text = "▲";
            collapsedBorder.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(EffectiveCornerRadius, EffectiveCornerRadius, 0, 0) };
            collapsedBorder.Stroke = EffectiveFocusBorderColor;
            expandedBorder.Stroke = EffectiveFocusBorderColor;
            _highlightedIndex = FilteredItems.Count > 0 ? 0 : -1;
            UpdateHighlightVisual();
            RaiseOpened();
#if ANDROID
            AndroidBackButtonHandler.Register(this, Close);
#endif

            // Focus the appropriate element when dropdown opens
            if (IsSearchVisible)
            {
                Dispatcher.Dispatch(() => searchEntry?.Focus());
            }
            else
            {
                // Focus the hidden keyboard capture entry for keyboard navigation when search is hidden
                Dispatcher.Dispatch(() => keyboardCaptureEntry?.Focus());
            }
        }
        else
        {
            expandedBorder.IsVisible = false;
            dropdownArrow.Text = "▼";
            collapsedBorder.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(EffectiveCornerRadius) };
            collapsedBorder.Stroke = defaultStroke;
            searchEntry.Text = string.Empty;
            searchEntry.Unfocus();
#if ANDROID
            AndroidBackButtonHandler.Unregister(this);
#endif
            RaiseClosed();
        }
    }

    /// <summary>
    /// Gets the bounds of this control relative to the page/window.
    /// </summary>
    private Rect GetBoundsRelativeToPage()
    {
        var x = 0.0;
        var y = 0.0;

        // Walk up the visual tree to calculate absolute position
        Element? current = this;
        while (current != null)
        {
            if (current is VisualElement ve)
            {
                x += ve.X;
                y += ve.Y;

                // Also account for scroll position in parent ScrollViews
                if (current.Parent is ScrollView scrollView)
                {
                    x -= scrollView.ScrollX;
                    y -= scrollView.ScrollY;
                }
            }
            current = current.Parent;
        }

        return new Rect(x, y, Width, Height);
    }

    private void SelectItem(object item)
    {
        _isUpdatingFromSelection = true;
        try
        {
            SelectedItem = item;

            if (!string.IsNullOrEmpty(ValueMemberPath))
            {
                SelectedValue = GetPropertyValue(item, ValueMemberPath);
            }

            UpdateDisplayState();
        }
        finally
        {
            _isUpdatingFromSelection = false;
        }

        Close();
        RaiseSelectionChanged(item);
    }

    private void SelectItemByValue(object? value)
    {
        if (value == null || ItemsSource == null)
        {
            SelectedItem = null;
            return;
        }

        foreach (var item in ItemsSource)
        {
            if (item == null) continue;

            var itemValue = !string.IsNullOrEmpty(ValueMemberPath)
                ? GetPropertyValue(item, ValueMemberPath)
                : item;

            if (Equals(itemValue, value))
            {
                _isUpdatingFromSelection = true;
                try
                {
                    SelectedItem = item;
                    UpdateDisplayState();
                }
                finally
                {
                    _isUpdatingFromSelection = false;
                }
                return;
            }
        }
    }

    private void ApplyDefaultValue()
    {
        if (DefaultValue == null || SelectedItem != null || ItemsSource == null)
            return;

        foreach (var item in ItemsSource)
        {
            if (item == null) continue;

            if (!string.IsNullOrEmpty(ValueMemberPath))
            {
                var itemValue = GetPropertyValue(item, ValueMemberPath);
                if (Equals(itemValue, DefaultValue))
                {
                    SelectItem(item);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(DisplayMemberPath))
            {
                var displayValue = GetPropertyValue(item, DisplayMemberPath);
                if (Equals(displayValue, DefaultValue))
                {
                    SelectItem(item);
                    return;
                }
            }

            if (Equals(item, DefaultValue))
            {
                SelectItem(item);
                return;
            }
        }
    }

    private void UpdateDisplayState()
    {
        if (SelectedItem != null)
        {
            DisplayText = GetDisplayText(SelectedItem);
            HasSelection = true;
            DisplayTextColor = Application.Current?.RequestedTheme == AppTheme.Dark
                ? Colors.White
                : Color.FromArgb("#1F1F1F");

            if (!string.IsNullOrEmpty(IconMemberPath))
            {
                SelectedIconSource = GetPropertyValue(SelectedItem, IconMemberPath)?.ToString();
                HasSelectedIcon = !string.IsNullOrEmpty(SelectedIconSource);
            }
            else
            {
                SelectedIconSource = null;
                HasSelectedIcon = false;
            }
        }
        else
        {
            DisplayText = Placeholder;
            HasSelection = false;
            SelectedIconSource = null;
            HasSelectedIcon = false;
            DisplayTextColor = Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#9CA3AF")
                : Color.FromArgb("#6B7280");
        }
    }

    private void UpdateFilteredItems(string? searchText)
    {
        FilteredItems.Clear();

        if (ItemsSource == null)
            return;

        var search = searchText?.Trim() ?? string.Empty;
        var hasSearch = !string.IsNullOrWhiteSpace(search);

        foreach (var item in ItemsSource)
        {
            if (item == null)
                continue;

            var displayText = GetDisplayText(item);

            if (!hasSearch || displayText.Contains(search, StringComparison.OrdinalIgnoreCase))
            {
                FilteredItems.Add(item);
            }
        }
    }

    private void UpdateListMaxHeight()
    {
        ListMaxHeight = VisibleItemCount * 44;
    }

    private string GetDisplayText(object item)
    {
        if (item == null)
            return string.Empty;

        if (!string.IsNullOrEmpty(DisplayMemberPath))
        {
            var value = GetPropertyValue(item, DisplayMemberPath);
            return value?.ToString() ?? string.Empty;
        }

        return item.ToString() ?? string.Empty;
    }

    private static object? GetPropertyValue(object item, string propertyPath)
    {
        var property = item.GetType().GetProperty(propertyPath);
        return property?.GetValue(item);
    }

    #endregion

    #region IKeyboardNavigable Implementation

    /// <inheritdoc />
    public bool CanReceiveFocus => IsEnabled && IsVisible;

    /// <inheritdoc />
    public bool IsKeyboardNavigationEnabled
    {
        get => _isKeyboardNavigationEnabled;
        set
        {
            _isKeyboardNavigationEnabled = value;
            OnPropertyChanged(nameof(IsKeyboardNavigationEnabled));
        }
    }

    /// <inheritdoc />
    public bool HasKeyboardFocus => IsFocused;

    /// <summary>
    /// Identifies the GotFocusCommand bindable property.
    /// </summary>
    public static readonly BindableProperty GotFocusCommandProperty = BindableProperty.Create(
        nameof(GotFocusCommand),
        typeof(ICommand),
        typeof(ComboBox));

    /// <summary>
    /// Identifies the LostFocusCommand bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandProperty = BindableProperty.Create(
        nameof(LostFocusCommand),
        typeof(ICommand),
        typeof(ComboBox));

    /// <summary>
    /// Identifies the KeyPressCommand bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandProperty = BindableProperty.Create(
        nameof(KeyPressCommand),
        typeof(ICommand),
        typeof(ComboBox));

    /// <inheritdoc />
    public ICommand? GotFocusCommand
    {
        get => (ICommand?)GetValue(GotFocusCommandProperty);
        set => SetValue(GotFocusCommandProperty, value);
    }

    /// <inheritdoc />
    public ICommand? LostFocusCommand
    {
        get => (ICommand?)GetValue(LostFocusCommandProperty);
        set => SetValue(LostFocusCommandProperty, value);
    }

    /// <inheritdoc />
    public ICommand? KeyPressCommand
    {
        get => (ICommand?)GetValue(KeyPressCommandProperty);
        set => SetValue(KeyPressCommandProperty, value);
    }

    /// <inheritdoc />
    public event EventHandler<Base.KeyboardFocusEventArgs>? KeyboardFocusGained;

    /// <inheritdoc />
#pragma warning disable CS0067 // Event is never used (raised by platform-specific handlers)
    public event EventHandler<Base.KeyboardFocusEventArgs>? KeyboardFocusLost;
#pragma warning restore CS0067

    /// <inheritdoc />
    public event EventHandler<Base.KeyEventArgs>? KeyPressed;

    /// <inheritdoc />
#pragma warning disable CS0067 // Event is never used (raised by platform-specific handlers)
    public event EventHandler<Base.KeyEventArgs>? KeyReleased;
#pragma warning restore CS0067

    /// <inheritdoc />
    public bool HandleKeyPress(Base.KeyEventArgs e)
    {
        if (!IsKeyboardNavigationEnabled) return false;

        // Raise event first
        KeyPressed?.Invoke(this, e);
        if (e.Handled) return true;

        // Execute command if set
        if (KeyPressCommand?.CanExecute(e) == true)
        {
            KeyPressCommand.Execute(e);
            if (e.Handled) return true;
        }

        return e.Key switch
        {
            "ArrowDown" => HandleDownKey(),
            "ArrowUp" => HandleUpKey(),
            "Enter" or "Space" => HandleEnterKey(),
            "Escape" => HandleEscapeKey(),
            "Home" => HandleHomeKey(),
            "End" => HandleEndKey(),
            "C" when e.IsPlatformCommandPressed => HandleCopyKey(),
            "X" when e.IsPlatformCommandPressed => HandleCutKey(),
            "V" when e.IsPlatformCommandPressed => HandlePasteKey(),
            _ => false
        };
    }

    /// <inheritdoc />
    public IReadOnlyList<Base.KeyboardShortcut> GetKeyboardShortcuts()
    {
        if (_keyboardShortcuts.Count == 0)
        {
            _keyboardShortcuts.AddRange(new[]
            {
                new Base.KeyboardShortcut { Key = "Alt+ArrowDown", Description = "Open dropdown", Category = "Action" },
                new Base.KeyboardShortcut { Key = "Alt+ArrowUp", Description = "Close dropdown", Category = "Action" },
                new Base.KeyboardShortcut { Key = "ArrowDown", Description = "Open dropdown / Move to next item", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "ArrowUp", Description = "Move to previous item", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "Enter", Description = "Select highlighted item / Open dropdown", Category = "Action" },
                new Base.KeyboardShortcut { Key = "Space", Description = "Open dropdown", Category = "Action" },
                new Base.KeyboardShortcut { Key = "Escape", Description = "Close dropdown", Category = "Action" },
                new Base.KeyboardShortcut { Key = "Home", Description = "Move to first item", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "End", Description = "Move to last item", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "Ctrl+C", Description = "Copy", Category = "Clipboard" },
                new Base.KeyboardShortcut { Key = "Ctrl+X", Description = "Cut", Category = "Clipboard" },
                new Base.KeyboardShortcut { Key = "Ctrl+V", Description = "Paste", Category = "Clipboard" },
            });
        }
        return _keyboardShortcuts;
    }

    /// <inheritdoc />
    public new bool Focus()
    {
        if (!CanReceiveFocus) return false;

        var result = base.Focus();
        if (result)
        {
            KeyboardFocusGained?.Invoke(this, new Base.KeyboardFocusEventArgs(true));
            GotFocusCommand?.Execute(this);
        }
        return result;
    }

    #region Keyboard Handlers

    private bool HandleDownKey()
    {
        if (!_isExpanded)
        {
            Open();
            _highlightedIndex = FilteredItems.IndexOf(SelectedItem ?? new object());
            if (_highlightedIndex < 0) _highlightedIndex = 0;
            return true;
        }

        if (FilteredItems.Count > 0)
        {
            _highlightedIndex = (_highlightedIndex + 1) % FilteredItems.Count;
            UpdateHighlightVisual();
        }
        return true;
    }

    private bool HandleUpKey()
    {
        if (!_isExpanded)
        {
            return false;
        }

        if (FilteredItems.Count > 0)
        {
            _highlightedIndex = _highlightedIndex <= 0 ? FilteredItems.Count - 1 : _highlightedIndex - 1;
            UpdateHighlightVisual();
        }
        return true;
    }

    private bool HandleEnterKey()
    {
        if (!_isExpanded)
        {
            Open();
            return true;
        }

        if (_highlightedIndex >= 0 && _highlightedIndex < FilteredItems.Count)
        {
            SelectItem(FilteredItems[_highlightedIndex]);
        }
        return true;
    }

    private bool HandleEscapeKey()
    {
        if (_isExpanded)
        {
            Close();
            return true;
        }
        return false;
    }

    private bool HandleHomeKey()
    {
        if (_isExpanded && FilteredItems.Count > 0)
        {
            _highlightedIndex = 0;
            UpdateHighlightVisual();
            return true;
        }
        return false;
    }

    private bool HandleEndKey()
    {
        if (_isExpanded && FilteredItems.Count > 0)
        {
            _highlightedIndex = FilteredItems.Count - 1;
            UpdateHighlightVisual();
            return true;
        }
        return false;
    }

    private bool HandleCopyKey() { Copy(); return CanCopy; }
    private bool HandleCutKey() { Cut(); return CanCut; }
    private bool HandlePasteKey() { Paste(); return CanPaste; }

    private void UpdateHighlightVisual()
    {
        _isUpdatingHighlight = true;
        try
        {
            // Scroll the highlighted item into view and update visual selection
            if (_highlightedIndex >= 0 && _highlightedIndex < FilteredItems.Count)
            {
                var highlightedItem = FilteredItems[_highlightedIndex];
                itemsList.SelectedItem = highlightedItem;
                itemsList.ScrollTo(_highlightedIndex, position: ScrollToPosition.MakeVisible, animate: false);
            }
            else
            {
                itemsList.SelectedItem = null;
            }
        }
        finally
        {
            _isUpdatingHighlight = false;
        }
    }

    #endregion

    #endregion

    #region Context Menu Gestures

    private void SetupContextMenuGestures()
    {
        // Platform-specific context menu setup happens in HandlerChanged
        this.HandlerChanged += OnHandlerChangedForContextMenu;

        // Add long-press gesture to the collapsed border (mobile)
        AddLongPressGesture(collapsedBorder);
    }

    private void OnHandlerChangedForContextMenu(object? sender, EventArgs e)
    {
#if WINDOWS
        SetupWindowsContextMenu();
#elif MACCATALYST
        SetupMacContextMenu();
#endif
    }

#if WINDOWS
    private void SetupWindowsContextMenu()
    {
        if (Handler?.PlatformView is Microsoft.UI.Xaml.UIElement element)
        {
            element.RightTapped += OnWindowsRightTapped;
        }
    }

    private void OnWindowsRightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
    {
        var position = e.GetPosition(sender as Microsoft.UI.Xaml.UIElement);
        var mauiPosition = new Point(position.X, position.Y);
        e.Handled = true;
        _longPressCts?.Cancel();
        _ = ShowContextMenuAsync(mauiPosition);
    }
#endif

#if MACCATALYST
    private void SetupMacContextMenu()
    {
        if (Handler?.PlatformView is UIKit.UIView view)
        {
            var tapRecognizer = new UIKit.UITapGestureRecognizer(OnMacSecondaryClick);
            tapRecognizer.ButtonMaskRequired = UIKit.UIEventButtonMask.Secondary;
            view.AddGestureRecognizer(tapRecognizer);
        }
    }

    private void OnMacSecondaryClick(UIKit.UITapGestureRecognizer recognizer)
    {
        var location = recognizer.LocationInView(recognizer.View);
        var mauiPosition = new Point(location.X, location.Y);
        _longPressCts?.Cancel();
        _ = ShowContextMenuAsync(mauiPosition);
    }
#endif

    private void AddLongPressGesture(View target)
    {
        Point? pressPosition = null;

        var pointerGesture = new PointerGestureRecognizer();

        pointerGesture.PointerPressed += (s, e) =>
        {
            pressPosition = e.GetPosition(target);
            _longPressCts?.Cancel();
            _longPressCts = new CancellationTokenSource();
            _ = DetectLongPressAsync(_longPressCts.Token, pressPosition);
        };

        pointerGesture.PointerMoved += (s, e) =>
        {
            if (pressPosition == null || _longPressCts == null) return;
            var currentPosition = e.GetPosition(target);
            if (currentPosition == null) return;

            var dx = currentPosition.Value.X - pressPosition.Value.X;
            var dy = currentPosition.Value.Y - pressPosition.Value.Y;
            if (Math.Sqrt(dx * dx + dy * dy) > 10)
            {
                _longPressCts?.Cancel();
                _longPressCts = null;
            }
        };

        pointerGesture.PointerReleased += (s, e) =>
        {
            _longPressCts?.Cancel();
            _longPressCts = null;
        };

        target.GestureRecognizers.Add(pointerGesture);
    }

    private async Task DetectLongPressAsync(CancellationToken cancellationToken, Point? position)
    {
        try
        {
            await Task.Delay(500, cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await ShowContextMenuAsync(position);
                });
            }
        }
        catch (TaskCanceledException)
        {
            // Long press was cancelled (pointer released or moved)
        }
    }

    #endregion
}
