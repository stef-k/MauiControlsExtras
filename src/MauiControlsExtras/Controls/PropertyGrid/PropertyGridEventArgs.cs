namespace MauiControlsExtras.Controls;

/// <summary>
/// Event arguments for property value changes in the PropertyGrid.
/// </summary>
public class PropertyValueChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the property item that changed.
    /// </summary>
    public PropertyItem Property { get; }

    /// <summary>
    /// Gets the old value.
    /// </summary>
    public object? OldValue { get; }

    /// <summary>
    /// Gets the new value.
    /// </summary>
    public object? NewValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyValueChangedEventArgs"/> class.
    /// </summary>
    public PropertyValueChangedEventArgs(PropertyItem property, object? oldValue, object? newValue)
    {
        Property = property;
        OldValue = oldValue;
        NewValue = newValue;
    }
}

/// <summary>
/// Event arguments for property value changing (cancelable).
/// </summary>
public class PropertyValueChangingEventArgs : EventArgs
{
    /// <summary>
    /// Gets the property item that is changing.
    /// </summary>
    public PropertyItem Property { get; }

    /// <summary>
    /// Gets the current value.
    /// </summary>
    public object? CurrentValue { get; }

    /// <summary>
    /// Gets the proposed new value.
    /// </summary>
    public object? NewValue { get; }

    /// <summary>
    /// Gets or sets whether the change should be cancelled.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyValueChangingEventArgs"/> class.
    /// </summary>
    public PropertyValueChangingEventArgs(PropertyItem property, object? currentValue, object? newValue)
    {
        Property = property;
        CurrentValue = currentValue;
        NewValue = newValue;
    }
}

/// <summary>
/// Event arguments for selected object changes.
/// </summary>
public class SelectedObjectChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the old selected object.
    /// </summary>
    public object? OldObject { get; }

    /// <summary>
    /// Gets the new selected object.
    /// </summary>
    public object? NewObject { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectedObjectChangedEventArgs"/> class.
    /// </summary>
    public SelectedObjectChangedEventArgs(object? oldObject, object? newObject)
    {
        OldObject = oldObject;
        NewObject = newObject;
    }
}

/// <summary>
/// Event arguments for property selection changes.
/// </summary>
public class PropertySelectionChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the previously selected property.
    /// </summary>
    public PropertyItem? OldProperty { get; }

    /// <summary>
    /// Gets the newly selected property.
    /// </summary>
    public PropertyItem? NewProperty { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertySelectionChangedEventArgs"/> class.
    /// </summary>
    public PropertySelectionChangedEventArgs(PropertyItem? oldProperty, PropertyItem? newProperty)
    {
        OldProperty = oldProperty;
        NewProperty = newProperty;
    }
}
