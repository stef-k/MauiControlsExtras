using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;
using MauiControlsExtras.Base;

namespace MauiControlsExtras.Controls;

/// <summary>
/// A navigation control for browsing data sources with First, Previous, Next, Last buttons
/// and optional Add/Delete functionality. Similar to WinForms BindingNavigator.
/// </summary>
public partial class BindingNavigator : StyledControlBase, IKeyboardNavigable
{
    #region Private Fields

    private IList? _itemsList;
    private INotifyCollectionChanged? _notifyCollection;
    private bool _hasKeyboardFocus;

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="ItemsSource"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource),
        typeof(IEnumerable),
        typeof(BindingNavigator),
        null,
        propertyChanged: OnItemsSourceChanged);

    /// <summary>
    /// Identifies the <see cref="Position"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PositionProperty = BindableProperty.Create(
        nameof(Position),
        typeof(int),
        typeof(BindingNavigator),
        0,
        BindingMode.TwoWay,
        propertyChanged: OnPositionChanged);

    /// <summary>
    /// Identifies the <see cref="ShowAddButton"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowAddButtonProperty = BindableProperty.Create(
        nameof(ShowAddButton),
        typeof(bool),
        typeof(BindingNavigator),
        true);

    /// <summary>
    /// Identifies the <see cref="ShowDeleteButton"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowDeleteButtonProperty = BindableProperty.Create(
        nameof(ShowDeleteButton),
        typeof(bool),
        typeof(BindingNavigator),
        true);

    /// <summary>
    /// Identifies the <see cref="ShowSaveButtons"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowSaveButtonsProperty = BindableProperty.Create(
        nameof(ShowSaveButtons),
        typeof(bool),
        typeof(BindingNavigator),
        false);

    /// <summary>
    /// Identifies the <see cref="ShowRefreshButton"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowRefreshButtonProperty = BindableProperty.Create(
        nameof(ShowRefreshButton),
        typeof(bool),
        typeof(BindingNavigator),
        false);

    /// <summary>
    /// Identifies the <see cref="ShowSeparators"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowSeparatorsProperty = BindableProperty.Create(
        nameof(ShowSeparators),
        typeof(bool),
        typeof(BindingNavigator),
        true);

    /// <summary>
    /// Identifies the <see cref="ButtonSize"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ButtonSizeProperty = BindableProperty.Create(
        nameof(ButtonSize),
        typeof(double),
        typeof(BindingNavigator),
        32.0);

    /// <summary>
    /// Identifies the <see cref="FirstButtonContent"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FirstButtonContentProperty = BindableProperty.Create(
        nameof(FirstButtonContent),
        typeof(string),
        typeof(BindingNavigator),
        "⏮");

    /// <summary>
    /// Identifies the <see cref="PreviousButtonContent"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PreviousButtonContentProperty = BindableProperty.Create(
        nameof(PreviousButtonContent),
        typeof(string),
        typeof(BindingNavigator),
        "◀");

    /// <summary>
    /// Identifies the <see cref="NextButtonContent"/> bindable property.
    /// </summary>
    public static readonly BindableProperty NextButtonContentProperty = BindableProperty.Create(
        nameof(NextButtonContent),
        typeof(string),
        typeof(BindingNavigator),
        "▶");

    /// <summary>
    /// Identifies the <see cref="LastButtonContent"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LastButtonContentProperty = BindableProperty.Create(
        nameof(LastButtonContent),
        typeof(string),
        typeof(BindingNavigator),
        "⏭");

    /// <summary>
    /// Identifies the <see cref="AddButtonContent"/> bindable property.
    /// </summary>
    public static readonly BindableProperty AddButtonContentProperty = BindableProperty.Create(
        nameof(AddButtonContent),
        typeof(string),
        typeof(BindingNavigator),
        "+");

    /// <summary>
    /// Identifies the <see cref="DeleteButtonContent"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DeleteButtonContentProperty = BindableProperty.Create(
        nameof(DeleteButtonContent),
        typeof(string),
        typeof(BindingNavigator),
        "−");

    /// <summary>
    /// Identifies the <see cref="SaveButtonContent"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SaveButtonContentProperty = BindableProperty.Create(
        nameof(SaveButtonContent),
        typeof(string),
        typeof(BindingNavigator),
        "💾");

    /// <summary>
    /// Identifies the <see cref="CancelButtonContent"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CancelButtonContentProperty = BindableProperty.Create(
        nameof(CancelButtonContent),
        typeof(string),
        typeof(BindingNavigator),
        "✕");

    /// <summary>
    /// Identifies the <see cref="RefreshButtonContent"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RefreshButtonContentProperty = BindableProperty.Create(
        nameof(RefreshButtonContent),
        typeof(string),
        typeof(BindingNavigator),
        "↻");

    /// <summary>
    /// Identifies the <see cref="IsKeyboardNavigationEnabled"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsKeyboardNavigationEnabledProperty = BindableProperty.Create(
        nameof(IsKeyboardNavigationEnabled),
        typeof(bool),
        typeof(BindingNavigator),
        true);

    #endregion

    #region Command Properties

    /// <summary>
    /// Identifies the <see cref="PositionChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PositionChangedCommandProperty = BindableProperty.Create(
        nameof(PositionChangedCommand),
        typeof(ICommand),
        typeof(BindingNavigator));

    /// <summary>
    /// Identifies the <see cref="AddCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty AddCommandProperty = BindableProperty.Create(
        nameof(AddCommand),
        typeof(ICommand),
        typeof(BindingNavigator));

    /// <summary>
    /// Identifies the <see cref="DeleteCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DeleteCommandProperty = BindableProperty.Create(
        nameof(DeleteCommand),
        typeof(ICommand),
        typeof(BindingNavigator));

    /// <summary>
    /// Identifies the <see cref="SaveCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SaveCommandProperty = BindableProperty.Create(
        nameof(SaveCommand),
        typeof(ICommand),
        typeof(BindingNavigator));

    /// <summary>
    /// Identifies the <see cref="CancelCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CancelCommandProperty = BindableProperty.Create(
        nameof(CancelCommand),
        typeof(ICommand),
        typeof(BindingNavigator));

    /// <summary>
    /// Identifies the <see cref="RefreshCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RefreshCommandProperty = BindableProperty.Create(
        nameof(RefreshCommand),
        typeof(ICommand),
        typeof(BindingNavigator));

    /// <summary>
    /// Identifies the <see cref="GotFocusCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty GotFocusCommandProperty = BindableProperty.Create(
        nameof(GotFocusCommand),
        typeof(ICommand),
        typeof(BindingNavigator));

    /// <summary>
    /// Identifies the <see cref="LostFocusCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandProperty = BindableProperty.Create(
        nameof(LostFocusCommand),
        typeof(ICommand),
        typeof(BindingNavigator));

    /// <summary>
    /// Identifies the <see cref="KeyPressCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandProperty = BindableProperty.Create(
        nameof(KeyPressCommand),
        typeof(ICommand),
        typeof(BindingNavigator));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the data source to navigate.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the current position (0-based index).
    /// </summary>
    public int Position
    {
        get => (int)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    /// <summary>
    /// Gets the display position (1-based for UI).
    /// </summary>
    public string DisplayPosition => Count > 0 ? (Position + 1).ToString() : "0";

    /// <summary>
    /// Gets the total count of items.
    /// </summary>
    public int Count => _itemsList?.Count ?? 0;

    /// <summary>
    /// Gets the current item at the current position.
    /// </summary>
    public object? CurrentItem => Count > 0 && Position >= 0 && Position < Count
        ? _itemsList?[Position]
        : null;

    /// <summary>
    /// Gets or sets whether the Add button is shown.
    /// </summary>
    public bool ShowAddButton
    {
        get => (bool)GetValue(ShowAddButtonProperty);
        set => SetValue(ShowAddButtonProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the Delete button is shown.
    /// </summary>
    public bool ShowDeleteButton
    {
        get => (bool)GetValue(ShowDeleteButtonProperty);
        set => SetValue(ShowDeleteButtonProperty, value);
    }

    /// <summary>
    /// Gets or sets whether Save/Cancel buttons are shown.
    /// </summary>
    public bool ShowSaveButtons
    {
        get => (bool)GetValue(ShowSaveButtonsProperty);
        set => SetValue(ShowSaveButtonsProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the Refresh button is shown.
    /// </summary>
    public bool ShowRefreshButton
    {
        get => (bool)GetValue(ShowRefreshButtonProperty);
        set => SetValue(ShowRefreshButtonProperty, value);
    }

    /// <summary>
    /// Gets or sets whether separators are shown between button groups.
    /// </summary>
    public bool ShowSeparators
    {
        get => (bool)GetValue(ShowSeparatorsProperty);
        set => SetValue(ShowSeparatorsProperty, value);
    }

    /// <summary>
    /// Gets or sets the button size.
    /// </summary>
    public double ButtonSize
    {
        get => (double)GetValue(ButtonSizeProperty);
        set => SetValue(ButtonSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the First button content.
    /// </summary>
    public string FirstButtonContent
    {
        get => (string)GetValue(FirstButtonContentProperty);
        set => SetValue(FirstButtonContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the Previous button content.
    /// </summary>
    public string PreviousButtonContent
    {
        get => (string)GetValue(PreviousButtonContentProperty);
        set => SetValue(PreviousButtonContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the Next button content.
    /// </summary>
    public string NextButtonContent
    {
        get => (string)GetValue(NextButtonContentProperty);
        set => SetValue(NextButtonContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the Last button content.
    /// </summary>
    public string LastButtonContent
    {
        get => (string)GetValue(LastButtonContentProperty);
        set => SetValue(LastButtonContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the Add button content.
    /// </summary>
    public string AddButtonContent
    {
        get => (string)GetValue(AddButtonContentProperty);
        set => SetValue(AddButtonContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the Delete button content.
    /// </summary>
    public string DeleteButtonContent
    {
        get => (string)GetValue(DeleteButtonContentProperty);
        set => SetValue(DeleteButtonContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the Save button content.
    /// </summary>
    public string SaveButtonContent
    {
        get => (string)GetValue(SaveButtonContentProperty);
        set => SetValue(SaveButtonContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the Cancel button content.
    /// </summary>
    public string CancelButtonContent
    {
        get => (string)GetValue(CancelButtonContentProperty);
        set => SetValue(CancelButtonContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the Refresh button content.
    /// </summary>
    public string RefreshButtonContent
    {
        get => (string)GetValue(RefreshButtonContentProperty);
        set => SetValue(RefreshButtonContentProperty, value);
    }

    /// <summary>
    /// Gets whether navigation to first is possible.
    /// </summary>
    public bool CanGoFirst => Count > 0 && Position > 0;

    /// <summary>
    /// Gets whether navigation to previous is possible.
    /// </summary>
    public bool CanGoPrevious => Count > 0 && Position > 0;

    /// <summary>
    /// Gets whether navigation to next is possible.
    /// </summary>
    public bool CanGoNext => Count > 0 && Position < Count - 1;

    /// <summary>
    /// Gets whether navigation to last is possible.
    /// </summary>
    public bool CanGoLast => Count > 0 && Position < Count - 1;

    #endregion

    #region Command Properties Implementation

    /// <summary>
    /// Gets or sets the command executed when position changes.
    /// </summary>
    public ICommand? PositionChangedCommand
    {
        get => (ICommand?)GetValue(PositionChangedCommandProperty);
        set => SetValue(PositionChangedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when Add is clicked.
    /// </summary>
    public ICommand? AddCommand
    {
        get => (ICommand?)GetValue(AddCommandProperty);
        set => SetValue(AddCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when Delete is clicked.
    /// </summary>
    public ICommand? DeleteCommand
    {
        get => (ICommand?)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when Save is clicked.
    /// </summary>
    public ICommand? SaveCommand
    {
        get => (ICommand?)GetValue(SaveCommandProperty);
        set => SetValue(SaveCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when Cancel is clicked.
    /// </summary>
    public ICommand? CancelCommand
    {
        get => (ICommand?)GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when Refresh is clicked.
    /// </summary>
    public ICommand? RefreshCommand
    {
        get => (ICommand?)GetValue(RefreshCommandProperty);
        set => SetValue(RefreshCommandProperty, value);
    }

    /// <inheritdoc/>
    public ICommand? GotFocusCommand
    {
        get => (ICommand?)GetValue(GotFocusCommandProperty);
        set => SetValue(GotFocusCommandProperty, value);
    }

    /// <inheritdoc/>
    public ICommand? LostFocusCommand
    {
        get => (ICommand?)GetValue(LostFocusCommandProperty);
        set => SetValue(LostFocusCommandProperty, value);
    }

    /// <inheritdoc/>
    public ICommand? KeyPressCommand
    {
        get => (ICommand?)GetValue(KeyPressCommandProperty);
        set => SetValue(KeyPressCommandProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the position changes.
    /// </summary>
    public event EventHandler<PositionChangedEventArgs>? PositionChanged;

    /// <summary>
    /// Occurs before the position changes (cancelable).
    /// </summary>
    public event EventHandler<PositionChangingEventArgs>? PositionChanging;

    /// <summary>
    /// Occurs when Add is requested.
    /// </summary>
    public event EventHandler<BindingNavigatorItemEventArgs>? Adding;

    /// <summary>
    /// Occurs when Delete is requested.
    /// </summary>
    public event EventHandler<BindingNavigatorItemEventArgs>? Deleting;

    /// <summary>
    /// Occurs when Save is requested.
    /// </summary>
    public event EventHandler? Saving;

    /// <summary>
    /// Occurs when Cancel is requested.
    /// </summary>
    public event EventHandler? Cancelling;

    /// <summary>
    /// Occurs when Refresh is requested.
    /// </summary>
    public event EventHandler? Refreshing;

    /// <inheritdoc/>
    public event EventHandler<KeyboardFocusEventArgs>? KeyboardFocusGained;

    /// <inheritdoc/>
#pragma warning disable CS0067
    public event EventHandler<KeyboardFocusEventArgs>? KeyboardFocusLost;
#pragma warning restore CS0067

    /// <inheritdoc/>
    public event EventHandler<KeyEventArgs>? KeyPressed;

    /// <inheritdoc/>
#pragma warning disable CS0067
    public event EventHandler<KeyEventArgs>? KeyReleased;
#pragma warning restore CS0067

    #endregion

    #region IKeyboardNavigable Implementation

    /// <inheritdoc/>
    public bool CanReceiveFocus => IsEnabled && IsVisible;

    /// <inheritdoc/>
    public bool IsKeyboardNavigationEnabled
    {
        get => (bool)GetValue(IsKeyboardNavigationEnabledProperty);
        set => SetValue(IsKeyboardNavigationEnabledProperty, value);
    }

    /// <inheritdoc/>
    public bool HasKeyboardFocus => _hasKeyboardFocus;

    /// <inheritdoc/>
    public bool HandleKeyPress(KeyEventArgs e)
    {
        if (!IsKeyboardNavigationEnabled) return false;

        KeyPressed?.Invoke(this, e);
        if (e.Handled) return true;

        if (KeyPressCommand?.CanExecute(e) == true)
        {
            KeyPressCommand.Execute(e);
            if (e.Handled) return true;
        }

        switch (e.Key)
        {
            case "Home":
                MoveFirst();
                return true;
            case "End":
                MoveLast();
                return true;
            case "ArrowLeft":
            case "PageUp":
                MovePrevious();
                return true;
            case "ArrowRight":
            case "PageDown":
                MoveNext();
                return true;
            case "Insert":
                Add();
                return true;
            case "Delete":
                Delete();
                return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public IReadOnlyList<KeyboardShortcut> GetKeyboardShortcuts()
    {
        return new List<KeyboardShortcut>
        {
            new() { Key = "Home", Description = "Move to first", Category = "Navigation" },
            new() { Key = "End", Description = "Move to last", Category = "Navigation" },
            new() { Key = "ArrowLeft", Description = "Move to previous", Category = "Navigation" },
            new() { Key = "ArrowRight", Description = "Move to next", Category = "Navigation" },
            new() { Key = "PageUp", Description = "Move to previous", Category = "Navigation" },
            new() { Key = "PageDown", Description = "Move to next", Category = "Navigation" },
            new() { Key = "Insert", Description = "Add new item", Category = "Editing" },
            new() { Key = "Delete", Description = "Delete current item", Category = "Editing" }
        };
    }

    /// <inheritdoc/>
    public new bool Focus()
    {
        if (!CanReceiveFocus) return false;
        _hasKeyboardFocus = true;
        OnPropertyChanged(nameof(HasKeyboardFocus));
        KeyboardFocusGained?.Invoke(this, new KeyboardFocusEventArgs(true));
        GotFocusCommand?.Execute(this);
        return true;
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="BindingNavigator"/> class.
    /// </summary>
    public BindingNavigator()
    {
        InitializeComponent();
        UpdateButtonStates();
    }

    #endregion

    #region Navigation Methods

    /// <summary>
    /// Moves to the first item.
    /// </summary>
    public void MoveFirst()
    {
        if (CanGoFirst)
        {
            NavigateTo(0);
        }
    }

    /// <summary>
    /// Moves to the previous item.
    /// </summary>
    public void MovePrevious()
    {
        if (CanGoPrevious)
        {
            NavigateTo(Position - 1);
        }
    }

    /// <summary>
    /// Moves to the next item.
    /// </summary>
    public void MoveNext()
    {
        if (CanGoNext)
        {
            NavigateTo(Position + 1);
        }
    }

    /// <summary>
    /// Moves to the last item.
    /// </summary>
    public void MoveLast()
    {
        if (CanGoLast)
        {
            NavigateTo(Count - 1);
        }
    }

    /// <summary>
    /// Navigates to a specific position.
    /// </summary>
    /// <param name="position">The target position.</param>
    /// <returns>True if navigation succeeded.</returns>
    public bool NavigateTo(int position)
    {
        if (position < 0 || position >= Count)
            return false;

        if (position == Position)
            return true;

        // Raise changing event
        var changingArgs = new PositionChangingEventArgs(Position, position);
        PositionChanging?.Invoke(this, changingArgs);
        if (changingArgs.Cancel)
            return false;

        var oldPosition = Position;
        Position = position;

        return true;
    }

    /// <summary>
    /// Adds a new item (raises Adding event).
    /// </summary>
    public void Add()
    {
        var args = new BindingNavigatorItemEventArgs(null, Count);
        Adding?.Invoke(this, args);
        if (!args.Cancel)
        {
            AddCommand?.Execute(args);
        }
    }

    /// <summary>
    /// Deletes the current item (raises Deleting event).
    /// </summary>
    public void Delete()
    {
        if (CurrentItem == null) return;

        var args = new BindingNavigatorItemEventArgs(CurrentItem, Position);
        Deleting?.Invoke(this, args);
        if (!args.Cancel)
        {
            DeleteCommand?.Execute(args);
        }
    }

    /// <summary>
    /// Refreshes the data (raises Refreshing event).
    /// </summary>
    public void Refresh()
    {
        Refreshing?.Invoke(this, EventArgs.Empty);
        RefreshCommand?.Execute(null);
        UpdateCount();
        UpdateButtonStates();
    }

    #endregion

    #region Event Handlers

    private void OnFirstClicked(object? sender, EventArgs e) => MoveFirst();
    private void OnPreviousClicked(object? sender, EventArgs e) => MovePrevious();
    private void OnNextClicked(object? sender, EventArgs e) => MoveNext();
    private void OnLastClicked(object? sender, EventArgs e) => MoveLast();
    private void OnAddClicked(object? sender, EventArgs e) => Add();
    private void OnDeleteClicked(object? sender, EventArgs e) => Delete();
    private void OnRefreshClicked(object? sender, EventArgs e) => Refresh();

    private void OnSaveClicked(object? sender, EventArgs e)
    {
        Saving?.Invoke(this, EventArgs.Empty);
        SaveCommand?.Execute(null);
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        Cancelling?.Invoke(this, EventArgs.Empty);
        CancelCommand?.Execute(null);
    }

    private void OnPositionEntryCompleted(object? sender, EventArgs e)
    {
        if (int.TryParse(positionEntry.Text, out var displayPos))
        {
            var zeroBasedPos = displayPos - 1;
            if (zeroBasedPos >= 0 && zeroBasedPos < Count)
            {
                NavigateTo(zeroBasedPos);
            }
        }
        positionEntry.Text = DisplayPosition;
    }

    #endregion

    #region Property Changed Handlers

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BindingNavigator navigator)
        {
            // Unsubscribe from old collection
            if (navigator._notifyCollection != null)
            {
                navigator._notifyCollection.CollectionChanged -= navigator.OnCollectionChanged;
            }

            // Set up new source
            navigator._itemsList = newValue as IList;
            navigator._notifyCollection = newValue as INotifyCollectionChanged;

            if (navigator._notifyCollection != null)
            {
                navigator._notifyCollection.CollectionChanged += navigator.OnCollectionChanged;
            }

            navigator.UpdateCount();
            navigator.Position = navigator.Count > 0 ? 0 : -1;
            navigator.UpdateButtonStates();
        }
    }

    private static void OnPositionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BindingNavigator navigator)
        {
            var oldPos = (int)oldValue;
            var newPos = (int)newValue;

            navigator.OnPropertyChanged(nameof(DisplayPosition));
            navigator.OnPropertyChanged(nameof(CurrentItem));
            navigator.OnPropertyChanged(nameof(CanGoFirst));
            navigator.OnPropertyChanged(nameof(CanGoPrevious));
            navigator.OnPropertyChanged(nameof(CanGoNext));
            navigator.OnPropertyChanged(nameof(CanGoLast));
            navigator.UpdateButtonStates();

            var args = new PositionChangedEventArgs(oldPos, newPos, navigator.CurrentItem);
            navigator.PositionChanged?.Invoke(navigator, args);
            navigator.PositionChangedCommand?.Execute(args);
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateCount();

        // Adjust position if needed
        if (Position >= Count)
        {
            Position = Math.Max(0, Count - 1);
        }

        UpdateButtonStates();
    }

    #endregion

    #region Helper Methods

    private void UpdateCount()
    {
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged(nameof(DisplayPosition));
    }

    private void UpdateButtonStates()
    {
        firstButton.IsEnabled = CanGoFirst;
        previousButton.IsEnabled = CanGoPrevious;
        nextButton.IsEnabled = CanGoNext;
        lastButton.IsEnabled = CanGoLast;
        deleteButton.IsEnabled = Count > 0;
    }

    #endregion
}
