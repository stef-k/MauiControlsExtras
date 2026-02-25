using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.Maui.Controls.Internals;

namespace MauiControlsExtras.Converters;

/// <summary>
/// Converts a MauiAsset resource path to an ImageSource.
/// Use this for images in Resources/Raw that need to be displayed in Image controls.
/// Uses StreamImageSource with async loading from app package.
/// </summary>
/// <remarks>
/// <para>
/// This converter caches loaded image bytes to avoid repeated file access.
/// Use <see cref="ClearCache"/> to clear the cache if needed.
/// </para>
/// <example>
/// XAML usage:
/// <code>
/// &lt;Image Source="{Binding IconPath, Converter={StaticResource MauiAssetImageConverter}}" /&gt;
/// </code>
/// </example>
/// </remarks>
[Preserve(AllMembers = true)]
public class MauiAssetImageConverter : IValueConverter
{
    /// <summary>
    /// Thread-safe cache of loaded image bytes to avoid repeated file access.
    /// </summary>
    private static readonly ConcurrentDictionary<string, byte[]?> _bytesCache = new();

    /// <summary>
    /// Converts a resource path to an ImageSource.
    /// </summary>
    /// <param name="value">The resource path (e.g., "icons/marker.png").</param>
    /// <param name="targetType">The target type (ImageSource).</param>
    /// <param name="parameter">Not used.</param>
    /// <param name="culture">Culture info.</param>
    /// <returns>A StreamImageSource for the resource, or null if not found.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string resourcePath || string.IsNullOrWhiteSpace(resourcePath))
        {
            return null;
        }

        // Check bytes cache first
        if (_bytesCache.TryGetValue(resourcePath, out var cachedBytes))
        {
            if (cachedBytes == null)
                return null;

            return ImageSource.FromStream(() => new MemoryStream(cachedBytes));
        }

        // Return a StreamImageSource that loads asynchronously
        return new StreamImageSource
        {
            Stream = async (cancellationToken) =>
            {
                try
                {
                    // Check cache again (another thread might have loaded it)
                    if (_bytesCache.TryGetValue(resourcePath, out var bytes))
                    {
                        return bytes != null ? new MemoryStream(bytes) : null;
                    }

                    // Load from app package
                    using var stream = await FileSystem.Current.OpenAppPackageFileAsync(resourcePath);
                    if (stream == null)
                    {
                        _bytesCache[resourcePath] = null;
                        return null;
                    }

                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream, cancellationToken);
                    var imageBytes = memoryStream.ToArray();

                    // Cache the bytes
                    _bytesCache[resourcePath] = imageBytes;

                    return new MemoryStream(imageBytes);
                }
                catch
                {
                    _bytesCache[resourcePath] = null;
                    return null;
                }
            }
        };
    }

    /// <summary>
    /// Not supported - one-way conversion only.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("MauiAssetImageConverter only supports one-way conversion.");
    }

    /// <summary>
    /// Clears the image cache.
    /// Call this method to free memory if images are no longer needed.
    /// </summary>
    public static void ClearCache()
    {
        _bytesCache.Clear();
    }
}
