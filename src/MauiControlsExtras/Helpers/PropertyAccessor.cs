using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace MauiControlsExtras.Helpers;

/// <summary>
/// Centralized, cached property accessor for string-based property paths.
/// Provides AOT-annotated reflection helpers used as fallback when Func-based
/// accessors are not configured.
/// </summary>
internal static class PropertyAccessor
{
    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> _cache = new();

    /// <summary>
    /// Gets a property value from an object using a string property path.
    /// </summary>
    [RequiresUnreferencedCode("Use the corresponding *Func property for AOT/trimming compatibility.")]
    internal static object? GetValue(object item, string propertyPath)
    {
        if (string.IsNullOrEmpty(propertyPath))
            return null;

        var property = GetPropertyInfo(item.GetType(), propertyPath);
        return property?.GetValue(item);
    }

    /// <summary>
    /// Sets a property value on an object using a string property path.
    /// </summary>
    /// <returns>True if the property was found and set; false otherwise.</returns>
    [RequiresUnreferencedCode("Use the corresponding *Func property for AOT/trimming compatibility.")]
    internal static bool SetValue(object item, string propertyPath, object? value)
    {
        if (string.IsNullOrEmpty(propertyPath))
            return false;

        var property = GetPropertyInfo(item.GetType(), propertyPath);
        if (property == null || !property.CanWrite)
            return false;

        var convertedValue = ConvertToType(value, property.PropertyType);
        property.SetValue(item, convertedValue);
        return true;
    }

    /// <summary>
    /// Gets the cached PropertyInfo for a type/property path combination.
    /// </summary>
    [RequiresUnreferencedCode("Reflection-based property access may not work under AOT/trimming.")]
    private static PropertyInfo? GetPropertyInfo(Type type, string propertyPath)
    {
        return _cache.GetOrAdd((type, propertyPath), key => key.Item1.GetProperty(key.Item2));
    }

    /// <summary>
    /// Returns the default value for a given type without using Activator.CreateInstance for known types.
    /// </summary>
    internal static object? GetDefaultValue(Type targetType)
    {
        if (!targetType.IsValueType)
            return null;

        if (targetType == typeof(int)) return 0;
        if (targetType == typeof(long)) return 0L;
        if (targetType == typeof(double)) return 0.0;
        if (targetType == typeof(float)) return 0.0f;
        if (targetType == typeof(decimal)) return 0m;
        if (targetType == typeof(bool)) return false;
        if (targetType == typeof(DateTime)) return default(DateTime);
        if (targetType == typeof(TimeSpan)) return default(TimeSpan);
        if (targetType == typeof(byte)) return (byte)0;
        if (targetType == typeof(short)) return (short)0;
        if (targetType == typeof(char)) return '\0';
        if (targetType == typeof(Guid)) return Guid.Empty;
        if (targetType == typeof(DateTimeOffset)) return default(DateTimeOffset);

        // Nullable<T> defaults to null
        if (Nullable.GetUnderlyingType(targetType) != null)
            return null;

        // Fallback for other value types (enums, custom structs)
        return Activator.CreateInstance(targetType);
    }

    /// <summary>
    /// Converts a value to the specified target type, using direct casts for
    /// common types to avoid reflection-heavy paths.
    /// </summary>
    internal static object? ConvertToType(object? value, Type targetType)
    {
        if (value == null)
            return GetDefaultValue(targetType);

        var valueType = value.GetType();
        if (targetType.IsAssignableFrom(valueType))
            return value;

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
        {
            if (value is string s && string.IsNullOrWhiteSpace(s))
                return null;
            targetType = underlyingType;
        }

        // Handle empty string for value types
        if (value is string str && string.IsNullOrWhiteSpace(str))
            return GetDefaultValue(targetType);

        try
        {
            // Direct casts for common types (avoids Convert.ChangeType reflection)
            if (targetType == typeof(decimal)) return Convert.ToDecimal(value);
            if (targetType == typeof(double)) return Convert.ToDouble(value);
            if (targetType == typeof(float)) return Convert.ToSingle(value);
            if (targetType == typeof(int)) return Convert.ToInt32(value);
            if (targetType == typeof(long)) return Convert.ToInt64(value);
            if (targetType == typeof(bool)) return Convert.ToBoolean(value);
            if (targetType == typeof(DateTime)) return Convert.ToDateTime(value);
            if (targetType == typeof(string)) return value.ToString();
            if (targetType == typeof(short)) return Convert.ToInt16(value);
            if (targetType == typeof(byte)) return Convert.ToByte(value);

            // General conversion
            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            return GetDefaultValue(targetType);
        }
    }
}
