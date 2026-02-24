namespace MauiControlsExtras.Controls;

/// <summary>
/// Defines metadata for a single property, enabling AOT-safe property grid editing
/// without runtime reflection. Register entries via <see cref="PropertyGrid.RegisterMetadata"/>.
/// </summary>
public class PropertyMetadataEntry
{
    /// <summary>
    /// Gets the property name (must match the actual property name on the target object).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the display name shown in the property grid. Defaults to <see cref="Name"/> if null.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Gets the description shown in the property grid description panel.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the category for grouping. Defaults to "Misc" if null.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Gets whether this property is read-only.
    /// </summary>
    public bool IsReadOnly { get; init; }

    /// <summary>
    /// Gets the property type (used for editor selection).
    /// </summary>
    public required Type PropertyType { get; init; }

    /// <summary>
    /// Gets the minimum value (for numeric editors).
    /// </summary>
    public object? Minimum { get; init; }

    /// <summary>
    /// Gets the maximum value (for numeric editors).
    /// </summary>
    public object? Maximum { get; init; }

    /// <summary>
    /// Gets the custom editor type, if any.
    /// </summary>
    public Type? EditorType { get; init; }

    /// <summary>
    /// Gets the AOT-safe getter function. Required.
    /// </summary>
    public required Func<object, object?> GetValue { get; init; }

    /// <summary>
    /// Gets the AOT-safe setter action. Null for read-only properties.
    /// </summary>
    public Action<object, object?>? SetValue { get; init; }

    /// <summary>
    /// Gets sub-property metadata for expandable complex types.
    /// </summary>
    public IReadOnlyList<PropertyMetadataEntry>? SubProperties { get; init; }
}
