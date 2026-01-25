using System.ComponentModel;

namespace MauiControlsExtras.Controls;

/// <summary>
/// Represents a category of properties in the PropertyGrid.
/// </summary>
public class PropertyCategory : INotifyPropertyChanged
{
    private bool _isExpanded = true;

    /// <summary>
    /// Gets the category name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the properties in this category.
    /// </summary>
    public IReadOnlyList<PropertyItem> Properties { get; }

    /// <summary>
    /// Gets or sets whether this category is expanded.
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
            }
        }
    }

    /// <summary>
    /// Gets the number of properties in this category.
    /// </summary>
    public int PropertyCount => Properties.Count;

    /// <summary>
    /// Gets the icon for expand/collapse.
    /// </summary>
    public string ExpanderIcon => IsExpanded ? "▼" : "▶";

    /// <summary>
    /// Occurs when a property changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyCategory"/> class.
    /// </summary>
    public PropertyCategory(string name, IReadOnlyList<PropertyItem> properties)
    {
        Name = name;
        Properties = properties;
    }

    /// <summary>
    /// Toggles the expanded state.
    /// </summary>
    public void ToggleExpanded()
    {
        IsExpanded = !IsExpanded;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExpanderIcon)));
    }
}
