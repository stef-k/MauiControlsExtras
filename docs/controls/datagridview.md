# DataGridView

A feature-rich data grid control for displaying and editing tabular data.

## Features

- **Sorting** - Click column headers to sort
- **Filtering** - Filter data by column values
- **Grouping** - Group rows by column values
- **Editing** - In-cell editing with validation
- **Selection** - Single, multiple, and range selection
- **Virtual Scrolling** - Efficient handling of large datasets
- **Column Types** - Text, numeric, checkbox, combo box, date picker, time picker columns
- **Export** - Export to CSV, TSV, JSON formats
- **Print** - Print with customizable options
- **Undo/Redo** - Full undo/redo support for edits
- **Clipboard** - Copy/paste support
- **Keyboard Navigation** - Full keyboard support for desktop

## Basic Usage

```xml
<extras:DataGridView ItemsSource="{Binding Employees}">
    <extras:DataGridView.Columns>
        <extras:DataGridTextColumn Header="Name" Binding="Name" />
        <extras:DataGridTextColumn Header="Department" Binding="Department" />
        <extras:DataGridNumericColumn Header="Salary" Binding="Salary" Format="C0" />
        <extras:DataGridCheckBoxColumn Header="Active" Binding="IsActive" />
    </extras:DataGridView.Columns>
</extras:DataGridView>
```

## Editable Grid

```xml
<extras:DataGridView
    ItemsSource="{Binding Employees}"
    CanUserEdit="True"
    CanUserAddRows="True"
    CanUserDeleteRows="True"
    CellEditEndedCommand="{Binding SaveChangesCommand}">
    ...
</extras:DataGridView>
```

## Sorting and Filtering

```xml
<extras:DataGridView
    ItemsSource="{Binding Employees}"
    EnableSorting="True"
    EnableFiltering="True"
    DefaultSortColumn="Name"
    DefaultSortDirection="Ascending">
    ...
</extras:DataGridView>
```

## Column Types

### DataGridTextColumn

```xml
<extras:DataGridTextColumn
    Header="Name"
    Binding="Name"
    Width="200"
    IsReadOnly="False"
    MaxLength="100" />
```

### DataGridNumericColumn

```xml
<extras:DataGridNumericColumn
    Header="Price"
    Binding="Price"
    Format="C2"
    Minimum="0"
    Maximum="10000"
    Increment="0.01" />
```

### DataGridCheckBoxColumn

```xml
<extras:DataGridCheckBoxColumn
    Header="Active"
    Binding="IsActive" />
```

### DataGridComboBoxColumn

Uses the library's custom ComboBox control with built-in search/filtering support.

```xml
<extras:DataGridComboBoxColumn
    Header="Category"
    Binding="CategoryId"
    ItemsSource="{Binding Categories}"
    DisplayMemberPath="Name"
    SelectedValuePath="Id"
    Placeholder="Search categories..."
    VisibleItemCount="8" />
```

**Features:**
- Search/filter dropdown items by typing
- Keyboard navigation (↑/↓/Enter/Escape/Home/End)
- Customizable placeholder text
- Configurable visible item count

### DataGridDatePickerColumn

```xml
<extras:DataGridDatePickerColumn
    Header="Hire Date"
    Binding="HireDate"
    Format="d"
    MinimumDate="2020-01-01"
    MaximumDate="2030-12-31" />
```

### DataGridTimePickerColumn

```xml
<extras:DataGridTimePickerColumn
    Header="Start Time"
    Binding="StartTime"
    Format="t" />
```

## Virtual Scrolling

For large datasets, enable virtual scrolling:

```xml
<extras:DataGridView
    ItemsSource="{Binding LargeDataSet}"
    EnableVirtualization="True"
    VirtualizationBufferSize="5"
    RowHeight="44" />
```

## Selection

```xml
<extras:DataGridView
    ItemsSource="{Binding Items}"
    SelectionMode="Multiple"
    SelectedItem="{Binding SelectedItem, Mode=TwoWay}"
    SelectedItems="{Binding SelectedItems}"
    SelectionChangedCommand="{Binding HandleSelectionCommand}" />
```

## Grouping

```xml
<extras:DataGridView
    ItemsSource="{Binding Employees}"
    GroupByColumn="Department"
    ShowGroupHeaders="True"
    IsGroupExpandedByDefault="True" />
```

## Export

```csharp
// Export to CSV
var csv = await myDataGrid.ExportAsync(new DataGridExportOptions
{
    Format = ExportFormat.Csv,
    IncludeHeaders = true,
    VisibleColumnsOnly = true
});

// Export to clipboard
await myDataGrid.CopyToClipboardAsync();
```

## Print

```csharp
await myDataGrid.PrintAsync(new DataGridPrintOptions
{
    Title = "Employee Report",
    Orientation = PageOrientation.Landscape,
    IncludeGridLines = true
});
```

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| ↑ / ↓ | Move between rows |
| ← / → | Move between columns |
| Enter | Commit edit, move down |
| Tab | Commit edit, move right |
| F2 | Begin editing |
| Escape | Cancel edit |
| Delete | Delete selected rows |
| Ctrl+C | Copy selection |
| Ctrl+V | Paste |
| Ctrl+Z | Undo |
| Ctrl+Y | Redo |
| Home/End | First/last column |
| Ctrl+Home/End | First/last cell |

## Events

| Event | Description |
|-------|-------------|
| SelectionChanged | Selection has changed |
| CellEditStarted | Cell editing started |
| CellEditEnded | Cell editing completed |
| RowEditEnded | Row editing completed |
| SortChanged | Sort column/direction changed |
| FilterChanged | Filter applied/removed |
| ContextMenuOpening | Right-click menu opening |

## Commands

| Command | Description |
|---------|-------------|
| SelectionChangedCommand | Execute when selection changes |
| CellEditEndedCommand | Execute when cell edit completes |
| SortChangedCommand | Execute when sorting changes |
| DeleteRowCommand | Execute when row is deleted |

## Properties Reference

See [DataGridView API Reference](../api/datagridview.md) for complete property list.
