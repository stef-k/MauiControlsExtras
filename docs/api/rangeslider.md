# RangeSlider API Reference

Full API documentation for the `MauiControlsExtras.Controls.RangeSlider` control.

## Namespace

```csharp
using MauiControlsExtras.Controls;
```

## Class Definition

```csharp
public partial class RangeSlider : StyledControlBase, IValidatable, IKeyboardNavigable
```

## Inheritance

Inherits from [StyledControlBase](base-classes.md#styledcontrolbase). See base class documentation for inherited styling properties.

## Interfaces

- [IValidatable](interfaces.md#ivalidatable) - Validation support
- [IKeyboardNavigable](interfaces.md#ikeyboardnavigable) - Keyboard navigation support

---

## Properties

### Core Properties

#### LowerValue

Gets or sets the lower thumb value.

```csharp
public double LowerValue { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `double` | `0` | Yes | TwoWay |

---

#### UpperValue

Gets or sets the upper thumb value.

```csharp
public double UpperValue { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `double` | `100` | Yes | TwoWay |

---

#### Minimum

Gets or sets the minimum value.

```csharp
public double Minimum { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `0` | Yes |

---

#### Maximum

Gets or sets the maximum value.

```csharp
public double Maximum { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `100` | Yes |

---

#### Step

Gets or sets the step increment for thumb movement.

```csharp
public double Step { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `1` | Yes |

---

#### MinimumRange

Gets or sets the minimum distance between thumbs.

```csharp
public double MinimumRange { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `0` | Yes |

---

### Appearance Properties

#### Orientation

Gets or sets the slider orientation.

```csharp
public SliderOrientation Orientation { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `SliderOrientation` | `Horizontal` | Yes |

---

#### TrackHeight

Gets or sets the height of the slider track.

```csharp
public double TrackHeight { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `4` | Yes |

---

#### TrackColor

Gets or sets the color of the inactive track.

```csharp
public Color? TrackColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` (uses muted foreground) | Yes |

---

#### ActiveTrackColor

Gets or sets the color of the active (selected) track between thumbs.

```csharp
public Color? ActiveTrackColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` (uses AccentColor) | Yes |

---

#### ThumbSize

Gets or sets the size of the thumb controls.

```csharp
public double ThumbSize { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `20` | Yes |

---

#### ThumbColor

Gets or sets the color of the thumb controls.

```csharp
public Color? ThumbColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` (uses AccentColor) | Yes |

---

#### ThumbBorderColor

Gets or sets the border color of the thumbs.

```csharp
public Color? ThumbBorderColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` | Yes |

---

#### ThumbBorderThickness

Gets or sets the border thickness of the thumbs.

```csharp
public double ThumbBorderThickness { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `0` | Yes |

---

### Label Properties

#### ShowLabels

Gets or sets whether value labels are shown.

```csharp
public bool ShowLabels { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### LabelFormat

Gets or sets the format string for labels.

```csharp
public string? LabelFormat { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string?` | `null` (uses default formatting) | Yes |

---

#### ShowMinMaxLabels

Gets or sets whether minimum/maximum labels are shown.

```csharp
public bool ShowMinMaxLabels { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

### Tick Properties

#### ShowTicks

Gets or sets whether tick marks are shown.

```csharp
public bool ShowTicks { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

#### TickFrequency

Gets or sets the frequency of tick marks.

```csharp
public double TickFrequency { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `10` | Yes |

---

#### TickColor

Gets or sets the color of tick marks.

```csharp
public Color? TickColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` | Yes |

---

### Validation Properties

#### IsRequired

Gets or sets whether a range selection is required.

```csharp
public bool IsRequired { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

#### RequiredErrorMessage

Gets or sets the error message when required validation fails.

```csharp
public string RequiredErrorMessage { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"Range selection is required"` | Yes |

---

#### IsValid (Read-only)

Gets whether the current values pass validation.

```csharp
public bool IsValid { get; }
```

| Type | Bindable |
|------|----------|
| `bool` | Yes |

---

#### ValidationErrors (Read-only)

Gets the list of current validation error messages.

```csharp
public IReadOnlyList<string> ValidationErrors { get; }
```

| Type | Bindable |
|------|----------|
| `IReadOnlyList<string>` | Yes |

---

### Read-only Properties

#### Range (Read-only)

Gets the current range (UpperValue - LowerValue).

```csharp
public double Range { get; }
```

| Type | Bindable |
|------|----------|
| `double` | Yes |

---

## Events

### LowerValueChanged

Occurs when the lower value changes.

```csharp
public event EventHandler<double>? LowerValueChanged;
```

**Event Args:** The new lower value.

---

### UpperValueChanged

Occurs when the upper value changes.

```csharp
public event EventHandler<double>? UpperValueChanged;
```

**Event Args:** The new upper value.

---

### RangeChanged

Occurs when either value changes.

```csharp
public event EventHandler<RangeChangedEventArgs>? RangeChanged;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| LowerValue | `double` | Current lower value |
| UpperValue | `double` | Current upper value |
| Range | `double` | Distance between values |

---

### DragStarted

Occurs when thumb dragging begins.

```csharp
public event EventHandler<RangeSliderDragEventArgs>? DragStarted;
```

---

### DragCompleted

Occurs when thumb dragging ends.

```csharp
public event EventHandler<RangeSliderDragEventArgs>? DragCompleted;
```

---

## Commands

### LowerValueChangedCommand

Executed when the lower value changes.

```csharp
public ICommand? LowerValueChangedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Value | `double` |

---

### UpperValueChangedCommand

Executed when the upper value changes.

```csharp
public ICommand? UpperValueChangedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Value | `double` |

---

### RangeChangedCommand

Executed when either value changes.

```csharp
public ICommand? RangeChangedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Args | `RangeChangedEventArgs` |

---

### ValidateCommand

Executed after validation completes.

```csharp
public ICommand? ValidateCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Result | `ValidationResult` |

---

## Methods

### SetRange(double lower, double upper)

Sets both values at once.

```csharp
public void SetRange(double lower, double upper)
```

---

### Reset()

Resets values to minimum and maximum.

```csharp
public void Reset()
```

---

### Validate()

Performs validation and returns the result.

```csharp
public ValidationResult Validate()
```

| Returns | Description |
|---------|-------------|
| `ValidationResult` | Contains IsValid and any error messages |

---

## Enumerations

### SliderOrientation

```csharp
public enum SliderOrientation
{
    Horizontal,  // Left to right
    Vertical     // Bottom to top
}
```

---

## Keyboard Shortcuts

| Key | Description |
|-----|-------------|
| Tab | Switch between lower and upper thumb |
| Arrow Left/Down | Decrease focused thumb value by step |
| Arrow Right/Up | Increase focused thumb value by step |
| Page Down | Decrease by 10 × step |
| Page Up | Increase by 10 × step |
| Home | Set focused thumb to minimum (or other thumb + MinimumRange) |
| End | Set focused thumb to maximum (or other thumb - MinimumRange) |

---

## Usage Examples

### Basic Range Selection

```xml
<extras:RangeSlider LowerValue="{Binding MinPrice, Mode=TwoWay}"
                    UpperValue="{Binding MaxPrice, Mode=TwoWay}"
                    Minimum="0"
                    Maximum="1000"
                    Step="10" />
```

### With Labels and Formatting

```xml
<extras:RangeSlider LowerValue="{Binding StartAge}"
                    UpperValue="{Binding EndAge}"
                    Minimum="0"
                    Maximum="100"
                    ShowLabels="True"
                    LabelFormat="N0"
                    ShowMinMaxLabels="True" />
```

### Custom Appearance

```xml
<extras:RangeSlider LowerValue="20"
                    UpperValue="80"
                    TrackHeight="6"
                    TrackColor="#E0E0E0"
                    ActiveTrackColor="#2196F3"
                    ThumbSize="24"
                    ThumbColor="#1976D2"
                    ThumbBorderColor="#FFFFFF"
                    ThumbBorderThickness="2" />
```

### With Ticks

```xml
<extras:RangeSlider LowerValue="{Binding Start}"
                    UpperValue="{Binding End}"
                    Minimum="0"
                    Maximum="100"
                    Step="5"
                    ShowTicks="True"
                    TickFrequency="10"
                    TickColor="#9E9E9E" />
```

### Minimum Range Constraint

```xml
<!-- Thumbs must be at least 100 apart -->
<extras:RangeSlider LowerValue="{Binding MinDate}"
                    UpperValue="{Binding MaxDate}"
                    Minimum="0"
                    Maximum="365"
                    MinimumRange="30"
                    LabelFormat="Day {0}" />
```

### Vertical Orientation

```xml
<extras:RangeSlider Orientation="Vertical"
                    LowerValue="{Binding MinTemp}"
                    UpperValue="{Binding MaxTemp}"
                    Minimum="-20"
                    Maximum="50"
                    HeightRequest="200" />
```

### Code-Behind

```csharp
// Create range slider
var slider = new RangeSlider
{
    Minimum = 0,
    Maximum = 1000,
    Step = 10,
    MinimumRange = 50,
    ShowLabels = true,
    LabelFormat = "${0:N0}"
};

// Set initial range
slider.SetRange(200, 800);

// Handle changes
slider.RangeChanged += (sender, args) =>
{
    Console.WriteLine($"Range: {args.LowerValue} - {args.UpperValue}");
    Console.WriteLine($"Span: {args.Range}");
};

// Handle drag events for expensive operations
slider.DragStarted += (s, e) => SuspendFiltering();
slider.DragCompleted += (s, e) => ApplyFilter(slider.LowerValue, slider.UpperValue);

// Validate
var result = slider.Validate();
```
