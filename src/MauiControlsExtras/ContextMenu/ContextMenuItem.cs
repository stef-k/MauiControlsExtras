using System.Windows.Input;

namespace MauiControlsExtras.ContextMenu;

/// <summary>
/// Represents a single item in a context menu. Supports text, icons, commands, actions, submenus, and separators.
/// </summary>
public class ContextMenuItem : BindableObject
{
    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="Text"/> bindable property.
    /// </summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(ContextMenuItem),
        null);

    /// <summary>
    /// Identifies the <see cref="Icon"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon),
        typeof(ImageSource),
        typeof(ContextMenuItem),
        null);

    /// <summary>
    /// Identifies the <see cref="IconGlyph"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IconGlyphProperty = BindableProperty.Create(
        nameof(IconGlyph),
        typeof(string),
        typeof(ContextMenuItem),
        null);

    /// <summary>
    /// Identifies the <see cref="Command"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(ContextMenuItem),
        null);

    /// <summary>
    /// Identifies the <see cref="CommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter),
        typeof(object),
        typeof(ContextMenuItem),
        null);

    /// <summary>
    /// Identifies the <see cref="IsEnabled"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create(
        nameof(IsEnabled),
        typeof(bool),
        typeof(ContextMenuItem),
        true);

    /// <summary>
    /// Identifies the <see cref="IsVisible"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(
        nameof(IsVisible),
        typeof(bool),
        typeof(ContextMenuItem),
        true);

    /// <summary>
    /// Identifies the <see cref="IsSeparator"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsSeparatorProperty = BindableProperty.Create(
        nameof(IsSeparator),
        typeof(bool),
        typeof(ContextMenuItem),
        false);

    /// <summary>
    /// Identifies the <see cref="KeyboardShortcut"/> bindable property.
    /// </summary>
    public static readonly BindableProperty KeyboardShortcutProperty = BindableProperty.Create(
        nameof(KeyboardShortcut),
        typeof(string),
        typeof(ContextMenuItem),
        null);

    #endregion

    #region Private Fields

    private Action? _action;
    private readonly ContextMenuItemCollection _subItems = new();

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the display text for the menu item.
    /// </summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon image source.
    /// </summary>
    public ImageSource? Icon
    {
        get => (ImageSource?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon glyph (for font icons like Segoe MDL2 Assets).
    /// </summary>
    public string? IconGlyph
    {
        get => (string?)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the item is selected.
    /// </summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to the command.
    /// </summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the action to execute when the item is selected.
    /// This is an alternative to Command for simple scenarios.
    /// </summary>
    public Action? Action
    {
        get => _action;
        set => _action = value;
    }

    /// <summary>
    /// Gets or sets whether the menu item is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the menu item is visible.
    /// </summary>
    public bool IsVisible
    {
        get => (bool)GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets whether this item represents a separator line.
    /// </summary>
    public bool IsSeparator
    {
        get => (bool)GetValue(IsSeparatorProperty);
        set => SetValue(IsSeparatorProperty, value);
    }

    /// <summary>
    /// Gets or sets the keyboard shortcut hint text (e.g., "Ctrl+C").
    /// This is for display purposes only; actual keyboard handling must be implemented separately.
    /// </summary>
    public string? KeyboardShortcut
    {
        get => (string?)GetValue(KeyboardShortcutProperty);
        set => SetValue(KeyboardShortcutProperty, value);
    }

    /// <summary>
    /// Gets the collection of sub-menu items. If non-empty, this item acts as a submenu parent.
    /// </summary>
    public ContextMenuItemCollection SubItems => _subItems;

    /// <summary>
    /// Gets whether this item has any sub-items.
    /// </summary>
    public bool HasSubItems => _subItems.Count > 0;

    #endregion

    #region Methods

    /// <summary>
    /// Executes the menu item's action. Invokes Command if set, otherwise invokes Action.
    /// </summary>
    public void Execute()
    {
        if (!IsEnabled)
            return;

        if (Command != null && Command.CanExecute(CommandParameter))
        {
            Command.Execute(CommandParameter);
        }
        else
        {
            Action?.Invoke();
        }
    }

    /// <summary>
    /// Gets whether the menu item can be executed.
    /// </summary>
    public bool CanExecute()
    {
        if (!IsEnabled)
            return false;

        if (Command != null)
            return Command.CanExecute(CommandParameter);

        return Action != null || HasSubItems;
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a separator menu item.
    /// </summary>
    public static ContextMenuItem Separator() => new() { IsSeparator = true };

    /// <summary>
    /// Creates a menu item with text and an action.
    /// </summary>
    /// <param name="text">The display text.</param>
    /// <param name="action">The action to execute when selected.</param>
    /// <param name="iconGlyph">Optional icon glyph.</param>
    /// <param name="keyboardShortcut">Optional keyboard shortcut hint.</param>
    public static ContextMenuItem Create(string text, Action action, string? iconGlyph = null, string? keyboardShortcut = null)
    {
        return new ContextMenuItem
        {
            Text = text,
            Action = action,
            IconGlyph = iconGlyph,
            KeyboardShortcut = keyboardShortcut
        };
    }

    /// <summary>
    /// Creates a menu item with text and a command.
    /// </summary>
    /// <param name="text">The display text.</param>
    /// <param name="command">The command to execute when selected.</param>
    /// <param name="parameter">Optional command parameter.</param>
    /// <param name="iconGlyph">Optional icon glyph.</param>
    /// <param name="keyboardShortcut">Optional keyboard shortcut hint.</param>
    public static ContextMenuItem Create(string text, ICommand command, object? parameter = null, string? iconGlyph = null, string? keyboardShortcut = null)
    {
        return new ContextMenuItem
        {
            Text = text,
            Command = command,
            CommandParameter = parameter,
            IconGlyph = iconGlyph,
            KeyboardShortcut = keyboardShortcut
        };
    }

    /// <summary>
    /// Creates a submenu item with child items.
    /// </summary>
    /// <param name="text">The display text.</param>
    /// <param name="subItems">The child menu items.</param>
    /// <param name="iconGlyph">Optional icon glyph.</param>
    public static ContextMenuItem CreateSubMenu(string text, IEnumerable<ContextMenuItem> subItems, string? iconGlyph = null)
    {
        var item = new ContextMenuItem
        {
            Text = text,
            IconGlyph = iconGlyph
        };

        foreach (var subItem in subItems)
        {
            item.SubItems.Add(subItem);
        }

        return item;
    }

    #endregion
}
