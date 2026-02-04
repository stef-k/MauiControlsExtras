using System.Globalization;
using System.Windows.Input;
using MauiControlsExtras.Base;
using MauiControlsExtras.Base.Validation;

namespace MauiControlsExtras.Controls;

/// <summary>
/// Specifies the placement of increment/decrement buttons.
/// </summary>
public enum ButtonPlacement
{
    /// <summary>Buttons stacked vertically on the right side.</summary>
    Right,
    /// <summary>Buttons stacked vertically on the left side.</summary>
    Left,
    /// <summary>Decrement on left, increment on right.</summary>
    LeftAndRight,
    /// <summary>Buttons stacked vertically (up on top, down on bottom) on the right.</summary>
    Stacked
}

/// <summary>
/// A numeric input control with increment/decrement buttons.
/// </summary>
public partial class NumericUpDown : TextStyledControlBase, IValidatable, Base.IKeyboardNavigable
{
    #region Fields

    private Entry? _entry;
    private Button? _incrementButton;
    private Button? _decrementButton;
    private Grid? _mainGrid;
    private bool _isUpdatingText;
    private readonly List<string> _validationErrors = new();
    private bool _isKeyboardNavigationEnabled = true;
    private static readonly List<Base.KeyboardShortcut> _keyboardShortcuts = new();

    // Long-press repeat support
    private CancellationTokenSource? _repeatCts;
    private const int RepeatDelay = 500;
    private const int RepeatInterval = 50;

    #endregion

    #region Constructors

    public NumericUpDown()
    {
        InitializeComponent();
        _mainGrid = this.FindByName<Grid>("mainGrid");
        BuildLayout();
    }

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="Value"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(double?),
        typeof(NumericUpDown),
        null,
        BindingMode.TwoWay,
        propertyChanged: OnValueChanged);

    /// <summary>
    /// Identifies the <see cref="Minimum"/> bindable property.
    /// </summary>
    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
        nameof(Minimum),
        typeof(double),
        typeof(NumericUpDown),
        double.MinValue,
        propertyChanged: OnMinimumChanged);

    /// <summary>
    /// Identifies the <see cref="Maximum"/> bindable property.
    /// </summary>
    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum),
        typeof(double),
        typeof(NumericUpDown),
        double.MaxValue,
        propertyChanged: OnMaximumChanged);

    /// <summary>
    /// Identifies the <see cref="Step"/> bindable property.
    /// </summary>
    public static readonly BindableProperty StepProperty = BindableProperty.Create(
        nameof(Step),
        typeof(double),
        typeof(NumericUpDown),
        1.0);

    /// <summary>
    /// Identifies the <see cref="DecimalPlaces"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DecimalPlacesProperty = BindableProperty.Create(
        nameof(DecimalPlaces),
        typeof(int),
        typeof(NumericUpDown),
        0,
        propertyChanged: OnDecimalPlacesChanged);

    /// <summary>
    /// Identifies the <see cref="Format"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FormatProperty = BindableProperty.Create(
        nameof(Format),
        typeof(string),
        typeof(NumericUpDown),
        null,
        propertyChanged: OnFormatChanged);

    /// <summary>
    /// Identifies the <see cref="IsReadOnly"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(
        nameof(IsReadOnly),
        typeof(bool),
        typeof(NumericUpDown),
        false,
        propertyChanged: OnIsReadOnlyChanged);

    /// <summary>
    /// Identifies the <see cref="Placeholder"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder),
        typeof(string),
        typeof(NumericUpDown),
        null,
        propertyChanged: OnPlaceholderChanged);

    /// <summary>
    /// Identifies the <see cref="ButtonPlacement"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ButtonPlacementProperty = BindableProperty.Create(
        nameof(ButtonPlacement),
        typeof(ButtonPlacement),
        typeof(NumericUpDown),
        ButtonPlacement.Right,
        propertyChanged: OnButtonPlacementChanged);

    /// <summary>
    /// Identifies the <see cref="IsRequired"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(
        nameof(IsRequired),
        typeof(bool),
        typeof(NumericUpDown),
        false);

    /// <summary>
    /// Identifies the <see cref="RequiredErrorMessage"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RequiredErrorMessageProperty = BindableProperty.Create(
        nameof(RequiredErrorMessage),
        typeof(string),
        typeof(NumericUpDown),
        "This field is required.");

    #endregion

    #region Command Properties

    /// <summary>
    /// Identifies the <see cref="ValueChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ValueChangedCommandProperty = BindableProperty.Create(
        nameof(ValueChangedCommand),
        typeof(ICommand),
        typeof(NumericUpDown));

    /// <summary>
    /// Identifies the <see cref="ValueChangedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ValueChangedCommandParameterProperty = BindableProperty.Create(
        nameof(ValueChangedCommandParameter),
        typeof(object),
        typeof(NumericUpDown));

    /// <summary>
    /// Identifies the <see cref="IncrementCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IncrementCommandProperty = BindableProperty.Create(
        nameof(IncrementCommand),
        typeof(ICommand),
        typeof(NumericUpDown));

    /// <summary>
    /// Identifies the <see cref="DecrementCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DecrementCommandProperty = BindableProperty.Create(
        nameof(DecrementCommand),
        typeof(ICommand),
        typeof(NumericUpDown));

    /// <summary>
    /// Identifies the <see cref="ValidateCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ValidateCommandProperty = BindableProperty.Create(
        nameof(ValidateCommand),
        typeof(ICommand),
        typeof(NumericUpDown));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the current value. Null represents no value (empty state).
    /// </summary>
    public double? Value
    {
        get => (double?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum allowed value.
    /// </summary>
    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum allowed value.
    /// </summary>
    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>
    /// Gets or sets the increment/decrement step amount.
    /// </summary>
    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of decimal places to display.
    /// </summary>
    public int DecimalPlaces
    {
        get => (int)GetValue(DecimalPlacesProperty);
        set => SetValue(DecimalPlacesProperty, value);
    }

    /// <summary>
    /// Gets or sets the custom format string (e.g., "C2" for currency).
    /// </summary>
    public string? Format
    {
        get => (string?)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    /// <summary>
    /// Gets or sets whether direct text entry is disabled.
    /// </summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// Gets or sets the placeholder text when value is null.
    /// </summary>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>
    /// Gets or sets the button placement style.
    /// </summary>
    public ButtonPlacement ButtonPlacement
    {
        get => (ButtonPlacement)GetValue(ButtonPlacementProperty);
        set => SetValue(ButtonPlacementProperty, value);
    }

    /// <summary>
    /// Gets or sets whether this field is required.
    /// </summary>
    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    /// <summary>
    /// Gets or sets the error message for required validation.
    /// </summary>
    public string RequiredErrorMessage
    {
        get => (string)GetValue(RequiredErrorMessageProperty);
        set => SetValue(RequiredErrorMessageProperty, value);
    }

    #endregion

    #region Command Properties (CLR)

    /// <summary>
    /// Gets or sets the command to execute when the value changes.
    /// </summary>
    public ICommand? ValueChangedCommand
    {
        get => (ICommand?)GetValue(ValueChangedCommandProperty);
        set => SetValue(ValueChangedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter for <see cref="ValueChangedCommand"/>.
    /// </summary>
    public object? ValueChangedCommandParameter
    {
        get => GetValue(ValueChangedCommandParameterProperty);
        set => SetValue(ValueChangedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when increment is triggered.
    /// </summary>
    public ICommand? IncrementCommand
    {
        get => (ICommand?)GetValue(IncrementCommandProperty);
        set => SetValue(IncrementCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when decrement is triggered.
    /// </summary>
    public ICommand? DecrementCommand
    {
        get => (ICommand?)GetValue(DecrementCommandProperty);
        set => SetValue(DecrementCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when validation occurs.
    /// </summary>
    public ICommand? ValidateCommand
    {
        get => (ICommand?)GetValue(ValidateCommandProperty);
        set => SetValue(ValidateCommandProperty, value);
    }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Gets the formatted display text for the current value.
    /// </summary>
    public string DisplayText
    {
        get
        {
            if (Value is null)
                return string.Empty;

            if (!string.IsNullOrEmpty(Format))
                return Value.Value.ToString(Format, CultureInfo.CurrentCulture);

            return Value.Value.ToString($"F{DecimalPlaces}", CultureInfo.CurrentCulture);
        }
    }

    /// <summary>
    /// Gets whether the increment button should be enabled.
    /// </summary>
    public bool CanIncrement => !IsReadOnly && (!Value.HasValue || Value.Value < Maximum);

    /// <summary>
    /// Gets whether the decrement button should be enabled.
    /// </summary>
    public bool CanDecrement => !IsReadOnly && (!Value.HasValue || Value.Value > Minimum);

    /// <summary>
    /// Gets the current border color based on focus and validation state.
    /// </summary>
    public Color CurrentBorderColor
    {
        get
        {
            if (!IsValid)
                return EffectiveErrorBorderColor;
            if (_entry?.IsFocused == true)
                return EffectiveFocusBorderColor;
            return EffectiveBorderColor;
        }
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the value changes.
    /// </summary>
    public event EventHandler<ValueChangedEventArgs>? ValueChanged;

    #endregion

    #region IValidatable Implementation

    /// <inheritdoc/>
    public bool IsValid => _validationErrors.Count == 0;

    /// <inheritdoc/>
    public IReadOnlyList<string> ValidationErrors => _validationErrors.AsReadOnly();

    /// <inheritdoc/>
    public ValidationResult Validate()
    {
        _validationErrors.Clear();

        if (IsRequired && !Value.HasValue)
        {
            _validationErrors.Add(RequiredErrorMessage);
        }

        if (Value.HasValue)
        {
            if (Value.Value < Minimum)
                _validationErrors.Add($"Value must be at least {Minimum}.");
            if (Value.Value > Maximum)
                _validationErrors.Add($"Value must be at most {Maximum}.");
        }

        OnPropertyChanged(nameof(IsValid));
        OnPropertyChanged(nameof(ValidationErrors));
        OnPropertyChanged(nameof(CurrentBorderColor));

        var result = _validationErrors.Count == 0
            ? ValidationResult.Success
            : ValidationResult.Failure(_validationErrors);

        if (ValidateCommand?.CanExecute(result) == true)
        {
            ValidateCommand.Execute(result);
        }

        return result;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Increases the value by the <see cref="Step"/> amount.
    /// </summary>
    public void Increment()
    {
        if (!CanIncrement) return;

        var newValue = (Value ?? 0) + Step;
        Value = Math.Min(newValue, Maximum);

        IncrementCommand?.Execute(Value);
    }

    /// <summary>
    /// Decreases the value by the <see cref="Step"/> amount.
    /// </summary>
    public void Decrement()
    {
        if (!CanDecrement) return;

        var newValue = (Value ?? 0) - Step;
        Value = Math.Max(newValue, Minimum);

        DecrementCommand?.Execute(Value);
    }

    #endregion

    #region Property Changed Handlers

    private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is NumericUpDown control)
        {
            control.OnValueChangedInternal((double?)oldValue, (double?)newValue);
        }
    }

    private void OnValueChangedInternal(double? oldValue, double? newValue)
    {
        // Clamp to range if needed
        if (newValue.HasValue)
        {
            var clamped = Math.Clamp(newValue.Value, Minimum, Maximum);
            if (Math.Abs(clamped - newValue.Value) > double.Epsilon)
            {
                Value = clamped;
                return;
            }
        }

        // Update entry text
        UpdateEntryText();

        // Notify computed properties
        OnPropertyChanged(nameof(DisplayText));
        OnPropertyChanged(nameof(CanIncrement));
        OnPropertyChanged(nameof(CanDecrement));
        UpdateButtonStates();

        // Raise event
        var args = new ValueChangedEventArgs(oldValue, newValue);
        ValueChanged?.Invoke(this, args);

        // Execute command
        var parameter = ValueChangedCommandParameter ?? newValue;
        if (ValueChangedCommand?.CanExecute(parameter) == true)
        {
            ValueChangedCommand.Execute(parameter);
        }

        // Validate
        Validate();
    }

    private static void OnMinimumChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is NumericUpDown control)
        {
            // Re-clamp current value if needed
            if (control.Value.HasValue && control.Value.Value < (double)newValue)
            {
                control.Value = (double)newValue;
            }
            control.OnPropertyChanged(nameof(CanDecrement));
            control.UpdateButtonStates();
        }
    }

    private static void OnMaximumChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is NumericUpDown control)
        {
            // Re-clamp current value if needed
            if (control.Value.HasValue && control.Value.Value > (double)newValue)
            {
                control.Value = (double)newValue;
            }
            control.OnPropertyChanged(nameof(CanIncrement));
            control.UpdateButtonStates();
        }
    }

    private static void OnDecimalPlacesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is NumericUpDown control)
        {
            control.UpdateEntryText();
            control.OnPropertyChanged(nameof(DisplayText));
        }
    }

    private static void OnFormatChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is NumericUpDown control)
        {
            control.UpdateEntryText();
            control.OnPropertyChanged(nameof(DisplayText));
        }
    }

    private static void OnIsReadOnlyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is NumericUpDown control)
        {
            if (control._entry != null)
            {
                control._entry.IsReadOnly = (bool)newValue;
            }
            control.OnPropertyChanged(nameof(CanIncrement));
            control.OnPropertyChanged(nameof(CanDecrement));
            control.UpdateButtonStates();
        }
    }

    private static void OnPlaceholderChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is NumericUpDown control && control._entry != null)
        {
            control._entry.Placeholder = (string?)newValue;
        }
    }

    private static void OnButtonPlacementChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is NumericUpDown control)
        {
            control.BuildLayout();
        }
    }

    #endregion

    #region Layout Building

    private void BuildLayout()
    {
        if (_mainGrid == null) return;

        _mainGrid.Children.Clear();
        _mainGrid.ColumnDefinitions.Clear();
        _mainGrid.RowDefinitions.Clear();

        // Create entry
        _entry = new Entry
        {
            Keyboard = Keyboard.Numeric,
            IsReadOnly = IsReadOnly,
            Placeholder = Placeholder,
            BackgroundColor = Colors.Transparent,
            FontSize = EffectiveFontSize,
            TextColor = EffectiveTextColor,
            PlaceholderColor = EffectivePlaceholderColor,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill
        };
        _entry.TextChanged += OnEntryTextChanged;
        _entry.Unfocused += OnEntryUnfocused;
        _entry.Focused += OnEntryFocused;

        // Create buttons
        _incrementButton = CreateButton("▲", OnIncrementClicked, OnIncrementPressed, OnButtonReleased);
        _decrementButton = CreateButton("▼", OnDecrementClicked, OnDecrementPressed, OnButtonReleased);

        switch (ButtonPlacement)
        {
            case ButtonPlacement.Right:
                BuildRightLayout();
                break;
            case ButtonPlacement.Left:
                BuildLeftLayout();
                break;
            case ButtonPlacement.LeftAndRight:
                BuildLeftAndRightLayout();
                break;
            case ButtonPlacement.Stacked:
                BuildStackedLayout();
                break;
        }

        UpdateEntryText();
        UpdateButtonStates();
    }

    private void BuildRightLayout()
    {
        _mainGrid!.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(36)));

        _mainGrid.Children.Add(_entry!);
        Grid.SetColumn(_entry!, 0);

        var buttonStack = new VerticalStackLayout
        {
            Spacing = 0,
            VerticalOptions = LayoutOptions.Center
        };
        buttonStack.Children.Add(_incrementButton!);
        buttonStack.Children.Add(_decrementButton!);
        _mainGrid.Children.Add(buttonStack);
        Grid.SetColumn(buttonStack, 1);
    }

    private void BuildLeftLayout()
    {
        _mainGrid!.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(36)));
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        var buttonStack = new VerticalStackLayout
        {
            Spacing = 0,
            VerticalOptions = LayoutOptions.Center
        };
        buttonStack.Children.Add(_incrementButton!);
        buttonStack.Children.Add(_decrementButton!);
        _mainGrid.Children.Add(buttonStack);
        Grid.SetColumn(buttonStack, 0);

        _mainGrid.Children.Add(_entry!);
        Grid.SetColumn(_entry!, 1);
    }

    private void BuildLeftAndRightLayout()
    {
        _mainGrid!.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(44)));
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(44)));

        _decrementButton!.Text = "−";
        _decrementButton.FontSize = 20;
        _decrementButton.WidthRequest = 44;
        _decrementButton.HeightRequest = 44;
        _mainGrid.Children.Add(_decrementButton);
        Grid.SetColumn(_decrementButton, 0);

        _mainGrid.Children.Add(_entry!);
        Grid.SetColumn(_entry!, 1);
        _entry!.HorizontalTextAlignment = TextAlignment.Center;

        _incrementButton!.Text = "+";
        _incrementButton.FontSize = 20;
        _incrementButton.WidthRequest = 44;
        _incrementButton.HeightRequest = 44;
        _mainGrid.Children.Add(_incrementButton);
        Grid.SetColumn(_incrementButton, 2);
    }

    private void BuildStackedLayout()
    {
        _mainGrid!.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(36)));
        _mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        _mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));

        _mainGrid.Children.Add(_entry!);
        Grid.SetColumn(_entry!, 0);
        Grid.SetRowSpan(_entry!, 2);

        _incrementButton!.HeightRequest = 22;
        _mainGrid.Children.Add(_incrementButton);
        Grid.SetColumn(_incrementButton, 1);
        Grid.SetRow(_incrementButton, 0);

        _decrementButton!.HeightRequest = 22;
        _mainGrid.Children.Add(_decrementButton);
        Grid.SetColumn(_decrementButton, 1);
        Grid.SetRow(_decrementButton, 1);
    }

    private Button CreateButton(string text, EventHandler clicked, EventHandler pressed, EventHandler released)
    {
        var button = new Button
        {
            Text = text,
            FontSize = 12,
            WidthRequest = 36,
            HeightRequest = 22,
            Padding = 0,
            BackgroundColor = Colors.Transparent,
            TextColor = EffectiveForegroundColor,
            BorderWidth = 0
        };
        button.Clicked += clicked;
        button.Pressed += pressed;
        button.Released += released;
        return button;
    }

    private void UpdateButtonStates()
    {
        if (_incrementButton != null)
        {
            _incrementButton.IsEnabled = CanIncrement;
            _incrementButton.Opacity = CanIncrement ? 1.0 : 0.4;
        }
        if (_decrementButton != null)
        {
            _decrementButton.IsEnabled = CanDecrement;
            _decrementButton.Opacity = CanDecrement ? 1.0 : 0.4;
        }
    }

    private void UpdateEntryText()
    {
        if (_entry == null || _isUpdatingText) return;

        _isUpdatingText = true;
        _entry.Text = DisplayText;
        _isUpdatingText = false;
    }

    #endregion

    #region Event Handlers

    private void OnEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isUpdatingText) return;

        // Allow empty for nullable
        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            Value = null;
            return;
        }

        // Try parse the text
        if (double.TryParse(e.NewTextValue, NumberStyles.Any, CultureInfo.CurrentCulture, out var parsed))
        {
            Value = parsed;
        }
    }

    private void OnEntryUnfocused(object? sender, FocusEventArgs e)
    {
        // Reformat the text on unfocus
        UpdateEntryText();
        OnPropertyChanged(nameof(CurrentBorderColor));
    }

    private void OnEntryFocused(object? sender, FocusEventArgs e)
    {
        OnPropertyChanged(nameof(CurrentBorderColor));
    }

    private void OnIncrementClicked(object? sender, EventArgs e)
    {
        Increment();
    }

    private void OnDecrementClicked(object? sender, EventArgs e)
    {
        Decrement();
    }

    private async void OnIncrementPressed(object? sender, EventArgs e)
    {
        _repeatCts = new CancellationTokenSource();
        await RepeatActionAsync(Increment, _repeatCts.Token);
    }

    private async void OnDecrementPressed(object? sender, EventArgs e)
    {
        _repeatCts = new CancellationTokenSource();
        await RepeatActionAsync(Decrement, _repeatCts.Token);
    }

    private void OnButtonReleased(object? sender, EventArgs e)
    {
        _repeatCts?.Cancel();
        _repeatCts = null;
    }

    private async Task RepeatActionAsync(Action action, CancellationToken token)
    {
        try
        {
            await Task.Delay(RepeatDelay, token);
            while (!token.IsCancellationRequested)
            {
                action();
                await Task.Delay(RepeatInterval, token);
            }
        }
        catch (TaskCanceledException)
        {
            // Expected when button released
        }
    }

    #endregion

    #region Theme Updates

    /// <inheritdoc/>
    public override void OnThemeChanged(AppTheme theme)
    {
        base.OnThemeChanged(theme);

        if (_entry != null)
        {
            _entry.TextColor = EffectiveTextColor;
            _entry.PlaceholderColor = EffectivePlaceholderColor;
        }

        if (_incrementButton != null)
        {
            _incrementButton.TextColor = EffectiveForegroundColor;
        }

        if (_decrementButton != null)
        {
            _decrementButton.TextColor = EffectiveForegroundColor;
        }

        OnPropertyChanged(nameof(CurrentBorderColor));
    }

    #endregion

    #region IKeyboardNavigable Implementation

    /// <inheritdoc />
    public bool CanReceiveFocus => IsEnabled && IsVisible;

    /// <inheritdoc />
    public bool IsKeyboardNavigationEnabled
    {
        get => _isKeyboardNavigationEnabled;
        set
        {
            _isKeyboardNavigationEnabled = value;
            OnPropertyChanged(nameof(IsKeyboardNavigationEnabled));
        }
    }

    /// <inheritdoc />
    public bool HasKeyboardFocus => IsFocused;

    /// <summary>
    /// Identifies the GotFocusCommand bindable property.
    /// </summary>
    public static readonly BindableProperty GotFocusCommandProperty = BindableProperty.Create(
        nameof(GotFocusCommand),
        typeof(ICommand),
        typeof(NumericUpDown));

    /// <summary>
    /// Identifies the LostFocusCommand bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandProperty = BindableProperty.Create(
        nameof(LostFocusCommand),
        typeof(ICommand),
        typeof(NumericUpDown));

    /// <summary>
    /// Identifies the KeyPressCommand bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandProperty = BindableProperty.Create(
        nameof(KeyPressCommand),
        typeof(ICommand),
        typeof(NumericUpDown));

    /// <inheritdoc />
    public ICommand? GotFocusCommand
    {
        get => (ICommand?)GetValue(GotFocusCommandProperty);
        set => SetValue(GotFocusCommandProperty, value);
    }

    /// <inheritdoc />
    public ICommand? LostFocusCommand
    {
        get => (ICommand?)GetValue(LostFocusCommandProperty);
        set => SetValue(LostFocusCommandProperty, value);
    }

    /// <inheritdoc />
    public ICommand? KeyPressCommand
    {
        get => (ICommand?)GetValue(KeyPressCommandProperty);
        set => SetValue(KeyPressCommandProperty, value);
    }

    /// <inheritdoc />
    public event EventHandler<Base.KeyboardFocusEventArgs>? KeyboardFocusGained;

    /// <inheritdoc />
#pragma warning disable CS0067 // Event is never used (raised by platform-specific handlers)
    public event EventHandler<Base.KeyboardFocusEventArgs>? KeyboardFocusLost;
#pragma warning restore CS0067

    /// <inheritdoc />
    public event EventHandler<Base.KeyEventArgs>? KeyPressed;

    /// <inheritdoc />
#pragma warning disable CS0067 // Event is never used (raised by platform-specific handlers)
    public event EventHandler<Base.KeyEventArgs>? KeyReleased;
#pragma warning restore CS0067

    /// <inheritdoc />
    public bool HandleKeyPress(Base.KeyEventArgs e)
    {
        if (!IsKeyboardNavigationEnabled) return false;

        // Raise event first
        KeyPressed?.Invoke(this, e);
        if (e.Handled) return true;

        // Execute command if set
        if (KeyPressCommand?.CanExecute(e) == true)
        {
            KeyPressCommand.Execute(e);
            if (e.Handled) return true;
        }

        return e.Key switch
        {
            "ArrowUp" => HandleIncrementKey(),
            "ArrowDown" => HandleDecrementKey(),
            "PageUp" => HandleLargeIncrementKey(),
            "PageDown" => HandleLargeDecrementKey(),
            "Home" => HandleHomeKey(),
            "End" => HandleEndKey(),
            _ => false
        };
    }

    /// <inheritdoc />
    public IReadOnlyList<Base.KeyboardShortcut> GetKeyboardShortcuts()
    {
        if (_keyboardShortcuts.Count == 0)
        {
            _keyboardShortcuts.AddRange(new[]
            {
                new Base.KeyboardShortcut { Key = "ArrowUp", Description = "Increment value by step", Category = "Value" },
                new Base.KeyboardShortcut { Key = "ArrowDown", Description = "Decrement value by step", Category = "Value" },
                new Base.KeyboardShortcut { Key = "PageUp", Description = "Increment by large step (10x)", Category = "Value" },
                new Base.KeyboardShortcut { Key = "PageDown", Description = "Decrement by large step (10x)", Category = "Value" },
                new Base.KeyboardShortcut { Key = "Home", Description = "Set to minimum value", Category = "Value" },
                new Base.KeyboardShortcut { Key = "End", Description = "Set to maximum value", Category = "Value" },
            });
        }
        return _keyboardShortcuts;
    }

    /// <inheritdoc />
    public new bool Focus()
    {
        if (!CanReceiveFocus) return false;

        var result = _entry?.Focus() ?? base.Focus();
        if (result)
        {
            KeyboardFocusGained?.Invoke(this, new Base.KeyboardFocusEventArgs(true));
            GotFocusCommand?.Execute(this);
        }
        return result;
    }

    private bool HandleIncrementKey()
    {
        Increment();
        return true;
    }

    private bool HandleDecrementKey()
    {
        Decrement();
        return true;
    }

    private bool HandleLargeIncrementKey()
    {
        var largeStep = Step * 10;
        Value = Math.Min((Value ?? 0) + largeStep, Maximum);
        return true;
    }

    private bool HandleLargeDecrementKey()
    {
        var largeStep = Step * 10;
        Value = Math.Max((Value ?? 0) - largeStep, Minimum);
        return true;
    }

    private bool HandleHomeKey()
    {
        if (Minimum != double.MinValue)
        {
            Value = Minimum;
            return true;
        }
        return false;
    }

    private bool HandleEndKey()
    {
        if (Maximum != double.MaxValue)
        {
            Value = Maximum;
            return true;
        }
        return false;
    }

    #endregion
}

/// <summary>
/// Provides data for the <see cref="NumericUpDown.ValueChanged"/> event.
/// </summary>
public class ValueChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueChangedEventArgs"/> class.
    /// </summary>
    public ValueChangedEventArgs(double? oldValue, double? newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    /// <summary>
    /// Gets the previous value.
    /// </summary>
    public double? OldValue { get; }

    /// <summary>
    /// Gets the new value.
    /// </summary>
    public double? NewValue { get; }
}
