namespace MauiControlsExtras.Controls;

/// <summary>
/// Static registry for AOT-safe property metadata. Register metadata for types that
/// will be used with <see cref="PropertyGrid"/> to avoid runtime reflection.
/// </summary>
public static class PropertyMetadataRegistry
{
    private static readonly Dictionary<Type, List<PropertyMetadataEntry>> _registry = new();

    /// <summary>
    /// Registers AOT-safe property metadata for a type.
    /// </summary>
    public static void Register(Type type, params PropertyMetadataEntry[] entries)
    {
        _registry[type] = new List<PropertyMetadataEntry>(entries);
    }

    /// <summary>
    /// Registers AOT-safe property metadata for a type (generic variant).
    /// </summary>
    public static void Register<T>(params PropertyMetadataEntry[] entries)
    {
        Register(typeof(T), entries);
    }

    /// <summary>
    /// Returns true if metadata has been registered for the specified type.
    /// </summary>
    public static bool HasMetadata(Type type) => _registry.ContainsKey(type);

    /// <summary>
    /// Tries to get registered metadata for the specified type.
    /// </summary>
    internal static bool TryGetMetadata(Type type, out List<PropertyMetadataEntry>? metadata)
    {
        return _registry.TryGetValue(type, out metadata);
    }
}
