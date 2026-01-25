# Control Development Guide

This guide provides step-by-step instructions for creating new controls in the MauiControlsExtras library. Follow these patterns to ensure consistency across all controls.

## Quick Start Checklist

Before creating a new control, verify:

- [ ] The control doesn't already exist or is planned
- [ ] You've identified the correct base class
- [ ] You've identified which interfaces to implement
- [ ] You understand the control's events and corresponding commands
- [ ] **MANDATORY**: You've planned keyboard navigation support (see [IKeyboardNavigable](#55-implementing-ikeyboardnavigable))
- [ ] **MANDATORY**: You've planned mouse interaction support for desktop platforms

## Step 1: Choose the Base Class

Select the base class that best matches your control's primary function:

| If your control... | Use this base class |
|-------------------|---------------------|
| Displays/edits text | `TextStyledControlBase` |
| Shows a list/collection | `ListStyledControlBase` |
| Has a collapsible header | `HeaderedControlBase` |
| Shows navigation state (steps, breadcrumb) | `NavigationControlBase` |
| Primarily uses animations | `AnimatedControlBase` |
| Just needs styling (none of the above) | `StyledControlBase` |

## Step 2: Create the Control Files

### 2.1 Create XAML File

Create `src/MauiControlsExtras/Controls/{ControlName}.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<base:TextStyledControlBase xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                            xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                            xmlns:base="clr-namespace:MauiControlsExtras.Base"
                            x:Class="MauiControlsExtras.Controls.MyControl"
                            x:Name="thisControl">

    <!-- Always bind to Effective* properties -->
    <Border StrokeThickness="{Binding EffectiveBorderThickness, Source={x:Reference thisControl}}"
            Stroke="{Binding EffectiveBorderColor, Source={x:Reference thisControl}}">
        <Border.StrokeShape>
            <RoundRectangle CornerRadius="{Binding EffectiveCornerRadius, Source={x:Reference thisControl}}" />
        </Border.StrokeShape>

        <!-- Control content here -->

    </Border>
</base:TextStyledControlBase>
```

### 2.2 Create Code-Behind File

Create `src/MauiControlsExtras/Controls/{ControlName}.xaml.cs`:

```csharp
using System.Windows.Input;
using MauiControlsExtras.Base;
using MauiControlsExtras.Base.Validation;

namespace MauiControlsExtras.Controls;

/// <summary>
/// Brief description of what this control does.
/// </summary>
public partial class MyControl : TextStyledControlBase, IValidatable
{
    #region Constructors

    public MyControl()
    {
        InitializeComponent();
    }

    #endregion

    #region Bindable Properties

    // Your control-specific properties here

    #endregion

    #region Events

    // Your control-specific events here

    #endregion

    #region Commands

    // Command properties here

    #endregion

    #region IValidatable Implementation

    // If implementing IValidatable

    #endregion

    #region Private Methods

    // Internal logic here

    #endregion
}
```

## Step 3: Add Control-Specific Properties

### 3.1 Define Bindable Properties

Follow this pattern for each property:

```csharp
#region Bindable Properties

/// <summary>
/// Identifies the <see cref="Value"/> bindable property.
/// </summary>
public static readonly BindableProperty ValueProperty = BindableProperty.Create(
    nameof(Value),
    typeof(double),
    typeof(MyControl),
    0.0,
    propertyChanged: OnValueChanged);

#endregion

#region Properties

/// <summary>
/// Gets or sets the current value.
/// </summary>
public double Value
{
    get => (double)GetValue(ValueProperty);
    set => SetValue(ValueProperty, value);
}

#endregion

#region Property Changed Handlers

private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
{
    if (bindable is MyControl control)
    {
        control.OnValueChanged((double)oldValue, (double)newValue);
    }
}

/// <summary>
/// Called when the <see cref="Value"/> property changes.
/// </summary>
protected virtual void OnValueChanged(double oldValue, double newValue)
{
    // React to value change
    // Raise events, execute commands, update UI
}

#endregion
```

### 3.2 Property Naming Rules

- **Bindable property**: `{Name}Property`
- **CLR property**: PascalCase (same as name)
- **Static handler**: `On{Name}Changed`
- **Virtual handler**: `On{Name}Changed` (same name, allows override)

## Step 4: Add Events and Commands

### 4.1 Define Events

```csharp
#region Events

/// <summary>
/// Occurs when the value changes.
/// </summary>
public event EventHandler<ValueChangedEventArgs>? ValueChanged;

#endregion
```

### 4.2 Define Command Properties

Every event needs a corresponding command:

```csharp
#region Command Properties

/// <summary>
/// Identifies the <see cref="ValueChangedCommand"/> bindable property.
/// </summary>
public static readonly BindableProperty ValueChangedCommandProperty = BindableProperty.Create(
    nameof(ValueChangedCommand),
    typeof(ICommand),
    typeof(MyControl));

/// <summary>
/// Identifies the <see cref="ValueChangedCommandParameter"/> bindable property.
/// </summary>
public static readonly BindableProperty ValueChangedCommandParameterProperty = BindableProperty.Create(
    nameof(ValueChangedCommandParameter),
    typeof(object),
    typeof(MyControl));

/// <summary>
/// Gets or sets the command to execute when the value changes.
/// </summary>
public ICommand? ValueChangedCommand
{
    get => (ICommand?)GetValue(ValueChangedCommandProperty);
    set => SetValue(ValueChangedCommandProperty, value);
}

/// <summary>
/// Gets or sets the parameter to pass to <see cref="ValueChangedCommand"/>.
/// </summary>
public object? ValueChangedCommandParameter
{
    get => GetValue(ValueChangedCommandParameterProperty);
    set => SetValue(ValueChangedCommandParameterProperty, value);
}

#endregion
```

### 4.3 Raise Events and Execute Commands

In your property change handler or action method:

```csharp
protected virtual void OnValueChanged(double oldValue, double newValue)
{
    var args = new ValueChangedEventArgs(oldValue, newValue);

    // 1. Raise the event
    ValueChanged?.Invoke(this, args);

    // 2. Execute the command
    var parameter = ValueChangedCommandParameter ?? newValue;
    if (ValueChangedCommand?.CanExecute(parameter) == true)
    {
        ValueChangedCommand.Execute(parameter);
    }

    // 3. Validate if applicable
    if (this is IValidatable validatable)
    {
        validatable.Validate();
    }
}
```

## Step 5: Implement Optional Interfaces

### 5.1 Implementing IValidatable

```csharp
public partial class MyControl : TextStyledControlBase, IValidatable
{
    #region Validation Properties

    public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(
        nameof(IsRequired),
        typeof(bool),
        typeof(MyControl),
        false);

    public static readonly BindableProperty ValidateCommandProperty = BindableProperty.Create(
        nameof(ValidateCommand),
        typeof(ICommand),
        typeof(MyControl));

    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    public ICommand? ValidateCommand
    {
        get => (ICommand?)GetValue(ValidateCommandProperty);
        set => SetValue(ValidateCommandProperty, value);
    }

    #endregion

    #region IValidatable Implementation

    private readonly List<string> _validationErrors = new();

    public bool IsValid => _validationErrors.Count == 0;

    public IReadOnlyList<string> ValidationErrors => _validationErrors.AsReadOnly();

    public ValidationResult Validate()
    {
        _validationErrors.Clear();

        // Required field validation
        if (IsRequired && IsEmpty())
        {
            _validationErrors.Add("This field is required.");
        }

        // Add your control-specific validation here
        // if (SomeCondition)
        //     _validationErrors.Add("Specific error message");

        // Notify property changes
        OnPropertyChanged(nameof(IsValid));
        OnPropertyChanged(nameof(ValidationErrors));

        // Execute validation command
        var result = _validationErrors.Count == 0
            ? ValidationResult.Success
            : ValidationResult.Failure(_validationErrors);

        if (ValidateCommand?.CanExecute(result) == true)
        {
            ValidateCommand.Execute(result);
        }

        return result;
    }

    private bool IsEmpty()
    {
        // Implement based on your control's value type
        return Value == default;
    }

    #endregion
}
```

### 5.2 Implementing IClipboardSupport

```csharp
public partial class MyControl : TextStyledControlBase, IClipboardSupport
{
    #region IClipboardSupport Properties

    public bool CanCopy => HasSelection || !string.IsNullOrEmpty(Text);
    public bool CanCut => CanCopy && !IsReadOnly;
    public bool CanPaste => !IsReadOnly; // Check clipboard content if needed

    public ICommand? CopyCommand { get; set; }
    public ICommand? CutCommand { get; set; }
    public ICommand? PasteCommand { get; set; }

    #endregion

    #region IClipboardSupport Implementation

    public void Copy()
    {
        if (!CanCopy) return;

        var content = GetClipboardContent();
        if (content != null)
        {
            Clipboard.SetTextAsync(content.ToString());
        }

        if (CopyCommand?.CanExecute(content) == true)
        {
            CopyCommand.Execute(content);
        }
    }

    public void Cut()
    {
        if (!CanCut) return;

        Copy();
        DeleteSelection(); // Implement based on your control

        if (CutCommand?.CanExecute(null) == true)
        {
            CutCommand.Execute(null);
        }
    }

    public async void Paste()
    {
        if (!CanPaste) return;

        var text = await Clipboard.GetTextAsync();
        if (!string.IsNullOrEmpty(text))
        {
            InsertText(text); // Implement based on your control
        }

        if (PasteCommand?.CanExecute(text) == true)
        {
            PasteCommand.Execute(text);
        }
    }

    public object? GetClipboardContent()
    {
        // Return selected text or all text
        return HasSelection ? SelectedText : Text;
    }

    #endregion
}
```

### 5.3 Implementing ISelectable

```csharp
public partial class MyControl : TextStyledControlBase, ISelectable
{
    #region ISelectable Properties

    public bool HasSelection => SelectionLength > 0;
    public bool IsAllSelected => SelectionLength == Text?.Length;
    public bool SupportsMultipleSelection => false; // Text controls typically false

    public ICommand? SelectAllCommand { get; set; }
    public ICommand? ClearSelectionCommand { get; set; }
    public ICommand? SelectionChangedCommand { get; set; }

    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

    #endregion

    #region ISelectable Implementation

    public void SelectAll()
    {
        var oldSelection = GetSelection();
        // Platform-specific implementation
        SelectionStart = 0;
        SelectionLength = Text?.Length ?? 0;

        RaiseSelectionChanged(oldSelection, GetSelection());

        if (SelectAllCommand?.CanExecute(null) == true)
        {
            SelectAllCommand.Execute(null);
        }
    }

    public void ClearSelection()
    {
        var oldSelection = GetSelection();
        SelectionLength = 0;

        RaiseSelectionChanged(oldSelection, null);

        if (ClearSelectionCommand?.CanExecute(null) == true)
        {
            ClearSelectionCommand.Execute(null);
        }
    }

    public object? GetSelection()
    {
        if (!HasSelection) return null;
        return new { Start = SelectionStart, Length = SelectionLength, Text = SelectedText };
    }

    public void SetSelection(object? selection)
    {
        var oldSelection = GetSelection();

        if (selection == null)
        {
            ClearSelection();
            return;
        }

        // Parse selection object and apply
        // Throw ArgumentException if incompatible type

        RaiseSelectionChanged(oldSelection, GetSelection());
    }

    private void RaiseSelectionChanged(object? oldSelection, object? newSelection)
    {
        var args = new SelectionChangedEventArgs(oldSelection, newSelection);
        SelectionChanged?.Invoke(this, args);

        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(IsAllSelected));

        if (SelectionChangedCommand?.CanExecute(newSelection) == true)
        {
            SelectionChangedCommand.Execute(newSelection);
        }
    }

    #endregion
}
```

### 5.4 Implementing IUndoRedo

```csharp
public partial class MyControl : TextStyledControlBase, IUndoRedo
{
    #region Undo/Redo State

    private readonly Stack<UndoableAction> _undoStack = new();
    private readonly Stack<UndoableAction> _redoStack = new();
    private int _batchNestingLevel = 0;
    private List<UndoableAction>? _batchActions;

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;
    public int UndoCount => _undoStack.Count;
    public int RedoCount => _redoStack.Count;
    public int UndoLimit { get; set; } = 100;

    public ICommand? UndoCommand { get; set; }
    public ICommand? RedoCommand { get; set; }

    #endregion

    #region IUndoRedo Implementation

    public bool Undo()
    {
        if (!CanUndo) return false;

        var action = _undoStack.Pop();
        action.Undo();
        _redoStack.Push(action);

        NotifyUndoRedoChanged();

        if (UndoCommand?.CanExecute(null) == true)
        {
            UndoCommand.Execute(null);
        }

        return true;
    }

    public bool Redo()
    {
        if (!CanRedo) return false;

        var action = _redoStack.Pop();
        action.Redo();
        _undoStack.Push(action);

        NotifyUndoRedoChanged();

        if (RedoCommand?.CanExecute(null) == true)
        {
            RedoCommand.Execute(null);
        }

        return true;
    }

    public void ClearUndoHistory()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        NotifyUndoRedoChanged();
    }

    public string? GetUndoDescription() =>
        _undoStack.TryPeek(out var action) ? action.Description : null;

    public string? GetRedoDescription() =>
        _redoStack.TryPeek(out var action) ? action.Description : null;

    public void BeginBatchOperation(string? description = null)
    {
        if (_batchNestingLevel == 0)
        {
            _batchActions = new List<UndoableAction>();
        }
        _batchNestingLevel++;
    }

    public void EndBatchOperation()
    {
        if (_batchNestingLevel == 0)
            throw new InvalidOperationException("No batch operation in progress.");

        _batchNestingLevel--;

        if (_batchNestingLevel == 0 && _batchActions?.Count > 0)
        {
            var batch = new BatchUndoableAction(_batchActions);
            RecordUndoableAction(batch);
            _batchActions = null;
        }
    }

    public void CancelBatchOperation()
    {
        if (_batchNestingLevel == 0)
            throw new InvalidOperationException("No batch operation in progress.");

        // Undo all batch actions in reverse order
        if (_batchActions != null)
        {
            for (int i = _batchActions.Count - 1; i >= 0; i--)
            {
                _batchActions[i].Undo();
            }
        }

        _batchNestingLevel = 0;
        _batchActions = null;
    }

    #endregion

    #region Undo Infrastructure

    protected void RecordUndoableAction(UndoableAction action)
    {
        if (_batchNestingLevel > 0)
        {
            _batchActions?.Add(action);
            return;
        }

        _redoStack.Clear(); // Clear redo on new action

        _undoStack.Push(action);

        // Enforce limit
        while (_undoStack.Count > UndoLimit && UndoLimit > 0)
        {
            // Remove oldest (bottom of stack) - requires converting to list
            var list = _undoStack.ToList();
            list.RemoveAt(list.Count - 1);
            _undoStack.Clear();
            foreach (var item in list.AsEnumerable().Reverse())
            {
                _undoStack.Push(item);
            }
        }

        NotifyUndoRedoChanged();
    }

    private void NotifyUndoRedoChanged()
    {
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
        OnPropertyChanged(nameof(UndoCount));
        OnPropertyChanged(nameof(RedoCount));
    }

    #endregion
}

// Supporting classes
public abstract class UndoableAction
{
    public string Description { get; }
    protected UndoableAction(string description) => Description = description;
    public abstract void Undo();
    public abstract void Redo();
}

public class BatchUndoableAction : UndoableAction
{
    private readonly List<UndoableAction> _actions;

    public BatchUndoableAction(List<UndoableAction> actions)
        : base("Batch operation")
    {
        _actions = new List<UndoableAction>(actions);
    }

    public override void Undo()
    {
        for (int i = _actions.Count - 1; i >= 0; i--)
            _actions[i].Undo();
    }

    public override void Redo()
    {
        foreach (var action in _actions)
            action.Redo();
    }
}
```

### 5.5 Implementing IKeyboardNavigable (MANDATORY)

**All interactive controls MUST implement keyboard navigation for desktop platform support.**

```csharp
using MauiControlsExtras.Base;
using MauiControlsExtras.Behaviors;
using System.Windows.Input;

public partial class MyControl : TextStyledControlBase, IKeyboardNavigable
{
    #region Keyboard Fields

    private KeyboardBehavior? _keyboardBehavior;
    private bool _isKeyboardNavigationEnabled = true;
    private static readonly List<KeyboardShortcut> _keyboardShortcuts = new();

    #endregion

    #region IKeyboardNavigable Properties

    public bool CanReceiveFocus => IsEnabled && IsVisible;

    public bool IsKeyboardNavigationEnabled
    {
        get => _isKeyboardNavigationEnabled;
        set
        {
            _isKeyboardNavigationEnabled = value;
            OnPropertyChanged(nameof(IsKeyboardNavigationEnabled));
        }
    }

    public bool HasKeyboardFocus => IsFocused;

    #endregion

    #region IKeyboardNavigable Commands

    public static readonly BindableProperty GotFocusCommandProperty = BindableProperty.Create(
        nameof(GotFocusCommand),
        typeof(ICommand),
        typeof(MyControl));

    public static readonly BindableProperty LostFocusCommandProperty = BindableProperty.Create(
        nameof(LostFocusCommand),
        typeof(ICommand),
        typeof(MyControl));

    public static readonly BindableProperty KeyPressCommandProperty = BindableProperty.Create(
        nameof(KeyPressCommand),
        typeof(ICommand),
        typeof(MyControl));

    public ICommand? GotFocusCommand
    {
        get => (ICommand?)GetValue(GotFocusCommandProperty);
        set => SetValue(GotFocusCommandProperty, value);
    }

    public ICommand? LostFocusCommand
    {
        get => (ICommand?)GetValue(LostFocusCommandProperty);
        set => SetValue(LostFocusCommandProperty, value);
    }

    public ICommand? KeyPressCommand
    {
        get => (ICommand?)GetValue(KeyPressCommandProperty);
        set => SetValue(KeyPressCommandProperty, value);
    }

    #endregion

    #region IKeyboardNavigable Events

    public event EventHandler<KeyboardFocusEventArgs>? KeyboardFocusGained;
    public event EventHandler<KeyboardFocusEventArgs>? KeyboardFocusLost;
    public event EventHandler<KeyEventArgs>? KeyPressed;
    public event EventHandler<KeyEventArgs>? KeyReleased;

    #endregion

    #region IKeyboardNavigable Implementation

    public bool HandleKeyPress(KeyEventArgs e)
    {
        if (!IsKeyboardNavigationEnabled) return false;

        // Raise event first - allows external handlers to process
        KeyPressed?.Invoke(this, e);
        if (e.Handled) return true;

        // Execute command if set
        if (KeyPressCommand?.CanExecute(e) == true)
        {
            KeyPressCommand.Execute(e);
            if (e.Handled) return true;
        }

        // Handle standard keys
        return e.Key switch
        {
            "Enter" => HandleEnterKey(e),
            "Escape" => HandleEscapeKey(e),
            "Up" => HandleUpKey(e),
            "Down" => HandleDownKey(e),
            "Left" => HandleLeftKey(e),
            "Right" => HandleRightKey(e),
            "Tab" => HandleTabKey(e),
            "Home" => HandleHomeKey(e),
            "End" => HandleEndKey(e),
            "PageUp" => HandlePageUpKey(e),
            "PageDown" => HandlePageDownKey(e),
            "A" when e.IsPlatformCommandPressed => HandleSelectAllKey(e),
            "C" when e.IsPlatformCommandPressed => HandleCopyKey(e),
            "V" when e.IsPlatformCommandPressed => HandlePasteKey(e),
            "X" when e.IsPlatformCommandPressed => HandleCutKey(e),
            "Z" when e.IsPlatformCommandPressed => HandleUndoKey(e),
            "Y" when e.IsPlatformCommandPressed => HandleRedoKey(e),
            _ => false
        };
    }

    public IReadOnlyList<KeyboardShortcut> GetKeyboardShortcuts()
    {
        if (_keyboardShortcuts.Count == 0)
        {
            // Initialize shortcuts - customize for your control
            _keyboardShortcuts.AddRange(new[]
            {
                new KeyboardShortcut { Key = "Enter", Description = "Activate item", Category = "General" },
                new KeyboardShortcut { Key = "Escape", Description = "Cancel / Close", Category = "General" },
                new KeyboardShortcut { Key = "Up", Description = "Move up", Category = "Navigation" },
                new KeyboardShortcut { Key = "Down", Description = "Move down", Category = "Navigation" },
                new KeyboardShortcut { Key = "A", Modifiers = "Ctrl", Description = "Select all", Category = "Selection" },
            });
        }
        return _keyboardShortcuts;
    }

    public new bool Focus()
    {
        if (!CanReceiveFocus) return false;

        var result = base.Focus();
        if (result)
        {
            KeyboardFocusGained?.Invoke(this, new KeyboardFocusEventArgs(true));
            GotFocusCommand?.Execute(this);
        }
        return result;
    }

    #endregion

    #region Key Handlers (override in derived classes)

    protected virtual bool HandleEnterKey(KeyEventArgs e) => false;
    protected virtual bool HandleEscapeKey(KeyEventArgs e) => false;
    protected virtual bool HandleUpKey(KeyEventArgs e) => false;
    protected virtual bool HandleDownKey(KeyEventArgs e) => false;
    protected virtual bool HandleLeftKey(KeyEventArgs e) => false;
    protected virtual bool HandleRightKey(KeyEventArgs e) => false;
    protected virtual bool HandleTabKey(KeyEventArgs e) => false;
    protected virtual bool HandleHomeKey(KeyEventArgs e) => false;
    protected virtual bool HandleEndKey(KeyEventArgs e) => false;
    protected virtual bool HandlePageUpKey(KeyEventArgs e) => false;
    protected virtual bool HandlePageDownKey(KeyEventArgs e) => false;

    protected virtual bool HandleSelectAllKey(KeyEventArgs e)
    {
        if (this is ISelectable selectable)
        {
            selectable.SelectAll();
            return true;
        }
        return false;
    }

    protected virtual bool HandleCopyKey(KeyEventArgs e)
    {
        if (this is IClipboardSupport clipboard && clipboard.CanCopy)
        {
            clipboard.Copy();
            return true;
        }
        return false;
    }

    protected virtual bool HandlePasteKey(KeyEventArgs e)
    {
        if (this is IClipboardSupport clipboard && clipboard.CanPaste)
        {
            clipboard.Paste();
            return true;
        }
        return false;
    }

    protected virtual bool HandleCutKey(KeyEventArgs e)
    {
        if (this is IClipboardSupport clipboard && clipboard.CanCut)
        {
            clipboard.Cut();
            return true;
        }
        return false;
    }

    protected virtual bool HandleUndoKey(KeyEventArgs e)
    {
        if (this is IUndoRedo undoRedo && undoRedo.CanUndo)
        {
            undoRedo.Undo();
            return true;
        }
        return false;
    }

    protected virtual bool HandleRedoKey(KeyEventArgs e)
    {
        if (this is IUndoRedo undoRedo && undoRedo.CanRedo)
        {
            undoRedo.Redo();
            return true;
        }
        return false;
    }

    #endregion

    #region Keyboard Setup

    private void SetupKeyboardBehavior()
    {
        _keyboardBehavior = new KeyboardBehavior();
        _keyboardBehavior.KeyPressed += OnKeyboardBehaviorKeyPressed;
        this.Behaviors.Add(_keyboardBehavior);
    }

    private void OnKeyboardBehaviorKeyPressed(object? sender, KeyEventArgs e)
    {
        HandleKeyPress(e);
    }

    #endregion

    #region Focus Event Handlers

    // Wire these up in your constructor or OnApplyTemplate
    private void OnControlFocused(object? sender, FocusEventArgs e)
    {
        KeyboardFocusGained?.Invoke(this, new KeyboardFocusEventArgs(true));
        GotFocusCommand?.Execute(this);
    }

    private void OnControlUnfocused(object? sender, FocusEventArgs e)
    {
        KeyboardFocusLost?.Invoke(this, new KeyboardFocusEventArgs(false));
        LostFocusCommand?.Execute(this);
    }

    #endregion
}
```

#### Standard Keyboard Shortcuts Reference

All controls should support these shortcuts where applicable:

| Category | Key | Action |
|----------|-----|--------|
| **Navigation** | ↑ / ↓ | Move up/down |
| | ← / → | Move left/right |
| | Home / End | First/last item |
| | Page Up/Down | Page navigation |
| | Tab | Next focusable element |
| | Shift+Tab | Previous focusable element |
| **Selection** | Enter / Space | Activate/select item |
| | Ctrl+A | Select all |
| | Escape | Cancel/close |
| **Clipboard** | Ctrl+C | Copy |
| | Ctrl+X | Cut |
| | Ctrl+V | Paste |
| **Undo/Redo** | Ctrl+Z | Undo |
| | Ctrl+Y | Redo |
| **Editing** | F2 | Begin edit |
| | Delete | Delete selected |

**Note**: On macOS, Ctrl shortcuts use Command (⌘) instead.

### 5.6 Mouse Interaction Requirements (MANDATORY)

All controls must support proper mouse interactions for desktop platforms:

```csharp
public partial class MyControl : TextStyledControlBase
{
    #region Mouse Event Properties

    public static readonly BindableProperty MouseDoubleClickCommandProperty = BindableProperty.Create(
        nameof(MouseDoubleClickCommand),
        typeof(ICommand),
        typeof(MyControl));

    public static readonly BindableProperty MouseRightClickCommandProperty = BindableProperty.Create(
        nameof(MouseRightClickCommand),
        typeof(ICommand),
        typeof(MyControl));

    public ICommand? MouseDoubleClickCommand
    {
        get => (ICommand?)GetValue(MouseDoubleClickCommandProperty);
        set => SetValue(MouseDoubleClickCommandProperty, value);
    }

    public ICommand? MouseRightClickCommand
    {
        get => (ICommand?)GetValue(MouseRightClickCommandProperty);
        set => SetValue(MouseRightClickCommandProperty, value);
    }

    #endregion

    #region Mouse Events

    public event EventHandler<MouseEventArgs>? MouseDoubleClick;
    public event EventHandler<MouseEventArgs>? MouseRightClick;
    public event EventHandler<MouseWheelEventArgs>? MouseWheelScrolled;
    public event EventHandler<PointerEventArgs>? PointerEntered;
    public event EventHandler<PointerEventArgs>? PointerExited;

    #endregion

    #region Mouse Setup

    private void SetupMouseHandling()
    {
        // Double-tap gesture for activation
        var doubleTapGesture = new TapGestureRecognizer
        {
            NumberOfTapsRequired = 2
        };
        doubleTapGesture.Tapped += OnDoubleTapped;
        this.GestureRecognizers.Add(doubleTapGesture);

        // Pointer handlers for hover effects
        var pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerEntered += OnPointerEntered;
        pointerGesture.PointerExited += OnPointerExited;
        this.GestureRecognizers.Add(pointerGesture);
    }

    private void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        var args = new MouseEventArgs(MouseButton.Left, e.GetPosition(this));
        MouseDoubleClick?.Invoke(this, args);

        if (MouseDoubleClickCommand?.CanExecute(args) == true)
            MouseDoubleClickCommand.Execute(args);
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        // Update visual state for hover
        VisualStateManager.GoToState(this, "PointerOver");
        PointerEntered?.Invoke(this, e);
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        // Reset visual state
        VisualStateManager.GoToState(this, "Normal");
        PointerExited?.Invoke(this, e);
    }

    #endregion
}

// Supporting class for mouse events
public class MouseEventArgs : EventArgs
{
    public MouseButton Button { get; }
    public Point? Position { get; }

    public MouseEventArgs(MouseButton button, Point? position = null)
    {
        Button = button;
        Position = position;
    }
}

public enum MouseButton { Left, Right, Middle }

public class MouseWheelEventArgs : EventArgs
{
    public double Delta { get; }
    public bool Handled { get; set; }

    public MouseWheelEventArgs(double delta) => Delta = delta;
}
```

#### Required Mouse Behaviors

| Interaction | Expected Behavior |
|-------------|-------------------|
| **Left Click** | Select item, set focus |
| **Double Click** | Activate item (e.g., begin edit, expand) |
| **Right Click** | Show context menu |
| **Mouse Wheel** | Scroll content |
| **Hover** | Visual feedback (highlight, tooltip) |
| **Drag** | Selection, resize, reorder |

## Step 6: XAML Best Practices

### 6.1 Use Effective Properties

Always bind to `Effective*` properties in XAML:

```xml
<!-- CORRECT -->
<Border Stroke="{Binding EffectiveBorderColor, Source={x:Reference thisControl}}"
        StrokeThickness="{Binding EffectiveBorderThickness, Source={x:Reference thisControl}}">
    <Border.StrokeShape>
        <RoundRectangle CornerRadius="{Binding EffectiveCornerRadius, Source={x:Reference thisControl}}" />
    </Border.StrokeShape>
</Border>

<!-- WRONG - may be null -->
<Border Stroke="{Binding BorderColor, Source={x:Reference thisControl}}">
```

### 6.2 Theme-Aware Colors

For colors not exposed via base class, use `AppThemeBinding`:

```xml
<Label TextColor="{AppThemeBinding Light=#212121, Dark=#FFFFFF}" />
<Border BackgroundColor="{AppThemeBinding Light=#FFFFFF, Dark=#424242}" />
```

### 6.3 Gesture Recognizers

Connect UI elements to handlers:

```xml
<Border x:Name="mainBorder">
    <Border.GestureRecognizers>
        <TapGestureRecognizer Tapped="OnMainBorderTapped" />
    </Border.GestureRecognizers>
</Border>
```

### 6.4 Conditional Visibility

Bind visibility to control state:

```xml
<Label x:Name="errorLabel"
       Text="{Binding ValidationErrors[0], Source={x:Reference thisControl}}"
       IsVisible="{Binding IsValid, Source={x:Reference thisControl}, Converter={StaticResource InvertedBoolConverter}}"
       TextColor="{Binding EffectiveErrorColor, Source={x:Reference thisControl}}" />
```

## Step 7: Testing Your Control

### 7.1 Required Test Coverage

Create tests for:

1. **Default values** - All properties have correct defaults
2. **Effective property fallback** - Returns theme value when null
3. **Command execution** - Commands are invoked correctly
4. **Event raising** - Events fire at the right times
5. **Validation** - Validation logic works correctly

### 7.2 Test File Location

Place tests in `tests/MauiControlsExtras.Tests/{ControlName}Tests.cs`

### 7.3 Test Template

```csharp
using Xunit;
using MauiControlsExtras.Controls;
using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Tests;

public class MyControlTests
{
    [Fact]
    public void DefaultValue_IsCorrect()
    {
        var control = new MyControl();
        Assert.Equal(0.0, control.Value);
    }

    [Fact]
    public void EffectiveCornerRadius_WhenNotSet_ReturnsThemeDefault()
    {
        var control = new MyControl();
        Assert.Equal(
            MauiControlsExtrasTheme.Current.DefaultCornerRadius,
            control.EffectiveCornerRadius);
    }

    [Fact]
    public void ValueChangedCommand_WhenValueChanges_IsExecuted()
    {
        var executedValue = (double?)null;
        var control = new MyControl
        {
            ValueChangedCommand = new Command<double>(v => executedValue = v)
        };

        control.Value = 42.0;

        Assert.Equal(42.0, executedValue);
    }

    [Fact]
    public void Validate_WhenRequiredAndEmpty_ReturnsFailure()
    {
        var control = new MyControl { IsRequired = true };

        var result = control.Validate();

        Assert.False(result.IsValid);
        Assert.Contains("required", result.FirstError, StringComparison.OrdinalIgnoreCase);
    }
}
```

## Complete Example: NumericUpDown Control

Here's a complete example implementing all patterns:

### NumericUpDown.xaml

```xml
<?xml version="1.0" encoding="utf-8" ?>
<base:TextStyledControlBase xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                            xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                            xmlns:base="clr-namespace:MauiControlsExtras.Base"
                            x:Class="MauiControlsExtras.Controls.NumericUpDown"
                            x:Name="thisControl">

    <Border StrokeThickness="{Binding EffectiveBorderThickness, Source={x:Reference thisControl}}"
            Stroke="{Binding EffectiveBorderColor, Source={x:Reference thisControl}}"
            BackgroundColor="{AppThemeBinding Light=#FFFFFF, Dark=#424242}"
            Padding="8,4">
        <Border.StrokeShape>
            <RoundRectangle CornerRadius="{Binding EffectiveCornerRadius, Source={x:Reference thisControl}}" />
        </Border.StrokeShape>

        <Grid ColumnDefinitions="*,Auto,Auto">
            <Entry x:Name="valueEntry"
                   Grid.Column="0"
                   Text="{Binding DisplayValue, Source={x:Reference thisControl}}"
                   Keyboard="Numeric"
                   FontSize="{Binding EffectiveFontSize, Source={x:Reference thisControl}}"
                   TextColor="{Binding EffectiveTextColor, Source={x:Reference thisControl}}"
                   Completed="OnEntryCompleted" />

            <Button Grid.Column="1"
                    Text="▲"
                    WidthRequest="32"
                    Clicked="OnIncrementClicked"
                    BackgroundColor="Transparent" />

            <Button Grid.Column="2"
                    Text="▼"
                    WidthRequest="32"
                    Clicked="OnDecrementClicked"
                    BackgroundColor="Transparent" />
        </Grid>
    </Border>
</base:TextStyledControlBase>
```

### NumericUpDown.xaml.cs (abbreviated)

```csharp
using System.Windows.Input;
using MauiControlsExtras.Base;
using MauiControlsExtras.Base.Validation;

namespace MauiControlsExtras.Controls;

public partial class NumericUpDown : TextStyledControlBase, IValidatable
{
    public NumericUpDown()
    {
        InitializeComponent();
    }

    // Value property
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(double), typeof(NumericUpDown), 0.0,
        propertyChanged: OnValueChanged);

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, Math.Clamp(value, Minimum, Maximum));
    }

    // Min/Max properties
    public static readonly BindableProperty MinimumProperty = ...;
    public static readonly BindableProperty MaximumProperty = ...;
    public static readonly BindableProperty StepProperty = ...;

    // Commands
    public static readonly BindableProperty ValueChangedCommandProperty = ...;
    public static readonly BindableProperty IncrementCommandProperty = ...;
    public static readonly BindableProperty DecrementCommandProperty = ...;

    // Events
    public event EventHandler<ValueChangedEventArgs>? ValueChanged;

    // Property change handler
    private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is NumericUpDown control)
        {
            var args = new ValueChangedEventArgs((double)oldValue, (double)newValue);
            control.ValueChanged?.Invoke(control, args);

            var parameter = control.ValueChangedCommandParameter ?? newValue;
            if (control.ValueChangedCommand?.CanExecute(parameter) == true)
                control.ValueChangedCommand.Execute(parameter);

            control.Validate();
            control.OnPropertyChanged(nameof(DisplayValue));
        }
    }

    // UI handlers
    private void OnIncrementClicked(object sender, EventArgs e)
    {
        Value += Step;
        IncrementCommand?.Execute(Value);
    }

    private void OnDecrementClicked(object sender, EventArgs e)
    {
        Value -= Step;
        DecrementCommand?.Execute(Value);
    }

    // IValidatable implementation
    public bool IsValid => _validationErrors.Count == 0;
    public IReadOnlyList<string> ValidationErrors => _validationErrors.AsReadOnly();
    // ... rest of implementation
}
```

## Troubleshooting

### Common Issues

| Problem | Solution |
|---------|----------|
| Binding not updating | Check `OnPropertyChanged` is called for effective properties |
| Command not executing | Verify `CanExecute` returns true |
| Theme changes not reflected | Ensure control implements `IThemeAware` and subscribes to `ThemeChanged` |
| Null reference in XAML | Use `Effective*` properties instead of nullable properties |
| Validation not showing errors | Bind to `IsValid` and `ValidationErrors` properties |
| Keyboard events not firing | Ensure `KeyboardBehavior` is attached and control is focusable |
| Mouse hover not working | Add `PointerGestureRecognizer` to control's GestureRecognizers |
| Context menu not showing | Implement right-click/long-press gesture handling |
| Focus ring not visible | Set `VisualStateManager` states for Focused state |

### Debugging Tips

1. Add `Debug.WriteLine` in property change handlers
2. Use Visual Studio's Live Visual Tree to inspect bindings
3. Check for binding errors in Output window
4. Verify `x:Name="thisControl"` is set on root element

## Further Reading

- [ARCHITECTURE.md](../ARCHITECTURE.md) - Detailed architecture documentation
- [.NET MAUI Bindable Properties](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/bindable-properties)
- [MVVM Pattern](https://learn.microsoft.com/en-us/dotnet/maui/xaml/fundamentals/mvvm)
