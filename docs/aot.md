# AOT & Trimming Support

MAUI Controls Extras is fully compatible with **NativeAOT** and **IL trimming** since v3.0.0. The library's project file enables `IsTrimmable`, `IsAotCompatible`, and `EnableTrimAnalyzer` — no additional configuration is needed in your app.

## No Changes Needed for Most Controls

Controls that don't use reflection-based property paths — such as Accordion, Calendar, Wizard, NumericUpDown, RangeSlider, Rating, Breadcrumb, BindingNavigator, RichTextEditor, and TokenEntry — work out of the box with AOT and trimming.

Internally, all effective-property bindings, value converters, and CLR property getters are protected via `[DynamicDependency]` and `[Preserve]` attributes. You don't need to do anything for these.

## Controls with Reflection-Based Property Paths

The following controls offer string-based property paths that use reflection at runtime. In AOT/trimmed builds, the trimmer may remove the reflected members. Each control provides **Func-based alternatives** that are AOT-safe:

| Control | String Path | Func Alternative |
|---------|------------|------------------|
| [ComboBox](controls/combobox.md) | `DisplayMemberPath` | `DisplayMemberFunc` |
| | `ValueMemberPath` | `ValueMemberFunc` |
| | `IconMemberPath` | `IconMemberFunc` |
| [MultiSelectComboBox](controls/multiselectcombobox.md) | `DisplayMemberPath` | `DisplayMemberFunc` |
| [TreeView](controls/treeview.md) | `DisplayMemberPath` | `DisplayMemberFunc` |
| | `ChildrenPath` | `ChildrenFunc` |
| | `IconMemberPath` | `IconMemberFunc` |
| | `IsExpandedPath` | `IsExpandedFunc` |
| | `HasChildrenPath` | `HasChildrenFunc` |
| [DataGridView](controls/datagridview.md) columns | `PropertyPath` | `CellValueFunc` / `CellValueSetter` |
| [DataGridComboBoxColumn](controls/datagridview.md) | `DisplayMemberPath` | `DisplayMemberFunc` |
| | `SelectedValuePath` | `SelectedValueFunc` |
| [PropertyGrid](controls/propertygrid.md) | Reflection discovery | `RegisterMetadata<T>()` |

## Recommended Approach: Func-Based Properties

Set the Func-based properties in code-behind or your ViewModel setup. When set, they take priority over the string-based paths.

```csharp
// ComboBox
myComboBox.DisplayMemberFunc = item => ((Country)item).Name;
myComboBox.ValueMemberFunc   = item => ((Country)item).Id;
myComboBox.IconMemberFunc    = item => ((Country)item).FlagPath;

// TreeView
myTreeView.DisplayMemberFunc = item => ((TreeNode)item).Name;
myTreeView.ChildrenFunc      = item => ((TreeNode)item).Children;

// DataGrid column
var nameColumn = new DataGridTextColumn
{
    Header = "Name",
    Binding = "Name",
    CellValueFunc   = item => ((Product)item).Name,
    CellValueSetter = (item, value) => ((Product)item).Name = (string)value!
};
```

## Alternative: ItemTemplate with Compiled Bindings

For controls that support `ItemTemplate`, you can use compiled bindings in XAML instead:

```xml
<extras:ComboBox ItemsSource="{Binding Countries}">
    <extras:ComboBox.ItemTemplate>
        <DataTemplate x:DataType="models:Country">
            <Label Text="{Binding Name}" />
        </DataTemplate>
    </extras:ComboBox.ItemTemplate>
</extras:ComboBox>
```

This approach is AOT-safe because the MAUI XAML compiler resolves bindings at build time.

## Alternative: Preserve Model Types

If you prefer to keep using string-based property paths, annotate your model types to prevent the trimmer from removing their properties:

```csharp
using System.Diagnostics.CodeAnalysis;

[DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(Country))]
void ConfigureComboBox()
{
    myComboBox.DisplayMemberPath = "Name";
    myComboBox.ValueMemberPath = "Id";
}
```

## PropertyGrid Metadata Registration

PropertyGrid uses reflection extensively to discover properties. For AOT/trimmed builds, register metadata at startup instead:

```csharp
PropertyGrid.RegisterMetadata<Product>(
    new PropertyMetadataEntry
    {
        Name = "Name",
        DisplayName = "Product Name",
        Category = "General",
        PropertyType = typeof(string),
        GetValue = obj => ((Product)obj).Name,
        SetValue = (obj, val) => ((Product)obj).Name = (string)val!
    }
);
```

When metadata is registered for a type, PropertyGrid uses it instead of reflection. See the [PropertyGrid documentation](controls/propertygrid.md) for the full API.

> **Note:** Metadata matching is exact-type only. Registering metadata for a base class does **not** cover derived types — register each concrete type separately.

## What the Library Handles Internally

All internal plumbing is already annotated for AOT safety — you don't need to take any action for these:

- **Effective-property bindings** — `[DynamicDependency]` attributes protect the CLR property getters that back each `EffectiveX` property across all controls.
- **Value converters** — `[Preserve]` attributes ensure internal converters (e.g., `InverseBoolConverter`, `StringNotNullOrEmptyConverter`) survive trimming.
- **Control visual trees** — All controls that build their visual tree in code are fully preserved.

The only area where user action may be needed is when **your model types** are accessed via string-based property paths, as described in the sections above.
