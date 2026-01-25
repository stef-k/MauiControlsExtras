# Keyboard Navigation

All MAUI Controls Extras controls support comprehensive keyboard navigation for desktop platforms.

## Global Shortcuts

These shortcuts work across all controls:

| Key | Action |
|-----|--------|
| Tab | Move to next focusable element |
| Shift+Tab | Move to previous focusable element |
| Enter | Activate/confirm current selection |
| Escape | Cancel/close/deselect |

## Platform Differences

| Action | Windows | macOS |
|--------|---------|-------|
| Copy | Ctrl+C | ⌘+C |
| Cut | Ctrl+X | ⌘+X |
| Paste | Ctrl+V | ⌘+V |
| Select All | Ctrl+A | ⌘+A |
| Undo | Ctrl+Z | ⌘+Z |
| Redo | Ctrl+Y | ⌘+Shift+Z |

## Control-Specific Shortcuts

### DataGridView

| Key | Action |
|-----|--------|
| ↑ / ↓ | Move to previous/next row |
| ← / → | Move to previous/next column |
| Home | Move to first cell in row |
| End | Move to last cell in row |
| Ctrl+Home | Move to first cell in grid |
| Ctrl+End | Move to last cell in grid |
| Page Up | Scroll up one page |
| Page Down | Scroll down one page |
| F2 | Begin editing current cell |
| Enter | Commit edit and move down |
| Tab | Commit edit and move right |
| Escape | Cancel edit |
| Delete | Delete selected row(s) |
| Ctrl+C | Copy selected cells/rows |
| Ctrl+V | Paste from clipboard |
| Space | Toggle row selection |
| Shift+↑/↓ | Extend selection |
| Ctrl+Click | Toggle individual row selection |

### TreeView

| Key | Action |
|-----|--------|
| ↑ / ↓ | Move to previous/next visible node |
| ← | Collapse node or move to parent |
| → | Expand node or move to first child |
| Home | Move to first node |
| End | Move to last visible node |
| Enter / Space | Toggle expand/collapse |
| * (Asterisk) | Expand all children |
| - (Minus) | Collapse current node |
| + (Plus) | Expand current node |

### ComboBox / MultiSelectComboBox

| Key | Action |
|-----|--------|
| Alt+↓ | Open dropdown |
| Escape | Close dropdown |
| ↑ / ↓ | Navigate items in dropdown |
| Enter | Select highlighted item |
| Home | Move to first item |
| End | Move to last item |
| Type characters | Filter/search items |
| Backspace | Clear search text |
| Space | Toggle checkbox (MultiSelect only) |

### NumericUpDown

| Key | Action |
|-----|--------|
| ↑ | Increment value |
| ↓ | Decrement value |
| Page Up | Increment by large step |
| Page Down | Decrement by large step |
| Home | Set to minimum value |
| End | Set to maximum value |
| Enter | Commit entered value |
| Escape | Revert to previous value |

### RangeSlider

| Key | Action |
|-----|--------|
| ← / → | Move focused thumb |
| Shift+← / → | Move by larger step |
| Home | Move thumb to minimum |
| End | Move thumb to maximum |
| Tab | Switch between thumbs |

### Rating

| Key | Action |
|-----|--------|
| ← / → | Decrease/increase rating |
| Home | Set to minimum rating |
| End | Set to maximum rating |
| 1-5 | Set specific rating value |

### TokenEntry

| Key | Action |
|-----|--------|
| Enter | Add current text as token |
| Backspace | Remove last token (when input empty) |
| Delete | Remove selected token |
| ← / → | Navigate between tokens |
| Escape | Clear input / deselect token |
| Ctrl+A | Select all tokens |

### MaskedEntry

| Key | Action |
|-----|--------|
| ← / → | Move cursor within mask |
| Home / End | Move to start/end of input |
| Delete / Backspace | Clear character at position |
| Tab | Move to next mask section |

## Implementing Keyboard Support

Controls implement the `IKeyboardNavigable` interface:

```csharp
public interface IKeyboardNavigable
{
    bool CanReceiveFocus { get; }
    bool IsKeyboardNavigationEnabled { get; set; }
    bool HasKeyboardFocus { get; }
    bool HandleKeyPress(KeyEventArgs e);
    IReadOnlyList<KeyboardShortcut> GetKeyboardShortcuts();
    bool Focus();
}
```

### Handling Key Events

```csharp
// Subscribe to key events
myControl.KeyPressed += (sender, e) =>
{
    if (e.Key == "Enter" && e.IsPlatformCommandPressed)
    {
        // Handle Ctrl+Enter (Windows) or Cmd+Enter (macOS)
        SubmitForm();
        e.Handled = true;
    }
};
```

### Querying Available Shortcuts

```csharp
// Get all keyboard shortcuts for a control
var shortcuts = myControl.GetKeyboardShortcuts();

foreach (var shortcut in shortcuts)
{
    Console.WriteLine($"{shortcut.DisplayString}: {shortcut.Description}");
}
```

## Accessibility Considerations

- All keyboard navigation supports screen readers
- Focus indicators are visible for all focusable elements
- Logical tab order follows visual layout
- Keyboard shortcuts have ARIA labels where applicable

## Customization

### Disabling Keyboard Navigation

```csharp
myControl.IsKeyboardNavigationEnabled = false;
```

### Custom Key Bindings

Use the `KeyPressCommand` to handle keys in your view model:

```xml
<extras:DataGridView
    KeyPressCommand="{Binding HandleKeyCommand}"
    KeyPressCommandParameter="{Binding SelectedItem}" />
```

```csharp
public ICommand HandleKeyCommand => new Command<KeyEventArgs>(e =>
{
    if (e.Key == "Delete")
    {
        DeleteSelectedItem();
        e.Handled = true;
    }
});
```
