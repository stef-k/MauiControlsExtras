# Mouse Interaction

All MAUI Controls Extras controls support comprehensive mouse interaction for desktop platforms.

## Standard Mouse Interactions

| Action | Effect |
|--------|--------|
| Click | Select item, set focus |
| Double-click | Activate item (edit, expand, etc.) |
| Right-click | Show context menu |
| Mouse wheel | Scroll content |
| Hover | Show visual feedback, tooltips |
| Drag | Select range, resize, reorder |

## Control-Specific Interactions

### DataGridView

| Action | Effect |
|--------|--------|
| Click cell | Select cell/row |
| Double-click cell | Begin editing |
| Click column header | Sort by column |
| Drag column header | Reorder columns |
| Drag column border | Resize column |
| Click row header | Select entire row |
| Drag to select | Select cell range |
| Right-click | Show context menu |
| Ctrl+Click | Toggle row selection |
| Shift+Click | Extend selection to clicked row |
| Scroll wheel | Scroll vertically |
| Shift+Scroll | Scroll horizontally |

### TreeView

| Action | Effect |
|--------|--------|
| Click node | Select node |
| Double-click node | Expand/collapse node |
| Click expand arrow | Toggle node expansion |
| Right-click | Show context menu |
| Drag node | Reorder (if enabled) |
| Hover | Highlight node |

### ComboBox / MultiSelectComboBox

| Action | Effect |
|--------|--------|
| Click control | Toggle dropdown |
| Click item | Select item (and close dropdown) |
| Click checkbox | Toggle item selection (MultiSelect) |
| Scroll in dropdown | Navigate long lists |
| Hover item | Highlight item |
| Click clear button | Clear selection |

### NumericUpDown

| Action | Effect |
|--------|--------|
| Click up button | Increment value |
| Click down button | Decrement value |
| Hold button | Repeat increment/decrement |
| Double-click value | Select all for editing |
| Scroll wheel | Increment/decrement value |

### RangeSlider

| Action | Effect |
|--------|--------|
| Drag thumb | Move thumb position |
| Click track | Move nearest thumb to position |
| Double-click track | Move thumb to exact position |
| Scroll wheel | Adjust focused thumb |

### Rating

| Action | Effect |
|--------|--------|
| Click star | Set rating to that value |
| Hover star | Preview rating |
| Drag across stars | Adjust rating smoothly |

### TokenEntry

| Action | Effect |
|--------|--------|
| Click token | Select token |
| Click token X | Remove token |
| Double-click token | Edit token |
| Click input area | Focus for typing |

### MaskedEntry

| Action | Effect |
|--------|--------|
| Click position | Set cursor position |
| Double-click | Select current section |
| Triple-click | Select all |

## Context Menus

Right-click (or Ctrl+Click on macOS) shows context-sensitive menus:

### DataGridView Context Menu

- Copy (Ctrl+C)
- Cut (Ctrl+X)
- Paste (Ctrl+V)
- Delete (Del)
- Select All (Ctrl+A)
- Expand All / Collapse All (for grouped data)

### TreeView Context Menu

- Expand
- Collapse
- Expand All
- Collapse All
- Copy
- Delete

### Text Controls Context Menu

- Cut
- Copy
- Paste
- Select All
- Undo
- Redo

## Hover Effects

Controls provide visual feedback on hover:

- **Highlighted state** - Row/item background color changes
- **Tooltips** - Additional information displayed
- **Cursor changes** - Indicates available actions (resize, etc.)
- **Button states** - Hover state on clickable elements

## Drag Operations

### Selection Dragging

```xml
<extras:DataGridView
    SelectionMode="Multiple"
    DragSelectEnabled="True" />
```

### Column Reordering

```xml
<extras:DataGridView
    CanUserReorderColumns="True" />
```

### Column Resizing

```xml
<extras:DataGridView
    CanUserResizeColumns="True" />
```

## Handling Mouse Events

### Mouse Event Commands

```xml
<extras:DataGridView
    MouseDoubleClickCommand="{Binding EditItemCommand}"
    MouseRightClickCommand="{Binding ShowOptionsCommand}" />
```

### Code-Behind Events

```csharp
myDataGrid.MouseDoubleClick += (sender, e) =>
{
    var clickedItem = e.Item;
    var clickedColumn = e.Column;
    OpenEditDialog(clickedItem);
};

myDataGrid.ContextMenuOpening += (sender, e) =>
{
    // Customize context menu based on clicked item
    if (e.Item is ReadOnlyRecord)
    {
        e.Actions.RemoveAll(a => a.Label == "Delete");
    }
};
```

## Scroll Wheel Support

Most scrollable controls support mouse wheel:

| Control | Wheel Action |
|---------|--------------|
| DataGridView | Scroll rows, Shift+Wheel scrolls columns |
| TreeView | Scroll nodes |
| ComboBox dropdown | Scroll items |
| NumericUpDown | Increment/decrement value |
| RangeSlider | Adjust focused thumb |
| Rating | Adjust rating |

### Customizing Scroll Behavior

```csharp
myControl.MouseWheelScrolled += (sender, e) =>
{
    if (e.Delta > 0)
        myControl.ScrollUp();
    else
        myControl.ScrollDown();

    e.Handled = true;
};
```

## Cursor Styles

Controls change cursor to indicate available actions:

| Cursor | Meaning |
|--------|---------|
| Arrow | Default, clickable |
| Hand | Hyperlink or drag source |
| I-Beam | Text input |
| Resize (horizontal) | Column resize available |
| Resize (vertical) | Row resize available |
| Cross | Selection drag |
| Wait | Loading data |
| Not-allowed | Action not permitted |

## Touch vs. Mouse

On touch-enabled devices, controls adapt:

- Long-press replaces right-click for context menus
- Touch targets are larger
- Drag gestures work for scrolling
- Pinch-to-zoom where applicable (Rating, etc.)
