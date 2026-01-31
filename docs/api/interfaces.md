# Interfaces API Reference

MAUI Controls Extras provides several interfaces that controls implement for consistent functionality.

## IKeyboardNavigable

Interface for controls that support keyboard navigation.

```csharp
public interface IKeyboardNavigable
{
    // Properties
    bool CanReceiveFocus { get; }
    bool IsKeyboardNavigationEnabled { get; set; }
    bool HasKeyboardFocus { get; }

    // Methods
    bool HandleKeyPress(KeyEventArgs e);
    IReadOnlyList<KeyboardShortcut> GetKeyboardShortcuts();
    bool Focus();

    // Commands
    ICommand? GotFocusCommand { get; set; }
    ICommand? LostFocusCommand { get; set; }
    ICommand? KeyPressCommand { get; set; }

    // Events
    event EventHandler<KeyboardFocusEventArgs>? KeyboardFocusGained;
    event EventHandler<KeyboardFocusEventArgs>? KeyboardFocusLost;
    event EventHandler<KeyEventArgs>? KeyPressed;
    event EventHandler<KeyEventArgs>? KeyReleased;
}
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| CanReceiveFocus | bool | Whether the control can receive keyboard focus |
| IsKeyboardNavigationEnabled | bool | Whether keyboard navigation is enabled |
| HasKeyboardFocus | bool | Whether the control currently has keyboard focus |

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| HandleKeyPress(KeyEventArgs e) | bool | Handle a key press; returns true if handled |
| GetKeyboardShortcuts() | IReadOnlyList<KeyboardShortcut> | Get all supported shortcuts |
| Focus() | bool | Attempt to set keyboard focus |

---

## IClipboardSupport

Interface for controls that support clipboard operations.

```csharp
public interface IClipboardSupport
{
    // Properties
    bool CanCopy { get; }
    bool CanCut { get; }
    bool CanPaste { get; }

    // Methods
    void Copy();
    void Cut();
    void Paste();
    object? GetClipboardContent();

    // Commands
    ICommand? CopyCommand { get; set; }
    ICommand? CutCommand { get; set; }
    ICommand? PasteCommand { get; set; }
}
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| CanCopy | bool | Whether copy operation is available |
| CanCut | bool | Whether cut operation is available |
| CanPaste | bool | Whether paste operation is available |

### Methods

| Method | Description |
|--------|-------------|
| Copy() | Copy selection to clipboard |
| Cut() | Cut selection to clipboard |
| Paste() | Paste from clipboard |
| GetClipboardContent() | Get content that would be copied |

---

## IUndoRedo

Interface for controls that support undo/redo operations.

```csharp
public interface IUndoRedo
{
    // Properties
    bool CanUndo { get; }
    bool CanRedo { get; }
    int UndoCount { get; }
    int RedoCount { get; }
    int UndoLimit { get; set; }

    // Methods
    bool Undo();
    bool Redo();
    void ClearUndoHistory();
    string? GetUndoDescription();
    string? GetRedoDescription();
    void BeginBatchOperation(string? description = null);
    void EndBatchOperation();
    void CancelBatchOperation();

    // Commands
    ICommand? UndoCommand { get; set; }
    ICommand? RedoCommand { get; set; }
}
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| CanUndo | bool | Whether undo is available |
| CanRedo | bool | Whether redo is available |
| UndoCount | int | Number of undoable operations |
| RedoCount | int | Number of redoable operations |
| UndoLimit | int | Maximum undo history size |

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| Undo() | bool | Undo last operation; returns success |
| Redo() | bool | Redo last undone operation |
| ClearUndoHistory() | void | Clear all undo/redo history |
| GetUndoDescription() | string? | Description of next undo operation |
| GetRedoDescription() | string? | Description of next redo operation |
| BeginBatchOperation(string?) | void | Start grouping operations |
| EndBatchOperation() | void | End batch and commit as single undo |
| CancelBatchOperation() | void | Cancel batch and revert all changes |

---

## ISelectable

Interface for controls that support selection.

```csharp
public interface ISelectable
{
    // Properties
    bool HasSelection { get; }
    bool IsAllSelected { get; }
    bool SupportsMultipleSelection { get; }

    // Methods
    void SelectAll();
    void ClearSelection();
    object? GetSelection();
    void SetSelection(object? selection);

    // Commands
    ICommand? SelectAllCommand { get; set; }
    ICommand? ClearSelectionCommand { get; set; }
    ICommand? SelectionChangedCommand { get; set; }

    // Events
    event EventHandler<SelectionChangedEventArgs>? SelectionChanged;
}
```

---

## IContextMenuSupport

Interface for controls that support context menus with platform-specific native implementations.

```csharp
public interface IContextMenuSupport
{
    // Properties
    ContextMenuItemCollection ContextMenuItems { get; }
    bool ShowDefaultContextMenu { get; set; }

    // Methods
    void ShowContextMenu(Point? position = null);

    // Events
    event EventHandler<ContextMenuOpeningEventArgs>? ContextMenuOpening;
}
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| ContextMenuItems | ContextMenuItemCollection | Collection of custom menu items |
| ShowDefaultContextMenu | bool | Whether to show built-in context menu items (Copy, Paste, etc.) |

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| ShowContextMenu(Point?) | void | Programmatically shows the context menu at the specified position |

### Events

| Event | Args | Description |
|-------|------|-------------|
| ContextMenuOpening | ContextMenuOpeningEventArgs | Raised before the context menu is shown; allows customization and cancellation |

### Platform Implementations

| Platform | Native Control |
|----------|---------------|
| Windows | MenuFlyout with FontIcon support |
| macOS | UIMenu via UIContextMenuInteraction |
| iOS | UIAlertController (action sheet style) |
| Android | PopupMenu |

---

## IValidatable

Interface for controls that support validation.

```csharp
public interface IValidatable
{
    // Properties
    bool IsValid { get; }
    IReadOnlyList<string> ValidationErrors { get; }

    // Methods
    ValidationResult Validate();

    // Commands
    ICommand? ValidateCommand { get; set; }
}
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| IsValid | bool | Whether the current value passes all validation rules |
| ValidationErrors | IReadOnlyList&lt;string&gt; | List of current validation error messages |

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| Validate() | ValidationResult | Performs validation and returns the result |

### Commands

| Command | Parameter | Description |
|---------|-----------|-------------|
| ValidateCommand | ValidationResult | Executed after validation completes |

### Common Validation Properties

Controls implementing `IValidatable` typically also provide these properties (not part of the interface):

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| IsRequired | bool | false | Whether a value is required |
| RequiredErrorMessage | string | varies | Error message when required validation fails |

### Controls Implementing IValidatable

- ComboBox
- MaskedEntry
- MultiSelectComboBox
- NumericUpDown
- RangeSlider
- Rating
- TokenEntry

### Usage Example

```csharp
// Check validation state
if (!myControl.IsValid)
{
    foreach (var error in myControl.ValidationErrors)
    {
        Debug.WriteLine(error);
    }
}

// Trigger validation manually
var result = myControl.Validate();
if (!result.IsValid)
{
    DisplayErrors(result.Errors);
}

// Use ValidateCommand for MVVM
<extras:NumericUpDown
    Value="{Binding Quantity}"
    IsRequired="True"
    ValidateCommand="{Binding OnValidationCommand}" />
```

---

## Supporting Types

### KeyboardShortcut

```csharp
public record KeyboardShortcut
{
    public required string Key { get; init; }
    public string? Modifiers { get; init; }
    public required string Description { get; init; }
    public string? Category { get; init; }
    public bool IsEnabled { get; init; } = true;
    public string DisplayString { get; } // e.g., "Ctrl+C"
}
```

### KeyEventArgs

```csharp
public class KeyEventArgs : EventArgs
{
    public string Key { get; }
    public KeyModifiers Modifiers { get; }
    public bool Handled { get; set; }
    public bool IsControlPressed { get; }
    public bool IsShiftPressed { get; }
    public bool IsAltPressed { get; }
    public bool IsPlatformCommandPressed { get; } // Ctrl on Windows, Cmd on macOS
}
```

### KeyModifiers

```csharp
[Flags]
public enum KeyModifiers
{
    None = 0,
    Control = 1,
    Shift = 2,
    Alt = 4,
    PlatformCommand = 8
}
```

### ValidationResult

```csharp
public class ValidationResult
{
    public bool IsValid { get; }
    public IReadOnlyList<string> Errors { get; }
    public string? FirstError { get; }

    public static ValidationResult Success { get; }
    public static ValidationResult Failure(IEnumerable<string> errors);
}
```

### ContextMenuItem

Represents a single item in a context menu.

```csharp
public class ContextMenuItem : BindableObject
{
    // Properties
    string? Text { get; set; }
    ImageSource? Icon { get; set; }
    string? IconGlyph { get; set; }          // Font icon glyph (e.g., Segoe MDL2)
    ICommand? Command { get; set; }
    object? CommandParameter { get; set; }
    Action? Action { get; set; }              // Alternative to Command
    bool IsEnabled { get; set; }
    bool IsVisible { get; set; }
    bool IsSeparator { get; set; }
    string? KeyboardShortcut { get; set; }    // Display hint (e.g., "Ctrl+C")
    ContextMenuItemCollection SubItems { get; }
    bool HasSubItems { get; }

    // Methods
    void Execute();
    bool CanExecute();

    // Factory Methods
    static ContextMenuItem Separator();
    static ContextMenuItem Create(string text, Action action, string? iconGlyph = null, string? keyboardShortcut = null);
    static ContextMenuItem Create(string text, ICommand command, object? parameter = null, string? iconGlyph = null, string? keyboardShortcut = null);
    static ContextMenuItem CreateSubMenu(string text, IEnumerable<ContextMenuItem> subItems, string? iconGlyph = null);
}
```

### ContextMenuItemCollection

A collection of context menu items with helper methods.

```csharp
public class ContextMenuItemCollection : ObservableCollection<ContextMenuItem>
{
    void AddSeparator();
    ContextMenuItem Add(string text, Action action, string? iconGlyph = null, string? keyboardShortcut = null);
    ContextMenuItem Add(string text, ICommand command, object? parameter = null, string? iconGlyph = null, string? keyboardShortcut = null);
    ContextMenuItem AddSubMenu(string text, IEnumerable<ContextMenuItem> subItems, string? iconGlyph = null);
    ContextMenuItem AddSubMenu(string text, Action<ContextMenuItemCollection> buildSubMenu, string? iconGlyph = null);
    void AddRange(IEnumerable<ContextMenuItem> items);
    ContextMenuItem? FindByText(string text);
    IEnumerable<ContextMenuItem> GetVisibleItems();
}
```

### ContextMenuOpeningEventArgs

Event args for the ContextMenuOpening event.

```csharp
public class ContextMenuOpeningEventArgs : EventArgs
{
    ContextMenuItemCollection Items { get; }
    object? TargetElement { get; }
    Point Position { get; }
    View? AnchorView { get; }
    bool Cancel { get; set; }
    bool Handled { get; set; }
}
```

### DataGridContextMenuOpeningEventArgs

Extended event args for DataGridView context menus with cell information.

```csharp
public class DataGridContextMenuOpeningEventArgs : ContextMenuOpeningEventArgs
{
    object? Item { get; }
    DataGridColumn? Column { get; }
    int RowIndex { get; }
    int ColumnIndex { get; }
    object? CellValue { get; }
    bool IsHeader { get; }
    bool IsDataCell { get; }
}
```
