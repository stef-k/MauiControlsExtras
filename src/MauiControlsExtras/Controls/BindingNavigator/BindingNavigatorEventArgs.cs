namespace MauiControlsExtras.Controls;

/// <summary>
/// Event arguments for position changes in the BindingNavigator.
/// </summary>
public class PositionChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the old position.
    /// </summary>
    public int OldPosition { get; }

    /// <summary>
    /// Gets the new position.
    /// </summary>
    public int NewPosition { get; }

    /// <summary>
    /// Gets the item at the new position.
    /// </summary>
    public object? Item { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PositionChangedEventArgs"/> class.
    /// </summary>
    public PositionChangedEventArgs(int oldPosition, int newPosition, object? item)
    {
        OldPosition = oldPosition;
        NewPosition = newPosition;
        Item = item;
    }
}

/// <summary>
/// Event arguments for position changing (cancelable).
/// </summary>
public class PositionChangingEventArgs : EventArgs
{
    /// <summary>
    /// Gets the current position.
    /// </summary>
    public int CurrentPosition { get; }

    /// <summary>
    /// Gets the proposed new position.
    /// </summary>
    public int NewPosition { get; }

    /// <summary>
    /// Gets or sets whether the change should be cancelled.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PositionChangingEventArgs"/> class.
    /// </summary>
    public PositionChangingEventArgs(int currentPosition, int newPosition)
    {
        CurrentPosition = currentPosition;
        NewPosition = newPosition;
    }
}

/// <summary>
/// Event arguments for add/delete operations.
/// </summary>
public class BindingNavigatorItemEventArgs : EventArgs
{
    /// <summary>
    /// Gets the item being added or deleted.
    /// </summary>
    public object? Item { get; }

    /// <summary>
    /// Gets the position of the item.
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// Gets or sets whether the operation should be cancelled.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BindingNavigatorItemEventArgs"/> class.
    /// </summary>
    public BindingNavigatorItemEventArgs(object? item, int position)
    {
        Item = item;
        Position = position;
    }
}
