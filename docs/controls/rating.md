# Rating

A star/icon-based rating control.

## Features

- **Configurable Rating** - Set maximum value and precision mode
- **Custom Icons** - Choose from Star, Heart, Circle, or Thumb icons
- **Read-Only Mode** - Display ratings without interaction
- **Precision Modes** - Support for full, half, or exact ratings
- **Keyboard Navigation** - Full keyboard support

## Basic Usage

```xml
<extras:Rating
    Value="{Binding UserRating, Mode=TwoWay}"
    Maximum="5" />
```

## Precision Modes

The `Precision` property controls the rating granularity:

### Full Precision (Default)

Only whole numbers (1, 2, 3, etc.):

```xml
<extras:Rating
    Value="{Binding Rating}"
    Maximum="5"
    Precision="Full" />
```

### Half Precision

Whole and half values (1, 1.5, 2, 2.5, etc.):

```xml
<extras:Rating
    Value="{Binding Rating}"
    Maximum="5"
    Precision="Half" />
```

### Exact Precision

Any decimal value (useful for displaying averages):

```xml
<extras:Rating
    Value="{Binding AverageRating}"
    Maximum="5"
    Precision="Exact"
    IsReadOnly="True" />
```

## Custom Icons

Use the `Icon` property to choose from built-in icon types:

```xml
<!-- Star icons (default) -->
<extras:Rating Value="{Binding Rating}" Icon="Star" />

<!-- Heart icons -->
<extras:Rating Value="{Binding Rating}" Icon="Heart" />

<!-- Circle icons -->
<extras:Rating Value="{Binding Rating}" Icon="Circle" />

<!-- Thumbs up icons -->
<extras:Rating Value="{Binding Rating}" Icon="Thumb" />
```

## Read-Only Display

```xml
<extras:Rating
    Value="{Binding AverageRating}"
    IsReadOnly="True" />
```

## Customization

```xml
<extras:Rating
    Value="{Binding Rating}"
    FilledColor="#FFD700"
    EmptyColor="#E0E0E0"
    IconSize="32"
    Spacing="8" />
```

## Allow Clear

By default, tapping the current rating value clears it to 0. Disable this behavior:

```xml
<extras:Rating
    Value="{Binding Rating}"
    AllowClear="False" />
```

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| ← / ↓ | Decrease rating |
| → / ↑ | Increase rating |
| Home / Delete | Clear rating (set to 0) |
| End | Set to maximum |
| 1-5 | Set specific value |

## Events

| Event | Description |
|-------|-------------|
| ValueChanged | Rating value changed |
| Cleared | Rating was cleared to 0 |
| ValidationChanged | Validation state changed |

## Commands

| Command | Description |
|---------|-------------|
| ValueChangedCommand | Execute when rating changes |
| ClearedCommand | Execute when rating is cleared |

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Value | double | 0 | Current rating value |
| Maximum | int | 5 | Maximum rating (number of icons) |
| Precision | RatingPrecision | Full | Rating granularity mode |
| Icon | RatingIcon | Star | Icon type to display |
| IsReadOnly | bool | false | Disable interaction |
| AllowClear | bool | true | Allow tapping same value to clear |
| FilledColor | Color | #FFD700 | Color of filled icons |
| EmptyColor | Color | #E0E0E0 | Color of empty icons |
| IconSize | double | 32 | Size of rating icons |
| Spacing | double | 4 | Space between icons |

## Enums

### RatingPrecision

| Value | Description |
|-------|-------------|
| Full | Only whole numbers (1, 2, 3, etc.) |
| Half | Whole and half values (1, 1.5, 2, 2.5, etc.) |
| Exact | Any decimal value (display only, for averages) |

### RatingIcon

| Value | Description |
|-------|-------------|
| Star | Star icon (★/☆) - default |
| Heart | Heart icon (♥/♡) |
| Circle | Circle icon (●/○) |
| Thumb | Thumbs up icon (👍) |

## Validation Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| IsRequired | bool | false | Whether a rating is required |
| RequiredErrorMessage | string | "A rating is required." | Error message when required but not provided |
| MinimumValue | double? | null | Minimum required value for validation |
| MinimumValueErrorMessage | string | "Rating must be at least {0}." | Error message when below minimum |
| IsValid | bool | true | Whether current value passes validation |
| ValidationErrors | IReadOnlyList\<string\> | - | List of current validation errors |
| ValidateCommand | ICommand | null | Command executed when validation is triggered |
