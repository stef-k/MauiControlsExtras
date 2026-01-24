using System.Windows.Input;
using MauiControlsExtras.Base;
using MauiControlsExtras.Base.Validation;

namespace MauiControlsExtras.Controls;

/// <summary>
/// Precision mode for the Rating control.
/// </summary>
public enum RatingPrecision
{
    /// <summary>
    /// Only whole numbers (1, 2, 3, etc.)
    /// </summary>
    Full,

    /// <summary>
    /// Whole and half values (1, 1.5, 2, 2.5, etc.)
    /// </summary>
    Half,

    /// <summary>
    /// Any decimal value (display only, for averages).
    /// </summary>
    Exact
}

/// <summary>
/// Built-in icon types for the Rating control.
/// </summary>
public enum RatingIcon
{
    /// <summary>
    /// Star icon (default).
    /// </summary>
    Star,

    /// <summary>
    /// Heart icon.
    /// </summary>
    Heart,

    /// <summary>
    /// Circle icon.
    /// </summary>
    Circle,

    /// <summary>
    /// Thumbs up icon.
    /// </summary>
    Thumb
}

/// <summary>
/// A star rating input control for capturing user ratings or displaying average scores.
/// </summary>
public partial class Rating : StyledControlBase, IValidatable
{
    #region Private Fields

    private readonly List<string> _validationErrors = new();
    private bool _isValid = true;
    private bool _isUpdatingIcons;

    // Unicode characters for icons
    private const string StarFilled = "★";
    private const string StarEmpty = "☆";
    private const string StarHalf = "⯪";
    private const string HeartFilled = "♥";
    private const string HeartEmpty = "♡";
    private const string CircleFilled = "●";
    private const string CircleEmpty = "○";
    private const string ThumbFilled = "👍";
    private const string ThumbEmpty = "👍";

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="Value"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(double),
        typeof(Rating),
        0.0,
        BindingMode.TwoWay,
        propertyChanged: OnValueChanged);

    /// <summary>
    /// Identifies the <see cref="Maximum"/> bindable property.
    /// </summary>
    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum),
        typeof(int),
        typeof(Rating),
        5,
        propertyChanged: OnMaximumChanged);

    /// <summary>
    /// Identifies the <see cref="Precision"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PrecisionProperty = BindableProperty.Create(
        nameof(Precision),
        typeof(RatingPrecision),
        typeof(Rating),
        RatingPrecision.Full,
        propertyChanged: OnPrecisionChanged);

    /// <summary>
    /// Identifies the <see cref="IsReadOnly"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(
        nameof(IsReadOnly),
        typeof(bool),
        typeof(Rating),
        false);

    /// <summary>
    /// Identifies the <see cref="Icon"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon),
        typeof(RatingIcon),
        typeof(Rating),
        RatingIcon.Star,
        propertyChanged: OnIconChanged);

    /// <summary>
    /// Identifies the <see cref="FilledColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FilledColorProperty = BindableProperty.Create(
        nameof(FilledColor),
        typeof(Color),
        typeof(Rating),
        Color.FromArgb("#FFD700"),
        propertyChanged: OnFilledColorChanged);

    /// <summary>
    /// Identifies the <see cref="EmptyColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty EmptyColorProperty = BindableProperty.Create(
        nameof(EmptyColor),
        typeof(Color),
        typeof(Rating),
        Color.FromArgb("#E0E0E0"),
        propertyChanged: OnEmptyColorChanged);

    /// <summary>
    /// Identifies the <see cref="IconSize"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IconSizeProperty = BindableProperty.Create(
        nameof(IconSize),
        typeof(double),
        typeof(Rating),
        32.0,
        propertyChanged: OnIconSizeChanged);

    /// <summary>
    /// Identifies the <see cref="Spacing"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SpacingProperty = BindableProperty.Create(
        nameof(Spacing),
        typeof(double),
        typeof(Rating),
        4.0);

    /// <summary>
    /// Identifies the <see cref="AllowClear"/> bindable property.
    /// </summary>
    public static readonly BindableProperty AllowClearProperty = BindableProperty.Create(
        nameof(AllowClear),
        typeof(bool),
        typeof(Rating),
        true);

    #endregion

    #region Validation Bindable Properties

    /// <summary>
    /// Identifies the <see cref="IsRequired"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(
        nameof(IsRequired),
        typeof(bool),
        typeof(Rating),
        false);

    /// <summary>
    /// Identifies the <see cref="RequiredErrorMessage"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RequiredErrorMessageProperty = BindableProperty.Create(
        nameof(RequiredErrorMessage),
        typeof(string),
        typeof(Rating),
        "A rating is required.");

    /// <summary>
    /// Identifies the <see cref="MinimumValue"/> bindable property.
    /// </summary>
    public static readonly BindableProperty MinimumValueProperty = BindableProperty.Create(
        nameof(MinimumValue),
        typeof(double?),
        typeof(Rating),
        null);

    /// <summary>
    /// Identifies the <see cref="MinimumValueErrorMessage"/> bindable property.
    /// </summary>
    public static readonly BindableProperty MinimumValueErrorMessageProperty = BindableProperty.Create(
        nameof(MinimumValueErrorMessage),
        typeof(string),
        typeof(Rating),
        "Rating must be at least {0}.");

    /// <summary>
    /// Identifies the <see cref="ValidateCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ValidateCommandProperty = BindableProperty.Create(
        nameof(ValidateCommand),
        typeof(ICommand),
        typeof(Rating));

    #endregion

    #region Command Bindable Properties

    /// <summary>
    /// Identifies the <see cref="ValueChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ValueChangedCommandProperty = BindableProperty.Create(
        nameof(ValueChangedCommand),
        typeof(ICommand),
        typeof(Rating));

    /// <summary>
    /// Identifies the <see cref="ValueChangedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ValueChangedCommandParameterProperty = BindableProperty.Create(
        nameof(ValueChangedCommandParameter),
        typeof(object),
        typeof(Rating));

    /// <summary>
    /// Identifies the <see cref="ClearedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ClearedCommandProperty = BindableProperty.Create(
        nameof(ClearedCommand),
        typeof(ICommand),
        typeof(Rating));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the current rating value (two-way bindable).
    /// </summary>
    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum rating (number of icons).
    /// </summary>
    public int Maximum
    {
        get => (int)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>
    /// Gets or sets the precision mode.
    /// </summary>
    public RatingPrecision Precision
    {
        get => (RatingPrecision)GetValue(PrecisionProperty);
        set => SetValue(PrecisionProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the control is read-only (display only).
    /// </summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon type.
    /// </summary>
    public RatingIcon Icon
    {
        get => (RatingIcon)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets the color for filled portion.
    /// </summary>
    public Color FilledColor
    {
        get => (Color)GetValue(FilledColorProperty);
        set => SetValue(FilledColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the color for empty portion.
    /// </summary>
    public Color EmptyColor
    {
        get => (Color)GetValue(EmptyColorProperty);
        set => SetValue(EmptyColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the size of each icon.
    /// </summary>
    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the space between icons.
    /// </summary>
    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets whether tapping the current value clears to 0.
    /// </summary>
    public bool AllowClear
    {
        get => (bool)GetValue(AllowClearProperty);
        set => SetValue(AllowClearProperty, value);
    }

    #endregion

    #region Validation Properties

    /// <summary>
    /// Gets or sets whether a rating is required.
    /// </summary>
    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    /// <summary>
    /// Gets or sets the error message when rating is required but not provided.
    /// </summary>
    public string RequiredErrorMessage
    {
        get => (string)GetValue(RequiredErrorMessageProperty);
        set => SetValue(RequiredErrorMessageProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum required value.
    /// </summary>
    public double? MinimumValue
    {
        get => (double?)GetValue(MinimumValueProperty);
        set => SetValue(MinimumValueProperty, value);
    }

    /// <summary>
    /// Gets or sets the error message when value is below minimum.
    /// </summary>
    public string MinimumValueErrorMessage
    {
        get => (string)GetValue(MinimumValueErrorMessageProperty);
        set => SetValue(MinimumValueErrorMessageProperty, value);
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
    /// Gets or sets the command to execute when the value changes.
    /// </summary>
    public ICommand? ValueChangedCommand
    {
        get => (ICommand?)GetValue(ValueChangedCommandProperty);
        set => SetValue(ValueChangedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter for ValueChangedCommand.
    /// </summary>
    public object? ValueChangedCommandParameter
    {
        get => GetValue(ValueChangedCommandParameterProperty);
        set => SetValue(ValueChangedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the rating is cleared.
    /// </summary>
    public ICommand? ClearedCommand
    {
        get => (ICommand?)GetValue(ClearedCommandProperty);
        set => SetValue(ClearedCommandProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the rating value changes.
    /// </summary>
    public event EventHandler<double>? ValueChanged;

    /// <summary>
    /// Occurs when the rating is cleared.
    /// </summary>
    public event EventHandler? Cleared;

    /// <summary>
    /// Occurs when the validation state changes.
    /// </summary>
    public event EventHandler<bool>? ValidationChanged;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the Rating control.
    /// </summary>
    public Rating()
    {
        InitializeComponent();
        BuildIcons();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Clears the rating to 0.
    /// </summary>
    public void Clear()
    {
        Value = 0;
        Cleared?.Invoke(this, EventArgs.Empty);

        if (ClearedCommand?.CanExecute(null) == true)
        {
            ClearedCommand.Execute(null);
        }
    }

    /// <summary>
    /// Performs validation and returns the result.
    /// </summary>
    public ValidationResult Validate()
    {
        var previousIsValid = _isValid;
        _validationErrors.Clear();

        // Check required validation
        if (IsRequired && Value <= 0)
        {
            _validationErrors.Add(RequiredErrorMessage);
        }

        // Check minimum value
        if (MinimumValue.HasValue && Value < MinimumValue.Value)
        {
            _validationErrors.Add(string.Format(MinimumValueErrorMessage, MinimumValue.Value));
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

    #region Private Methods - Icon Building

    private void BuildIcons()
    {
        if (_isUpdatingIcons)
            return;

        _isUpdatingIcons = true;

        iconsContainer.Children.Clear();

        for (int i = 1; i <= Maximum; i++)
        {
            var iconIndex = i;
            var iconView = CreateIconView(iconIndex);
            iconsContainer.Children.Add(iconView);
        }

        UpdateIconStates();

        _isUpdatingIcons = false;
    }

    private View CreateIconView(int index)
    {
        var grid = new Grid
        {
            WidthRequest = IconSize,
            HeightRequest = IconSize
        };

        // Empty icon (background)
        var emptyLabel = new Label
        {
            Text = GetEmptyIcon(),
            FontSize = IconSize * 0.9,
            TextColor = EmptyColor,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
        grid.Children.Add(emptyLabel);

        // Filled icon (foreground, clipped for partial fill)
        var filledLabel = new Label
        {
            Text = GetFilledIcon(),
            FontSize = IconSize * 0.9,
            TextColor = FilledColor,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            Opacity = 0
        };
        filledLabel.SetValue(Grid.ColumnProperty, 0);
        grid.Children.Add(filledLabel);

        // Store the index
        grid.AutomationId = $"rating_icon_{index}";

        // Add tap gesture for interaction
        if (!IsReadOnly)
        {
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) => OnIconTapped(index, e);
            grid.GestureRecognizers.Add(tapGesture);
        }

        return grid;
    }

    private string GetFilledIcon() => Icon switch
    {
        RatingIcon.Heart => HeartFilled,
        RatingIcon.Circle => CircleFilled,
        RatingIcon.Thumb => ThumbFilled,
        _ => StarFilled
    };

    private string GetEmptyIcon() => Icon switch
    {
        RatingIcon.Heart => HeartEmpty,
        RatingIcon.Circle => CircleEmpty,
        RatingIcon.Thumb => ThumbEmpty,
        _ => StarEmpty
    };

    private void UpdateIconStates()
    {
        if (iconsContainer == null)
            return;

        for (int i = 0; i < iconsContainer.Children.Count; i++)
        {
            var iconIndex = i + 1;
            var grid = iconsContainer.Children[i] as Grid;
            if (grid == null || grid.Children.Count < 2)
                continue;

            var filledLabel = grid.Children[1] as Label;
            if (filledLabel == null)
                continue;

            double fillAmount;

            if (Value >= iconIndex)
            {
                // Fully filled
                fillAmount = 1.0;
            }
            else if (Value > iconIndex - 1)
            {
                // Partially filled
                fillAmount = Value - (iconIndex - 1);
            }
            else
            {
                // Empty
                fillAmount = 0.0;
            }

            // Use opacity to show fill state
            // For exact precision, we show partial fills
            // For full/half, we round to nearest allowed value
            if (Precision == RatingPrecision.Full)
            {
                filledLabel.Opacity = fillAmount >= 0.5 ? 1.0 : 0.0;
            }
            else if (Precision == RatingPrecision.Half)
            {
                if (fillAmount >= 0.75)
                    filledLabel.Opacity = 1.0;
                else if (fillAmount >= 0.25)
                    filledLabel.Opacity = 0.5; // Half fill shown as reduced opacity
                else
                    filledLabel.Opacity = 0.0;
            }
            else // Exact
            {
                filledLabel.Opacity = fillAmount;
            }
        }
    }

    #endregion

    #region Event Handlers

    private void OnIconTapped(int index, TappedEventArgs e)
    {
        if (IsReadOnly)
            return;

        double newValue;

        if (Precision == RatingPrecision.Half)
        {
            // Determine if tap was on left or right half
            var element = e.Parameter as View ?? iconsContainer.Children[index - 1] as View;
            if (element != null)
            {
                var position = e.GetPosition(element);
                if (position.HasValue)
                {
                    var halfWidth = element.Width / 2;
                    if (position.Value.X < halfWidth)
                    {
                        // Left half - set to X.0 (actually index - 0.5)
                        newValue = index - 0.5;
                    }
                    else
                    {
                        // Right half - set to X.0 (index)
                        newValue = index;
                    }
                }
                else
                {
                    newValue = index;
                }
            }
            else
            {
                newValue = index;
            }
        }
        else
        {
            newValue = index;
        }

        // Check if tapping same value to clear
        if (AllowClear && Math.Abs(Value - newValue) < 0.001)
        {
            Clear();
        }
        else
        {
            Value = newValue;
        }
    }

    #endregion

    #region Property Changed Handlers

    private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Rating rating)
        {
            var value = (double)newValue;

            // Clamp value to valid range
            var clampedValue = Math.Max(0, Math.Min(value, rating.Maximum));
            if (Math.Abs(clampedValue - value) > 0.001)
            {
                rating.Value = clampedValue;
                return;
            }

            rating.UpdateIconStates();
            rating.RaiseValueChanged(value);
        }
    }

    private static void OnMaximumChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Rating rating)
        {
            rating.BuildIcons();

            // Clamp value if needed
            if (rating.Value > rating.Maximum)
            {
                rating.Value = rating.Maximum;
            }
        }
    }

    private static void OnPrecisionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Rating rating)
        {
            rating.UpdateIconStates();
        }
    }

    private static void OnIconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Rating rating)
        {
            rating.BuildIcons();
        }
    }

    private static void OnFilledColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Rating rating)
        {
            rating.UpdateIconColors();
        }
    }

    private static void OnEmptyColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Rating rating)
        {
            rating.UpdateIconColors();
        }
    }

    private static void OnIconSizeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Rating rating)
        {
            rating.BuildIcons();
        }
    }

    private void UpdateIconColors()
    {
        if (iconsContainer == null)
            return;

        foreach (var child in iconsContainer.Children)
        {
            if (child is Grid grid && grid.Children.Count >= 2)
            {
                if (grid.Children[0] is Label emptyLabel)
                {
                    emptyLabel.TextColor = EmptyColor;
                }
                if (grid.Children[1] is Label filledLabel)
                {
                    filledLabel.TextColor = FilledColor;
                }
            }
        }
    }

    #endregion

    #region Event Raising Methods

    private void RaiseValueChanged(double value)
    {
        ValueChanged?.Invoke(this, value);

        var parameter = ValueChangedCommandParameter ?? value;
        if (ValueChangedCommand?.CanExecute(parameter) == true)
        {
            ValueChangedCommand.Execute(parameter);
        }
    }

    #endregion
}
