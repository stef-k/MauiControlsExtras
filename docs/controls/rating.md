# Rating

A star/icon-based rating control.

## Features

- **Configurable Rating** - Set min, max, and step values
- **Custom Icons** - Use any icon or symbol
- **Read-Only Mode** - Display ratings without interaction
- **Half Stars** - Support for fractional ratings
- **Keyboard Navigation** - Full keyboard support

## Basic Usage

```xml
<extras:Rating
    Value="{Binding UserRating, Mode=TwoWay}"
    Maximum="5" />
```

## Half Star Ratings

```xml
<extras:Rating
    Value="{Binding Rating}"
    Maximum="5"
    Step="0.5"
    AllowHalfStars="True" />
```

## Custom Icons

```xml
<extras:Rating
    Value="{Binding Rating}"
    EmptyIcon="&#xE734;"
    FilledIcon="&#xE735;"
    IconFontFamily="Segoe MDL2 Assets" />
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

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| ← | Decrease rating |
| → | Increase rating |
| Home | Set to minimum |
| End | Set to maximum |
| 1-5 | Set specific value |

## Events

| Event | Description |
|-------|-------------|
| ValueChanged | Rating value changed |

## Commands

| Command | Description |
|---------|-------------|
| ValueChangedCommand | Execute when rating changes |

## Properties

| Property | Type | Description |
|----------|------|-------------|
| Value | double | Current rating value |
| Minimum | double | Minimum rating (default: 0) |
| Maximum | double | Maximum rating (default: 5) |
| Step | double | Rating increment (default: 1) |
| AllowHalfStars | bool | Allow fractional values |
| IsReadOnly | bool | Disable interaction |
| FilledColor | Color | Color of filled stars |
| EmptyColor | Color | Color of empty stars |
| IconSize | double | Size of rating icons |
| Spacing | double | Space between icons |
| EmptyIcon | string | Icon for empty state |
| FilledIcon | string | Icon for filled state |
