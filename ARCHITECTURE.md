# MauiControlsExtras Architecture

This document describes the architectural decisions, patterns, and conventions for the MauiControlsExtras library. All contributors and agents should follow these guidelines when developing new controls or modifying existing ones.

## Overview

MauiControlsExtras provides custom controls for .NET MAUI applications, designed to fill gaps for CRUD/LOB (Line of Business) applications. The library emphasizes:

- **Consistent Styling**: All controls share common styling properties through a base class hierarchy
- **MVVM Support**: Every user action has both an event and a corresponding command
- **Theme Integration**: Controls respect global themes while allowing per-instance customization
- **Optional Behaviors**: Interface-based patterns for validation, clipboard, selection, and undo/redo
- **Internal Reuse**: When a library control can fulfill a need, prefer it over standard MAUI controls (e.g., DataGridComboBoxColumn uses our ComboBox, not MAUI Picker)

## Project Structure

```
src/MauiControlsExtras/
├── Base/                           # Base classes and interfaces
│   ├── StyledControlBase.cs        # Core styling (all controls)
│   ├── TextStyledControlBase.cs    # Typography (text controls)
│   ├── ListStyledControlBase.cs    # Collection styling
│   ├── HeaderedControlBase.cs      # Header styling
│   ├── NavigationControlBase.cs    # Navigation state colors
│   ├── AnimatedControlBase.cs      # Animation support
│   ├── IClipboardSupport.cs        # Copy/cut/paste interface
│   ├── ISelectable.cs              # Selection interface
│   ├── IUndoRedo.cs                # Undo/redo interface
│   └── Validation/
│       ├── IValidatable.cs         # Validation interface
│       └── ValidationResult.cs     # Validation result type
├── Theming/
│   ├── IThemeAware.cs              # Theme change notifications
│   ├── ControlsTheme.cs            # Theme definition class
│   └── MauiControlsExtrasTheme.cs  # Static theme manager
├── Controls/                       # Control implementations
│   ├── ComboBox.xaml/.cs
│   └── [other controls]
└── Converters/                     # Value converters
    └── MauiAssetImageConverter.cs
```

## Base Class Hierarchy

### Inheritance Design

Controls inherit from the most specific base class that provides the properties they need:

```
ContentView
    └── StyledControlBase              (colors, borders, shadows)
            ├── TextStyledControlBase      (+ typography)
            │       └── ComboBox, NumericUpDown, MaskedEntry, etc.
            ├── ListStyledControlBase      (+ collection styling)
            │       └── TreeView, DataGridView, etc.
            ├── HeaderedControlBase        (+ header styling)
            │       └── Accordion, PropertyGrid, Calendar, etc.
            ├── NavigationControlBase      (+ navigation states)
            │       └── Wizard, Breadcrumb, BindingNavigator, etc.
            └── AnimatedControlBase        (+ animation support)
                    └── [animated controls]
```

### Choosing a Base Class

| Control Type | Base Class | Key Features |
|--------------|------------|--------------|
| Text input controls | `TextStyledControlBase` | Font, text color, placeholder |
| Dropdown/combo controls | `TextStyledControlBase` | Font, text color, placeholder |
| List/grid controls | `ListStyledControlBase` | Alternating rows, selection colors, separators |
| Accordion/expandable | `HeaderedControlBase` | Header styling, border |
| Wizard/stepper | `NavigationControlBase` | Active/inactive/visited states |
| Animated controls | `AnimatedControlBase` | Duration, easing, enable flag |
| Simple styled controls | `StyledControlBase` | Just colors, borders, shadows |

### StyledControlBase Properties

All controls inherit these core properties:

```csharp
// Colors
AccentColor         // Primary accent (default: #0078D4)
ForegroundColor     // Text/icon color (theme-aware)
DisabledColor       // Disabled state color
ErrorColor          // Validation error color (#D32F2F)
SuccessColor        // Success indication (#388E3C)
WarningColor        // Warning indication (#F57C00)

// Borders
CornerRadius        // Rounded corners (default: 8)
BorderColor         // Normal border (theme-aware)
BorderThickness     // Border width (default: 1.5)
FocusBorderColor    // Focused state (defaults to AccentColor)
ErrorBorderColor    // Error state (defaults to ErrorColor)
DisabledBorderColor // Disabled state

// Shadows
HasShadow           // Enable shadow (default: false)
ShadowColor         // Shadow color (theme-aware)
ShadowOffset        // Shadow position
ShadowRadius        // Shadow blur
ShadowOpacity       // Shadow transparency

// Elevation
Elevation           // Material-style elevation (0-24)
```

### Effective Properties Pattern

For every nullable property, there is an `Effective*` computed property that falls back to theme defaults:

```csharp
// Property definition
public Color? BorderColor { get; set; }

// Effective property (never null)
public Color EffectiveBorderColor =>
    BorderColor ?? MauiControlsExtrasTheme.GetBorderColor();
```

**Rule**: Always bind XAML to `Effective*` properties, not raw properties.

```xml
<!-- CORRECT -->
<Border Stroke="{Binding EffectiveBorderColor, Source={x:Reference thisControl}}" />

<!-- WRONG - may be null -->
<Border Stroke="{Binding BorderColor, Source={x:Reference thisControl}}" />
```

### Property Change Notifications

When a property changes, always notify both the raw and effective properties:

```csharp
private static void OnBorderColorChanged(BindableObject bindable, object oldValue, object newValue)
{
    if (bindable is StyledControlBase control)
    {
        control.OnPropertyChanged(nameof(EffectiveBorderColor)); // Always include
        control.OnBorderColorChanged((Color?)oldValue, (Color?)newValue);
    }
}
```

## Theming System

### Theme Structure

`ControlsTheme` contains all theme values organized by category:

```csharp
public class ControlsTheme
{
    // Semantic colors
    public Color AccentColor { get; set; }
    public Color ErrorColor { get; set; }
    public Color SuccessColor { get; set; }
    public Color WarningColor { get; set; }
    public Color InfoColor { get; set; }

    // Surface colors (separate for light/dark)
    public Color SurfaceColor { get; set; }
    public Color SurfaceColorDark { get; set; }

    // Selection colors
    public Color SelectionBackgroundColor { get; set; }
    public Color SelectionTextColor { get; set; }

    // Typography
    public double DefaultFontSize { get; set; }
    public string? FontFamily { get; set; }

    // Shape
    public double DefaultCornerRadius { get; set; }
    public double DefaultBorderThickness { get; set; }

    // Animation
    public int AnimationDuration { get; set; }
    public Easing AnimationEasing { get; set; }
    public bool EnableAnimations { get; set; }
}
```

### Predefined Themes

The library ships with predefined themes:

- `ControlsTheme.Default` - Balanced defaults
- `ControlsTheme.Modern` - Rounded, subtle shadows
- `ControlsTheme.Compact` - Reduced spacing, smaller radii
- `ControlsTheme.Fluent` - Microsoft Fluent-inspired
- `ControlsTheme.Material3` - Google Material 3-inspired
- `ControlsTheme.HighContrast` - Accessibility-focused

### Using Themes

```csharp
// Apply a predefined theme
MauiControlsExtrasTheme.ApplyTheme(ControlsTheme.Material3);

// Create a custom theme
var custom = MauiControlsExtrasTheme.CreateCustomTheme(theme =>
{
    theme.AccentColor = Colors.Purple;
    theme.DefaultCornerRadius = 12;
});
MauiControlsExtrasTheme.ApplyTheme(custom);

// Modify current theme
MauiControlsExtrasTheme.ModifyCurrentTheme(theme =>
{
    theme.EnableAnimations = false;
});
```

### Theme-Aware Controls

Controls implement `IThemeAware` and subscribe to theme changes:

```csharp
public abstract class StyledControlBase : ContentView, IThemeAware
{
    protected StyledControlBase()
    {
        MauiControlsExtrasTheme.ThemeChanged += OnGlobalThemeChanged;
    }

    public virtual void OnThemeChanged(AppTheme theme)
    {
        // Notify all effective properties
        OnPropertyChanged(nameof(EffectiveBorderColor));
        OnPropertyChanged(nameof(EffectiveForegroundColor));
        // ... etc
    }
}
```

## MVVM Command Pattern

### Design Rule

**Every user action must have both an event AND a command.**

```csharp
// Event (code-behind support)
public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

// Command (MVVM support)
public ICommand? SelectionChangedCommand { get; set; }
public object? SelectionChangedCommandParameter { get; set; }
```

### Command Invocation Pattern

When an action occurs, invoke both the event and command:

```csharp
private void OnItemSelected(object item)
{
    var args = new SelectionChangedEventArgs(oldItem, item);

    // Raise event
    SelectionChanged?.Invoke(this, args);

    // Execute command
    var parameter = SelectionChangedCommandParameter ?? item;
    if (SelectionChangedCommand?.CanExecute(parameter) == true)
    {
        SelectionChangedCommand.Execute(parameter);
    }
}
```

### Standard Commands by Action Type

| Action | Command Name | Command Parameter |
|--------|--------------|-------------------|
| Selection change | `SelectionChangedCommand` | Selected item or custom |
| Open/expand | `OpenedCommand` | Control instance or null |
| Close/collapse | `ClosedCommand` | Control instance or null |
| Clear/reset | `ClearCommand` | None |
| Validate | `ValidateCommand` | Validation result |
| Value change | `ValueChangedCommand` | New value |

## Interface-Based Optional Behaviors

Controls opt-in to behaviors by implementing interfaces. Do NOT add these to base classes.

### IContextMenuSupport

For controls that support right-click context menus with platform-specific native implementations:

```csharp
public interface IContextMenuSupport
{
    ContextMenuItemCollection ContextMenuItems { get; }
    bool ShowDefaultContextMenu { get; set; }
    event EventHandler<ContextMenuOpeningEventArgs>? ContextMenuOpening;
    void ShowContextMenu(Point? position = null);
}
```

**Platform implementations:**
- Windows: MenuFlyout with FontIcon support
- macOS: UIMenu via UIContextMenuInteraction
- iOS: UIAlertController (action sheet style)
- Android: PopupMenu

**Implementation requirements:**
- Use `ContextMenuService.Current` to show native menus
- Fire `ContextMenuOpening` event before showing menu
- Populate `ContextMenuItems` with custom items
- Add default items (Copy, Paste, etc.) when `ShowDefaultContextMenu` is true

### IValidatable

For controls that support input validation:

```csharp
public interface IValidatable
{
    bool IsValid { get; }
    IReadOnlyList<string> ValidationErrors { get; }
    ICommand? ValidateCommand { get; set; }
    ValidationResult Validate();
}
```

**Implementation requirements:**
- Add `IsRequired` property for required field validation
- Call `Validate()` on value changes or when explicitly requested
- Update `IsValid` and `ValidationErrors` after validation
- Apply `EffectiveErrorBorderColor` when invalid

### IClipboardSupport

For controls that support copy/cut/paste:

```csharp
public interface IClipboardSupport
{
    bool CanCopy { get; }
    bool CanCut { get; }
    bool CanPaste { get; }

    void Copy();
    void Cut();
    void Paste();
    object? GetClipboardContent();

    ICommand? CopyCommand { get; set; }
    ICommand? CutCommand { get; set; }
    ICommand? PasteCommand { get; set; }
}
```

**Implementation requirements:**
- Update `Can*` properties when selection or content changes
- Support keyboard shortcuts (Ctrl+C, Ctrl+X, Ctrl+V)
- Handle platform-specific clipboard APIs
- Cut/Paste should be undoable if control implements `IUndoRedo`

### ISelectable

For controls that support content/item selection:

```csharp
public interface ISelectable
{
    bool HasSelection { get; }
    bool IsAllSelected { get; }
    bool SupportsMultipleSelection { get; }

    void SelectAll();
    void ClearSelection();
    object? GetSelection();
    void SetSelection(object? selection);

    event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

    ICommand? SelectAllCommand { get; set; }
    ICommand? ClearSelectionCommand { get; set; }
    ICommand? SelectionChangedCommand { get; set; }
}
```

**Implementation requirements:**
- Support keyboard shortcut Ctrl+A for SelectAll
- Raise `SelectionChanged` for all selection modifications
- Update `HasSelection` and `IsAllSelected` appropriately

### IUndoRedo

For controls that support undo/redo:

```csharp
public interface IUndoRedo
{
    bool CanUndo { get; }
    bool CanRedo { get; }
    int UndoCount { get; }
    int RedoCount { get; }
    int UndoLimit { get; set; }

    bool Undo();
    bool Redo();
    void ClearUndoHistory();
    string? GetUndoDescription();
    string? GetRedoDescription();

    void BeginBatchOperation(string? description = null);
    void EndBatchOperation();
    void CancelBatchOperation();

    ICommand? UndoCommand { get; set; }
    ICommand? RedoCommand { get; set; }
}
```

**Implementation requirements:**
- Support Ctrl+Z (undo) and Ctrl+Y/Ctrl+Shift+Z (redo)
- Clear redo stack when new changes occur after undo
- Implement batch operations for multi-step changes
- Respect `UndoLimit` to prevent memory issues
- Clear history when loading new data

## Keyboard, Mouse & Focus Support (MANDATORY)

All controls in this library MUST support keyboard navigation, mouse interactions, and proper focus management. This is NOT optional - controls without desktop platform support are considered incomplete.

### Target Platforms

The library targets all MAUI-supported platforms:
- **Windows** (desktop) - Full keyboard + mouse
- **macOS** (via Mac Catalyst) - Full keyboard + mouse
- **iOS** (tablet/phone) - Touch + external keyboard
- **Android** (tablet/phone) - Touch + external keyboard

### IKeyboardNavigable Interface (MANDATORY)

All interactive controls MUST implement `IKeyboardNavigable`:

```csharp
public interface IKeyboardNavigable
{
    /// <summary>
    /// Gets whether this control can receive keyboard focus.
    /// </summary>
    bool CanReceiveFocus { get; }

    /// <summary>
    /// Gets or sets whether keyboard navigation is enabled.
    /// </summary>
    bool IsKeyboardNavigationEnabled { get; set; }

    /// <summary>
    /// Handles a key press event. Returns true if handled.
    /// </summary>
    bool HandleKeyPress(KeyEventArgs e);

    /// <summary>
    /// Gets the keyboard shortcuts supported by this control.
    /// </summary>
    IReadOnlyList<KeyboardShortcut> GetKeyboardShortcuts();
}
```

### Standard Keyboard Shortcuts

Controls MUST support these standard shortcuts where applicable:

| Shortcut | Action | Applicable Controls |
|----------|--------|---------------------|
| `Tab` | Move focus to next control | All |
| `Shift+Tab` | Move focus to previous control | All |
| `Enter` | Activate/confirm | Buttons, ComboBox, DataGrid |
| `Escape` | Cancel/close | Popups, Dropdowns, Edit mode |
| `Space` | Toggle/select | CheckBox, Toggle, TreeView |
| `Ctrl+A` | Select all | Text, DataGrid, Lists |
| `Ctrl+C` | Copy | IClipboardSupport controls |
| `Ctrl+X` | Cut | IClipboardSupport controls |
| `Ctrl+V` | Paste | IClipboardSupport controls |
| `Ctrl+Z` | Undo | IUndoRedo controls |
| `Ctrl+Y` | Redo | IUndoRedo controls |
| `Delete` | Delete selection | DataGrid, TokenEntry, Lists |
| `F2` | Enter edit mode | DataGrid, Editable cells |
| `Arrow Keys` | Navigate | All navigable controls |
| `Home/End` | Navigate to start/end | Lists, DataGrid, Text |
| `Page Up/Down` | Page navigation | Lists, DataGrid |

### Control-Specific Keyboard Requirements

#### DataGridView
- `Arrow Up/Down` - Navigate rows
- `Arrow Left/Right` - Navigate columns
- `Tab` - Move to next cell
- `Shift+Tab` - Move to previous cell
- `Enter` - Commit edit and move down
- `Escape` - Cancel edit
- `F2` - Enter edit mode
- `Delete` - Delete selected rows (if allowed)
- `Ctrl+C/X/V/Z/Y` - Clipboard and undo

#### TreeView
- `Arrow Up/Down` - Navigate items
- `Arrow Right` - Expand node / move to first child
- `Arrow Left` - Collapse node / move to parent
- `Enter` - Activate item
- `Space` - Toggle selection (multi-select mode)
- `+/-` or `*` - Expand/collapse

#### ComboBox / MultiSelectComboBox
- `Arrow Up/Down` - Navigate items in dropdown
- `Enter` - Select item and close
- `Escape` - Close without selecting
- `Space` - Toggle selection (multi-select)
- `Type-ahead` - Filter/search items
- `Alt+Down` - Open dropdown
- `Home/End` - Jump to first/last item

#### NumericUpDown
- `Arrow Up` - Increment value
- `Arrow Down` - Decrement value
- `Page Up` - Increment by large step
- `Page Down` - Decrement by large step
- `Home` - Set to minimum
- `End` - Set to maximum

#### TokenEntry
- `Enter` - Confirm current token
- `Backspace` - Delete last token (when input empty)
- `Delete` - Delete selected token
- `Arrow Left/Right` - Navigate tokens
- `Ctrl+A` - Select all tokens

#### RangeSlider
- `Arrow Left/Right` - Move selected thumb
- `Tab` - Switch between thumbs
- `Home/End` - Move to min/max
- `Page Up/Down` - Large step movement

#### Rating
- `Arrow Left/Right` - Decrease/increase rating
- `1-5` (or `1-N`) - Set specific rating
- `0` or `Delete` - Clear rating

### Mouse Interaction Requirements

#### All Controls
- **Focus on click** - Clicking a control MUST focus it
- **Visual focus indicator** - Focused controls MUST show clear visual feedback
- **Hover states** - Desktop controls SHOULD show hover feedback

#### Context Menus (Right-Click)
Controls with actions MUST support right-click context menus:
- DataGridView - Copy, Paste, Delete, Undo/Redo
- TreeView - Expand All, Collapse All, actions
- TokenEntry - Copy, Delete token
- Text controls - Cut, Copy, Paste, Select All

#### Mouse Wheel
- **DataGridView** - Scroll vertically
- **ComboBox dropdown** - Scroll items
- **NumericUpDown** - Increment/decrement value
- **RangeSlider** - Adjust value (when focused)
- **Rating** - Adjust rating (when focused)

#### Double-Click
- **DataGridView** - Enter edit mode
- **TreeView** - Expand/collapse or activate
- **ComboBox** - Open dropdown

### Focus Management

#### Focus Visual States
All focusable controls MUST define these visual states:
```csharp
// Required focus-related properties (inherited from StyledControlBase)
FocusBorderColor      // Border color when focused
FocusBackgroundColor  // Background when focused (optional)
```

#### Tab Order
- Controls MUST participate in tab navigation
- Use `TabIndex` for custom ordering
- `IsTabStop="False"` only for non-interactive elements

#### Focus Trap
Modal controls (popups, dialogs) MUST trap focus:
- Tab cycles within the control
- Escape closes and returns focus

### Implementation Pattern

```csharp
public partial class MyControl : StyledControlBase, IKeyboardNavigable
{
    public bool CanReceiveFocus => IsEnabled && IsVisible;
    public bool IsKeyboardNavigationEnabled { get; set; } = true;

    public MyControl()
    {
        InitializeComponent();

        // Attach keyboard behavior
        Behaviors.Add(new KeyboardBehavior
        {
            KeyPressedCommand = new Command<KeyEventArgs>(OnKeyPressed)
        });
    }

    public bool HandleKeyPress(KeyEventArgs e)
    {
        if (!IsKeyboardNavigationEnabled) return false;

        switch (e.Key)
        {
            case Keys.Enter:
                Activate();
                return true;
            case Keys.Escape:
                Cancel();
                return true;
            // ... handle other keys
        }

        // Handle shortcuts
        if (e.Modifiers.HasFlag(KeyModifiers.Control))
        {
            switch (e.Key)
            {
                case Keys.C when this is IClipboardSupport cs:
                    cs.Copy();
                    return true;
                case Keys.V when this is IClipboardSupport cs:
                    cs.Paste();
                    return true;
                case Keys.Z when this is IUndoRedo ur:
                    ur.Undo();
                    return true;
                case Keys.Y when this is IUndoRedo ur:
                    ur.Redo();
                    return true;
            }
        }

        return false;
    }

    public IReadOnlyList<KeyboardShortcut> GetKeyboardShortcuts()
    {
        var shortcuts = new List<KeyboardShortcut>
        {
            new("Enter", "Activate control"),
            new("Escape", "Cancel operation"),
        };

        if (this is IClipboardSupport)
        {
            shortcuts.Add(new("Ctrl+C", "Copy"));
            shortcuts.Add(new("Ctrl+V", "Paste"));
        }

        return shortcuts;
    }
}
```

### Testing Requirements

Controls MUST be tested for:
1. **All keyboard shortcuts** - Verify each shortcut works
2. **Tab navigation** - Verify focus moves correctly
3. **Focus visuals** - Verify focus indicator is visible
4. **Mouse interactions** - Verify click, right-click, hover, wheel
5. **Cross-platform** - Test on Windows, macOS minimum

## XAML Conventions

### Control Root Element

Always use `x:Name="thisControl"` for the root element:

```xml
<base:TextStyledControlBase
    xmlns:base="clr-namespace:MauiControlsExtras.Base"
    x:Class="MauiControlsExtras.Controls.MyControl"
    x:Name="thisControl">
```

### Binding to Control Properties

Use `Source={x:Reference thisControl}` for bindings:

```xml
<Border Stroke="{Binding EffectiveBorderColor, Source={x:Reference thisControl}}"
        StrokeThickness="{Binding EffectiveBorderThickness, Source={x:Reference thisControl}}">
```

### Theme-Aware Colors in XAML

Use `AppThemeBinding` for colors not exposed via Effective properties:

```xml
<Label TextColor="{AppThemeBinding Light=#212121, Dark=#FFFFFF}" />
```

### Shadow Application

Conditionally apply shadows based on `HasShadow`:

```xml
<Border.Shadow>
    <Shadow Brush="{Binding EffectiveShadowColor, Source={x:Reference thisControl}}"
            Offset="{Binding EffectiveShadowOffset, Source={x:Reference thisControl}}"
            Radius="{Binding EffectiveShadowRadius, Source={x:Reference thisControl}}"
            Opacity="{Binding EffectiveShadowOpacity, Source={x:Reference thisControl}}" />
</Border.Shadow>
```

## Naming Conventions

### Properties

| Type | Convention | Example |
|------|------------|---------|
| Bindable property | `{Name}Property` | `AccentColorProperty` |
| CLR property | PascalCase | `AccentColor` |
| Effective property | `Effective{Name}` | `EffectiveAccentColor` |
| Command | `{Action}Command` | `SelectionChangedCommand` |
| Command parameter | `{Action}CommandParameter` | `SelectionChangedCommandParameter` |

### Events

| Type | Convention | Example |
|------|------------|---------|
| Event | `{Action}` (past tense preferred) | `SelectionChanged`, `Opened` |
| Event args | `{Action}EventArgs` | `SelectionChangedEventArgs` |

### Methods

| Type | Convention | Example |
|------|------------|---------|
| Property changed handler | `On{Property}Changed` | `OnAccentColorChanged` |
| Event handler | `On{ElementName}{Event}` | `OnCollapsedTapped` |
| Virtual override hook | `On{Property}Changed` (protected virtual) | Allows subclass override |

## Testing Guidelines

### Required Tests

1. **Property defaults** - Verify default values match documentation
2. **Effective property fallback** - Verify fallback to theme when null/-1
3. **Command execution** - Verify commands are invoked with correct parameters
4. **Event raising** - Verify events are raised at correct times
5. **Validation** - Verify validation logic and error messages

### Test Structure

```csharp
[Fact]
public void AccentColor_WhenNotSet_ReturnsThemeDefault()
{
    var control = new MyControl();
    Assert.Equal(MauiControlsExtrasTheme.Current.AccentColor, control.EffectiveAccentColor);
}

[Fact]
public void SelectionChangedCommand_WhenItemSelected_IsExecuted()
{
    var executed = false;
    var control = new MyControl
    {
        SelectionChangedCommand = new Command(() => executed = true)
    };

    control.SelectedItem = new object();

    Assert.True(executed);
}
```

## Migration Guide

### Converting Existing Controls

1. Change base class to appropriate styled base
2. Remove properties now inherited from base (e.g., `AccentColor`)
3. Update XAML bindings to use `Effective*` properties
4. Add command equivalents for all events
5. Implement relevant interfaces (`IValidatable`, `IClipboardSupport`, etc.)
6. Add tests for new functionality

### Breaking Changes Policy

- Property removals: Deprecated for one major version before removal
- Property renames: Both names supported for one major version
- Base class changes: Document migration path in release notes

## Version History

| Version | Changes |
|---------|---------|
| 1.0.0 | Initial release with ComboBox |
| 2.0.0 | Added base class hierarchy, theming, MVVM commands, validation, data action interfaces |
