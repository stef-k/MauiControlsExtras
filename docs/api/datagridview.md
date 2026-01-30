# DataGridView API Reference

Complete API reference for the DataGridView control.

## Class Definition

```csharp
public class DataGridView : StyledControlBase, IUndoRedo, IClipboardSupport, IKeyboardNavigable, IContextMenuSupport
```

## Bindable Properties

### Data Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| ItemsSource | IEnumerable | null | Data source for the grid |
| Columns | ObservableCollection<DataGridColumn> | | Column definitions |
| SelectedItem | object | null | Currently selected item |
| SelectedItems | IList | | Selected items (multi-select) |
| SelectedIndex | int | -1 | Selected row index |

### Editing Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| CanUserEdit | bool | false | Allow cell editing |
| CanUserAddRows | bool | false | Allow adding new rows |
| CanUserDeleteRows | bool | false | Allow deleting rows |
| EditMode | DataGridEditMode | SingleClick | Trigger for edit mode |
| IsReadOnly | bool | false | Make entire grid read-only |

### Sorting and Filtering

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| EnableSorting | bool | false | Allow column sorting |
| EnableFiltering | bool | false | Show filter UI |
| SortColumn | string | null | Current sort column |
| SortDirection | SortDirection | None | Current sort direction |
| FilterExpression | string | null | Current filter |

### Selection Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| SelectionMode | DataGridSelectionMode | Single | Selection behavior |
| SelectionUnit | DataGridSelectionUnit | FullRow | What can be selected |

### Visual Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| RowHeight | double | 44 | Height of data rows |
| HeaderHeight | double | 48 | Height of header row |
| ShowGridLines | bool | true | Show cell borders |
| AlternatingRowBackground | Color | null | Alternate row color |
| ShowRowNumbers | bool | false | Display row numbers |

### Grouping Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| GroupByColumn | string | null | Column to group by |
| ShowGroupHeaders | bool | true | Show group headers |
| IsGroupExpandedByDefault | bool | true | Expand groups initially |

### Virtualization Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| EnableVirtualization | bool | false | Enable virtual scrolling |
| VirtualizationBufferSize | int | 5 | Buffer rows above/below |

### Context Menu Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| ShowDefaultContextMenu | bool | true | Show built-in context menu items (Copy, Cut, Paste, Undo, Redo, Delete) |
| ContextMenuItems | ContextMenuItemCollection | | Custom context menu items defined in XAML or code |
| ContextMenuTemplate | DataTemplate | null | Custom template for full control over context menu appearance |

## Events

### Selection Events

```csharp
event EventHandler<DataGridSelectionChangedEventArgs> SelectionChanged;
```

### Editing Events

```csharp
event EventHandler<DataGridCellEditEventArgs> CellEditStarted;
event EventHandler<DataGridCellEditEventArgs> CellEditEnded;
event EventHandler<DataGridRowEditEventArgs> RowEditEnded;
```

### Sort/Filter Events

```csharp
event EventHandler<DataGridSortEventArgs> SortChanged;
event EventHandler<DataGridFilterEventArgs> FilterChanged;
```

### Context Menu Events

```csharp
event EventHandler<DataGridContextMenuEventArgs> ContextMenuOpening;
event EventHandler<DataGridContextMenuOpeningEventArgs> ContextMenuItemsOpening;
```

The `ContextMenuItemsOpening` event provides access to the `ContextMenuItemCollection` for dynamic customization before the menu is displayed.

## Commands

| Command | Parameter Type | Description |
|---------|---------------|-------------|
| SelectionChangedCommand | object | On selection change |
| CellEditEndedCommand | DataGridCellEditEventArgs | On cell edit complete |
| RowEditEndedCommand | DataGridRowEditEventArgs | On row edit complete |
| SortChangedCommand | DataGridSortEventArgs | On sort change |
| DeleteRowCommand | object | On row delete |
| ContextMenuOpeningCommand | DataGridContextMenuOpeningEventArgs | On context menu opening |

## Methods

### Data Methods

```csharp
void ScrollToItem(object item);
void ScrollToIndex(int index);
void RefreshData();
```

### Selection Methods

```csharp
void SelectAll();
void ClearSelection();
void SelectRow(int index);
void SelectRows(IEnumerable<int> indices);
```

### Editing Methods

```csharp
void BeginEdit(int rowIndex, int columnIndex);
void EndEdit(bool commit = true);
void CancelEdit();
```

### Context Menu Methods

```csharp
void ShowContextMenu(object? item, DataGridColumn? column, int rowIndex, int columnIndex);
Task ShowContextMenuAsync(object? item, DataGridColumn? column, int rowIndex, int columnIndex, Point? position, View? anchorView = null);
```

### Export Methods

```csharp
Task<string> ExportAsync(DataGridExportOptions options);
Task<bool> PrintAsync(DataGridPrintOptions options);
```

### Clipboard Methods (IClipboardSupport)

```csharp
void Copy();
void Cut();
void Paste();
object GetClipboardContent();
```

### Undo/Redo Methods (IUndoRedo)

```csharp
bool Undo();
bool Redo();
void ClearUndoHistory();
void BeginBatchOperation(string description);
void EndBatchOperation();
void CancelBatchOperation();
```

## Column Types

### DataGridTextColumn

```csharp
public class DataGridTextColumn : DataGridColumn
{
    string MaxLength { get; set; }
    TextAlignment TextAlignment { get; set; }
}
```

### DataGridNumericColumn

```csharp
public class DataGridNumericColumn : DataGridColumn
{
    string Format { get; set; }
    double Minimum { get; set; }
    double Maximum { get; set; }
    double Increment { get; set; }
}
```

### DataGridCheckBoxColumn

```csharp
public class DataGridCheckBoxColumn : DataGridColumn
{
    bool AllowIndeterminate { get; set; }
}
```

### DataGridComboBoxColumn

Uses the library's custom ComboBox control with search/filtering support.

```csharp
public class DataGridComboBoxColumn : DataGridColumn
{
    string Binding { get; set; }              // Property path for selected value
    IEnumerable ItemsSource { get; set; }     // Dropdown items collection
    string DisplayMemberPath { get; set; }    // Property to display in dropdown
    string SelectedValuePath { get; set; }    // Property for selected value
    string Placeholder { get; set; }          // Search placeholder text (default: "Select...")
    int VisibleItemCount { get; set; }        // Visible items in dropdown (default: 6)
}
```

**Features:**
- Built-in search/filtering of dropdown items
- Keyboard navigation support
- Theme-aware styling

### DataGridDatePickerColumn

```csharp
public class DataGridDatePickerColumn : DataGridColumn
{
    string Binding { get; set; }
    string Format { get; set; }
    DateTime? MinimumDate { get; set; }
    DateTime? MaximumDate { get; set; }
}
```

### DataGridTimePickerColumn

```csharp
public class DataGridTimePickerColumn : DataGridColumn
{
    string Binding { get; set; }
    string Format { get; set; }
}
```

## Enumerations

### DataGridSelectionMode

```csharp
public enum DataGridSelectionMode
{
    None,
    Single,
    Multiple,
    Extended
}
```

### DataGridSelectionUnit

```csharp
public enum DataGridSelectionUnit
{
    Cell,
    FullRow,
    CellOrRow
}
```

### DataGridEditMode

```csharp
public enum DataGridEditMode
{
    SingleClick,
    DoubleClick,
    F2Key
}
```

### SortDirection

```csharp
public enum SortDirection
{
    None,
    Ascending,
    Descending
}
```

## Event Args

### DataGridCellEditEventArgs

```csharp
public class DataGridCellEditEventArgs : EventArgs
{
    object Item { get; }
    DataGridColumn Column { get; }
    int RowIndex { get; }
    int ColumnIndex { get; }
    object OldValue { get; }
    object NewValue { get; }
    bool Cancel { get; set; }
}
```

### DataGridContextMenuEventArgs

```csharp
public class DataGridContextMenuEventArgs : EventArgs
{
    object Item { get; }
    DataGridColumn Column { get; }
    int RowIndex { get; }
    int ColumnIndex { get; }
    bool Cancel { get; set; }
    List<DataGridContextMenuAction> Actions { get; }
}
```

### DataGridContextMenuOpeningEventArgs

Extended event args for the new context menu system with full cell information.

```csharp
public class DataGridContextMenuOpeningEventArgs : ContextMenuOpeningEventArgs
{
    object? Item { get; }              // Data item at the context menu location
    DataGridColumn? Column { get; }    // Column at the context menu location
    int RowIndex { get; }              // Row index (-1 if not on a row)
    int ColumnIndex { get; }           // Column index (-1 if not on a column)
    object? CellValue { get; }         // Cell value at the location
    bool IsHeader { get; }             // True if triggered on header
    bool IsDataCell { get; }           // True if triggered on data cell
}
```
