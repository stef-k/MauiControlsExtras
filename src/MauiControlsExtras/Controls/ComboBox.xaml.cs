using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiControlsExtras.Base;
using MauiControlsExtras.Base.Validation;
using MauiControlsExtras.Converters;
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
public partial class ComboBox : StyledControlBase, IValidatable
{
    private bool _isExpanded;
    private bool _isUpdatingFromSelection;
    private CancellationTokenSource? _debounceTokenSource;
    private List<string> _validationErrors = new();

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
                MainThread.BeginInvokeOnMainThread(() => UpdateFilteredItems(e.NewTextValue));
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
                                    Value = Application.Current?.RequestedTheme == AppTheme.Dark
                                        ? Color.FromArgb("#424242")
                                        : Color.FromArgb("#F5F5F5")
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
        _isExpanded = !_isExpanded;

        var accentColor = AccentColor;
        var defaultStroke = EffectiveBorderColor;

        if (_isExpanded)
        {
            UpdateFilteredItems(string.Empty);
            expandedBorder.IsVisible = true;
            dropdownArrow.Text = "▲";
            collapsedBorder.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(CornerRadius, CornerRadius, 0, 0) };
            collapsedBorder.Stroke = EffectiveFocusBorderColor;
            expandedBorder.Stroke = EffectiveFocusBorderColor;
            RaiseOpened();
        }
        else
        {
            expandedBorder.IsVisible = false;
            dropdownArrow.Text = "▼";
            collapsedBorder.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(CornerRadius) };
            collapsedBorder.Stroke = defaultStroke;
            searchEntry.Text = string.Empty;
            searchEntry.Unfocus();
            RaiseClosed();
        }
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
}
