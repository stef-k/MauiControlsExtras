using System.Windows.Input;
using MauiControlsExtras.Base;
using MauiControlsExtras.Base.Validation;

namespace MauiControlsExtras.Controls;

/// <summary>
/// Orientation for the RangeSlider control.
/// </summary>
public enum SliderOrientation
{
    /// <summary>
    /// Horizontal orientation (default).
    /// </summary>
    Horizontal,

    /// <summary>
    /// Vertical orientation.
    /// </summary>
    Vertical
}

/// <summary>
/// Event arguments for range value changes.
/// </summary>
public class RangeChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the new lower value.
    /// </summary>
    public double LowerValue { get; }

    /// <summary>
    /// Gets the new upper value.
    /// </summary>
    public double UpperValue { get; }

    /// <summary>
    /// Initializes a new instance of RangeChangedEventArgs.
    /// </summary>
    public RangeChangedEventArgs(double lowerValue, double upperValue)
    {
        LowerValue = lowerValue;
        UpperValue = upperValue;
    }
}

/// <summary>
/// A dual-thumb slider control for selecting a range of values (minimum and maximum).
/// </summary>
public partial class RangeSlider : StyledControlBase, IValidatable, Base.IKeyboardNavigable
{
    #region Private Fields

    private double _lowerThumbStartX;
    private double _upperThumbStartX;
    private double _lowerThumbStartY;
    private double _upperThumbStartY;
    private bool _isDragging;
    private double _trackWidth;
    private double _trackHeight;
    private bool _lowerThumbActive = true; // Which thumb is active for keyboard control

    private readonly List<string> _validationErrors = new();
    private bool _isValid = true;
    private bool _isKeyboardNavigationEnabled = true;
    private static readonly List<Base.KeyboardShortcut> _keyboardShortcuts = new();

    #endregion

    #region Range Bindable Properties

    /// <summary>
    /// Identifies the <see cref="Minimum"/> bindable property.
    /// </summary>
    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
        nameof(Minimum),
        typeof(double),
        typeof(RangeSlider),
        0.0,
        propertyChanged: OnRangePropertyChanged);

    /// <summary>
    /// Identifies the <see cref="Maximum"/> bindable property.
    /// </summary>
    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum),
        typeof(double),
        typeof(RangeSlider),
        100.0,
        propertyChanged: OnRangePropertyChanged);

    /// <summary>
    /// Identifies the <see cref="LowerValue"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LowerValueProperty = BindableProperty.Create(
        nameof(LowerValue),
        typeof(double),
        typeof(RangeSlider),
        0.0,
        BindingMode.TwoWay,
        propertyChanged: OnLowerValueChanged);

    /// <summary>
    /// Identifies the <see cref="UpperValue"/> bindable property.
    /// </summary>
    public static readonly BindableProperty UpperValueProperty = BindableProperty.Create(
        nameof(UpperValue),
        typeof(double),
        typeof(RangeSlider),
        100.0,
        BindingMode.TwoWay,
        propertyChanged: OnUpperValueChanged);

    /// <summary>
    /// Identifies the <see cref="Step"/> bindable property.
    /// </summary>
    public static readonly BindableProperty StepProperty = BindableProperty.Create(
        nameof(Step),
        typeof(double),
        typeof(RangeSlider),
        1.0);

    /// <summary>
    /// Identifies the <see cref="MinimumRange"/> bindable property.
    /// </summary>
    public static readonly BindableProperty MinimumRangeProperty = BindableProperty.Create(
        nameof(MinimumRange),
        typeof(double),
        typeof(RangeSlider),
        0.0);

    #endregion

    #region Appearance Bindable Properties

    /// <summary>
    /// Identifies the <see cref="ShowLabels"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowLabelsProperty = BindableProperty.Create(
        nameof(ShowLabels),
        typeof(bool),
        typeof(RangeSlider),
        false);

    /// <summary>
    /// Identifies the <see cref="ShowMinMaxLabels"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowMinMaxLabelsProperty = BindableProperty.Create(
        nameof(ShowMinMaxLabels),
        typeof(bool),
        typeof(RangeSlider),
        false);

    /// <summary>
    /// Identifies the <see cref="LabelFormat"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LabelFormatProperty = BindableProperty.Create(
        nameof(LabelFormat),
        typeof(string),
        typeof(RangeSlider),
        "F0",
        propertyChanged: OnLabelFormatChanged);

    /// <summary>
    /// Identifies the <see cref="TrackColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty TrackColorProperty = BindableProperty.Create(
        nameof(TrackColor),
        typeof(Color),
        typeof(RangeSlider),
        null);

    /// <summary>
    /// Identifies the <see cref="RangeColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RangeColorProperty = BindableProperty.Create(
        nameof(RangeColor),
        typeof(Color),
        typeof(RangeSlider),
        null);

    /// <summary>
    /// Identifies the <see cref="ThumbColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ThumbColorProperty = BindableProperty.Create(
        nameof(ThumbColor),
        typeof(Color),
        typeof(RangeSlider),
        null);

    /// <summary>
    /// Identifies the <see cref="ThumbSize"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ThumbSizeProperty = BindableProperty.Create(
        nameof(ThumbSize),
        typeof(double),
        typeof(RangeSlider),
        24.0,
        propertyChanged: OnThumbSizeChanged);

    /// <summary>
    /// Identifies the <see cref="TrackHeight"/> bindable property.
    /// </summary>
    public static readonly BindableProperty TrackHeightProperty = BindableProperty.Create(
        nameof(TrackHeight),
        typeof(double),
        typeof(RangeSlider),
        4.0);

    /// <summary>
    /// Identifies the <see cref="Orientation"/> bindable property.
    /// </summary>
    public static readonly BindableProperty OrientationProperty = BindableProperty.Create(
        nameof(Orientation),
        typeof(SliderOrientation),
        typeof(RangeSlider),
        SliderOrientation.Horizontal,
        propertyChanged: OnOrientationChanged);

    #endregion

    #region Validation Bindable Properties

    /// <summary>
    /// Identifies the <see cref="IsRequired"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(
        nameof(IsRequired),
        typeof(bool),
        typeof(RangeSlider),
        false);

    /// <summary>
    /// Identifies the <see cref="RequiredErrorMessage"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RequiredErrorMessageProperty = BindableProperty.Create(
        nameof(RequiredErrorMessage),
        typeof(string),
        typeof(RangeSlider),
        "A valid range selection is required.");

    /// <summary>
    /// Identifies the <see cref="ValidateCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ValidateCommandProperty = BindableProperty.Create(
        nameof(ValidateCommand),
        typeof(ICommand),
        typeof(RangeSlider));

    #endregion

    #region Command Bindable Properties

    /// <summary>
    /// Identifies the <see cref="LowerValueChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LowerValueChangedCommandProperty = BindableProperty.Create(
        nameof(LowerValueChangedCommand),
        typeof(ICommand),
        typeof(RangeSlider));

    /// <summary>
    /// Identifies the <see cref="LowerValueChangedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LowerValueChangedCommandParameterProperty = BindableProperty.Create(
        nameof(LowerValueChangedCommandParameter),
        typeof(object),
        typeof(RangeSlider));

    /// <summary>
    /// Identifies the <see cref="UpperValueChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty UpperValueChangedCommandProperty = BindableProperty.Create(
        nameof(UpperValueChangedCommand),
        typeof(ICommand),
        typeof(RangeSlider));

    /// <summary>
    /// Identifies the <see cref="UpperValueChangedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty UpperValueChangedCommandParameterProperty = BindableProperty.Create(
        nameof(UpperValueChangedCommandParameter),
        typeof(object),
        typeof(RangeSlider));

    /// <summary>
    /// Identifies the <see cref="RangeChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RangeChangedCommandProperty = BindableProperty.Create(
        nameof(RangeChangedCommand),
        typeof(ICommand),
        typeof(RangeSlider));

    /// <summary>
    /// Identifies the <see cref="RangeChangedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RangeChangedCommandParameterProperty = BindableProperty.Create(
        nameof(RangeChangedCommandParameter),
        typeof(object),
        typeof(RangeSlider));

    /// <summary>
    /// Identifies the <see cref="DragStartedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DragStartedCommandProperty = BindableProperty.Create(
        nameof(DragStartedCommand),
        typeof(ICommand),
        typeof(RangeSlider));

    /// <summary>
    /// Identifies the <see cref="DragStartedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DragStartedCommandParameterProperty = BindableProperty.Create(
        nameof(DragStartedCommandParameter),
        typeof(object),
        typeof(RangeSlider));

    /// <summary>
    /// Identifies the <see cref="DragCompletedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DragCompletedCommandProperty = BindableProperty.Create(
        nameof(DragCompletedCommand),
        typeof(ICommand),
        typeof(RangeSlider));

    /// <summary>
    /// Identifies the <see cref="DragCompletedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DragCompletedCommandParameterProperty = BindableProperty.Create(
        nameof(DragCompletedCommandParameter),
        typeof(object),
        typeof(RangeSlider));

    #endregion

    #region Range Properties

    /// <summary>
    /// Gets or sets the minimum possible value.
    /// </summary>
    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum possible value.
    /// </summary>
    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>
    /// Gets or sets the current lower bound value (two-way bindable).
    /// </summary>
    public double LowerValue
    {
        get => (double)GetValue(LowerValueProperty);
        set => SetValue(LowerValueProperty, value);
    }

    /// <summary>
    /// Gets or sets the current upper bound value (two-way bindable).
    /// </summary>
    public double UpperValue
    {
        get => (double)GetValue(UpperValueProperty);
        set => SetValue(UpperValueProperty, value);
    }

    /// <summary>
    /// Gets or sets the step increment.
    /// </summary>
    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum distance between the two thumbs.
    /// </summary>
    public double MinimumRange
    {
        get => (double)GetValue(MinimumRangeProperty);
        set => SetValue(MinimumRangeProperty, value);
    }

    #endregion

    #region Appearance Properties

    /// <summary>
    /// Gets or sets whether to show value labels on the thumbs.
    /// </summary>
    public bool ShowLabels
    {
        get => (bool)GetValue(ShowLabelsProperty);
        set => SetValue(ShowLabelsProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to show min/max labels at the ends of the track.
    /// </summary>
    public bool ShowMinMaxLabels
    {
        get => (bool)GetValue(ShowMinMaxLabelsProperty);
        set => SetValue(ShowMinMaxLabelsProperty, value);
    }

    /// <summary>
    /// Gets or sets the format string for labels.
    /// </summary>
    public string LabelFormat
    {
        get => (string)GetValue(LabelFormatProperty);
        set => SetValue(LabelFormatProperty, value);
    }

    /// <summary>
    /// Gets or sets the unselected track color.
    /// </summary>
    public Color? TrackColor
    {
        get => (Color?)GetValue(TrackColorProperty);
        set => SetValue(TrackColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected range track color.
    /// </summary>
    public Color? RangeColor
    {
        get => (Color?)GetValue(RangeColorProperty);
        set => SetValue(RangeColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the thumb color.
    /// </summary>
    public Color? ThumbColor
    {
        get => (Color?)GetValue(ThumbColorProperty);
        set => SetValue(ThumbColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the thumb diameter.
    /// </summary>
    public double ThumbSize
    {
        get => (double)GetValue(ThumbSizeProperty);
        set => SetValue(ThumbSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the track height (thickness).
    /// </summary>
    public double TrackHeight
    {
        get => (double)GetValue(TrackHeightProperty);
        set => SetValue(TrackHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the slider orientation.
    /// </summary>
    public SliderOrientation Orientation
    {
        get => (SliderOrientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    #endregion

    #region Validation Properties

    /// <summary>
    /// Gets or sets whether a valid range selection is required.
    /// </summary>
    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    /// <summary>
    /// Gets or sets the error message when validation fails due to required constraint.
    /// </summary>
    public string RequiredErrorMessage
    {
        get => (string)GetValue(RequiredErrorMessageProperty);
        set => SetValue(RequiredErrorMessageProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when validation is triggered.
    /// </summary>
    public ICommand? ValidateCommand
    {
        get => (ICommand?)GetValue(ValidateCommandProperty);
        set => SetValue(ValidateCommandProperty, value);
    }

    /// <summary>
    /// Gets whether the current value is valid.
    /// </summary>
    public bool IsValid => _isValid;

    /// <summary>
    /// Gets the list of current validation errors.
    /// </summary>
    public IReadOnlyList<string> ValidationErrors => _validationErrors.AsReadOnly();

    #endregion

    #region Command Properties

    /// <summary>
    /// Gets or sets the command to execute when the lower value changes.
    /// </summary>
    public ICommand? LowerValueChangedCommand
    {
        get => (ICommand?)GetValue(LowerValueChangedCommandProperty);
        set => SetValue(LowerValueChangedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter for LowerValueChangedCommand.
    /// </summary>
    public object? LowerValueChangedCommandParameter
    {
        get => GetValue(LowerValueChangedCommandParameterProperty);
        set => SetValue(LowerValueChangedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the upper value changes.
    /// </summary>
    public ICommand? UpperValueChangedCommand
    {
        get => (ICommand?)GetValue(UpperValueChangedCommandProperty);
        set => SetValue(UpperValueChangedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter for UpperValueChangedCommand.
    /// </summary>
    public object? UpperValueChangedCommandParameter
    {
        get => GetValue(UpperValueChangedCommandParameterProperty);
        set => SetValue(UpperValueChangedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when either value changes.
    /// </summary>
    public ICommand? RangeChangedCommand
    {
        get => (ICommand?)GetValue(RangeChangedCommandProperty);
        set => SetValue(RangeChangedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter for RangeChangedCommand.
    /// </summary>
    public object? RangeChangedCommandParameter
    {
        get => GetValue(RangeChangedCommandParameterProperty);
        set => SetValue(RangeChangedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the user starts dragging.
    /// </summary>
    public ICommand? DragStartedCommand
    {
        get => (ICommand?)GetValue(DragStartedCommandProperty);
        set => SetValue(DragStartedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter for DragStartedCommand.
    /// </summary>
    public object? DragStartedCommandParameter
    {
        get => GetValue(DragStartedCommandParameterProperty);
        set => SetValue(DragStartedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the user stops dragging.
    /// </summary>
    public ICommand? DragCompletedCommand
    {
        get => (ICommand?)GetValue(DragCompletedCommandProperty);
        set => SetValue(DragCompletedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter for DragCompletedCommand.
    /// </summary>
    public object? DragCompletedCommandParameter
    {
        get => GetValue(DragCompletedCommandParameterProperty);
        set => SetValue(DragCompletedCommandParameterProperty, value);
    }

    #endregion

    #region Effective Properties

    /// <summary>
    /// Gets the effective track color, falling back to a default gray.
    /// </summary>
    public Color EffectiveTrackColor => TrackColor ?? Color.FromArgb("#E0E0E0");

    /// <summary>
    /// Gets the effective range color, falling back to accent color.
    /// </summary>
    public Color EffectiveRangeColor => RangeColor ?? EffectiveAccentColor;

    /// <summary>
    /// Gets the effective thumb color, falling back to accent color.
    /// </summary>
    public Color EffectiveThumbColor => ThumbColor ?? EffectiveAccentColor;

    /// <summary>
    /// Gets the track corner radius based on track height.
    /// </summary>
    public double TrackCornerRadius => TrackHeight / 2;

    /// <summary>
    /// Gets the track area height (same as thumb size for touch targets).
    /// </summary>
    public double TrackAreaHeight => ThumbSize;

    /// <summary>
    /// Gets whether the orientation is horizontal.
    /// </summary>
    public bool IsHorizontal => Orientation == SliderOrientation.Horizontal;

    /// <summary>
    /// Gets whether the orientation is vertical.
    /// </summary>
    public bool IsVertical => Orientation == SliderOrientation.Vertical;

    #endregion

    #region Calculated Display Properties

    /// <summary>
    /// Gets the lower value formatted for display.
    /// </summary>
    public string LowerLabelText => LowerValue.ToString(LabelFormat);

    /// <summary>
    /// Gets the upper value formatted for display.
    /// </summary>
    public string UpperLabelText => UpperValue.ToString(LabelFormat);

    /// <summary>
    /// Gets the minimum value formatted for display.
    /// </summary>
    public string MinLabelText => Minimum.ToString(LabelFormat);

    /// <summary>
    /// Gets the maximum value formatted for display.
    /// </summary>
    public string MaxLabelText => Maximum.ToString(LabelFormat);

    /// <summary>
    /// Gets the lower thumb X position for horizontal layout.
    /// </summary>
    public double LowerThumbPosition => CalculateLowerThumbPosition();

    /// <summary>
    /// Gets the upper thumb X position for horizontal layout.
    /// </summary>
    public double UpperThumbPosition => CalculateUpperThumbPosition();

    /// <summary>
    /// Gets the lower thumb Y position for vertical layout.
    /// </summary>
    public double LowerThumbPositionVertical => CalculateLowerThumbPositionVertical();

    /// <summary>
    /// Gets the upper thumb Y position for vertical layout.
    /// </summary>
    public double UpperThumbPositionVertical => CalculateUpperThumbPositionVertical();

    /// <summary>
    /// Gets the range track margin for horizontal layout.
    /// </summary>
    public Thickness RangeTrackMargin => new(LowerThumbPosition + ThumbSize / 2, 0, 0, 0);

    /// <summary>
    /// Gets the range track width for horizontal layout.
    /// </summary>
    public double RangeTrackWidth => Math.Max(0, UpperThumbPosition - LowerThumbPosition);

    /// <summary>
    /// Gets the range track margin for vertical layout.
    /// </summary>
    public Thickness RangeTrackMarginVertical => new(0, 0, 0, -LowerThumbPositionVertical + ThumbSize / 2);

    /// <summary>
    /// Gets the range track height for vertical layout.
    /// </summary>
    public double RangeTrackHeightVertical => Math.Max(0, LowerThumbPositionVertical - UpperThumbPositionVertical);

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the lower value changes.
    /// </summary>
    public event EventHandler<double>? LowerValueChanged;

    /// <summary>
    /// Occurs when the upper value changes.
    /// </summary>
    public event EventHandler<double>? UpperValueChanged;

    /// <summary>
    /// Occurs when either value changes.
    /// </summary>
    public event EventHandler<RangeChangedEventArgs>? RangeChanged;

    /// <summary>
    /// Occurs when the user starts dragging a thumb.
    /// </summary>
    public event EventHandler? DragStarted;

    /// <summary>
    /// Occurs when the user stops dragging a thumb.
    /// </summary>
    public event EventHandler? DragCompleted;

    /// <summary>
    /// Occurs when the validation state changes.
    /// </summary>
    public event EventHandler<bool>? ValidationChanged;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the RangeSlider control.
    /// </summary>
    public RangeSlider()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        // Ensure track dimensions are properly initialized after layout
        Dispatcher.Dispatch(() =>
        {
            if (Orientation == SliderOrientation.Horizontal)
            {
                _trackWidth = sliderGrid?.Width ?? Width;
                if (_trackWidth <= 0)
                    _trackWidth = sliderGrid?.DesiredSize.Width ?? 200;
            }
            else
            {
                _trackHeight = verticalSliderGrid?.Height ?? Height;
                if (_trackHeight <= 0)
                    _trackHeight = verticalSliderGrid?.DesiredSize.Height ?? 200;
            }
            UpdateThumbPositions();
        });
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets both lower and upper values at once.
    /// </summary>
    /// <param name="lower">The lower value.</param>
    /// <param name="upper">The upper value.</param>
    public void SetRange(double lower, double upper)
    {
        var clampedLower = Math.Max(Minimum, Math.Min(lower, Maximum - MinimumRange));
        var clampedUpper = Math.Max(Minimum + MinimumRange, Math.Min(upper, Maximum));

        if (clampedUpper - clampedLower < MinimumRange)
        {
            clampedUpper = clampedLower + MinimumRange;
        }

        LowerValue = clampedLower;
        UpperValue = clampedUpper;
    }

    /// <summary>
    /// Resets to the full range (Minimum to Maximum).
    /// </summary>
    public void Reset()
    {
        LowerValue = Minimum;
        UpperValue = Maximum;
    }

    /// <summary>
    /// Performs validation and returns the result.
    /// </summary>
    public ValidationResult Validate()
    {
        var previousIsValid = _isValid;
        _validationErrors.Clear();

        // Check required validation
        if (IsRequired && LowerValue >= UpperValue)
        {
            _validationErrors.Add(RequiredErrorMessage);
        }

        // Check range validity
        if (LowerValue < Minimum || LowerValue > Maximum)
        {
            _validationErrors.Add($"Lower value must be between {Minimum} and {Maximum}.");
        }

        if (UpperValue < Minimum || UpperValue > Maximum)
        {
            _validationErrors.Add($"Upper value must be between {Minimum} and {Maximum}.");
        }

        if (UpperValue - LowerValue < MinimumRange)
        {
            _validationErrors.Add($"The range must be at least {MinimumRange}.");
        }

        _isValid = _validationErrors.Count == 0;

        var result = new ValidationResult(_isValid, _validationErrors);

        if (_isValid != previousIsValid)
        {
            OnPropertyChanged(nameof(IsValid));
            ValidationChanged?.Invoke(this, _isValid);
        }

        if (ValidateCommand?.CanExecute(result) == true)
        {
            ValidateCommand.Execute(result);
        }

        return result;
    }

    #endregion

    #region Private Methods - Position Calculations

    private double CalculateLowerThumbPosition()
    {
        if (_trackWidth <= 0 || Maximum <= Minimum)
            return 0;

        var range = Maximum - Minimum;
        var normalizedValue = (LowerValue - Minimum) / range;
        var usableWidth = _trackWidth - ThumbSize;

        return normalizedValue * usableWidth;
    }

    private double CalculateUpperThumbPosition()
    {
        if (_trackWidth <= 0 || Maximum <= Minimum)
            return 0;

        var range = Maximum - Minimum;
        var normalizedValue = (UpperValue - Minimum) / range;
        var usableWidth = _trackWidth - ThumbSize;

        return normalizedValue * usableWidth;
    }

    private double CalculateLowerThumbPositionVertical()
    {
        if (_trackHeight <= 0 || Maximum <= Minimum)
            return 0;

        var range = Maximum - Minimum;
        var normalizedValue = (LowerValue - Minimum) / range;
        var usableHeight = _trackHeight - ThumbSize;

        // Inverted for vertical (bottom = low value)
        return -(normalizedValue * usableHeight);
    }

    private double CalculateUpperThumbPositionVertical()
    {
        if (_trackHeight <= 0 || Maximum <= Minimum)
            return 0;

        var range = Maximum - Minimum;
        var normalizedValue = (UpperValue - Minimum) / range;
        var usableHeight = _trackHeight - ThumbSize;

        // Inverted for vertical (top = high value)
        return -(normalizedValue * usableHeight);
    }

    private double SnapToStep(double value)
    {
        if (Step <= 0)
            return value;

        var steppedValue = Math.Round((value - Minimum) / Step) * Step + Minimum;
        return Math.Max(Minimum, Math.Min(steppedValue, Maximum));
    }

    private void UpdateThumbPositions()
    {
        OnPropertyChanged(nameof(LowerThumbPosition));
        OnPropertyChanged(nameof(UpperThumbPosition));
        OnPropertyChanged(nameof(LowerThumbPositionVertical));
        OnPropertyChanged(nameof(UpperThumbPositionVertical));
        OnPropertyChanged(nameof(RangeTrackMargin));
        OnPropertyChanged(nameof(RangeTrackWidth));
        OnPropertyChanged(nameof(RangeTrackMarginVertical));
        OnPropertyChanged(nameof(RangeTrackHeightVertical));
        OnPropertyChanged(nameof(LowerLabelText));
        OnPropertyChanged(nameof(UpperLabelText));
    }

    #endregion

    #region Event Handlers

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        if (Orientation == SliderOrientation.Horizontal)
        {
            var width = sliderGrid?.Width ?? Width;
            if (width > 0)
                _trackWidth = width;
        }
        else
        {
            var height = verticalSliderGrid?.Height ?? Height;
            if (height > 0)
                _trackHeight = height;
        }

        UpdateThumbPositions();
    }

    private void EnsureTrackDimensions()
    {
        // Fallback to get dimensions if they haven't been set yet
        if (Orientation == SliderOrientation.Horizontal && _trackWidth <= 0)
        {
            _trackWidth = sliderGrid?.Width ?? Width;
            if (_trackWidth <= 0)
                _trackWidth = sliderGrid?.DesiredSize.Width ?? 0;
        }
        else if (Orientation == SliderOrientation.Vertical && _trackHeight <= 0)
        {
            _trackHeight = verticalSliderGrid?.Height ?? Height;
            if (_trackHeight <= 0)
                _trackHeight = verticalSliderGrid?.DesiredSize.Height ?? 0;
        }
    }

    private void OnLowerThumbPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        HandleLowerThumbPan(e, isVertical: false);
    }

    private void OnUpperThumbPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        HandleUpperThumbPan(e, isVertical: false);
    }

    private void OnLowerThumbPanUpdatedVertical(object? sender, PanUpdatedEventArgs e)
    {
        HandleLowerThumbPan(e, isVertical: true);
    }

    private void OnUpperThumbPanUpdatedVertical(object? sender, PanUpdatedEventArgs e)
    {
        HandleUpperThumbPan(e, isVertical: true);
    }

    private void HandleLowerThumbPan(PanUpdatedEventArgs e, bool isVertical)
    {
        // Ensure track dimensions are available
        EnsureTrackDimensions();

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _lowerThumbStartX = LowerThumbPosition;
                _lowerThumbStartY = LowerThumbPositionVertical;
                if (!_isDragging)
                {
                    _isDragging = true;
                    RaiseDragStarted();
                }
                break;

            case GestureStatus.Running:
                double newPosition;
                double trackSize;
                double usableSize;

                if (isVertical)
                {
                    newPosition = _lowerThumbStartY - e.TotalY;
                    trackSize = _trackHeight;
                    usableSize = trackSize - ThumbSize;
                }
                else
                {
                    newPosition = _lowerThumbStartX + e.TotalX;
                    trackSize = _trackWidth;
                    usableSize = trackSize - ThumbSize;
                }

                if (trackSize <= 0 || usableSize <= 0)
                    return;

                var normalizedPosition = isVertical
                    ? -newPosition / usableSize
                    : newPosition / usableSize;

                normalizedPosition = Math.Max(0, Math.Min(normalizedPosition, 1));

                var newValue = Minimum + normalizedPosition * (Maximum - Minimum);
                newValue = SnapToStep(newValue);

                // Enforce constraints
                var maxLowerValue = UpperValue - MinimumRange;
                newValue = Math.Min(newValue, maxLowerValue);
                newValue = Math.Max(Minimum, newValue);

                if (Math.Abs(newValue - LowerValue) > 0.0001)
                {
                    LowerValue = newValue;
                }
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (_isDragging)
                {
                    _isDragging = false;
                    RaiseDragCompleted();
                }
                break;
        }
    }

    private void HandleUpperThumbPan(PanUpdatedEventArgs e, bool isVertical)
    {
        // Ensure track dimensions are available
        EnsureTrackDimensions();

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _upperThumbStartX = UpperThumbPosition;
                _upperThumbStartY = UpperThumbPositionVertical;
                if (!_isDragging)
                {
                    _isDragging = true;
                    RaiseDragStarted();
                }
                break;

            case GestureStatus.Running:
                double newPosition;
                double trackSize;
                double usableSize;

                if (isVertical)
                {
                    newPosition = _upperThumbStartY - e.TotalY;
                    trackSize = _trackHeight;
                    usableSize = trackSize - ThumbSize;
                }
                else
                {
                    newPosition = _upperThumbStartX + e.TotalX;
                    trackSize = _trackWidth;
                    usableSize = trackSize - ThumbSize;
                }

                if (trackSize <= 0 || usableSize <= 0)
                    return;

                var normalizedPosition = isVertical
                    ? -newPosition / usableSize
                    : newPosition / usableSize;

                normalizedPosition = Math.Max(0, Math.Min(normalizedPosition, 1));

                var newValue = Minimum + normalizedPosition * (Maximum - Minimum);
                newValue = SnapToStep(newValue);

                // Enforce constraints
                var minUpperValue = LowerValue + MinimumRange;
                newValue = Math.Max(newValue, minUpperValue);
                newValue = Math.Min(Maximum, newValue);

                if (Math.Abs(newValue - UpperValue) > 0.0001)
                {
                    UpperValue = newValue;
                }
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (_isDragging)
                {
                    _isDragging = false;
                    RaiseDragCompleted();
                }
                break;
        }
    }

    #endregion

    #region Property Changed Handlers

    private static void OnRangePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is RangeSlider slider)
        {
            slider.UpdateThumbPositions();
            slider.OnPropertyChanged(nameof(MinLabelText));
            slider.OnPropertyChanged(nameof(MaxLabelText));
        }
    }

    private static void OnLowerValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is RangeSlider slider)
        {
            var value = (double)newValue;

            // Clamp value within bounds
            var clampedValue = Math.Max(slider.Minimum, Math.Min(value, slider.UpperValue - slider.MinimumRange));
            if (Math.Abs(clampedValue - value) > 0.0001)
            {
                slider.LowerValue = clampedValue;
                return;
            }

            slider.UpdateThumbPositions();
            slider.RaiseLowerValueChanged(value);
            slider.RaiseRangeChanged();
        }
    }

    private static void OnUpperValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is RangeSlider slider)
        {
            var value = (double)newValue;

            // Clamp value within bounds
            var clampedValue = Math.Max(slider.LowerValue + slider.MinimumRange, Math.Min(value, slider.Maximum));
            if (Math.Abs(clampedValue - value) > 0.0001)
            {
                slider.UpperValue = clampedValue;
                return;
            }

            slider.UpdateThumbPositions();
            slider.RaiseUpperValueChanged(value);
            slider.RaiseRangeChanged();
        }
    }

    private static void OnLabelFormatChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is RangeSlider slider)
        {
            slider.OnPropertyChanged(nameof(LowerLabelText));
            slider.OnPropertyChanged(nameof(UpperLabelText));
            slider.OnPropertyChanged(nameof(MinLabelText));
            slider.OnPropertyChanged(nameof(MaxLabelText));
        }
    }

    private static void OnThumbSizeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is RangeSlider slider)
        {
            slider.OnPropertyChanged(nameof(TrackAreaHeight));
            slider.UpdateThumbPositions();
        }
    }

    private static void OnOrientationChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is RangeSlider slider)
        {
            slider.OnPropertyChanged(nameof(IsHorizontal));
            slider.OnPropertyChanged(nameof(IsVertical));
            slider.UpdateThumbPositions();
        }
    }

    #endregion

    #region Event Raising Methods

    private void RaiseLowerValueChanged(double value)
    {
        LowerValueChanged?.Invoke(this, value);

        var parameter = LowerValueChangedCommandParameter ?? value;
        if (LowerValueChangedCommand?.CanExecute(parameter) == true)
        {
            LowerValueChangedCommand.Execute(parameter);
        }
    }

    private void RaiseUpperValueChanged(double value)
    {
        UpperValueChanged?.Invoke(this, value);

        var parameter = UpperValueChangedCommandParameter ?? value;
        if (UpperValueChangedCommand?.CanExecute(parameter) == true)
        {
            UpperValueChangedCommand.Execute(parameter);
        }
    }

    private void RaiseRangeChanged()
    {
        var args = new RangeChangedEventArgs(LowerValue, UpperValue);
        RangeChanged?.Invoke(this, args);

        var parameter = RangeChangedCommandParameter ?? args;
        if (RangeChangedCommand?.CanExecute(parameter) == true)
        {
            RangeChangedCommand.Execute(parameter);
        }
    }

    private void RaiseDragStarted()
    {
        DragStarted?.Invoke(this, EventArgs.Empty);

        var parameter = DragStartedCommandParameter;
        if (DragStartedCommand?.CanExecute(parameter) == true)
        {
            DragStartedCommand.Execute(parameter);
        }
    }

    private void RaiseDragCompleted()
    {
        DragCompleted?.Invoke(this, EventArgs.Empty);

        var parameter = DragCompletedCommandParameter ?? new RangeChangedEventArgs(LowerValue, UpperValue);
        if (DragCompletedCommand?.CanExecute(parameter) == true)
        {
            DragCompletedCommand.Execute(parameter);
        }
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
        typeof(RangeSlider));

    /// <summary>
    /// Identifies the LostFocusCommand bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandProperty = BindableProperty.Create(
        nameof(LostFocusCommand),
        typeof(ICommand),
        typeof(RangeSlider));

    /// <summary>
    /// Identifies the KeyPressCommand bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandProperty = BindableProperty.Create(
        nameof(KeyPressCommand),
        typeof(ICommand),
        typeof(RangeSlider));

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

        var step = Step > 0 ? Step : (Maximum - Minimum) / 100;
        var largeStep = step * 10;

        return e.Key switch
        {
            "ArrowLeft" or "ArrowDown" => HandleDecrementKey(step),
            "ArrowRight" or "ArrowUp" => HandleIncrementKey(step),
            "PageDown" => HandleDecrementKey(largeStep),
            "PageUp" => HandleIncrementKey(largeStep),
            "Home" => HandleHomeKey(),
            "End" => HandleEndKey(),
            "Tab" when !e.IsShiftPressed => HandleSwitchThumb(false),
            "Tab" when e.IsShiftPressed => HandleSwitchThumb(true),
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
                new Base.KeyboardShortcut { Key = "Left/Down", Description = "Decrease active thumb value", Category = "Value" },
                new Base.KeyboardShortcut { Key = "Right/Up", Description = "Increase active thumb value", Category = "Value" },
                new Base.KeyboardShortcut { Key = "PageUp", Description = "Large increase", Category = "Value" },
                new Base.KeyboardShortcut { Key = "PageDown", Description = "Large decrease", Category = "Value" },
                new Base.KeyboardShortcut { Key = "Home", Description = "Set to minimum", Category = "Value" },
                new Base.KeyboardShortcut { Key = "End", Description = "Set to maximum", Category = "Value" },
                new Base.KeyboardShortcut { Key = "Tab", Description = "Switch between thumbs", Category = "Navigation" },
            });
        }
        return _keyboardShortcuts;
    }

    /// <inheritdoc />
    public new bool Focus()
    {
        if (!CanReceiveFocus) return false;

        var result = base.Focus();
        if (result)
        {
            KeyboardFocusGained?.Invoke(this, new Base.KeyboardFocusEventArgs(true));
            GotFocusCommand?.Execute(this);
        }
        return result;
    }

    private bool HandleIncrementKey(double step)
    {
        if (_lowerThumbActive)
        {
            var newValue = Math.Min(LowerValue + step, UpperValue - MinimumRange);
            LowerValue = Math.Min(newValue, Maximum);
        }
        else
        {
            UpperValue = Math.Min(UpperValue + step, Maximum);
        }
        return true;
    }

    private bool HandleDecrementKey(double step)
    {
        if (_lowerThumbActive)
        {
            LowerValue = Math.Max(LowerValue - step, Minimum);
        }
        else
        {
            var newValue = Math.Max(UpperValue - step, LowerValue + MinimumRange);
            UpperValue = Math.Max(newValue, Minimum);
        }
        return true;
    }

    private bool HandleHomeKey()
    {
        if (_lowerThumbActive)
        {
            LowerValue = Minimum;
        }
        else
        {
            UpperValue = LowerValue + MinimumRange;
        }
        return true;
    }

    private bool HandleEndKey()
    {
        if (_lowerThumbActive)
        {
            LowerValue = UpperValue - MinimumRange;
        }
        else
        {
            UpperValue = Maximum;
        }
        return true;
    }

    private bool HandleSwitchThumb(bool reverse)
    {
        _lowerThumbActive = !_lowerThumbActive;
        return true;
    }

    #endregion
}
