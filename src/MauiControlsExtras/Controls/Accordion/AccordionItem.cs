using System.Windows.Input;

namespace MauiControlsExtras.Controls;

/// <summary>
/// Represents a single expandable/collapsible item in an Accordion.
/// </summary>
public class AccordionItem : ContentView
{
    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="Header"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HeaderProperty = BindableProperty.Create(
        nameof(Header),
        typeof(string),
        typeof(AccordionItem),
        null);

    /// <summary>
    /// Identifies the <see cref="HeaderTemplate"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HeaderTemplateProperty = BindableProperty.Create(
        nameof(HeaderTemplate),
        typeof(DataTemplate),
        typeof(AccordionItem),
        null);

    /// <summary>
    /// Identifies the <see cref="Icon"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon),
        typeof(string),
        typeof(AccordionItem),
        null);

    /// <summary>
    /// Identifies the <see cref="IsEnabled"/> bindable property.
    /// </summary>
    public static new readonly BindableProperty IsEnabledProperty = BindableProperty.Create(
        nameof(IsEnabled),
        typeof(bool),
        typeof(AccordionItem),
        true);

    /// <summary>
    /// Identifies the <see cref="ExpandCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ExpandCommandProperty = BindableProperty.Create(
        nameof(ExpandCommand),
        typeof(ICommand),
        typeof(AccordionItem),
        null);

    /// <summary>
    /// Identifies the <see cref="ExpandCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ExpandCommandParameterProperty = BindableProperty.Create(
        nameof(ExpandCommandParameter),
        typeof(object),
        typeof(AccordionItem));

    /// <summary>
    /// Identifies the <see cref="CollapseCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CollapseCommandProperty = BindableProperty.Create(
        nameof(CollapseCommand),
        typeof(ICommand),
        typeof(AccordionItem),
        null);

    /// <summary>
    /// Identifies the <see cref="CollapseCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CollapseCommandParameterProperty = BindableProperty.Create(
        nameof(CollapseCommandParameter),
        typeof(object),
        typeof(AccordionItem));

    /// <summary>
    /// Identifies the <see cref="IsExpanded"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create(
        nameof(IsExpanded),
        typeof(bool),
        typeof(AccordionItem),
        false,
        BindingMode.TwoWay,
        propertyChanged: OnIsExpandedChanged);

    private static void OnIsExpandedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AccordionItem item && oldValue != newValue)
        {
            item.OnPropertyChanged(nameof(ExpanderIcon));
            item.IsExpandedChanged?.Invoke(item, (bool)newValue);
        }
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the header text.
    /// </summary>
    public string? Header
    {
        get => (string?)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets a template for the header.
    /// </summary>
    public DataTemplate? HeaderTemplate
    {
        get => (DataTemplate?)GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the header icon.
    /// </summary>
    public string? Icon
    {
        get => (string?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets whether this item is enabled.
    /// </summary>
    public new bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when expanding.
    /// </summary>
    public ICommand? ExpandCommand
    {
        get => (ICommand?)GetValue(ExpandCommandProperty);
        set => SetValue(ExpandCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="ExpandCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? ExpandCommandParameter
    {
        get => GetValue(ExpandCommandParameterProperty);
        set => SetValue(ExpandCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when collapsing.
    /// </summary>
    public ICommand? CollapseCommand
    {
        get => (ICommand?)GetValue(CollapseCommandProperty);
        set => SetValue(CollapseCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="CollapseCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? CollapseCommandParameter
    {
        get => GetValue(CollapseCommandParameterProperty);
        set => SetValue(CollapseCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets whether this item is expanded.
    /// </summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>
    /// Gets the expander icon based on expansion state.
    /// </summary>
    public string ExpanderIcon => IsExpanded ? "▼" : "▶";

    /// <summary>
    /// Gets the item index within the accordion.
    /// </summary>
    public int Index { get; internal set; }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the expansion state changes.
    /// </summary>
    internal event Action<AccordionItem, bool>? IsExpandedChanged;

    /// <summary>
    /// Occurs when the item is expanded.
    /// </summary>
    public event EventHandler? Expanded;

    /// <summary>
    /// Occurs when the item is collapsed.
    /// </summary>
    public event EventHandler? Collapsed;

    #endregion

    #region Methods

    /// <summary>
    /// Expands the item.
    /// </summary>
    public void Expand()
    {
        if (!IsExpanded && IsEnabled)
        {
            IsExpanded = true;
            Expanded?.Invoke(this, EventArgs.Empty);
            ExpandCommand?.Execute(ExpandCommandParameter ?? this);
        }
    }

    /// <summary>
    /// Collapses the item.
    /// </summary>
    public void Collapse()
    {
        if (IsExpanded)
        {
            IsExpanded = false;
            Collapsed?.Invoke(this, EventArgs.Empty);
            CollapseCommand?.Execute(CollapseCommandParameter ?? this);
        }
    }

    /// <summary>
    /// Toggles the expansion state.
    /// </summary>
    public void Toggle()
    {
        if (IsExpanded)
        {
            Collapse();
        }
        else
        {
            Expand();
        }
    }

    #endregion
}
