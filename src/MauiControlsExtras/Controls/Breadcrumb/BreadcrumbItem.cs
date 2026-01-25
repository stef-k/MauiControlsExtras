using System.ComponentModel;
using System.Windows.Input;

namespace MauiControlsExtras.Controls;

/// <summary>
/// Represents a single item in a breadcrumb navigation.
/// </summary>
public class BreadcrumbItem : INotifyPropertyChanged
{
    private string? _text;
    private string? _icon;
    private object? _tag;
    private bool _isEnabled = true;
    private bool _isCurrent;

    /// <summary>
    /// Gets or sets the display text.
    /// </summary>
    public string? Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
        }
    }

    /// <summary>
    /// Gets or sets the icon (glyph character).
    /// </summary>
    public string? Icon
    {
        get => _icon;
        set
        {
            if (_icon != value)
            {
                _icon = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Icon)));
            }
        }
    }

    /// <summary>
    /// Gets or sets custom data associated with this item.
    /// </summary>
    public object? Tag
    {
        get => _tag;
        set
        {
            if (_tag != value)
            {
                _tag = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tag)));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether this item is enabled (clickable).
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
            }
        }
    }

    /// <summary>
    /// Gets whether this item is the current (last) item.
    /// </summary>
    public bool IsCurrent
    {
        get => _isCurrent;
        internal set
        {
            if (_isCurrent != value)
            {
                _isCurrent = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCurrent)));
            }
        }
    }

    /// <summary>
    /// Gets the item index within the breadcrumb.
    /// </summary>
    public int Index { get; internal set; }

    /// <summary>
    /// Gets or sets a command executed when this item is clicked.
    /// </summary>
    public ICommand? Command { get; set; }

    /// <summary>
    /// Gets or sets the command parameter.
    /// </summary>
    public object? CommandParameter { get; set; }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="BreadcrumbItem"/> class.
    /// </summary>
    public BreadcrumbItem() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BreadcrumbItem"/> class with text.
    /// </summary>
    public BreadcrumbItem(string text)
    {
        Text = text;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BreadcrumbItem"/> class with text and icon.
    /// </summary>
    public BreadcrumbItem(string text, string icon)
    {
        Text = text;
        Icon = icon;
    }
}
