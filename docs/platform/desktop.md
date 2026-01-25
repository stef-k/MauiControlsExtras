# Desktop Platform Support

MAUI Controls Extras provides full support for desktop platforms (Windows and macOS), including comprehensive keyboard navigation and mouse interaction.

## Supported Platforms

| Platform | Keyboard | Mouse | Touch |
|----------|----------|-------|-------|
| Windows | ✅ Full | ✅ Full | ✅ Full |
| macOS Catalyst | ✅ Full | ✅ Full | ✅ Trackpad |
| iOS | N/A | N/A | ✅ Full |
| Android | Limited | Limited | ✅ Full |

## Key Features

### Keyboard Navigation

All controls support standard keyboard navigation patterns:

- **Tab/Shift+Tab** - Move focus between controls
- **Arrow Keys** - Navigate within controls (items, cells, nodes)
- **Enter/Space** - Activate/select the current item
- **Escape** - Cancel operations, close popups
- **Home/End** - Navigate to first/last item
- **Page Up/Down** - Page-based navigation in lists

[See full keyboard documentation](keyboard.md)

### Mouse Interaction

All controls provide intuitive mouse interactions:

- **Click** - Select items, set focus
- **Double-click** - Activate items (begin edit, expand nodes)
- **Right-click** - Show context menus
- **Mouse Wheel** - Scroll content
- **Hover** - Visual feedback (tooltips, highlights)
- **Drag** - Selection, resizing, reordering

[See full mouse documentation](mouse.md)

### Clipboard Operations

Controls that support content selection implement clipboard operations:

| Shortcut | Action | macOS Equivalent |
|----------|--------|------------------|
| Ctrl+C | Copy | ⌘+C |
| Ctrl+X | Cut | ⌘+X |
| Ctrl+V | Paste | ⌘+V |
| Ctrl+A | Select All | ⌘+A |

### Undo/Redo

Controls with editing capabilities support undo/redo:

| Shortcut | Action | macOS Equivalent |
|----------|--------|------------------|
| Ctrl+Z | Undo | ⌘+Z |
| Ctrl+Y | Redo | ⌘+Shift+Z |

## Control-Specific Features

### DataGridView

The DataGridView provides extensive desktop support:

- **Cell Navigation** - Arrow keys move between cells
- **Range Selection** - Shift+Click, Shift+Arrow for selecting ranges
- **Column Resizing** - Drag column borders
- **In-Cell Editing** - F2 to enter edit mode, Enter to commit
- **Context Menus** - Right-click for copy, paste, delete options
- **Virtual Scrolling** - Efficient handling of large datasets

### TreeView

- **Expand/Collapse** - Left/Right arrows, Space, Enter
- **Multi-level Navigation** - Arrow keys navigate tree hierarchy
- **Keyboard Selection** - Shift+Arrow for multi-select

### ComboBox Controls

- **Dropdown Toggle** - Alt+Down to open, Escape to close
- **Type-Ahead** - Start typing to filter/select
- **Quick Selection** - Arrow keys to navigate, Enter to select

## Accessibility

Desktop keyboard support enhances accessibility:

- All interactive elements are focusable via Tab
- Focus indicators are clearly visible
- Screen reader compatible labels
- High contrast theme support

## Configuration

### Enabling/Disabling Keyboard Navigation

```csharp
// Disable keyboard navigation for a specific control
myDataGrid.IsKeyboardNavigationEnabled = false;
```

### Custom Key Handlers

```csharp
// Handle custom key combinations
myControl.KeyPressed += (sender, e) =>
{
    if (e.Key == "F5" && !e.Handled)
    {
        RefreshData();
        e.Handled = true;
    }
};
```

### Custom Context Menus

```csharp
// Listen for context menu events
myDataGrid.ContextMenuOpening += (sender, e) =>
{
    // Add custom menu items
    e.Actions.Add(new DataGridContextMenuAction
    {
        Label = "Custom Action",
        Handler = () => DoCustomAction(e.Item)
    });
};
```

## Best Practices

1. **Test on Desktop** - Always test your app on Windows and/or macOS
2. **Provide Keyboard Alternatives** - Ensure all mouse actions have keyboard equivalents
3. **Respect Platform Conventions** - Use Cmd on macOS, Ctrl on Windows
4. **Show Shortcuts** - Display keyboard shortcuts in tooltips and menus
5. **Focus Management** - Ensure logical focus order in your layouts
