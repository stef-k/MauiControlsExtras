using System.Diagnostics.CodeAnalysis;

namespace MauiControlsExtras.Controls;

/// <summary>
/// Interface for custom property editors in the PropertyGrid.
/// </summary>
public interface IPropertyEditor
{
    /// <summary>
    /// Creates the editor view for a property.
    /// </summary>
    /// <param name="property">The property item to edit.</param>
    /// <returns>The editor view.</returns>
    View CreateEditor(PropertyItem property);

    /// <summary>
    /// Gets whether this editor supports the specified property type.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>True if supported, false otherwise.</returns>
    bool SupportsType(Type propertyType);
}

/// <summary>
/// Base implementation of IPropertyEditor.
/// </summary>
public abstract class PropertyEditorBase : IPropertyEditor
{
    /// <inheritdoc/>
    public abstract View CreateEditor(PropertyItem property);

    /// <inheritdoc/>
    public abstract bool SupportsType(Type propertyType);

    /// <summary>
    /// Creates a binding to the property value.
    /// </summary>
    [DynamicDependency(nameof(PropertyItem.Value), typeof(PropertyItem))]
    [UnconditionalSuppressMessage("AOT", "IL2026:RequiresUnreferencedCode",
        Justification = "Binds to PropertyItem.Value which is preserved via DynamicDependency.")]
    protected Binding CreateValueBinding(PropertyItem property, BindingMode mode = BindingMode.TwoWay)
    {
        return new Binding(nameof(PropertyItem.Value), mode, source: property);
    }
}
