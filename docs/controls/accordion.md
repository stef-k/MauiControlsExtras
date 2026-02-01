# Accordion

An accordion control with expandable/collapsible sections.

## Features

- **Expand Modes** - Single or multiple sections open
- **Animated Expansion** - Smooth expand/collapse animations
- **Custom Icons** - Configurable expand/collapse icons
- **Header Customization** - Style headers with colors and fonts
- **Keyboard Navigation** - Full keyboard support

## Basic Usage

```xml
<extras:Accordion>
    <extras:AccordionItem Header="Section 1">
        <Label Text="Content for section 1" />
    </extras:AccordionItem>

    <extras:AccordionItem Header="Section 2">
        <Label Text="Content for section 2" />
    </extras:AccordionItem>

    <extras:AccordionItem Header="Section 3">
        <Label Text="Content for section 3" />
    </extras:AccordionItem>
</extras:Accordion>
```

## Expand Modes

```xml
<!-- Single expansion (default) - only one section open at a time -->
<extras:Accordion ExpandMode="Single" />

<!-- Multiple expansion - any number of sections can be open -->
<extras:Accordion ExpandMode="Multiple" />

<!-- At least one must be open -->
<extras:Accordion ExpandMode="AtLeastOne" />
```

## Pre-expanded Items

```xml
<extras:Accordion>
    <extras:AccordionItem Header="Expanded by Default" IsExpanded="True">
        <Label Text="This section starts expanded" />
    </extras:AccordionItem>

    <extras:AccordionItem Header="Collapsed by Default">
        <Label Text="This section starts collapsed" />
    </extras:AccordionItem>
</extras:Accordion>
```

## Icon Position

```xml
<!-- Left side (default) -->
<extras:Accordion IconPosition="Left" />

<!-- Right side -->
<extras:Accordion IconPosition="Right" />

<!-- No icons -->
<extras:Accordion ShowIcons="False" />
```

## Custom Icons

```xml
<extras:AccordionItem Header="Custom Icons"
                      ExpandedIcon="▼"
                      CollapsedIcon="▶">
    <Label Text="Content" />
</extras:AccordionItem>
```

## Header Styling

```xml
<extras:Accordion
    HeaderBackgroundColor="#F5F5F5"
    HeaderTextColor="#333333"
    HeaderFontSize="18"
    HeaderFontAttributes="Bold"
    HeaderFontFamily="OpenSans"
    HeaderPadding="16,12" />
```

### Removing Bold Headers

Headers are bold by default. To use regular weight:

```xml
<extras:Accordion HeaderFontAttributes="None" />
```

### Legacy Styling

To match pre-migration styling (smaller, non-bold headers):

```xml
<extras:Accordion
    HeaderFontSize="14"
    HeaderFontAttributes="None"
    HeaderPadding="12,10" />
```

## Content Styling

```xml
<extras:Accordion
    ContentPadding="12,8"
    ShowDividers="True" />
```

## Animation

```xml
<!-- With animation (default) -->
<extras:Accordion AnimateExpansion="True" AnimationDuration="200" />

<!-- Without animation -->
<extras:Accordion AnimateExpansion="False" />
```

## Data Binding

```xml
<extras:Accordion ItemsSource="{Binding Sections}">
    <extras:Accordion.ItemTemplate>
        <DataTemplate>
            <extras:AccordionItem Header="{Binding Title}">
                <Label Text="{Binding Description}" />
            </extras:AccordionItem>
        </DataTemplate>
    </extras:Accordion.ItemTemplate>
</extras:Accordion>
```

## Code-Behind Operations

```csharp
// Expand/collapse programmatically
accordion.ExpandItem(0);
accordion.CollapseItem(0);
accordion.ToggleItem(0);

// Expand/collapse all
accordion.ExpandAll();
accordion.CollapseAll();

// Get expanded items
var expandedItems = accordion.GetExpandedItems();

// Check if item is expanded
bool isExpanded = accordion.IsItemExpanded(0);
```

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| ↑ | Select previous item |
| ↓ | Select next item |
| Home | Select first item |
| End | Select last item |
| Enter | Toggle selected item |
| Space | Toggle selected item |
| → | Expand selected item |
| ← | Collapse selected item |

## Events

| Event | Description |
|-------|-------------|
| ItemExpanded | Section was expanded |
| ItemCollapsed | Section was collapsed |
| ItemExpanding | Section is about to expand (cancelable) |
| ItemCollapsing | Section is about to collapse (cancelable) |

## Commands

| Command | Description |
|---------|-------------|
| ItemExpandedCommand | Execute when item expands |
| ItemCollapsedCommand | Execute when item collapses |

## Accordion Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Items | ObservableCollection&lt;AccordionItem&gt; | empty | Accordion items |
| ExpandMode | AccordionExpandMode | Single | Single, Multiple, AtLeastOne |
| IconPosition | ExpandIconPosition | Left | Left or Right |
| ShowIcons | bool | true | Show expand/collapse icons |
| AnimateExpansion | bool | true | Animate expand/collapse |
| AnimationDuration | uint | 200 | Animation duration (ms) |
| HeaderBackgroundColor | Color | null | Header background |
| HeaderTextColor | Color | null | Header text color |
| HeaderFontSize | double | 16 | Header font size |
| HeaderFontAttributes | FontAttributes | Bold | Header font style (Bold, Italic, None) |
| HeaderFontFamily | string | null | Header font family |
| HeaderPadding | Thickness | 12,8 | Header padding |
| HeaderHeight | double | -1 | Header height (-1 = auto) |
| HeaderBorderColor | Color | null | Header border color |
| HeaderBorderThickness | Thickness | 0,0,0,1 | Header border thickness |
| ContentPadding | Thickness | 12,8 | Content padding |
| ShowDividers | bool | true | Show section dividers |

## AccordionItem Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Header | string | null | Header text |
| HeaderTemplate | DataTemplate | null | Custom header template |
| Content | View | null | Section content |
| IsExpanded | bool | false | Whether section is expanded |
| IsEnabled | bool | true | Whether section is interactive |
| ExpandedIcon | string | "▼" | Icon when expanded |
| CollapsedIcon | string | "▶" | Icon when collapsed |
| Tag | object | null | Custom data |

## Enums

### AccordionExpandMode

| Value | Description |
|-------|-------------|
| Single | Only one section can be open |
| Multiple | Multiple sections can be open |
| AtLeastOne | At least one section must be open |

### ExpandIconPosition

| Value | Description |
|-------|-------------|
| Left | Icon on left side |
| Right | Icon on right side |
