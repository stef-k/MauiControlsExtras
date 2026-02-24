using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MauiControlsExtras.Helpers;

/// <summary>
/// Centralized, cached property accessor for string-based property names.
/// Provides AOT-annotated reflection helpers used as fallback when Func-based
/// accessors are not configured.
/// </summary>
internal static class PropertyAccessor
{
    // App-lifetime cache: entries are never evicted. This is acceptable for typical
    // MAUI apps where the set of (Type, propertyName) combinations is bounded by the
    // data model. For dynamic/plugin scenarios, call ClearCache() periodically.
    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> _cache = new();

    /// <summary>
    /// Gets a property value from an object using a string property name.
    /// </summary>
    [RequiresUnreferencedCode("Use the corresponding *Func property for AOT/trimming compatibility.")]
    internal static object? GetValue(object item, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (string.IsNullOrEmpty(propertyName))
            return null;

        var property = GetPropertyInfo(item.GetType(), propertyName);
        return property?.GetValue(item);
    }

    /// <summary>
    /// Sets a property value on an object using a string property name.
    /// </summary>
    /// <returns>True if the property was found and set; false otherwise.</returns>
    [RequiresUnreferencedCode("Use the corresponding *Func property for AOT/trimming compatibility.")]
    internal static bool SetValue(object item, string propertyName, object? value)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (string.IsNullOrEmpty(propertyName))
            return false;

        var property = GetPropertyInfo(item.GetType(), propertyName);
        if (property == null || !property.CanWrite)
            return false;

        var convertedValue = ConvertToType(value, property.PropertyType);
        property.SetValue(item, convertedValue);
        return true;
    }

    /// <summary>
    /// Gets the cached PropertyInfo for a type/property name combination.
    /// </summary>
    [RequiresUnreferencedCode("Reflection-based property access may not work under AOT/trimming.")]
    private static PropertyInfo? GetPropertyInfo(Type type, string propertyName)
    {
        return _cache.GetOrAdd((type, propertyName), key => key.Item1.GetProperty(key.Item2));
    }

    /// <summary>
    /// Clears the property info cache. Intended for testing or scenarios where
    /// types are dynamically loaded/unloaded.
    /// </summary>
    internal static void ClearCache() => _cache.Clear();

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

        // Enums: use Enum.ToObject to avoid Activator.CreateInstance under AOT
        if (targetType.IsEnum)
            return Enum.ToObject(targetType, 0);

        // Fallback for other value types (custom structs) — zeroed without constructor, AOT-safe
        return RuntimeHelpers.GetUninitializedObject(targetType);
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

        // Handle nullable types — use a local to avoid mutating the parameter
        var effectiveType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (effectiveType != targetType) // was nullable
        {
            if (value is string s && string.IsNullOrWhiteSpace(s))
                return null;
        }

        // Handle empty string for value types
        if (value is string str && string.IsNullOrWhiteSpace(str))
            return GetDefaultValue(effectiveType);

        try
        {
            // Direct casts for common types (avoids Convert.ChangeType reflection)
            if (effectiveType == typeof(decimal)) return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
            if (effectiveType == typeof(double)) return Convert.ToDouble(value, CultureInfo.InvariantCulture);
            if (effectiveType == typeof(float)) return Convert.ToSingle(value, CultureInfo.InvariantCulture);
            if (effectiveType == typeof(int)) return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            if (effectiveType == typeof(long)) return Convert.ToInt64(value, CultureInfo.InvariantCulture);
            if (effectiveType == typeof(bool)) return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
            if (effectiveType == typeof(DateTime)) return Convert.ToDateTime(value, CultureInfo.InvariantCulture);
            if (effectiveType == typeof(string)) return value.ToString();
            if (effectiveType == typeof(short)) return Convert.ToInt16(value, CultureInfo.InvariantCulture);
            if (effectiveType == typeof(byte)) return Convert.ToByte(value, CultureInfo.InvariantCulture);

            // Enum conversion
            if (effectiveType.IsEnum)
            {
                if (value is string enumStr)
                {
                    var parsed = Enum.Parse(effectiveType, enumStr, ignoreCase: true);
                    if (!effectiveType.IsDefined(typeof(FlagsAttribute), false) && !Enum.IsDefined(effectiveType, parsed))
                        return GetDefaultValue(effectiveType);
                    return parsed;
                }
                return Enum.ToObject(effectiveType, value);
            }

            // General conversion
            return Convert.ChangeType(value, effectiveType, CultureInfo.InvariantCulture);
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException
            or OverflowException or ArgumentException)
        {
            return GetDefaultValue(effectiveType);
        }
    }
}
