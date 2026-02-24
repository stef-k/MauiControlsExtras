using System.Collections.Concurrent;

namespace MauiControlsExtras.Controls;

/// <summary>
/// Static registry for AOT-safe property metadata. Register metadata for types that
/// will be used with <see cref="PropertyGrid"/> to avoid runtime reflection.
/// </summary>
public static class PropertyMetadataRegistry
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<PropertyMetadataEntry>> _registry = new();

    /// <summary>
    /// Registers AOT-safe property metadata for a type.
    /// </summary>
    public static void Register(Type type, params PropertyMetadataEntry[] entries)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (entries.Length == 0)
            throw new ArgumentException("At least one PropertyMetadataEntry must be provided.", nameof(entries));

        var snapshot = entries.ToArray();
        if (!_registry.TryAdd(type, snapshot))
            throw new InvalidOperationException(
                $"Metadata is already registered for type '{type.FullName}'. Call Unregister first to replace existing metadata.");
    }

    /// <summary>
    /// Registers AOT-safe property metadata for a type (generic variant).
    /// </summary>
    public static void Register<T>(params PropertyMetadataEntry[] entries)
    {
        Register(typeof(T), entries);
    }

    /// <summary>
    /// Removes registered metadata for the specified type.
    /// </summary>
    public static void Unregister(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        _registry.TryRemove(type, out _);
    }

    /// <summary>
    /// Removes all registered metadata. Intended for testing.
    /// </summary>
    internal static void Clear()
    {
        _registry.Clear();
    }

    /// <summary>
    /// Returns true if metadata has been registered for the specified type.
    /// Registration always guarantees at least one entry.
    /// </summary>
    public static bool HasMetadata(Type type) =>
        _registry.ContainsKey(type);

    /// <summary>
    /// Tries to get registered metadata for the specified type.
    /// </summary>
    internal static bool TryGetMetadata(Type type, out IReadOnlyList<PropertyMetadataEntry>? metadata)
    {
        if (_registry.TryGetValue(type, out var entries))
        {
            metadata = entries;
            return true;
        }
        metadata = null;
        return false;
    }
}
