# RangeSlider

A dual-thumb slider for selecting a range of values.

## Features

- **Range Selection** - Select min and max values
- **Customizable Range** - Set min/max bounds
- **Step Support** - Snap to increments
- **Value Display** - Show current values
- **Keyboard Navigation** - Full keyboard support

## Basic Usage

```xml
<extras:RangeSlider
    Minimum="0"
    Maximum="100"
    LowerValue="{Binding MinPrice, Mode=TwoWay}"
    UpperValue="{Binding MaxPrice, Mode=TwoWay}" />
```

## With Labels

```xml
<extras:RangeSlider
    Minimum="0"
    Maximum="1000"
    LowerValue="{Binding MinPrice}"
    UpperValue="{Binding MaxPrice}"
    ShowValueLabels="True"
    ValueLabelFormat="C0" />
```

## Step Increments

```xml
<extras:RangeSlider
    Minimum="0"
    Maximum="100"
    Step="5"
    LowerValue="{Binding Min}"
    UpperValue="{Binding Max}" />
```

## Minimum Range

```xml
<extras:RangeSlider
    Minimum="0"
    Maximum="100"
    MinimumRange="10"
    LowerValue="{Binding Min}"
    UpperValue="{Binding Max}" />
```

This ensures the thumbs stay at least 10 apart.

## Customization

```xml
<extras:RangeSlider
    TrackColor="#E0E0E0"
    RangeColor="#2196F3"
    ThumbColor="#1976D2"
    ThumbRadius="12" />
```

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| Tab | Switch between thumbs |
| ← / → | Move focused thumb |
| Shift+← / → | Move by larger step |
| Home | Move to minimum |
| End | Move to maximum |

## Events

| Event | Description |
|-------|-------------|
| LowerValueChanged | Lower value changed |
| UpperValueChanged | Upper value changed |
| RangeChanged | Either value changed |
| DragStarted | Thumb drag started |
| DragCompleted | Thumb drag completed |

## Commands

| Command | Description |
|---------|-------------|
| LowerValueChangedCommand | Execute when lower value changes |
| UpperValueChangedCommand | Execute when upper value changes |
| RangeChangedCommand | Execute when either value changes |

## Validation

RangeSlider implements `IValidatable` for built-in validation support.

```xml
<extras:RangeSlider
    Minimum="0"
    Maximum="100"
    LowerValue="{Binding MinPrice}"
    UpperValue="{Binding MaxPrice}"
    IsRequired="True"
    RequiredErrorMessage="Please select a valid price range"
    ValidateCommand="{Binding OnValidationCommand}" />
```

### Checking Validation State

```csharp
if (!rangeSlider.IsValid)
{
    foreach (var error in rangeSlider.ValidationErrors)
    {
        Debug.WriteLine(error);
    }
}

// Trigger validation manually
var result = rangeSlider.Validate();
```

### Validation Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| IsRequired | bool | false | Whether a valid range is required |
| RequiredErrorMessage | string | "This field is required." | Error message when validation fails |
| IsValid | bool | (read-only) | Current validation state |
| ValidationErrors | IReadOnlyList&lt;string&gt; | (read-only) | List of validation error messages |
| ValidateCommand | ICommand | null | Command executed when validation occurs |

## Properties

| Property | Type | Description |
|----------|------|-------------|
| Minimum | double | Minimum allowed value |
| Maximum | double | Maximum allowed value |
| LowerValue | double | Current lower value |
| UpperValue | double | Current upper value |
| Step | double | Snap increment |
| MinimumRange | double | Minimum distance between thumbs |
| ShowValueLabels | bool | Display value labels |
| ValueLabelFormat | string | Format for labels |
| TrackColor | Color | Track background color |
| RangeColor | Color | Selected range color |
| ThumbColor | Color | Thumb button color |
