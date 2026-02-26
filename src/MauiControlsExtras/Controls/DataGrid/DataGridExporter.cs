using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace MauiControlsExtras.Controls;

/// <summary>
/// Provides export functionality for DataGridView.
/// </summary>
public static class DataGridExporter
{
    /// <summary>
    /// Exports data to CSV format.
    /// </summary>
    public static string ExportToCsv(
        IEnumerable<object> items,
        IEnumerable<DataGridColumn> columns,
        DataGridExportOptions? options = null)
    {
        options ??= new DataGridExportOptions();
        var sb = new StringBuilder();
        var visibleColumns = GetExportColumns(columns, options);

        // Headers
        if (options.IncludeHeaders)
        {
            var headers = visibleColumns.Select(c => EscapeCsvField(c.Header, options.Delimiter));
            sb.AppendLine(string.Join(options.Delimiter, headers));
        }

        // Data rows
        foreach (var item in items)
        {
            var values = visibleColumns.Select(c => FormatCsvValue(c, item, options));
            sb.AppendLine(string.Join(options.Delimiter, values));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Exports data to CSV format asynchronously.
    /// </summary>
    public static async Task ExportToCsvAsync(
        Stream stream,
        IEnumerable<object> items,
        IEnumerable<DataGridColumn> columns,
        DataGridExportOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var csv = ExportToCsv(items, columns, options);
        var bytes = Encoding.UTF8.GetBytes(csv);
        await stream.WriteAsync(bytes, cancellationToken);
    }

    /// <summary>
    /// Exports data to JSON format.
    /// </summary>
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode",
        Justification = "Serializes List<Dictionary<string, object?>> — genuinely dynamic by design.")]
    public static string ExportToJson(
        IEnumerable<object> items,
        IEnumerable<DataGridColumn> columns,
        DataGridExportOptions? options = null)
    {
        options ??= new DataGridExportOptions();
        var visibleColumns = GetExportColumns(columns, options);

        var exportData = new List<Dictionary<string, object?>>();

        foreach (var item in items)
        {
            var row = new Dictionary<string, object?>();
            foreach (var column in visibleColumns)
            {
                var key = column.PropertyPath ?? column.Header;
                var value = column.GetCellValue(item);

                if (options.ApplyFormatting && value != null)
                {
                    value = FormatJsonValue(column, value, options);
                }

                row[key] = value;
            }
            exportData.Add(row);
        }

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = options.PrettyPrint
        };

        return JsonSerializer.Serialize(exportData, jsonOptions);
    }

    /// <summary>
    /// Exports data to JSON format asynchronously.
    /// </summary>
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode",
        Justification = "Serializes List<Dictionary<string, object?>> — genuinely dynamic by design.")]
    public static async Task ExportToJsonAsync(
        Stream stream,
        IEnumerable<object> items,
        IEnumerable<DataGridColumn> columns,
        DataGridExportOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new DataGridExportOptions();
        var visibleColumns = GetExportColumns(columns, options);

        var exportData = new List<Dictionary<string, object?>>();

        foreach (var item in items)
        {
            var row = new Dictionary<string, object?>();
            foreach (var column in visibleColumns)
            {
                var key = column.PropertyPath ?? column.Header;
                var value = column.GetCellValue(item);

                if (options.ApplyFormatting && value != null)
                {
                    value = FormatJsonValue(column, value, options);
                }

                row[key] = value;
            }
            exportData.Add(row);
        }

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = options.PrettyPrint
        };

        await JsonSerializer.SerializeAsync(stream, exportData, jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Formats data for clipboard copy as tab-separated values.
    /// </summary>
    public static string FormatForClipboard(
        IEnumerable<object> items,
        IEnumerable<DataGridColumn> columns,
        DataGridExportOptions? options = null)
    {
        options ??= new DataGridExportOptions { Delimiter = "\t" };
        options.Delimiter = "\t"; // Force tab delimiter for clipboard
        return ExportToCsv(items, columns, options);
    }

    private static List<DataGridColumn> GetExportColumns(IEnumerable<DataGridColumn> columns, DataGridExportOptions options)
    {
        return options.VisibleColumnsOnly
            ? columns.Where(c => c.IsVisible).ToList()
            : columns.ToList();
    }

    private static string FormatCsvValue(DataGridColumn column, object item, DataGridExportOptions options)
    {
        var value = column.GetCellValue(item);

        if (value == null)
            return string.Empty;

        string stringValue;

        if (options.ApplyFormatting)
        {
            if (value is DateTime dt)
            {
                stringValue = dt.ToString(options.DateFormat);
            }
            else if (value is DateTimeOffset dto)
            {
                stringValue = dto.ToString(options.DateFormat);
            }
            else if (column is DataGridTextColumn textColumn && !string.IsNullOrEmpty(textColumn.Format) && value is IFormattable formattable)
            {
                stringValue = formattable.ToString(textColumn.Format, null);
            }
            else
            {
                stringValue = value.ToString() ?? string.Empty;
            }
        }
        else
        {
            stringValue = value.ToString() ?? string.Empty;
        }

        return EscapeCsvField(stringValue, options.Delimiter);
    }

    private static object FormatJsonValue(DataGridColumn column, object value, DataGridExportOptions options)
    {
        if (value is DateTime dt)
        {
            return dt.ToString(options.DateFormat);
        }
        else if (value is DateTimeOffset dto)
        {
            return dto.ToString(options.DateFormat);
        }

        return value;
    }

    private static string EscapeCsvField(string field, string delimiter)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        // Check if escaping is needed
        var needsQuotes = field.Contains(delimiter) ||
                          field.Contains('"') ||
                          field.Contains('\n') ||
                          field.Contains('\r');

        if (!needsQuotes)
            return field;

        // Escape quotes by doubling them and wrap in quotes
        return $"\"{field.Replace("\"", "\"\"")}\"";
    }
}
