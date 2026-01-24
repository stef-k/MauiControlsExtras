using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows.Input;
using MauiControlsExtras.Base;

namespace MauiControlsExtras.Controls;

/// <summary>
/// Selection mode for the data grid.
/// </summary>
public enum DataGridSelectionMode
{
    /// <summary>
    /// No selection allowed.
    /// </summary>
    None,

    /// <summary>
    /// Single row selection.
    /// </summary>
    Single,

    /// <summary>
    /// Multiple row selection.
    /// </summary>
    Multiple
}

/// <summary>
/// Event arguments for cell events.
/// </summary>
public class DataGridCellEventArgs : EventArgs
{
    /// <summary>
    /// Gets the row item.
    /// </summary>
    public object Item { get; }

    /// <summary>
    /// Gets the column.
    /// </summary>
    public DataGridColumn Column { get; }

    /// <summary>
    /// Gets the row index.
    /// </summary>
    public int RowIndex { get; }

    /// <summary>
    /// Gets the column index.
    /// </summary>
    public int ColumnIndex { get; }

    /// <summary>
    /// Initializes a new instance of DataGridCellEventArgs.
    /// </summary>
    public DataGridCellEventArgs(object item, DataGridColumn column, int rowIndex, int columnIndex)
    {
        Item = item;
        Column = column;
        RowIndex = rowIndex;
        ColumnIndex = columnIndex;
    }
}

/// <summary>
/// Event arguments for sorting events.
/// </summary>
public class DataGridSortingEventArgs : EventArgs
{
    /// <summary>
    /// Gets the column being sorted.
    /// </summary>
    public DataGridColumn Column { get; }

    /// <summary>
    /// Gets or sets whether the sorting should be cancelled.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes a new instance of DataGridSortingEventArgs.
    /// </summary>
    public DataGridSortingEventArgs(DataGridColumn column)
    {
        Column = column;
    }
}

/// <summary>
/// A data grid control with column sorting, selection, and data binding.
/// </summary>
public partial class DataGridView : StyledControlBase
{
    #region Private Fields

    private readonly ObservableCollection<DataGridColumn> _columns = new();
    private readonly List<object> _sortedItems = new();
    private readonly HashSet<object> _selectedItems = new();
    private bool _isUpdating;
    private DataGridColumn? _currentSortColumn;

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="ItemsSource"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource),
        typeof(IEnumerable),
        typeof(DataGridView),
        null,
        propertyChanged: OnItemsSourceChanged);

    /// <summary>
    /// Identifies the <see cref="AutoGenerateColumns"/> bindable property.
    /// </summary>
    public static readonly BindableProperty AutoGenerateColumnsProperty = BindableProperty.Create(
        nameof(AutoGenerateColumns),
        typeof(bool),
        typeof(DataGridView),
        true);

    /// <summary>
    /// Identifies the <see cref="SelectedItem"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem),
        typeof(object),
        typeof(DataGridView),
        null,
        BindingMode.TwoWay,
        propertyChanged: OnSelectedItemChanged);

    /// <summary>
    /// Identifies the <see cref="SelectionMode"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectionModeProperty = BindableProperty.Create(
        nameof(SelectionMode),
        typeof(DataGridSelectionMode),
        typeof(DataGridView),
        DataGridSelectionMode.Single);

    /// <summary>
    /// Identifies the <see cref="CanUserSort"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CanUserSortProperty = BindableProperty.Create(
        nameof(CanUserSort),
        typeof(bool),
        typeof(DataGridView),
        true);

    /// <summary>
    /// Identifies the <see cref="CanUserFilter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CanUserFilterProperty = BindableProperty.Create(
        nameof(CanUserFilter),
        typeof(bool),
        typeof(DataGridView),
        true);

    /// <summary>
    /// Identifies the <see cref="CanUserEdit"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CanUserEditProperty = BindableProperty.Create(
        nameof(CanUserEdit),
        typeof(bool),
        typeof(DataGridView),
        false);

    /// <summary>
    /// Identifies the <see cref="RowHeight"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RowHeightProperty = BindableProperty.Create(
        nameof(RowHeight),
        typeof(double),
        typeof(DataGridView),
        44.0);

    /// <summary>
    /// Identifies the <see cref="HeaderHeight"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HeaderHeightProperty = BindableProperty.Create(
        nameof(HeaderHeight),
        typeof(double),
        typeof(DataGridView),
        48.0);

    /// <summary>
    /// Identifies the <see cref="AlternatingRowColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty AlternatingRowColorProperty = BindableProperty.Create(
        nameof(AlternatingRowColor),
        typeof(Color),
        typeof(DataGridView),
        null);

    /// <summary>
    /// Identifies the <see cref="GridLinesVisibility"/> bindable property.
    /// </summary>
    public static readonly BindableProperty GridLinesVisibilityProperty = BindableProperty.Create(
        nameof(GridLinesVisibility),
        typeof(GridLinesVisibility),
        typeof(DataGridView),
        Controls.GridLinesVisibility.Both);

    /// <summary>
    /// Identifies the <see cref="GridLineColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty GridLineColorProperty = BindableProperty.Create(
        nameof(GridLineColor),
        typeof(Color),
        typeof(DataGridView),
        Color.FromArgb("#E0E0E0"));

    /// <summary>
    /// Identifies the <see cref="SelectedRowColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectedRowColorProperty = BindableProperty.Create(
        nameof(SelectedRowColor),
        typeof(Color),
        typeof(DataGridView),
        Color.FromArgb("#E3F2FD"));

    /// <summary>
    /// Identifies the <see cref="IsRefreshing"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsRefreshingProperty = BindableProperty.Create(
        nameof(IsRefreshing),
        typeof(bool),
        typeof(DataGridView),
        false);

    /// <summary>
    /// Identifies the <see cref="EmptyView"/> bindable property.
    /// </summary>
    public static readonly BindableProperty EmptyViewProperty = BindableProperty.Create(
        nameof(EmptyView),
        typeof(View),
        typeof(DataGridView),
        null,
        propertyChanged: OnEmptyViewChanged);

    #endregion

    #region Command Bindable Properties

    /// <summary>
    /// Identifies the <see cref="SelectionChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectionChangedCommandProperty = BindableProperty.Create(
        nameof(SelectionChangedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CellTappedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CellTappedCommandProperty = BindableProperty.Create(
        nameof(CellTappedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CellDoubleTappedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CellDoubleTappedCommandProperty = BindableProperty.Create(
        nameof(CellDoubleTappedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="SortingCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SortingCommandProperty = BindableProperty.Create(
        nameof(SortingCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="SortedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SortedCommandProperty = BindableProperty.Create(
        nameof(SortedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the data source.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets the columns collection.
    /// </summary>
    public ObservableCollection<DataGridColumn> Columns => _columns;

    /// <summary>
    /// Gets or sets whether to auto-generate columns from the data type.
    /// </summary>
    public bool AutoGenerateColumns
    {
        get => (bool)GetValue(AutoGenerateColumnsProperty);
        set => SetValue(AutoGenerateColumnsProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected item.
    /// </summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Gets the selected items collection.
    /// </summary>
    public IReadOnlyCollection<object> SelectedItems => _selectedItems;

    /// <summary>
    /// Gets or sets the selection mode.
    /// </summary>
    public DataGridSelectionMode SelectionMode
    {
        get => (DataGridSelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the user can sort columns.
    /// </summary>
    public bool CanUserSort
    {
        get => (bool)GetValue(CanUserSortProperty);
        set => SetValue(CanUserSortProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the user can filter columns.
    /// </summary>
    public bool CanUserFilter
    {
        get => (bool)GetValue(CanUserFilterProperty);
        set => SetValue(CanUserFilterProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the user can edit cells.
    /// </summary>
    public bool CanUserEdit
    {
        get => (bool)GetValue(CanUserEditProperty);
        set => SetValue(CanUserEditProperty, value);
    }

    /// <summary>
    /// Gets or sets the row height.
    /// </summary>
    public double RowHeight
    {
        get => (double)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the header row height.
    /// </summary>
    public double HeaderHeight
    {
        get => (double)GetValue(HeaderHeightProperty);
        set => SetValue(HeaderHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the alternating row color.
    /// </summary>
    public Color? AlternatingRowColor
    {
        get => (Color?)GetValue(AlternatingRowColorProperty);
        set => SetValue(AlternatingRowColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the grid lines visibility.
    /// </summary>
    public GridLinesVisibility GridLinesVisibility
    {
        get => (GridLinesVisibility)GetValue(GridLinesVisibilityProperty);
        set => SetValue(GridLinesVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets the grid line color.
    /// </summary>
    public Color GridLineColor
    {
        get => (Color)GetValue(GridLineColorProperty);
        set => SetValue(GridLineColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected row color.
    /// </summary>
    public Color SelectedRowColor
    {
        get => (Color)GetValue(SelectedRowColorProperty);
        set => SetValue(SelectedRowColorProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the grid is refreshing/loading.
    /// </summary>
    public bool IsRefreshing
    {
        get => (bool)GetValue(IsRefreshingProperty);
        set => SetValue(IsRefreshingProperty, value);
    }

    /// <summary>
    /// Gets or sets the view shown when there's no data.
    /// </summary>
    public View? EmptyView
    {
        get => (View?)GetValue(EmptyViewProperty);
        set => SetValue(EmptyViewProperty, value);
    }

    #endregion

    #region Command Properties

    /// <summary>
    /// Gets or sets the command to execute when selection changes.
    /// </summary>
    public ICommand? SelectionChangedCommand
    {
        get => (ICommand?)GetValue(SelectionChangedCommandProperty);
        set => SetValue(SelectionChangedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when a cell is tapped.
    /// </summary>
    public ICommand? CellTappedCommand
    {
        get => (ICommand?)GetValue(CellTappedCommandProperty);
        set => SetValue(CellTappedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when a cell is double-tapped.
    /// </summary>
    public ICommand? CellDoubleTappedCommand
    {
        get => (ICommand?)GetValue(CellDoubleTappedCommandProperty);
        set => SetValue(CellDoubleTappedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute before sorting.
    /// </summary>
    public ICommand? SortingCommand
    {
        get => (ICommand?)GetValue(SortingCommandProperty);
        set => SetValue(SortingCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute after sorting.
    /// </summary>
    public ICommand? SortedCommand
    {
        get => (ICommand?)GetValue(SortedCommandProperty);
        set => SetValue(SortedCommandProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the selection changes.
    /// </summary>
    public event EventHandler<object?>? SelectionChanged;

    /// <summary>
    /// Occurs when a cell is tapped.
    /// </summary>
    public event EventHandler<DataGridCellEventArgs>? CellTapped;

    /// <summary>
    /// Occurs when a cell is double-tapped.
    /// </summary>
    public event EventHandler<DataGridCellEventArgs>? CellDoubleTapped;

    /// <summary>
    /// Occurs before sorting.
    /// </summary>
    public event EventHandler<DataGridSortingEventArgs>? Sorting;

    /// <summary>
    /// Occurs after sorting.
    /// </summary>
    public event EventHandler<DataGridColumn>? Sorted;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the DataGridView control.
    /// </summary>
    public DataGridView()
    {
        InitializeComponent();
        _columns.CollectionChanged += OnColumnsCollectionChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Scrolls to the specified item.
    /// </summary>
    public void ScrollTo(object item)
    {
        var index = _sortedItems.IndexOf(item);
        if (index >= 0)
        {
            var y = index * RowHeight;
            dataScrollView.ScrollToAsync(0, y, true);
        }
    }

    /// <summary>
    /// Refreshes the data display.
    /// </summary>
    public void RefreshData()
    {
        BuildGrid();
    }

    /// <summary>
    /// Clears the current sort.
    /// </summary>
    public void ClearSort()
    {
        if (_currentSortColumn != null)
        {
            _currentSortColumn.SortDirection = null;
            _currentSortColumn = null;
        }
        ApplySort();
        BuildDataRows();
    }

    #endregion

    #region Private Methods - Grid Building

    private void BuildGrid()
    {
        if (_isUpdating)
            return;

        _isUpdating = true;

        // Auto-generate columns if needed
        if (AutoGenerateColumns && _columns.Count == 0 && ItemsSource != null)
        {
            GenerateColumns();
        }

        // Build header
        BuildHeader();

        // Apply sort and build rows
        ApplySort();
        BuildDataRows();

        // Show/hide empty view
        UpdateEmptyView();

        _isUpdating = false;
    }

    private void GenerateColumns()
    {
        _columns.Clear();

        var firstItem = ItemsSource?.Cast<object>().FirstOrDefault();
        if (firstItem == null)
            return;

        var type = firstItem.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Skip complex types
            if (!IsSimpleType(property.PropertyType))
                continue;

            DataGridColumn column;

            if (property.PropertyType == typeof(bool))
            {
                column = new DataGridCheckBoxColumn
                {
                    Header = FormatHeader(property.Name),
                    Binding = property.Name
                };
            }
            else if (property.PropertyType == typeof(ImageSource) ||
                     property.PropertyType == typeof(string) && property.Name.Contains("Image", StringComparison.OrdinalIgnoreCase))
            {
                column = new DataGridImageColumn
                {
                    Header = FormatHeader(property.Name),
                    Binding = property.Name
                };
            }
            else
            {
                var textColumn = new DataGridTextColumn
                {
                    Header = FormatHeader(property.Name),
                    Binding = property.Name
                };

                // Apply default formats
                if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(double) || property.PropertyType == typeof(float))
                {
                    textColumn.Format = "N2";
                    textColumn.TextAlignment = TextAlignment.End;
                }
                else if (property.PropertyType == typeof(DateTime))
                {
                    textColumn.Format = "d";
                }

                column = textColumn;
            }

            _columns.Add(column);
        }
    }

    private static bool IsSimpleType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        return underlyingType.IsPrimitive ||
               underlyingType == typeof(string) ||
               underlyingType == typeof(decimal) ||
               underlyingType == typeof(DateTime) ||
               underlyingType == typeof(DateTimeOffset) ||
               underlyingType == typeof(TimeSpan) ||
               underlyingType == typeof(Guid) ||
               underlyingType.IsEnum ||
               underlyingType == typeof(ImageSource);
    }

    private static string FormatHeader(string propertyName)
    {
        // Insert spaces before capitals
        var result = string.Concat(propertyName.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? " " + c : c.ToString()));
        return result;
    }

    private void BuildHeader()
    {
        headerGrid.Children.Clear();
        headerGrid.ColumnDefinitions.Clear();

        var visibleColumns = _columns.Where(c => c.IsVisible).ToList();

        for (int i = 0; i < visibleColumns.Count; i++)
        {
            var column = visibleColumns[i];
            var colIndex = i;

            // Add column definition
            var width = column.Width < 0 ? GridLength.Auto : new GridLength(column.Width);
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });

            // Create header cell
            var headerCell = CreateHeaderCell(column, colIndex);
            headerGrid.Children.Add(headerCell);
            Grid.SetColumn(headerCell, i);
        }
    }

    private View CreateHeaderCell(DataGridColumn column, int columnIndex)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            Padding = new Thickness(8, 0),
            BackgroundColor = Colors.Transparent
        };

        // Header text
        var headerLabel = new Label
        {
            Text = column.Header,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            TextColor = EffectiveForegroundColor
        };
        grid.Children.Add(headerLabel);
        Grid.SetColumn(headerLabel, 0);

        // Sort indicator
        var sortLabel = new Label
        {
            Text = column.SortIndicator,
            FontSize = 10,
            VerticalOptions = LayoutOptions.Center,
            TextColor = EffectiveAccentColor,
            Margin = new Thickness(4, 0, 0, 0)
        };
        sortLabel.SetBinding(Label.TextProperty, new Binding(nameof(column.SortIndicator), source: column));
        grid.Children.Add(sortLabel);
        Grid.SetColumn(sortLabel, 1);

        // Add tap gesture for sorting
        if (CanUserSort && column.CanUserSort)
        {
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) => OnHeaderTapped(column);
            grid.GestureRecognizers.Add(tapGesture);
        }

        // Add bottom border
        if (GridLinesVisibility == GridLinesVisibility.Horizontal || GridLinesVisibility == GridLinesVisibility.Both)
        {
            var border = new BoxView
            {
                Color = GridLineColor,
                HeightRequest = 1,
                VerticalOptions = LayoutOptions.End
            };
            grid.Children.Add(border);
            Grid.SetColumnSpan(border, 2);
        }

        return grid;
    }

    private void BuildDataRows()
    {
        dataGrid.Children.Clear();
        dataGrid.RowDefinitions.Clear();
        dataGrid.ColumnDefinitions.Clear();

        if (_sortedItems.Count == 0)
            return;

        var visibleColumns = _columns.Where(c => c.IsVisible).ToList();

        // Add column definitions
        for (int i = 0; i < visibleColumns.Count; i++)
        {
            var column = visibleColumns[i];
            var width = column.Width < 0 ? GridLength.Auto : new GridLength(column.Width);
            dataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });
        }

        // Add row definitions and cells
        for (int rowIndex = 0; rowIndex < _sortedItems.Count; rowIndex++)
        {
            dataGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(RowHeight) });

            var item = _sortedItems[rowIndex];
            var isSelected = _selectedItems.Contains(item);
            var isAlternate = rowIndex % 2 == 1;

            for (int colIndex = 0; colIndex < visibleColumns.Count; colIndex++)
            {
                var column = visibleColumns[colIndex];
                var cell = CreateDataCell(item, column, rowIndex, colIndex, isSelected, isAlternate);
                dataGrid.Children.Add(cell);
                Grid.SetRow(cell, rowIndex);
                Grid.SetColumn(cell, colIndex);
            }
        }
    }

    private View CreateDataCell(object item, DataGridColumn column, int rowIndex, int colIndex, bool isSelected, bool isAlternate)
    {
        var container = new Grid
        {
            Padding = new Thickness(0)
        };

        // Background color
        Color bgColor;
        if (isSelected)
        {
            bgColor = SelectedRowColor;
        }
        else if (isAlternate && AlternatingRowColor != null)
        {
            bgColor = AlternatingRowColor;
        }
        else
        {
            bgColor = Colors.Transparent;
        }
        container.BackgroundColor = bgColor;

        // Cell content
        var content = column.CreateCellContent(item);
        container.Children.Add(content);

        // Grid lines
        if (GridLinesVisibility == GridLinesVisibility.Vertical || GridLinesVisibility == GridLinesVisibility.Both)
        {
            var rightBorder = new BoxView
            {
                Color = GridLineColor,
                WidthRequest = 1,
                HorizontalOptions = LayoutOptions.End
            };
            container.Children.Add(rightBorder);
        }

        if (GridLinesVisibility == GridLinesVisibility.Horizontal || GridLinesVisibility == GridLinesVisibility.Both)
        {
            var bottomBorder = new BoxView
            {
                Color = GridLineColor,
                HeightRequest = 1,
                VerticalOptions = LayoutOptions.End
            };
            container.Children.Add(bottomBorder);
        }

        // Store metadata for event handling
        container.AutomationId = $"cell_{rowIndex}_{colIndex}";

        // Add tap gestures
        var tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
        tapGesture.Tapped += (s, e) => OnCellTapped(item, column, rowIndex, colIndex);
        container.GestureRecognizers.Add(tapGesture);

        var doubleTapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
        doubleTapGesture.Tapped += (s, e) => OnCellDoubleTapped(item, column, rowIndex, colIndex);
        container.GestureRecognizers.Add(doubleTapGesture);

        return container;
    }

    #endregion

    #region Private Methods - Sorting

    private void ApplySort()
    {
        _sortedItems.Clear();

        if (ItemsSource == null)
            return;

        var items = ItemsSource.Cast<object>().ToList();

        if (_currentSortColumn != null && _currentSortColumn.SortDirection.HasValue)
        {
            var propertyPath = _currentSortColumn.PropertyPath;
            if (!string.IsNullOrEmpty(propertyPath))
            {
                if (_currentSortColumn.SortDirection == SortDirection.Ascending)
                {
                    items = items.OrderBy(i => _currentSortColumn.GetCellValue(i)).ToList();
                }
                else
                {
                    items = items.OrderByDescending(i => _currentSortColumn.GetCellValue(i)).ToList();
                }
            }
        }

        _sortedItems.AddRange(items);
    }

    private void ToggleSort(DataGridColumn column)
    {
        // Fire sorting event
        var sortingArgs = new DataGridSortingEventArgs(column);
        Sorting?.Invoke(this, sortingArgs);

        if (SortingCommand?.CanExecute(sortingArgs) == true)
        {
            SortingCommand.Execute(sortingArgs);
        }

        if (sortingArgs.Cancel)
            return;

        // Clear previous sort
        if (_currentSortColumn != null && _currentSortColumn != column)
        {
            _currentSortColumn.SortDirection = null;
        }

        // Toggle sort direction
        column.SortDirection = column.SortDirection switch
        {
            null => SortDirection.Ascending,
            SortDirection.Ascending => SortDirection.Descending,
            SortDirection.Descending => null,
            _ => null
        };

        _currentSortColumn = column.SortDirection.HasValue ? column : null;

        // Apply sort and rebuild
        ApplySort();
        BuildDataRows();

        // Fire sorted event
        Sorted?.Invoke(this, column);

        if (SortedCommand?.CanExecute(column) == true)
        {
            SortedCommand.Execute(column);
        }
    }

    #endregion

    #region Private Methods - Selection

    private void SelectItem(object item)
    {
        if (SelectionMode == DataGridSelectionMode.None)
            return;

        if (SelectionMode == DataGridSelectionMode.Single)
        {
            _selectedItems.Clear();
            _selectedItems.Add(item);
            SelectedItem = item;
        }
        else if (SelectionMode == DataGridSelectionMode.Multiple)
        {
            if (_selectedItems.Contains(item))
            {
                _selectedItems.Remove(item);
            }
            else
            {
                _selectedItems.Add(item);
            }
            SelectedItem = _selectedItems.FirstOrDefault();
        }

        BuildDataRows();
        RaiseSelectionChanged();
    }

    private void UpdateEmptyView()
    {
        var hasData = _sortedItems.Count > 0;
        emptyViewContainer.IsVisible = !hasData && EmptyView != null;
        dataScrollView.IsVisible = hasData;
    }

    #endregion

    #region Event Handlers

    private void OnHeaderTapped(DataGridColumn column)
    {
        ToggleSort(column);
    }

    private void OnCellTapped(object item, DataGridColumn column, int rowIndex, int colIndex)
    {
        SelectItem(item);

        var args = new DataGridCellEventArgs(item, column, rowIndex, colIndex);
        CellTapped?.Invoke(this, args);

        if (CellTappedCommand?.CanExecute(args) == true)
        {
            CellTappedCommand.Execute(args);
        }
    }

    private void OnCellDoubleTapped(object item, DataGridColumn column, int rowIndex, int colIndex)
    {
        var args = new DataGridCellEventArgs(item, column, rowIndex, colIndex);
        CellDoubleTapped?.Invoke(this, args);

        if (CellDoubleTappedCommand?.CanExecute(args) == true)
        {
            CellDoubleTappedCommand.Execute(args);
        }
    }

    private void OnDataScrollViewScrolled(object? sender, ScrolledEventArgs e)
    {
        // Sync header scroll with data scroll
        headerScrollView.ScrollToAsync(e.ScrollX, 0, false);
    }

    private void OnColumnsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        BuildGrid();
    }

    #endregion

    #region Property Changed Handlers

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGridView grid)
        {
            // Unsubscribe from old collection
            if (oldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= grid.OnItemsSourceCollectionChanged;
            }

            // Subscribe to new collection
            if (newValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += grid.OnItemsSourceCollectionChanged;
            }

            grid.BuildGrid();
        }
    }

    private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        BuildGrid();
    }

    private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGridView grid && newValue != null && !grid._isUpdating)
        {
            grid._selectedItems.Clear();
            grid._selectedItems.Add(newValue);
            grid.BuildDataRows();
        }
    }

    private static void OnEmptyViewChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGridView grid)
        {
            grid.emptyViewContainer.Content = newValue as View;
            grid.UpdateEmptyView();
        }
    }

    #endregion

    #region Event Raising Methods

    private void RaiseSelectionChanged()
    {
        SelectionChanged?.Invoke(this, SelectedItem);

        if (SelectionChangedCommand?.CanExecute(SelectedItem) == true)
        {
            SelectionChangedCommand.Execute(SelectedItem);
        }
    }

    #endregion
}
