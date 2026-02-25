using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using MauiControlsExtras.Base;
using MauiControlsExtras.Theming;
using Microsoft.Maui.Controls.Shapes;

namespace MauiControlsExtras.Controls;

/// <summary>
/// A wizard/stepper control for multi-step workflows.
/// Provides step indicators, navigation, validation, and customization options.
/// </summary>
[ContentProperty(nameof(Steps))]
public partial class Wizard : NavigationControlBase, IKeyboardNavigable
{
    #region Private Fields

    private readonly ObservableCollection<WizardStep> _steps = new();
    private int _currentIndex;
    private bool _hasKeyboardFocus;

    // Visual tree elements (built in code to avoid ContentProperty conflict)
    private Border stepIndicatorTop = null!;
    private HorizontalStackLayout stepIndicatorContainer = null!;
    private Label stepTitleLabel = null!;
    private Label stepDescriptionLabel = null!;
    private ContentView stepContentContainer = null!;
    private Label emptyLabel = null!;
    private Button cancelButton = null!;
    private Button skipButton = null!;
    private Button backButton = null!;
    private Button nextButton = null!;

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="NavigationMode"/> bindable property.
    /// </summary>
    public static readonly BindableProperty NavigationModeProperty = BindableProperty.Create(
        nameof(NavigationMode),
        typeof(WizardNavigationMode),
        typeof(Wizard),
        WizardNavigationMode.Linear);

    /// <summary>
    /// Identifies the <see cref="IndicatorPosition"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IndicatorPositionProperty = BindableProperty.Create(
        nameof(IndicatorPosition),
        typeof(StepIndicatorPosition),
        typeof(Wizard),
        StepIndicatorPosition.Top,
        propertyChanged: OnIndicatorPositionChanged);

    /// <summary>
    /// Identifies the <see cref="IndicatorStyle"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IndicatorStyleProperty = BindableProperty.Create(
        nameof(IndicatorStyle),
        typeof(StepIndicatorStyle),
        typeof(Wizard),
        StepIndicatorStyle.Circle,
        propertyChanged: OnIndicatorStyleChanged);

    /// <summary>
    /// Identifies the <see cref="ShowStepNumbers"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowStepNumbersProperty = BindableProperty.Create(
        nameof(ShowStepNumbers),
        typeof(bool),
        typeof(Wizard),
        true,
        propertyChanged: OnIndicatorStyleChanged);

    /// <summary>
    /// Identifies the <see cref="ShowStepTitles"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowStepTitlesProperty = BindableProperty.Create(
        nameof(ShowStepTitles),
        typeof(bool),
        typeof(Wizard),
        true,
        propertyChanged: OnIndicatorStyleChanged);

    /// <summary>
    /// Identifies the <see cref="ShowCancelButton"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowCancelButtonProperty = BindableProperty.Create(
        nameof(ShowCancelButton),
        typeof(bool),
        typeof(Wizard),
        true);

    /// <summary>
    /// Identifies the <see cref="ShowBackButton"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowBackButtonProperty = BindableProperty.Create(
        nameof(ShowBackButton),
        typeof(bool),
        typeof(Wizard),
        true);

    /// <summary>
    /// Identifies the <see cref="BackButtonText"/> bindable property.
    /// </summary>
    public static readonly BindableProperty BackButtonTextProperty = BindableProperty.Create(
        nameof(BackButtonText),
        typeof(string),
        typeof(Wizard),
        "Back");

    /// <summary>
    /// Identifies the <see cref="NextButtonText"/> bindable property.
    /// </summary>
    public static readonly BindableProperty NextButtonTextProperty = BindableProperty.Create(
        nameof(NextButtonText),
        typeof(string),
        typeof(Wizard),
        "Next");

    /// <summary>
    /// Identifies the <see cref="FinishButtonText"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FinishButtonTextProperty = BindableProperty.Create(
        nameof(FinishButtonText),
        typeof(string),
        typeof(Wizard),
        "Finish");

    /// <summary>
    /// Identifies the <see cref="CancelButtonText"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CancelButtonTextProperty = BindableProperty.Create(
        nameof(CancelButtonText),
        typeof(string),
        typeof(Wizard),
        "Cancel");

    /// <summary>
    /// Identifies the <see cref="SkipButtonText"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SkipButtonTextProperty = BindableProperty.Create(
        nameof(SkipButtonText),
        typeof(string),
        typeof(Wizard),
        "Skip");

    /// <summary>
    /// Identifies the <see cref="ValidateOnNext"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ValidateOnNextProperty = BindableProperty.Create(
        nameof(ValidateOnNext),
        typeof(bool),
        typeof(Wizard),
        true);

    /// <summary>
    /// Identifies the <see cref="AnimateTransitions"/> bindable property.
    /// </summary>
    public static readonly BindableProperty AnimateTransitionsProperty = BindableProperty.Create(
        nameof(AnimateTransitions),
        typeof(bool),
        typeof(Wizard),
        true);

    /// <summary>
    /// Identifies the <see cref="IsKeyboardNavigationEnabled"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsKeyboardNavigationEnabledProperty = BindableProperty.Create(
        nameof(IsKeyboardNavigationEnabled),
        typeof(bool),
        typeof(Wizard),
        true);

    /// <summary>
    /// Identifies the <see cref="ErrorStepColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ErrorStepColorProperty = BindableProperty.Create(
        nameof(ErrorStepColor),
        typeof(Color),
        typeof(Wizard),
        null);

    /// <summary>
    /// Identifies the <see cref="StepIndicatorBackgroundColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty StepIndicatorBackgroundColorProperty = BindableProperty.Create(
        nameof(StepIndicatorBackgroundColor),
        typeof(Color),
        typeof(Wizard),
        null);

    /// <summary>
    /// Identifies the <see cref="StepIndicatorPadding"/> bindable property.
    /// </summary>
    public static readonly BindableProperty StepIndicatorPaddingProperty = BindableProperty.Create(
        nameof(StepIndicatorPadding),
        typeof(Thickness),
        typeof(Wizard),
        new Thickness(12, 8));

    /// <summary>
    /// Identifies the <see cref="StepTitleFontSize"/> bindable property.
    /// </summary>
    public static readonly BindableProperty StepTitleFontSizeProperty = BindableProperty.Create(
        nameof(StepTitleFontSize),
        typeof(double),
        typeof(Wizard),
        16.0);

    /// <summary>
    /// Identifies the <see cref="StepTitleFontAttributes"/> bindable property.
    /// </summary>
    public static readonly BindableProperty StepTitleFontAttributesProperty = BindableProperty.Create(
        nameof(StepTitleFontAttributes),
        typeof(FontAttributes),
        typeof(Wizard),
        FontAttributes.Bold);

    #endregion

    #region Command Properties

    /// <summary>
    /// Identifies the <see cref="StepChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty StepChangedCommandProperty = BindableProperty.Create(
        nameof(StepChangedCommand),
        typeof(ICommand),
        typeof(Wizard));

    /// <summary>
    /// Identifies the <see cref="StepChangedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty StepChangedCommandParameterProperty = BindableProperty.Create(
        nameof(StepChangedCommandParameter),
        typeof(object),
        typeof(Wizard));

    /// <summary>
    /// Identifies the <see cref="StepChangingCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty StepChangingCommandProperty = BindableProperty.Create(
        nameof(StepChangingCommand),
        typeof(ICommand),
        typeof(Wizard));

    /// <summary>
    /// Identifies the <see cref="StepChangingCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty StepChangingCommandParameterProperty = BindableProperty.Create(
        nameof(StepChangingCommandParameter),
        typeof(object),
        typeof(Wizard));

    /// <summary>
    /// Identifies the <see cref="StepValidatingCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty StepValidatingCommandProperty = BindableProperty.Create(
        nameof(StepValidatingCommand),
        typeof(ICommand),
        typeof(Wizard));

    /// <summary>
    /// Identifies the <see cref="StepValidatingCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty StepValidatingCommandParameterProperty = BindableProperty.Create(
        nameof(StepValidatingCommandParameter),
        typeof(object),
        typeof(Wizard));

    /// <summary>
    /// Identifies the <see cref="FinishedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FinishedCommandProperty = BindableProperty.Create(
        nameof(FinishedCommand),
        typeof(ICommand),
        typeof(Wizard));

    /// <summary>
    /// Identifies the <see cref="FinishedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FinishedCommandParameterProperty = BindableProperty.Create(
        nameof(FinishedCommandParameter),
        typeof(object),
        typeof(Wizard));

    /// <summary>
    /// Identifies the <see cref="FinishingCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FinishingCommandProperty = BindableProperty.Create(
        nameof(FinishingCommand),
        typeof(ICommand),
        typeof(Wizard));

    /// <summary>
    /// Identifies the <see cref="FinishingCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FinishingCommandParameterProperty = BindableProperty.Create(
        nameof(FinishingCommandParameter),
        typeof(object),
        typeof(Wizard));

    /// <summary>
    /// Identifies the <see cref="CancelledCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CancelledCommandProperty = BindableProperty.Create(
        nameof(CancelledCommand),
        typeof(ICommand),
        typeof(Wizard));

    /// <summary>
    /// Identifies the <see cref="CancelledCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CancelledCommandParameterProperty = BindableProperty.Create(
        nameof(CancelledCommandParameter),
        typeof(object),
        typeof(Wizard));

    /// <summary>
    /// Identifies the <see cref="CancellingCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CancellingCommandProperty = BindableProperty.Create(
        nameof(CancellingCommand),
        typeof(ICommand),
        typeof(Wizard));

    /// <summary>
    /// Identifies the <see cref="CancellingCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CancellingCommandParameterProperty = BindableProperty.Create(
        nameof(CancellingCommandParameter),
        typeof(object),
        typeof(Wizard));

    /// <summary>
    /// Identifies the <see cref="GotFocusCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty GotFocusCommandProperty = BindableProperty.Create(
        nameof(GotFocusCommand),
        typeof(ICommand),
        typeof(Wizard));

    /// <summary>
    /// Identifies the <see cref="GotFocusCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty GotFocusCommandParameterProperty = BindableProperty.Create(
        nameof(GotFocusCommandParameter),
        typeof(object),
        typeof(Wizard));

    /// <summary>
    /// Identifies the <see cref="LostFocusCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandProperty = BindableProperty.Create(
        nameof(LostFocusCommand),
        typeof(ICommand),
        typeof(Wizard));

    /// <summary>
    /// Identifies the <see cref="LostFocusCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandParameterProperty = BindableProperty.Create(
        nameof(LostFocusCommandParameter),
        typeof(object),
        typeof(Wizard));

    /// <summary>
    /// Identifies the <see cref="KeyPressCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandProperty = BindableProperty.Create(
        nameof(KeyPressCommand),
        typeof(ICommand),
        typeof(Wizard));

    /// <summary>
    /// Identifies the <see cref="KeyPressCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandParameterProperty = BindableProperty.Create(
        nameof(KeyPressCommandParameter),
        typeof(object),
        typeof(Wizard));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the navigation mode (linear or non-linear).
    /// </summary>
    public WizardNavigationMode NavigationMode
    {
        get => (WizardNavigationMode)GetValue(NavigationModeProperty);
        set => SetValue(NavigationModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the position of the step indicator.
    /// </summary>
    public StepIndicatorPosition IndicatorPosition
    {
        get => (StepIndicatorPosition)GetValue(IndicatorPositionProperty);
        set => SetValue(IndicatorPositionProperty, value);
    }

    /// <summary>
    /// Gets or sets the style of the step indicator.
    /// </summary>
    public StepIndicatorStyle IndicatorStyle
    {
        get => (StepIndicatorStyle)GetValue(IndicatorStyleProperty);
        set => SetValue(IndicatorStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets whether step numbers are shown.
    /// </summary>
    public bool ShowStepNumbers
    {
        get => (bool)GetValue(ShowStepNumbersProperty);
        set => SetValue(ShowStepNumbersProperty, value);
    }

    /// <summary>
    /// Gets or sets whether step titles are shown in the indicator.
    /// </summary>
    public bool ShowStepTitles
    {
        get => (bool)GetValue(ShowStepTitlesProperty);
        set => SetValue(ShowStepTitlesProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the cancel button is shown.
    /// </summary>
    public bool ShowCancelButton
    {
        get => (bool)GetValue(ShowCancelButtonProperty);
        set => SetValue(ShowCancelButtonProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the back button is shown.
    /// </summary>
    public bool ShowBackButton
    {
        get => (bool)GetValue(ShowBackButtonProperty);
        set => SetValue(ShowBackButtonProperty, value);
    }

    /// <summary>
    /// Gets or sets the back button text.
    /// </summary>
    public string BackButtonText
    {
        get => (string)GetValue(BackButtonTextProperty);
        set => SetValue(BackButtonTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the next button text.
    /// </summary>
    public string NextButtonText
    {
        get => (string)GetValue(NextButtonTextProperty);
        set => SetValue(NextButtonTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the finish button text.
    /// </summary>
    public string FinishButtonText
    {
        get => (string)GetValue(FinishButtonTextProperty);
        set => SetValue(FinishButtonTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the cancel button text.
    /// </summary>
    public string CancelButtonText
    {
        get => (string)GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the skip button text.
    /// </summary>
    public string SkipButtonText
    {
        get => (string)GetValue(SkipButtonTextProperty);
        set => SetValue(SkipButtonTextProperty, value);
    }

    /// <summary>
    /// Gets or sets whether validation is performed when clicking Next.
    /// </summary>
    public bool ValidateOnNext
    {
        get => (bool)GetValue(ValidateOnNextProperty);
        set => SetValue(ValidateOnNextProperty, value);
    }

    /// <summary>
    /// Gets or sets whether step transitions are animated.
    /// </summary>
    public bool AnimateTransitions
    {
        get => (bool)GetValue(AnimateTransitionsProperty);
        set => SetValue(AnimateTransitionsProperty, value);
    }

    /// <summary>
    /// Gets or sets the color for steps with errors. When null, uses <see cref="StyledControlBase.EffectiveErrorColor"/>.
    /// </summary>
    public Color? ErrorStepColor
    {
        get => (Color?)GetValue(ErrorStepColorProperty);
        set => SetValue(ErrorStepColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the step indicator background color. When null, uses the theme surface color.
    /// </summary>
    public Color? StepIndicatorBackgroundColor
    {
        get => (Color?)GetValue(StepIndicatorBackgroundColorProperty);
        set => SetValue(StepIndicatorBackgroundColorProperty, value);
    }

    /// <summary>
    /// Gets the effective step indicator background color, falling back to surface color.
    /// </summary>
    public Color EffectiveStepIndicatorBackgroundColor =>
        StepIndicatorBackgroundColor ?? MauiControlsExtrasTheme.GetSurfaceColor();

    /// <summary>
    /// Gets or sets the step indicator internal padding.
    /// </summary>
    public Thickness StepIndicatorPadding
    {
        get => (Thickness)GetValue(StepIndicatorPaddingProperty);
        set => SetValue(StepIndicatorPaddingProperty, value);
    }

    /// <summary>
    /// Gets or sets the step title font size.
    /// </summary>
    public double StepTitleFontSize
    {
        get => (double)GetValue(StepTitleFontSizeProperty);
        set => SetValue(StepTitleFontSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the step title font attributes (bold, italic).
    /// </summary>
    public FontAttributes StepTitleFontAttributes
    {
        get => (FontAttributes)GetValue(StepTitleFontAttributesProperty);
        set => SetValue(StepTitleFontAttributesProperty, value);
    }

    /// <summary>
    /// Gets the collection of wizard steps.
    /// </summary>
    public ObservableCollection<WizardStep> Steps => _steps;

    /// <summary>
    /// Gets the current step index.
    /// </summary>
    public int CurrentIndex => _currentIndex;

    /// <summary>
    /// Gets the current step.
    /// </summary>
    public WizardStep? CurrentStep => _currentIndex >= 0 && _currentIndex < _steps.Count
        ? _steps[_currentIndex]
        : null;

    /// <summary>
    /// Gets whether the wizard can go back.
    /// </summary>
    public bool CanGoBack => _currentIndex > 0;

    /// <summary>
    /// Gets whether the wizard can go forward.
    /// </summary>
    public bool CanGoNext => _currentIndex < _steps.Count - 1;

    /// <summary>
    /// Gets whether the current step is the last step.
    /// </summary>
    public bool IsLastStep => _currentIndex == _steps.Count - 1;

    /// <summary>
    /// Gets whether the current step is the first step.
    /// </summary>
    public bool IsFirstStep => _currentIndex == 0;

    /// <summary>
    /// Gets the current border color based on focus state.
    /// </summary>
    public Color CurrentBorderColor =>
        _hasKeyboardFocus ? EffectiveFocusBorderColor : EffectiveBorderColor;

    /// <summary>
    /// Gets the step count.
    /// </summary>
    public int StepCount => _steps.Count;

    /// <summary>
    /// Gets the completion percentage (0-100).
    /// </summary>
    public double CompletionPercentage => _steps.Count > 0
        ? (_currentIndex / (double)_steps.Count) * 100
        : 0;

    #endregion

    #region Command Properties Implementation

    /// <summary>
    /// Gets or sets the command executed when the step changes.
    /// </summary>
    public ICommand? StepChangedCommand
    {
        get => (ICommand?)GetValue(StepChangedCommandProperty);
        set => SetValue(StepChangedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="StepChangedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? StepChangedCommandParameter
    {
        get => GetValue(StepChangedCommandParameterProperty);
        set => SetValue(StepChangedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the cancelable command executed before the step changes.
    /// The command parameter is <see cref="WizardStepChangingEventArgs"/>.
    /// Set Cancel = true to prevent navigation.
    /// </summary>
    public ICommand? StepChangingCommand
    {
        get => (ICommand?)GetValue(StepChangingCommandProperty);
        set => SetValue(StepChangingCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="StepChangingCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? StepChangingCommandParameter
    {
        get => GetValue(StepChangingCommandParameterProperty);
        set => SetValue(StepChangingCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the cancelable command executed before step validation.
    /// The command parameter is <see cref="WizardStepValidatingEventArgs"/>.
    /// Set Cancel = true to prevent navigation.
    /// </summary>
    public ICommand? StepValidatingCommand
    {
        get => (ICommand?)GetValue(StepValidatingCommandProperty);
        set => SetValue(StepValidatingCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="StepValidatingCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? StepValidatingCommandParameter
    {
        get => GetValue(StepValidatingCommandParameterProperty);
        set => SetValue(StepValidatingCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when the wizard finishes.
    /// </summary>
    public ICommand? FinishedCommand
    {
        get => (ICommand?)GetValue(FinishedCommandProperty);
        set => SetValue(FinishedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="FinishedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? FinishedCommandParameter
    {
        get => GetValue(FinishedCommandParameterProperty);
        set => SetValue(FinishedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the cancelable command executed before the wizard finishes.
    /// The command parameter is <see cref="WizardFinishingEventArgs"/>.
    /// Set Cancel = true to prevent finish.
    /// </summary>
    public ICommand? FinishingCommand
    {
        get => (ICommand?)GetValue(FinishingCommandProperty);
        set => SetValue(FinishingCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="FinishingCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? FinishingCommandParameter
    {
        get => GetValue(FinishingCommandParameterProperty);
        set => SetValue(FinishingCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when the wizard is cancelled.
    /// </summary>
    public ICommand? CancelledCommand
    {
        get => (ICommand?)GetValue(CancelledCommandProperty);
        set => SetValue(CancelledCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="CancelledCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? CancelledCommandParameter
    {
        get => GetValue(CancelledCommandParameterProperty);
        set => SetValue(CancelledCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the cancelable command executed before the wizard is cancelled.
    /// The command parameter is <see cref="WizardCancellingEventArgs"/>.
    /// Set Cancel = true to prevent cancellation.
    /// </summary>
    public ICommand? CancellingCommand
    {
        get => (ICommand?)GetValue(CancellingCommandProperty);
        set => SetValue(CancellingCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="CancellingCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? CancellingCommandParameter
    {
        get => GetValue(CancellingCommandParameterProperty);
        set => SetValue(CancellingCommandParameterProperty, value);
    }

    /// <inheritdoc/>
    public ICommand? GotFocusCommand
    {
        get => (ICommand?)GetValue(GotFocusCommandProperty);
        set => SetValue(GotFocusCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="GotFocusCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? GotFocusCommandParameter
    {
        get => GetValue(GotFocusCommandParameterProperty);
        set => SetValue(GotFocusCommandParameterProperty, value);
    }

    /// <inheritdoc/>
    public ICommand? LostFocusCommand
    {
        get => (ICommand?)GetValue(LostFocusCommandProperty);
        set => SetValue(LostFocusCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="LostFocusCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? LostFocusCommandParameter
    {
        get => GetValue(LostFocusCommandParameterProperty);
        set => SetValue(LostFocusCommandParameterProperty, value);
    }

    /// <inheritdoc/>
    public ICommand? KeyPressCommand
    {
        get => (ICommand?)GetValue(KeyPressCommandProperty);
        set => SetValue(KeyPressCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="KeyPressCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? KeyPressCommandParameter
    {
        get => GetValue(KeyPressCommandParameterProperty);
        set => SetValue(KeyPressCommandParameterProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the current step changes.
    /// </summary>
    public event EventHandler<WizardStepChangedEventArgs>? StepChanged;

    /// <summary>
    /// Occurs before the current step changes (cancelable).
    /// </summary>
    public event EventHandler<WizardStepChangingEventArgs>? StepChanging;

    /// <summary>
    /// Occurs before step validation (cancelable with validation errors).
    /// </summary>
    public event EventHandler<WizardStepValidatingEventArgs>? StepValidating;

    /// <summary>
    /// Occurs when the wizard finishes.
    /// </summary>
    public event EventHandler<WizardFinishedEventArgs>? Finished;

    /// <summary>
    /// Occurs before the wizard finishes (cancelable).
    /// </summary>
    public event EventHandler<WizardFinishingEventArgs>? Finishing;

    /// <summary>
    /// Occurs when the wizard is cancelled.
    /// </summary>
    public event EventHandler<WizardFinishedEventArgs>? Cancelled;

    /// <summary>
    /// Occurs before the wizard is cancelled (cancelable).
    /// </summary>
    public event EventHandler<WizardCancellingEventArgs>? Cancelling;

    /// <inheritdoc/>
    public event EventHandler<KeyboardFocusEventArgs>? KeyboardFocusGained;

    /// <inheritdoc/>
    public event EventHandler<KeyboardFocusEventArgs>? KeyboardFocusLost;

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
            KeyPressCommand.Execute(KeyPressCommandParameter ?? e);
            if (e.Handled) return true;
        }

        switch (e.Key)
        {
            case "ArrowLeft":
            case "PageUp":
                if (CanGoBack)
                {
                    GoBack();
                    return true;
                }
                break;
            case "ArrowRight":
            case "PageDown":
                if (CanGoNext)
                {
                    GoNext();
                    return true;
                }
                break;
            case "Home":
                if (_currentIndex > 0 && NavigationMode == WizardNavigationMode.NonLinear)
                {
                    GoToStep(0);
                    return true;
                }
                break;
            case "End":
                if (_currentIndex < _steps.Count - 1 && NavigationMode == WizardNavigationMode.NonLinear)
                {
                    GoToStep(_steps.Count - 1);
                    return true;
                }
                break;
            case "Enter":
                if (IsLastStep)
                {
                    Finish();
                }
                else
                {
                    GoNext();
                }
                return true;
            case "Escape":
                Cancel();
                return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public IReadOnlyList<KeyboardShortcut> GetKeyboardShortcuts()
    {
        return new List<KeyboardShortcut>
        {
            new() { Key = "ArrowLeft", Description = "Previous step", Category = "Navigation" },
            new() { Key = "ArrowRight", Description = "Next step", Category = "Navigation" },
            new() { Key = "PageUp", Description = "Previous step", Category = "Navigation" },
            new() { Key = "PageDown", Description = "Next step", Category = "Navigation" },
            new() { Key = "Home", Description = "First step (non-linear)", Category = "Navigation" },
            new() { Key = "End", Description = "Last step (non-linear)", Category = "Navigation" },
            new() { Key = "Enter", Description = "Next step / Finish", Category = "Actions" },
            new() { Key = "Escape", Description = "Cancel wizard", Category = "Actions" }
        };
    }

    /// <inheritdoc/>
    public new bool Focus()
    {
        if (!CanReceiveFocus) return false;
        _hasKeyboardFocus = true;
        OnPropertyChanged(nameof(HasKeyboardFocus));
        KeyboardFocusGained?.Invoke(this, new KeyboardFocusEventArgs(true));
        GotFocusCommand?.Execute(GotFocusCommandParameter ?? this);
        return true;
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Wizard"/> class.
    /// </summary>
    [DynamicDependency(nameof(CurrentBorderColor), typeof(Wizard))]
    [DynamicDependency(nameof(EffectiveStepIndicatorBackgroundColor), typeof(Wizard))]
    public Wizard()
    {
        InitializeComponent();
        BuildVisualTree();
        _steps.CollectionChanged += OnStepsCollectionChanged;
        Focused += OnControlFocused;
        Unfocused += OnControlUnfocused;
    }

    /// <summary>
    /// Builds the visual tree in code to avoid conflict with [ContentProperty(nameof(Steps))].
    /// </summary>
    private void BuildVisualTree()
    {
        // Step indicator container
        stepIndicatorContainer = new HorizontalStackLayout
        {
            Spacing = 0,
            HorizontalOptions = LayoutOptions.Center
        };

        var indicatorScrollView = new ScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
            Content = stepIndicatorContainer
        };

        // Step indicator top border
        stepIndicatorTop = new Border
        {
            Content = indicatorScrollView
        };
        stepIndicatorTop.SetBinding(Border.BackgroundColorProperty,
            new Binding(nameof(EffectiveStepIndicatorBackgroundColor), source: this));
        stepIndicatorTop.SetBinding(Border.PaddingProperty,
            new Binding(nameof(StepIndicatorPadding), source: this));

        // Step title label
        stepTitleLabel = new Label
        {
            Margin = new Thickness(0, 0, 0, 4)
        };
        stepTitleLabel.SetBinding(Label.FontSizeProperty,
            new Binding(nameof(StepTitleFontSize), source: this));
        stepTitleLabel.SetBinding(Label.FontAttributesProperty,
            new Binding(nameof(StepTitleFontAttributes), source: this));
        stepTitleLabel.SetBinding(Label.TextColorProperty,
            new Binding(nameof(EffectiveForegroundColor), source: this));

        // Step description label
        stepDescriptionLabel = new Label
        {
            FontSize = 13,
            Margin = new Thickness(0, 0, 0, 16)
        };
        stepDescriptionLabel.SetAppThemeColor(Label.TextColorProperty,
            Color.FromArgb("#666666"),
            Color.FromArgb("#AAAAAA"));

        // Step content container
        stepContentContainer = new ContentView();

        // Empty label
        emptyLabel = new Label
        {
            Text = "No steps defined",
            IsVisible = false,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        emptyLabel.SetAppThemeColor(Label.TextColorProperty,
            Color.FromArgb("#999999"),
            Color.FromArgb("#666666"));

        // Content grid (title, description, content, empty state)
        var contentGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };
        contentGrid.Add(stepTitleLabel, 0, 0);
        contentGrid.Add(stepDescriptionLabel, 0, 1);
        contentGrid.Add(stepContentContainer, 0, 2);
        contentGrid.Add(emptyLabel, 0, 0);
        Grid.SetRowSpan(emptyLabel, 3);

        // Content area border
        var contentBorder = new Border
        {
            Padding = new Thickness(16),
            BackgroundColor = Colors.Transparent,
            Content = contentGrid
        };

        // Navigation buttons
        cancelButton = new Button
        {
            BackgroundColor = Colors.Transparent
        };
        cancelButton.SetBinding(Button.TextProperty,
            new Binding(nameof(CancelButtonText), source: this));
        cancelButton.SetBinding(Button.IsVisibleProperty,
            new Binding(nameof(ShowCancelButton), source: this));
        cancelButton.SetAppThemeColor(Button.TextColorProperty,
            Color.FromArgb("#666666"),
            Color.FromArgb("#AAAAAA"));
        cancelButton.Clicked += OnCancelClicked;

        skipButton = new Button
        {
            IsVisible = false,
            BackgroundColor = Colors.Transparent,
            Margin = new Thickness(0, 0, 8, 0)
        };
        skipButton.SetBinding(Button.TextProperty,
            new Binding(nameof(SkipButtonText), source: this));
        skipButton.SetAppThemeColor(Button.TextColorProperty,
            Color.FromArgb("#666666"),
            Color.FromArgb("#AAAAAA"));
        skipButton.Clicked += OnSkipClicked;

        backButton = new Button
        {
            Margin = new Thickness(0, 0, 8, 0)
        };
        backButton.SetBinding(Button.TextProperty,
            new Binding(nameof(BackButtonText), source: this));
        backButton.SetBinding(Button.IsVisibleProperty,
            new Binding(nameof(ShowBackButton), source: this));
        backButton.SetAppThemeColor(Button.BackgroundColorProperty,
            Color.FromArgb("#E0E0E0"),
            Color.FromArgb("#404040"));
        backButton.SetBinding(Button.TextColorProperty,
            new Binding(nameof(EffectiveForegroundColor), source: this));
        backButton.Clicked += OnBackClicked;

        nextButton = new Button
        {
            TextColor = Colors.White
        };
        nextButton.SetBinding(Button.TextProperty,
            new Binding(nameof(NextButtonText), source: this));
        nextButton.SetBinding(Button.BackgroundColorProperty,
            new Binding(nameof(EffectiveAccentColor), source: this));
        nextButton.Clicked += OnNextClicked;

        // Navigation grid
        var navGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            }
        };
        navGrid.Add(cancelButton, 0, 0);
        navGrid.Add(new BoxView(), 1, 0);
        navGrid.Add(skipButton, 2, 0);
        navGrid.Add(backButton, 3, 0);
        navGrid.Add(nextButton, 4, 0);

        // Navigation border
        var navBorder = new Border
        {
            Padding = new Thickness(12, 8),
            Content = navGrid
        };
        navBorder.SetAppThemeColor(Border.BackgroundColorProperty,
            Color.FromArgb("#F5F5F5"),
            Color.FromArgb("#2D2D2D"));

        // Main grid
        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
        };
        mainGrid.Add(stepIndicatorTop, 0, 0);
        mainGrid.Add(contentBorder, 0, 1);
        mainGrid.Add(navBorder, 0, 2);

        // Corner radius shape
        var cornerRadiusShape = new RoundRectangle();
        cornerRadiusShape.SetBinding(RoundRectangle.CornerRadiusProperty,
            new Binding(nameof(EffectiveCornerRadius), source: this));

        // Outer border
        var outerBorder = new Border
        {
            StrokeShape = cornerRadiusShape,
            Content = mainGrid
        };
        outerBorder.SetBinding(Border.StrokeThicknessProperty,
            new Binding(nameof(EffectiveBorderThickness), source: this));
        outerBorder.SetBinding(Border.StrokeProperty,
            new Binding(nameof(CurrentBorderColor), source: this));
        outerBorder.SetAppThemeColor(Border.BackgroundColorProperty,
            Color.FromArgb("#FFFFFF"),
            Color.FromArgb("#1E1E1E"));

        Content = outerBorder;
    }

    #endregion

    #region Navigation Methods

    /// <summary>
    /// Navigates to the next step.
    /// </summary>
    /// <returns>True if navigation succeeded.</returns>
    public bool GoNext()
    {
        if (!CanGoNext) return false;

        // Validate current step if required
        if (ValidateOnNext && CurrentStep != null && !CurrentStep.Validate())
        {
            return false;
        }

        return GoToStep(_currentIndex + 1);
    }

    /// <summary>
    /// Navigates to the previous step.
    /// </summary>
    /// <returns>True if navigation succeeded.</returns>
    public bool GoBack()
    {
        if (!CanGoBack) return false;
        return GoToStep(_currentIndex - 1);
    }

    /// <summary>
    /// Navigates to a specific step.
    /// </summary>
    /// <param name="index">The step index.</param>
    /// <returns>True if navigation succeeded.</returns>
    public bool GoToStep(int index)
    {
        if (index < 0 || index >= _steps.Count)
            return false;

        if (index == _currentIndex)
            return true;

        // Check if non-linear navigation is allowed
        if (NavigationMode == WizardNavigationMode.Linear)
        {
            // In linear mode, can only go to adjacent steps or previously visited steps
            var targetStep = _steps[index];
            if (index > _currentIndex + 1 && targetStep.Status == WizardStepStatus.NotVisited)
            {
                return false;
            }
        }

        var oldStep = CurrentStep;
        var newStep = _steps[index];
        var oldIndex = _currentIndex;

        // Raise validating event/command first
        var validatingArgs = new WizardStepValidatingEventArgs(oldStep, newStep, oldIndex, index);
        StepValidating?.Invoke(this, validatingArgs);

        if (StepValidatingCommand?.CanExecute(validatingArgs) == true)
        {
            StepValidatingCommand.Execute(StepValidatingCommandParameter ?? validatingArgs);
        }

        if (validatingArgs.Cancel)
            return false;

        // Raise changing event
        var changingArgs = new WizardStepChangingEventArgs(oldStep, newStep, oldIndex, index);
        StepChanging?.Invoke(this, changingArgs);
        if (StepChangingCommand?.CanExecute(changingArgs) == true)
        {
            StepChangingCommand.Execute(StepChangingCommandParameter ?? changingArgs);
        }
        if (changingArgs.Cancel)
            return false;

        // Exit old step
        if (oldStep != null)
        {
            oldStep.OnExit(index > oldIndex);
        }

        // Update state
        _currentIndex = index;
        OnPropertyChanged(nameof(CurrentIndex));
        OnPropertyChanged(nameof(CurrentStep));
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(IsFirstStep));
        OnPropertyChanged(nameof(IsLastStep));
        OnPropertyChanged(nameof(CompletionPercentage));

        // Enter new step
        newStep.OnEnter();

        // Update UI
        UpdateStepContent();
        UpdateStepIndicator();
        UpdateNavigationButtons();

        // Raise changed event
        var changedArgs = new WizardStepChangedEventArgs(oldStep, newStep, oldIndex, index);
        StepChanged?.Invoke(this, changedArgs);
        StepChangedCommand?.Execute(StepChangedCommandParameter ?? changedArgs);

        return true;
    }

    /// <summary>
    /// Finishes the wizard.
    /// </summary>
    /// <returns>True if the wizard finished successfully.</returns>
    public bool Finish()
    {
        // Validate all steps if in linear mode
        if (ValidateOnNext)
        {
            foreach (var step in _steps)
            {
                if (!step.IsOptional && !step.Validate())
                {
                    GoToStep(step.Index);
                    return false;
                }
            }
        }

        // Raise finishing event
        var finishingArgs = new WizardFinishingEventArgs(_steps.ToList());
        Finishing?.Invoke(this, finishingArgs);
        if (FinishingCommand?.CanExecute(finishingArgs) == true)
        {
            FinishingCommand.Execute(FinishingCommandParameter ?? finishingArgs);
        }
        if (finishingArgs.Cancel)
            return false;

        // Mark current step as completed
        if (CurrentStep != null)
        {
            CurrentStep.OnExit(true);
        }

        // Raise finished event
        var finishedArgs = new WizardFinishedEventArgs(false, _steps.ToList());
        Finished?.Invoke(this, finishedArgs);
        FinishedCommand?.Execute(FinishedCommandParameter ?? finishedArgs);

        return true;
    }

    /// <summary>
    /// Cancels the wizard.
    /// </summary>
    /// <returns>True if the wizard was cancelled successfully.</returns>
    public bool Cancel()
    {
        // Raise cancelling event
        var cancellingArgs = new WizardCancellingEventArgs(_currentIndex);
        Cancelling?.Invoke(this, cancellingArgs);
        if (CancellingCommand?.CanExecute(cancellingArgs) == true)
        {
            CancellingCommand.Execute(CancellingCommandParameter ?? cancellingArgs);
        }
        if (cancellingArgs.Cancel)
            return false;

        // Raise cancelled event
        var cancelledArgs = new WizardFinishedEventArgs(true, _steps.ToList());
        Cancelled?.Invoke(this, cancelledArgs);
        CancelledCommand?.Execute(CancelledCommandParameter ?? cancelledArgs);

        return true;
    }

    /// <summary>
    /// Skips the current step.
    /// </summary>
    /// <returns>True if the step was skipped successfully.</returns>
    public bool SkipStep()
    {
        if (CurrentStep == null || !CurrentStep.CanSkip)
            return false;

        CurrentStep.Status = WizardStepStatus.Skipped;
        return GoNext();
    }

    /// <summary>
    /// Resets the wizard to the first step.
    /// </summary>
    public void Reset()
    {
        foreach (var step in _steps)
        {
            step.Status = WizardStepStatus.NotVisited;
            step.IsValid = true;
            step.ValidationMessage = null;
        }

        _currentIndex = -1;
        if (_steps.Count > 0)
        {
            GoToStep(0);
        }
    }

    #endregion

    #region UI Update Methods

    private void UpdateStepContent()
    {
        if (_steps.Count == 0)
        {
            emptyLabel.IsVisible = true;
            stepTitleLabel.IsVisible = false;
            stepDescriptionLabel.IsVisible = false;
            stepContentContainer.Content = null;
            return;
        }

        emptyLabel.IsVisible = false;

        var step = CurrentStep;
        if (step == null) return;

        stepTitleLabel.IsVisible = !string.IsNullOrEmpty(step.Title);
        stepTitleLabel.Text = step.Title;

        stepDescriptionLabel.IsVisible = !string.IsNullOrEmpty(step.Description);
        stepDescriptionLabel.Text = step.Description;

        if (AnimateTransitions)
        {
            AnimateStepTransition(step);
        }
        else
        {
            stepContentContainer.Content = step;
        }
    }

    private async void AnimateStepTransition(WizardStep step)
    {
        await stepContentContainer.FadeToAsync(0, 150);
        stepContentContainer.Content = step;
        await stepContentContainer.FadeToAsync(1, 150);
    }

    private void UpdateStepIndicator()
    {
        stepIndicatorTop.IsVisible = IndicatorPosition == StepIndicatorPosition.Top;
        if (IndicatorPosition == StepIndicatorPosition.None)
            return;

        stepIndicatorContainer.Children.Clear();

        for (int i = 0; i < _steps.Count; i++)
        {
            var step = _steps[i];
            var indicator = CreateStepIndicatorItem(step, i);
            stepIndicatorContainer.Children.Add(indicator);

            // Add connector line
            if (i < _steps.Count - 1)
            {
                var connector = CreateConnector(step, _steps[i + 1]);
                stepIndicatorContainer.Children.Add(connector);
            }
        }
    }

    private View CreateStepIndicatorItem(WizardStep step, int index)
    {
        var container = new VerticalStackLayout
        {
            Spacing = 4,
            HorizontalOptions = LayoutOptions.Center
        };

        View indicator;
        switch (IndicatorStyle)
        {
            case StepIndicatorStyle.Circle:
                indicator = CreateCircleIndicator(step, index);
                break;
            case StepIndicatorStyle.Dot:
                indicator = CreateDotIndicator(step);
                break;
            case StepIndicatorStyle.Progress:
                indicator = CreateProgressIndicator(step, index);
                break;
            case StepIndicatorStyle.Text:
            default:
                indicator = CreateTextIndicator(step, index);
                break;
        }

        container.Children.Add(indicator);

        if (ShowStepTitles && !string.IsNullOrEmpty(step.Title))
        {
            var titleLabel = new Label
            {
                Text = step.Title,
                FontSize = 11,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = GetIndicatorTextColor(step),
                MaximumWidthRequest = 80,
                LineBreakMode = LineBreakMode.TailTruncation
            };
            container.Children.Add(titleLabel);
        }

        // Make clickable in non-linear mode
        if (NavigationMode == WizardNavigationMode.NonLinear ||
            step.Status == WizardStepStatus.Completed ||
            step.Status == WizardStepStatus.Current)
        {
            var tapGesture = new TapGestureRecognizer();
            var capturedIndex = index;
            tapGesture.Tapped += (s, e) => GoToStep(capturedIndex);
            container.GestureRecognizers.Add(tapGesture);
        }

        return container;
    }

    private View CreateCircleIndicator(WizardStep step, int index)
    {
        var size = 32.0;
        var color = GetIndicatorColor(step);

        var circle = new Border
        {
            WidthRequest = size,
            HeightRequest = size,
            BackgroundColor = step.IsCurrent ? color : Colors.Transparent,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = size / 2 }
        };

        var content = new Label
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            FontSize = 13
        };

        if (step.Status == WizardStepStatus.Completed)
        {
            content.Text = "✓";
            content.TextColor = step.IsCurrent ? Colors.White : color;
        }
        else if (step.Status == WizardStepStatus.Error)
        {
            content.Text = "!";
            content.TextColor = step.IsCurrent ? Colors.White : color;
        }
        else if (ShowStepNumbers)
        {
            content.Text = (index + 1).ToString();
            content.TextColor = step.IsCurrent ? Colors.White : color;
        }
        else if (!string.IsNullOrEmpty(step.Icon))
        {
            content.Text = step.Icon;
            content.TextColor = step.IsCurrent ? Colors.White : color;
        }

        circle.Content = content;
        return circle;
    }

    private View CreateDotIndicator(WizardStep step)
    {
        var size = step.IsCurrent ? 12.0 : 8.0;
        var color = GetIndicatorColor(step);

        return new BoxView
        {
            WidthRequest = size,
            HeightRequest = size,
            Color = color,
            CornerRadius = size / 2,
            VerticalOptions = LayoutOptions.Center
        };
    }

    private View CreateProgressIndicator(WizardStep step, int index)
    {
        var color = GetIndicatorColor(step);
        var progress = step.IsCompleted ? 1.0 :
                       step.IsCurrent ? 0.5 : 0.0;

        var container = new Grid
        {
            WidthRequest = 60,
            HeightRequest = 8
        };

        // Background
        container.Children.Add(new BoxView
        {
            Color = MauiControlsExtrasTheme.GetBorderColor(),
            Opacity = 0.3,
            CornerRadius = 4
        });

        // Progress
        container.Children.Add(new BoxView
        {
            Color = color,
            WidthRequest = 60 * progress,
            HorizontalOptions = LayoutOptions.Start,
            CornerRadius = 4
        });

        return container;
    }

    private View CreateTextIndicator(WizardStep step, int index)
    {
        return new Label
        {
            Text = ShowStepNumbers ? $"Step {index + 1}" : step.Title ?? $"Step {index + 1}",
            FontSize = 12,
            FontAttributes = step.IsCurrent ? FontAttributes.Bold : FontAttributes.None,
            TextColor = GetIndicatorColor(step)
        };
    }

    private View CreateConnector(WizardStep currentStep, WizardStep nextStep)
    {
        var color = currentStep.IsCompleted
            ? EffectiveVisitedColor
            : EffectiveInactiveColor;

        return new BoxView
        {
            WidthRequest = 40,
            HeightRequest = 2,
            Color = color,
            Opacity = currentStep.IsCompleted ? 1.0 : 0.3,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(4, 0)
        };
    }

    private Color GetIndicatorColor(WizardStep step)
    {
        return step.Status switch
        {
            WizardStepStatus.Completed => EffectiveVisitedColor,
            WizardStepStatus.Error => ErrorStepColor ?? EffectiveErrorColor,
            WizardStepStatus.Current => EffectiveActiveColor,
            WizardStepStatus.Skipped => EffectiveDisabledNavigationColor,
            _ => EffectiveInactiveColor
        };
    }

    private Color GetIndicatorTextColor(WizardStep step)
    {
        return step.IsCurrent
            ? EffectiveForegroundColor
            : MauiControlsExtrasTheme.GetForegroundColor().WithAlpha(0.6f);
    }

    private void UpdateNavigationButtons()
    {
        // Update back button
        backButton.IsEnabled = CanGoBack;

        // Update next/finish button
        if (IsLastStep)
        {
            nextButton.Text = FinishButtonText;
        }
        else
        {
            nextButton.Text = NextButtonText;
        }

        // Update skip button
        skipButton.IsVisible = CurrentStep?.CanSkip == true;
    }

    #endregion

    #region Event Handlers

    private void OnControlFocused(object? sender, FocusEventArgs e)
    {
        _hasKeyboardFocus = true;
        OnPropertyChanged(nameof(HasKeyboardFocus));
        OnPropertyChanged(nameof(CurrentBorderColor));
        KeyboardFocusGained?.Invoke(this, new KeyboardFocusEventArgs(true));
        GotFocusCommand?.Execute(GotFocusCommandParameter ?? this);
    }

    private void OnControlUnfocused(object? sender, FocusEventArgs e)
    {
        _hasKeyboardFocus = false;
        OnPropertyChanged(nameof(HasKeyboardFocus));
        OnPropertyChanged(nameof(CurrentBorderColor));
        KeyboardFocusLost?.Invoke(this, new KeyboardFocusEventArgs(false));
        LostFocusCommand?.Execute(LostFocusCommandParameter ?? this);
    }

    private void OnStepsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Update step indices
        for (int i = 0; i < _steps.Count; i++)
        {
            _steps[i].Index = i;
        }

        OnPropertyChanged(nameof(StepCount));
        OnPropertyChanged(nameof(CompletionPercentage));

        // Initialize to first step if needed
        if (_steps.Count > 0 && _currentIndex < 0)
        {
            GoToStep(0);
        }
        else
        {
            UpdateStepContent();
            UpdateStepIndicator();
            UpdateNavigationButtons();
        }
    }

    private void OnBackClicked(object? sender, EventArgs e)
    {
        GoBack();
    }

    private void OnNextClicked(object? sender, EventArgs e)
    {
        if (IsLastStep)
        {
            Finish();
        }
        else
        {
            GoNext();
        }
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        Cancel();
    }

    private void OnSkipClicked(object? sender, EventArgs e)
    {
        SkipStep();
    }

    #endregion

    #region Property Changed Handlers

    private static void OnIndicatorPositionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Wizard wizard)
        {
            wizard.UpdateStepIndicator();
        }
    }

    private static void OnIndicatorStyleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Wizard wizard)
        {
            wizard.UpdateStepIndicator();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Adds a step to the wizard.
    /// </summary>
    /// <param name="step">The step to add.</param>
    public void AddStep(WizardStep step)
    {
        _steps.Add(step);
    }

    /// <summary>
    /// Removes a step from the wizard.
    /// </summary>
    /// <param name="step">The step to remove.</param>
    public void RemoveStep(WizardStep step)
    {
        _steps.Remove(step);
    }

    /// <summary>
    /// Clears all steps from the wizard.
    /// </summary>
    public void ClearSteps()
    {
        _steps.Clear();
        _currentIndex = -1;
        OnPropertyChanged(nameof(CurrentIndex));
        OnPropertyChanged(nameof(CurrentStep));
    }

    #endregion
}
