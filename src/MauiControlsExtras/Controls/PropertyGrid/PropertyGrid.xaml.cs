using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows.Input;
using MauiControlsExtras.Base;
using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Controls;

/// <summary>
/// A PropertyGrid control that automatically generates an editable UI from an object's properties.
/// Similar to WinForms PropertyGrid or Visual Studio's Properties panel.
/// </summary>
public partial class PropertyGrid : HeaderedControlBase, IKeyboardNavigable
{
    #region Private Fields

    private readonly List<PropertyCategory> _categories = new();
    private readonly List<PropertyItem> _flatProperties = new();
    private PropertyItem? _selectedProperty;
    private bool _hasKeyboardFocus;

    #endregion

    #region Metadata Registration

    /// <summary>
    /// Registers AOT-safe property metadata for a type. When registered, PropertyGrid
    /// uses the metadata instead of reflection to discover and access properties.
    /// </summary>
    public static void RegisterMetadata(Type type, params PropertyMetadataEntry[] entries)
    {
        PropertyMetadataRegistry.Register(type, entries);
    }

    /// <summary>
    /// Registers AOT-safe property metadata for a type (generic variant).
    /// </summary>
    public static void RegisterMetadata<T>(params PropertyMetadataEntry[] entries)
    {
        PropertyMetadataRegistry.Register<T>(entries);
    }

    /// <summary>
    /// Returns true if metadata has been registered for the specified type.
    /// </summary>
    public static bool HasMetadata(Type type) => PropertyMetadataRegistry.HasMetadata(type);

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="SelectedObject"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectedObjectProperty = BindableProperty.Create(
        nameof(SelectedObject),
        typeof(object),
        typeof(PropertyGrid),
        null,
        BindingMode.TwoWay,
        propertyChanged: OnSelectedObjectChanged);

    /// <summary>
    /// Identifies the <see cref="IsReadOnly"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(
        nameof(IsReadOnly),
        typeof(bool),
        typeof(PropertyGrid),
        false,
        propertyChanged: OnIsReadOnlyChanged);

    /// <summary>
    /// Identifies the <see cref="ShowCategories"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowCategoriesProperty = BindableProperty.Create(
        nameof(ShowCategories),
        typeof(bool),
        typeof(PropertyGrid),
        true,
        propertyChanged: OnShowCategoriesChanged);

    /// <summary>
    /// Identifies the <see cref="ShowDescription"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowDescriptionProperty = BindableProperty.Create(
        nameof(ShowDescription),
        typeof(bool),
        typeof(PropertyGrid),
        true);

    /// <summary>
    /// Identifies the <see cref="ShowSearchBox"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowSearchBoxProperty = BindableProperty.Create(
        nameof(ShowSearchBox),
        typeof(bool),
        typeof(PropertyGrid),
        true);

    /// <summary>
    /// Identifies the <see cref="SortMode"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SortModeProperty = BindableProperty.Create(
        nameof(SortMode),
        typeof(PropertySortMode),
        typeof(PropertyGrid),
        PropertySortMode.Categorized,
        propertyChanged: OnSortModeChanged);

    /// <summary>
    /// Identifies the <see cref="ExpandAllCategories"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ExpandAllCategoriesProperty = BindableProperty.Create(
        nameof(ExpandAllCategories),
        typeof(bool),
        typeof(PropertyGrid),
        false);

    /// <summary>
    /// Identifies the <see cref="SearchText"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SearchTextProperty = BindableProperty.Create(
        nameof(SearchText),
        typeof(string),
        typeof(PropertyGrid),
        null,
        propertyChanged: OnSearchTextChanged);

    /// <summary>
    /// Identifies the <see cref="IsKeyboardNavigationEnabled"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsKeyboardNavigationEnabledProperty = BindableProperty.Create(
        nameof(IsKeyboardNavigationEnabled),
        typeof(bool),
        typeof(PropertyGrid),
        true);

    #endregion

    #region Command Properties

    /// <summary>
    /// Identifies the <see cref="PropertyChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PropertyChangedCommandProperty = BindableProperty.Create(
        nameof(PropertyChangedCommand),
        typeof(ICommand),
        typeof(PropertyGrid));

    /// <summary>
    /// Identifies the <see cref="PropertyChangedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PropertyChangedCommandParameterProperty = BindableProperty.Create(
        nameof(PropertyChangedCommandParameter),
        typeof(object),
        typeof(PropertyGrid));

    /// <summary>
    /// Identifies the <see cref="PropertyChangingCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PropertyChangingCommandProperty = BindableProperty.Create(
        nameof(PropertyChangingCommand),
        typeof(ICommand),
        typeof(PropertyGrid));

    /// <summary>
    /// Identifies the <see cref="PropertyChangingCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PropertyChangingCommandParameterProperty = BindableProperty.Create(
        nameof(PropertyChangingCommandParameter),
        typeof(object),
        typeof(PropertyGrid));

    /// <summary>
    /// Identifies the <see cref="SelectedObjectChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectedObjectChangedCommandProperty = BindableProperty.Create(
        nameof(SelectedObjectChangedCommand),
        typeof(ICommand),
        typeof(PropertyGrid));

    /// <summary>
    /// Identifies the <see cref="SelectedObjectChangedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectedObjectChangedCommandParameterProperty = BindableProperty.Create(
        nameof(SelectedObjectChangedCommandParameter),
        typeof(object),
        typeof(PropertyGrid));

    /// <summary>
    /// Identifies the <see cref="PropertySelectionChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PropertySelectionChangedCommandProperty = BindableProperty.Create(
        nameof(PropertySelectionChangedCommand),
        typeof(ICommand),
        typeof(PropertyGrid));

    /// <summary>
    /// Identifies the <see cref="PropertySelectionChangedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PropertySelectionChangedCommandParameterProperty = BindableProperty.Create(
        nameof(PropertySelectionChangedCommandParameter),
        typeof(object),
        typeof(PropertyGrid));

    /// <summary>
    /// Identifies the <see cref="GotFocusCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty GotFocusCommandProperty = BindableProperty.Create(
        nameof(GotFocusCommand),
        typeof(ICommand),
        typeof(PropertyGrid));

    /// <summary>
    /// Identifies the <see cref="GotFocusCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty GotFocusCommandParameterProperty = BindableProperty.Create(
        nameof(GotFocusCommandParameter),
        typeof(object),
        typeof(PropertyGrid));

    /// <summary>
    /// Identifies the <see cref="LostFocusCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandProperty = BindableProperty.Create(
        nameof(LostFocusCommand),
        typeof(ICommand),
        typeof(PropertyGrid));

    /// <summary>
    /// Identifies the <see cref="LostFocusCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandParameterProperty = BindableProperty.Create(
        nameof(LostFocusCommandParameter),
        typeof(object),
        typeof(PropertyGrid));

    /// <summary>
    /// Identifies the <see cref="KeyPressCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandProperty = BindableProperty.Create(
        nameof(KeyPressCommand),
        typeof(ICommand),
        typeof(PropertyGrid));

    /// <summary>
    /// Identifies the <see cref="KeyPressCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandParameterProperty = BindableProperty.Create(
        nameof(KeyPressCommandParameter),
        typeof(object),
        typeof(PropertyGrid));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the object to edit.
    /// </summary>
    public object? SelectedObject
    {
        get => GetValue(SelectedObjectProperty);
        set => SetValue(SelectedObjectProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the property grid is read-only.
    /// </summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// Gets or sets whether properties are grouped by category.
    /// </summary>
    public bool ShowCategories
    {
        get => (bool)GetValue(ShowCategoriesProperty);
        set => SetValue(ShowCategoriesProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the description panel is shown.
    /// </summary>
    public bool ShowDescription
    {
        get => (bool)GetValue(ShowDescriptionProperty);
        set => SetValue(ShowDescriptionProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the search box is shown.
    /// </summary>
    public bool ShowSearchBox
    {
        get => (bool)GetValue(ShowSearchBoxProperty);
        set => SetValue(ShowSearchBoxProperty, value);
    }

    /// <summary>
    /// Gets or sets the property sort mode.
    /// </summary>
    public PropertySortMode SortMode
    {
        get => (PropertySortMode)GetValue(SortModeProperty);
        set => SetValue(SortModeProperty, value);
    }

    /// <summary>
    /// Gets or sets whether all categories start expanded.
    /// </summary>
    public bool ExpandAllCategories
    {
        get => (bool)GetValue(ExpandAllCategoriesProperty);
        set => SetValue(ExpandAllCategoriesProperty, value);
    }

    /// <summary>
    /// Gets or sets the search filter text.
    /// </summary>
    public string? SearchText
    {
        get => (string?)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    /// <summary>
    /// Gets whether there is search text.
    /// </summary>
    public bool HasSearchText => !string.IsNullOrEmpty(SearchText);

    /// <summary>
    /// Gets the property categories.
    /// </summary>
    public IReadOnlyList<PropertyCategory> Categories => _categories;

    /// <summary>
    /// Gets the currently selected property.
    /// </summary>
    public PropertyItem? SelectedProperty => _selectedProperty;

    #endregion

    #region Command Properties Implementation

    /// <summary>
    /// Gets or sets the command executed when a property value changes.
    /// </summary>
    public ICommand? PropertyChangedCommand
    {
        get => (ICommand?)GetValue(PropertyChangedCommandProperty);
        set => SetValue(PropertyChangedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="PropertyChangedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? PropertyChangedCommandParameter
    {
        get => GetValue(PropertyChangedCommandParameterProperty);
        set => SetValue(PropertyChangedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed before a property value changes.
    /// </summary>
    public ICommand? PropertyChangingCommand
    {
        get => (ICommand?)GetValue(PropertyChangingCommandProperty);
        set => SetValue(PropertyChangingCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="PropertyChangingCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? PropertyChangingCommandParameter
    {
        get => GetValue(PropertyChangingCommandParameterProperty);
        set => SetValue(PropertyChangingCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when the selected object changes.
    /// </summary>
    public ICommand? SelectedObjectChangedCommand
    {
        get => (ICommand?)GetValue(SelectedObjectChangedCommandProperty);
        set => SetValue(SelectedObjectChangedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="SelectedObjectChangedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? SelectedObjectChangedCommandParameter
    {
        get => GetValue(SelectedObjectChangedCommandParameterProperty);
        set => SetValue(SelectedObjectChangedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when property selection changes.
    /// </summary>
    public ICommand? PropertySelectionChangedCommand
    {
        get => (ICommand?)GetValue(PropertySelectionChangedCommandProperty);
        set => SetValue(PropertySelectionChangedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="PropertySelectionChangedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? PropertySelectionChangedCommandParameter
    {
        get => GetValue(PropertySelectionChangedCommandParameterProperty);
        set => SetValue(PropertySelectionChangedCommandParameterProperty, value);
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
    /// Occurs when a property value changes.
    /// </summary>
    public event EventHandler<PropertyValueChangedEventArgs>? PropertyValueChanged;

    /// <summary>
    /// Occurs before a property value changes (cancelable).
    /// </summary>
    public event EventHandler<PropertyValueChangingEventArgs>? PropertyValueChanging;

    /// <summary>
    /// Occurs when the selected object changes.
    /// </summary>
    public event EventHandler<SelectedObjectChangedEventArgs>? SelectedObjectChanged;

    /// <summary>
    /// Occurs when the property selection changes.
    /// </summary>
    public event EventHandler<PropertySelectionChangedEventArgs>? PropertySelectionChanged;

    /// <inheritdoc/>
    public event EventHandler<KeyboardFocusEventArgs>? KeyboardFocusGained;

    /// <inheritdoc/>
#pragma warning disable CS0067
    public event EventHandler<KeyboardFocusEventArgs>? KeyboardFocusLost;
#pragma warning restore CS0067

    /// <inheritdoc/>
    public event EventHandler<KeyEventArgs>? KeyPressed;

    /// <inheritdoc/>
#pragma warning disable CS0067
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
        if (!IsKeyboardNavigationEnabled) return false;

        KeyPressed?.Invoke(this, e);
        if (e.Handled) return true;

        if (KeyPressCommand?.CanExecute(e) == true)
        {
            KeyPressCommand.Execute(KeyPressCommandParameter ?? e);
            if (e.Handled) return true;
        }

        switch (e.Key)
        {
            case "ArrowUp":
                SelectPreviousProperty();
                return true;
            case "ArrowDown":
                SelectNextProperty();
                return true;
            case "ArrowLeft":
                if (_selectedProperty?.IsExpandable == true && _selectedProperty.IsExpanded)
                {
                    _selectedProperty.IsExpanded = false;
                    RefreshUI();
                    return true;
                }
                break;
            case "ArrowRight":
                if (_selectedProperty?.IsExpandable == true && !_selectedProperty.IsExpanded)
                {
                    _selectedProperty.IsExpanded = true;
                    RefreshUI();
                    return true;
                }
                break;
            case "Home":
                if (_flatProperties.Count > 0)
                {
                    SelectProperty(_flatProperties[0]);
                    return true;
                }
                break;
            case "End":
                if (_flatProperties.Count > 0)
                {
                    SelectProperty(_flatProperties[^1]);
                    return true;
                }
                break;
        }

        return false;
    }

    /// <inheritdoc/>
    public IReadOnlyList<KeyboardShortcut> GetKeyboardShortcuts()
    {
        return new List<KeyboardShortcut>
        {
            new() { Key = "ArrowUp", Description = "Select previous property", Category = "Navigation" },
            new() { Key = "ArrowDown", Description = "Select next property", Category = "Navigation" },
            new() { Key = "ArrowLeft", Description = "Collapse property", Category = "Navigation" },
            new() { Key = "ArrowRight", Description = "Expand property", Category = "Navigation" },
            new() { Key = "Home", Description = "Select first property", Category = "Navigation" },
            new() { Key = "End", Description = "Select last property", Category = "Navigation" }
        };
    }

    /// <inheritdoc/>
    public new bool Focus()
    {
        if (!CanReceiveFocus) return false;
        _hasKeyboardFocus = true;
        OnPropertyChanged(nameof(HasKeyboardFocus));
        KeyboardFocusGained?.Invoke(this, new KeyboardFocusEventArgs(true));
        GotFocusCommand?.Execute(GotFocusCommandParameter ?? this);
        return true;
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyGrid"/> class.
    /// </summary>
    public PropertyGrid()
    {
        InitializeComponent();
    }

    #endregion

    #region Property Discovery

    private void DiscoverProperties()
    {
        _categories.Clear();
        _flatProperties.Clear();

        if (SelectedObject == null)
        {
            emptyLabel.IsVisible = true;
            propertiesContainer.IsVisible = false;
            return;
        }

        emptyLabel.IsVisible = false;
        propertiesContainer.IsVisible = true;

        var type = SelectedObject.GetType();
        List<PropertyItem> properties;

        if (PropertyMetadataRegistry.TryGetMetadata(type, out var metadata) && metadata != null)
        {
            // AOT path: build PropertyItems from registered metadata
            properties = BuildFromMetadata(metadata, SelectedObject);
        }
        else
        {
            // Reflection path: existing logic
            properties = BuildFromReflection(type, SelectedObject);
        }

        // Filter by search text
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLowerInvariant();
            properties = properties
                .Where(p => p.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            (p.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }

        // Sort properties
        properties = SortMode switch
        {
            PropertySortMode.Alphabetical => properties.OrderBy(p => p.DisplayName).ToList(),
            PropertySortMode.Categorized => properties.OrderBy(p => p.Category).ThenBy(p => p.DisplayName).ToList(),
            _ => properties
        };

        _flatProperties.AddRange(properties);

        // Group by category
        if (ShowCategories && SortMode == PropertySortMode.Categorized)
        {
            var groups = properties.GroupBy(p => p.Category);
            foreach (var group in groups)
            {
                var category = new PropertyCategory(group.Key, group.ToList());
                category.IsExpanded = ExpandAllCategories || _categories.Count == 0;
                _categories.Add(category);
            }
        }
        else
        {
            // Single "All Properties" category
            var category = new PropertyCategory("All Properties", properties);
            category.IsExpanded = true;
            _categories.Add(category);
        }

        // Subscribe to property changes
        foreach (var prop in _flatProperties)
        {
            prop.ValueChanging += OnPropertyValueChanging;
            prop.ValueChanged += OnPropertyValueChanged;
        }

        RefreshUI();
    }

    private static List<PropertyItem> BuildFromMetadata(IReadOnlyList<PropertyMetadataEntry> metadata, object target)
    {
        return metadata.Select(m => new PropertyItem(m, target)).ToList();
    }

    [UnconditionalSuppressMessage("AOT", "IL2026:RequiresUnreferencedCode",
        Justification = "Reflection fallback for non-AOT scenarios. Use RegisterMetadata() for AOT compatibility.")]
    [UnconditionalSuppressMessage("AOT", "IL2070:DynamicallyAccessedMembers",
        Justification = "Reflection fallback for non-AOT scenarios. Use RegisterMetadata() for AOT compatibility.")]
    private static List<PropertyItem> BuildFromReflection(Type type, object target)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<BrowsableAttribute>()?.Browsable != false)
            .Select(p => new PropertyItem(p, target))
            .ToList();
    }

    #endregion

    #region UI Building

    private void RefreshUI()
    {
        propertiesContainer.Children.Clear();

        foreach (var category in _categories)
        {
            if (ShowCategories && SortMode == PropertySortMode.Categorized)
            {
                // Add category header
                var categoryHeader = CreateCategoryHeader(category);
                propertiesContainer.Children.Add(categoryHeader);
            }

            if (!ShowCategories || SortMode != PropertySortMode.Categorized || category.IsExpanded)
            {
                foreach (var property in category.Properties)
                {
                    var propertyRow = CreatePropertyRow(property);
                    propertiesContainer.Children.Add(propertyRow);
                }
            }
        }
    }

    private View CreateCategoryHeader(PropertyCategory category)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            },
            Padding = new Thickness(8, 6),
            BackgroundColor = EffectiveHeaderBackgroundColor
        };

        var expander = new Label
        {
            Text = category.ExpanderIcon,
            FontSize = 10,
            VerticalOptions = LayoutOptions.Center,
            TextColor = EffectiveForegroundColor
        };

        var title = new Label
        {
            Text = category.Name,
            FontAttributes = HeaderFontAttributes,
            FontSize = HeaderFontSize,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(8, 0, 0, 0),
            TextColor = EffectiveHeaderTextColor
        };

        grid.Add(expander, 0, 0);
        grid.Add(title, 1, 0);

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) =>
        {
            category.ToggleExpanded();
            expander.Text = category.ExpanderIcon;
            RefreshUI();
        };
        grid.GestureRecognizers.Add(tapGesture);

        return grid;
    }

    private View CreatePropertyRow(PropertyItem property)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(new GridLength(0.4, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(0.6, GridUnitType.Star))
            },
            Padding = new Thickness(12, 6),
            BackgroundColor = property.IsSelected
                ? MauiControlsExtrasTheme.Current.SelectedBackgroundColor
                : Colors.Transparent
        };

        // Property name
        var nameLabel = new Label
        {
            Text = property.DisplayName,
            FontSize = 13,
            VerticalOptions = LayoutOptions.Center,
            TextColor = property.IsReadOnly || IsReadOnly
                ? EffectiveDisabledColor
                : EffectiveForegroundColor
        };
        grid.Add(nameLabel, 0, 0);

        // Property editor
        var editor = CreateEditor(property);
        editor.VerticalOptions = LayoutOptions.Center;
        grid.Add(editor, 1, 0);

        // Selection handling
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) => SelectProperty(property);
        grid.GestureRecognizers.Add(tapGesture);

        // Add separator
        var container = new VerticalStackLayout { Spacing = 0 };
        container.Children.Add(grid);
        container.Children.Add(new BoxView
        {
            HeightRequest = 1,
            BackgroundColor = MauiControlsExtrasTheme.GetBorderColor(),
            Opacity = 0.3
        });

        return container;
    }

    private View CreateEditor(PropertyItem property)
    {
        var isReadOnly = property.IsReadOnly || IsReadOnly;
        var type = property.PropertyType;

        // Custom editor
        if (property.EditorType != null)
        {
            var customEditor = Activator.CreateInstance(property.EditorType) as IPropertyEditor;
            if (customEditor != null)
            {
                return customEditor.CreateEditor(property);
            }
        }

        // Built-in editors
        if (type == typeof(bool))
        {
            return CreateBoolEditor(property, isReadOnly);
        }

        if (type == typeof(string))
        {
            return CreateStringEditor(property, isReadOnly);
        }

        if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
        {
            return CreateIntEditor(property, isReadOnly);
        }

        if (type == typeof(double) || type == typeof(float) || type == typeof(decimal))
        {
            return CreateDoubleEditor(property, isReadOnly);
        }

        if (type == typeof(DateTime))
        {
            return CreateDateTimeEditor(property, isReadOnly);
        }

        if (type == typeof(TimeSpan))
        {
            return CreateTimeSpanEditor(property, isReadOnly);
        }

        if (type == typeof(Color))
        {
            return CreateColorEditor(property, isReadOnly);
        }

        if (type.IsEnum)
        {
            return CreateEnumEditor(property, isReadOnly);
        }

        // Default: read-only text
        return new Label
        {
            Text = property.DisplayValue,
            FontSize = 13,
            TextColor = EffectiveForegroundColor
        };
    }

    private View CreateBoolEditor(PropertyItem property, bool isReadOnly)
    {
        var toggle = new Switch
        {
            IsToggled = property.Value is true,
            IsEnabled = !isReadOnly
        };
        toggle.Toggled += (s, e) => property.Value = e.Value;
        property.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(PropertyItem.Value))
            {
                toggle.IsToggled = property.Value is true;
            }
        };
        return toggle;
    }

    private View CreateStringEditor(PropertyItem property, bool isReadOnly)
    {
        var entry = new Entry
        {
            Text = property.Value?.ToString() ?? "",
            IsReadOnly = isReadOnly,
            BackgroundColor = Colors.Transparent
        };
        entry.TextChanged += (s, e) => property.Value = e.NewTextValue;
        property.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(PropertyItem.Value))
            {
                entry.Text = property.Value?.ToString() ?? "";
            }
        };
        return entry;
    }

    private View CreateIntEditor(PropertyItem property, bool isReadOnly)
    {
        var entry = new Entry
        {
            Text = property.Value?.ToString() ?? "",
            Keyboard = Keyboard.Numeric,
            IsReadOnly = isReadOnly,
            BackgroundColor = Colors.Transparent
        };
        entry.Unfocused += (s, e) =>
        {
            if (int.TryParse(entry.Text, out var value))
            {
                // Apply range if specified
                if (property.Minimum != null && value < Convert.ToInt32(property.Minimum))
                    value = Convert.ToInt32(property.Minimum);
                if (property.Maximum != null && value > Convert.ToInt32(property.Maximum))
                    value = Convert.ToInt32(property.Maximum);
                property.Value = value;
            }
        };
        property.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(PropertyItem.Value))
            {
                entry.Text = property.Value?.ToString() ?? "";
            }
        };
        return entry;
    }

    private View CreateDoubleEditor(PropertyItem property, bool isReadOnly)
    {
        var entry = new Entry
        {
            Text = property.Value?.ToString() ?? "",
            Keyboard = Keyboard.Numeric,
            IsReadOnly = isReadOnly,
            BackgroundColor = Colors.Transparent
        };
        entry.Unfocused += (s, e) =>
        {
            if (double.TryParse(entry.Text, out var value))
            {
                // Apply range if specified
                if (property.Minimum != null && value < Convert.ToDouble(property.Minimum))
                    value = Convert.ToDouble(property.Minimum);
                if (property.Maximum != null && value > Convert.ToDouble(property.Maximum))
                    value = Convert.ToDouble(property.Maximum);
                property.Value = value;
            }
        };
        property.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(PropertyItem.Value))
            {
                entry.Text = property.Value?.ToString() ?? "";
            }
        };
        return entry;
    }

    private View CreateDateTimeEditor(PropertyItem property, bool isReadOnly)
    {
        var picker = new DatePicker
        {
            Date = property.Value is DateTime dt ? dt : DateTime.Today,
            IsEnabled = !isReadOnly
        };
        picker.DateSelected += (s, e) => property.Value = e.NewDate;
        property.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(PropertyItem.Value) && property.Value is DateTime dt)
            {
                picker.Date = dt;
            }
        };
        return picker;
    }

    private View CreateTimeSpanEditor(PropertyItem property, bool isReadOnly)
    {
        var picker = new TimePicker
        {
            Time = property.Value is TimeSpan ts ? ts : TimeSpan.Zero,
            IsEnabled = !isReadOnly
        };
        picker.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(TimePicker.Time))
            {
                property.Value = picker.Time;
            }
        };
        property.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(PropertyItem.Value) && property.Value is TimeSpan ts)
            {
                picker.Time = ts;
            }
        };
        return picker;
    }

    private View CreateColorEditor(PropertyItem property, bool isReadOnly)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 8
        };

        var colorBox = new BoxView
        {
            Color = property.Value as Color ?? Colors.Gray,
            WidthRequest = 24,
            HeightRequest = 24,
            CornerRadius = 4
        };

        var entry = new Entry
        {
            Text = (property.Value as Color)?.ToHex() ?? "",
            IsReadOnly = isReadOnly,
            BackgroundColor = Colors.Transparent,
            FontSize = 12
        };
        entry.Unfocused += (s, e) =>
        {
            if (Color.TryParse(entry.Text, out var color))
            {
                property.Value = color;
            }
        };

        property.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(PropertyItem.Value))
            {
                colorBox.Color = property.Value as Color ?? Colors.Gray;
                entry.Text = (property.Value as Color)?.ToHex() ?? "";
            }
        };

        grid.Add(colorBox, 0, 0);
        grid.Add(entry, 1, 0);

        return grid;
    }

    private View CreateEnumEditor(PropertyItem property, bool isReadOnly)
    {
        var values = Enum.GetValues(property.PropertyType);
        var picker = new Picker
        {
            IsEnabled = !isReadOnly
        };

        foreach (var value in values)
        {
            picker.Items.Add(Enum.GetName(property.PropertyType, value) ?? value.ToString() ?? "");
        }

        if (property.Value != null)
        {
            picker.SelectedIndex = Array.IndexOf(Enum.GetValues(property.PropertyType), property.Value);
        }

        picker.SelectedIndexChanged += (s, e) =>
        {
            if (picker.SelectedIndex >= 0)
            {
                var enumValues = Enum.GetValues(property.PropertyType);
                property.Value = enumValues.GetValue(picker.SelectedIndex);
            }
        };

        property.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(PropertyItem.Value) && property.Value != null)
            {
                picker.SelectedIndex = Array.IndexOf(Enum.GetValues(property.PropertyType), property.Value);
            }
        };

        return picker;
    }

    #endregion

    #region Selection

    private void SelectProperty(PropertyItem property)
    {
        var oldProperty = _selectedProperty;
        if (oldProperty != null)
        {
            oldProperty.IsSelected = false;
        }

        _selectedProperty = property;
        property.IsSelected = true;

        // Update description panel
        descriptionTitle.Text = property.DisplayName;
        descriptionText.Text = property.Description ?? $"Type: {property.TypeName}";

        RefreshUI();

        var args = new PropertySelectionChangedEventArgs(oldProperty, property);
        PropertySelectionChanged?.Invoke(this, args);
        PropertySelectionChangedCommand?.Execute(PropertySelectionChangedCommandParameter ?? args);
    }

    private void SelectNextProperty()
    {
        if (_flatProperties.Count == 0) return;

        var currentIndex = _selectedProperty != null ? _flatProperties.IndexOf(_selectedProperty) : -1;
        var nextIndex = (currentIndex + 1) % _flatProperties.Count;
        SelectProperty(_flatProperties[nextIndex]);
    }

    private void SelectPreviousProperty()
    {
        if (_flatProperties.Count == 0) return;

        var currentIndex = _selectedProperty != null ? _flatProperties.IndexOf(_selectedProperty) : 0;
        var prevIndex = currentIndex > 0 ? currentIndex - 1 : _flatProperties.Count - 1;
        SelectProperty(_flatProperties[prevIndex]);
    }

    #endregion

    #region Event Handlers

    private void OnPropertyValueChanging(object? sender, PropertyValueChangingEventArgs e)
    {
        PropertyValueChanging?.Invoke(this, e);
        if (PropertyChangingCommand?.CanExecute(e) == true)
        {
            PropertyChangingCommand.Execute(PropertyChangingCommandParameter ?? e);
        }
    }

    private void OnPropertyValueChanged(object? sender, PropertyValueChangedEventArgs e)
    {
        PropertyValueChanged?.Invoke(this, e);
        PropertyChangedCommand?.Execute(PropertyChangedCommandParameter ?? e);
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasSearchText));
        DiscoverProperties();
    }

    private void OnClearSearchClicked(object? sender, EventArgs e)
    {
        SearchText = null;
    }

    #endregion

    #region Property Changed Handlers

    private static void OnSelectedObjectChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PropertyGrid grid)
        {
            var args = new SelectedObjectChangedEventArgs(oldValue, newValue);
            grid.SelectedObjectChanged?.Invoke(grid, args);
            grid.SelectedObjectChangedCommand?.Execute(grid.SelectedObjectChangedCommandParameter ?? args);
            grid.DiscoverProperties();
        }
    }

    private static void OnIsReadOnlyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PropertyGrid grid)
        {
            grid.RefreshUI();
        }
    }

    private static void OnShowCategoriesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PropertyGrid grid)
        {
            grid.DiscoverProperties();
        }
    }

    private static void OnSortModeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PropertyGrid grid)
        {
            grid.DiscoverProperties();
        }
    }

    private static void OnSearchTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PropertyGrid grid)
        {
            grid.OnPropertyChanged(nameof(HasSearchText));
            grid.DiscoverProperties();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Refreshes the property grid from the current object.
    /// </summary>
    public void Refresh()
    {
        DiscoverProperties();
    }

    /// <summary>
    /// Expands all categories.
    /// </summary>
    public void ExpandAll()
    {
        foreach (var category in _categories)
        {
            category.IsExpanded = true;
        }
        RefreshUI();
    }

    /// <summary>
    /// Collapses all categories.
    /// </summary>
    public void CollapseAll()
    {
        foreach (var category in _categories)
        {
            category.IsExpanded = false;
        }
        RefreshUI();
    }

    #endregion
}
