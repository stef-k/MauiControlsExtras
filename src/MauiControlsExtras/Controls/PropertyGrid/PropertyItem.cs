using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MauiControlsExtras.Helpers;

namespace MauiControlsExtras.Controls;

/// <summary>
/// Represents a single property item in the PropertyGrid.
/// </summary>
public class PropertyItem : INotifyPropertyChanged
{
    private object? _value;
    private bool _isExpanded;
    private bool _isSelected;
    private readonly Func<object, object?>? _getterFunc;
    private readonly Action<object, object?>? _setterFunc;

    /// <summary>
    /// Gets the property info (null when constructed from metadata).
    /// </summary>
    internal PropertyInfo? PropertyInfo { get; }

    /// <summary>
    /// Gets the target object that contains this property.
    /// </summary>
    public object Target { get; }

    /// <summary>
    /// Gets the property name (programmatic).
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the display name (from DisplayNameAttribute or property name).
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the description (from DescriptionAttribute).
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the category (from CategoryAttribute or "Misc").
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// Gets the property type.
    /// </summary>
    public Type PropertyType { get; }

    /// <summary>
    /// Gets the property type name for display.
    /// </summary>
    public string TypeName => GetFriendlyTypeName(PropertyType);

    /// <summary>
    /// Gets whether this property is read-only.
    /// </summary>
    public bool IsReadOnly { get; }

    /// <summary>
    /// Gets whether this property can be expanded (has sub-properties).
    /// </summary>
    public bool IsExpandable { get; }

    /// <summary>
    /// Gets the sub-properties for expandable items.
    /// </summary>
    public IReadOnlyList<PropertyItem> SubProperties { get; }

    /// <summary>
    /// Gets the minimum value (from RangeAttribute).
    /// </summary>
    public object? Minimum { get; }

    /// <summary>
    /// Gets the maximum value (from RangeAttribute).
    /// </summary>
    public object? Maximum { get; }

    /// <summary>
    /// Gets the custom editor type (from EditorAttribute).
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type? EditorType { get; }

    /// <summary>
    /// Gets or sets the property value.
    /// </summary>
    public object? Value
    {
        get => _value;
        set
        {
            if (!Equals(_value, value))
            {
                if (IsReadOnly)
                    return;

                var oldValue = _value;
                var changingArgs = new PropertyValueChangingEventArgs(this, oldValue, value);
                ValueChanging?.Invoke(this, changingArgs);
                if (changingArgs.Cancel)
                {
                    return;
                }

                // Update the actual property first; only commit _value on success
                try
                {
                    if (_setterFunc != null)
                    {
                        _setterFunc(Target, value);
                    }
                    else if (PropertyInfo is { CanWrite: true })
                    {
                        SetValueViaReflection(value);
                    }

                    _value = value;
                }
                catch (Exception ex) when (ex is InvalidCastException or FormatException
                    or OverflowException or ArgumentException or InvalidOperationException
                    or TargetInvocationException)
                {
                    System.Diagnostics.Debug.WriteLine($"PropertyItem '{Name}': failed to set value: {ex.Message}");
                    // Re-sync _value with the actual state of the target
                    try { _value = _getterFunc != null ? _getterFunc(Target) : PropertyInfo?.GetValue(Target); }
                    catch { /* best-effort resync */ }
                    return;
                }

                OnPropertyChanged(nameof(Value));
                OnPropertyChanged(nameof(DisplayValue));
                ValueChanged?.Invoke(this, new PropertyValueChangedEventArgs(this, oldValue, value));
            }
        }
    }

    /// <summary>
    /// Gets the display value (formatted for display).
    /// </summary>
    public string DisplayValue
    {
        get
        {
            if (Value == null) return "(null)";
            if (PropertyType == typeof(Color)) return ((Color)Value).ToHex();
            if (PropertyType.IsEnum) return Enum.GetName(PropertyType, Value) ?? Value.ToString() ?? "";
            return Value.ToString() ?? "";
        }
    }

    /// <summary>
    /// Gets or sets whether this item is expanded (for expandable items).
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether this item is selected.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }

    /// <summary>
    /// Occurs before the property value changes (cancelable).
    /// </summary>
    public event EventHandler<PropertyValueChangingEventArgs>? ValueChanging;

    /// <summary>
    /// Occurs when the property value changes.
    /// </summary>
    public event EventHandler<PropertyValueChangedEventArgs>? ValueChanged;

    /// <summary>
    /// Occurs when a property changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyItem"/> class using reflection.
    /// </summary>
    [RequiresUnreferencedCode("Uses reflection to discover property attributes. Use PropertyItem(PropertyMetadataEntry, object) for AOT compatibility.")]
    internal PropertyItem(PropertyInfo propertyInfo, object target)
    {
        PropertyInfo = propertyInfo;
        Target = target;
        Name = propertyInfo.Name;
        PropertyType = propertyInfo.PropertyType;

        // Get display name
        var displayNameAttr = propertyInfo.GetCustomAttribute<DisplayNameAttribute>();
        DisplayName = displayNameAttr?.DisplayName ?? propertyInfo.Name;

        // Get description
        var descAttr = propertyInfo.GetCustomAttribute<DescriptionAttribute>();
        Description = descAttr?.Description;

        // Get category
        var categoryAttr = propertyInfo.GetCustomAttribute<CategoryAttribute>();
        Category = categoryAttr?.Category ?? "Misc";

        // Check if read-only
        var readOnlyAttr = propertyInfo.GetCustomAttribute<ReadOnlyAttribute>();
        IsReadOnly = readOnlyAttr?.IsReadOnly ?? !propertyInfo.CanWrite;

        // Get range
        var rangeAttr = propertyInfo.GetCustomAttribute<RangeAttribute>();
        Minimum = rangeAttr?.Minimum;
        Maximum = rangeAttr?.Maximum;

        // Get custom editor
        var editorAttr = propertyInfo.GetCustomAttribute<EditorAttribute>();
        if (editorAttr != null && !string.IsNullOrEmpty(editorAttr.EditorTypeName))
        {
            EditorType = Type.GetType(editorAttr.EditorTypeName);
        }

        // Check if expandable (complex object that isn't a string or primitive)
        IsExpandable = !PropertyType.IsPrimitive
                       && PropertyType != typeof(string)
                       && PropertyType != typeof(decimal)
                       && PropertyType != typeof(DateTime)
                       && PropertyType != typeof(TimeSpan)
                       && PropertyType != typeof(Color)
                       && !PropertyType.IsEnum
                       && PropertyType.GetProperties().Length > 0;

        // Get sub-properties for expandable items
        if (IsExpandable)
        {
            var subProps = new List<PropertyItem>();
            var currentValue = propertyInfo.GetValue(target);
            if (currentValue != null)
            {
                foreach (var subProp in PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (subProp.GetCustomAttribute<BrowsableAttribute>()?.Browsable != false)
                    {
                        subProps.Add(new PropertyItem(subProp, currentValue));
                    }
                }
            }
            SubProperties = subProps;
        }
        else
        {
            SubProperties = Array.Empty<PropertyItem>();
        }

        // Get initial value
        _value = propertyInfo.GetValue(target);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyItem"/> class from registered metadata (AOT-safe).
    /// </summary>
    internal PropertyItem(PropertyMetadataEntry metadata, object target)
    {
        Target = target;
        Name = metadata.Name;
        DisplayName = metadata.DisplayName ?? metadata.Name;
        Description = metadata.Description;
        Category = metadata.Category ?? "Misc";
        IsReadOnly = metadata.IsReadOnly;
        PropertyType = metadata.PropertyType;
        Minimum = metadata.Minimum;
        Maximum = metadata.Maximum;
        EditorType = metadata.EditorType;
        _getterFunc = metadata.GetValue;
        _setterFunc = metadata.SetValue;

        if (!IsReadOnly && _setterFunc == null)
            throw new ArgumentException(
                $"PropertyMetadataEntry '{metadata.Name}' has IsReadOnly=false but SetValue is null. "
                + "Provide a SetValue action or set IsReadOnly=true.",
                nameof(metadata));

        // Read the initial value once and reuse for both sub-property resolution and _value
        object? initialValue;
        try
        {
            initialValue = _getterFunc(target);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            initialValue = null;
        }

        // Build sub-properties from metadata.
        // Note: Sub-property targets are captured at construction time. If the parent
        // property value is replaced, sub-properties will reference the old object.
        // The PropertyGrid rebuilds items on SelectedObject change, so this is
        // only relevant if the parent value is replaced externally without re-binding.
        // Value types (structs) are boxed copies — sub-property setters would
        // mutate the copy without propagating back to the parent field.
        // Skip expansion for value types to prevent silent data loss.
        if (metadata.SubProperties is { Count: > 0 })
        {
            if (initialValue != null && !metadata.PropertyType.IsValueType)
            {
                IsExpandable = true;
                SubProperties = metadata.SubProperties
                    .Select(sub => new PropertyItem(sub, initialValue))
                    .ToList();
            }
            else
            {
                IsExpandable = false;
                SubProperties = Array.Empty<PropertyItem>();
            }
        }
        else
        {
            IsExpandable = false;
            SubProperties = Array.Empty<PropertyItem>();
        }

        _value = initialValue;
    }

    /// <summary>
    /// Refreshes the value from the target object.
    /// </summary>
    public void RefreshValue()
    {
        var newValue = _getterFunc != null
            ? _getterFunc(Target)
            : PropertyInfo?.GetValue(Target);
        if (!Equals(_value, newValue))
        {
            _value = newValue;
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(DisplayValue));
        }
    }

    [UnconditionalSuppressMessage("AOT", "IL2026:RequiresUnreferencedCode",
        Justification = "Reflection fallback for PropertyInfo-based items. Metadata-based items use _setterFunc.")]
    private void SetValueViaReflection(object? value)
    {
        PropertyInfo!.SetValue(Target, PropertyAccessor.ConvertToType(value, PropertyType));
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static string GetFriendlyTypeName(Type type)
    {
        if (type == typeof(int)) return "int";
        if (type == typeof(string)) return "string";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(double)) return "double";
        if (type == typeof(float)) return "float";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(DateTime)) return "DateTime";
        if (type == typeof(TimeSpan)) return "TimeSpan";
        if (type == typeof(Color)) return "Color";
        if (type.IsGenericType)
        {
            var name = type.Name;
            var index = name.IndexOf('`');
            if (index > 0) name = name[..index];
            var args = string.Join(", ", type.GetGenericArguments().Select(GetFriendlyTypeName));
            return $"{name}<{args}>";
        }
        return type.Name;
    }
}
