using System.ComponentModel;
using System.Windows.Input;
using MauiControlsExtras.Base;

namespace MauiControlsExtras.Controls;

/// <summary>
/// Represents a single step in a Wizard control.
/// </summary>
public class WizardStep : StyledControlBase, INotifyPropertyChanged
{
    #region Private Fields

    private WizardStepStatus _status = WizardStepStatus.NotVisited;
    private bool _isValid = true;
    private string? _validationMessage;

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="Title"/> bindable property.
    /// </summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(WizardStep),
        null);

    /// <summary>
    /// Identifies the <see cref="Description"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description),
        typeof(string),
        typeof(WizardStep),
        null);

    /// <summary>
    /// Identifies the <see cref="Icon"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon),
        typeof(string),
        typeof(WizardStep),
        null);

    /// <summary>
    /// Identifies the <see cref="CanSkip"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CanSkipProperty = BindableProperty.Create(
        nameof(CanSkip),
        typeof(bool),
        typeof(WizardStep),
        false);

    /// <summary>
    /// Identifies the <see cref="IsOptional"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsOptionalProperty = BindableProperty.Create(
        nameof(IsOptional),
        typeof(bool),
        typeof(WizardStep),
        false);

    /// <summary>
    /// Identifies the <see cref="StepTemplate"/> bindable property.
    /// </summary>
    public static readonly BindableProperty StepTemplateProperty = BindableProperty.Create(
        nameof(StepTemplate),
        typeof(DataTemplate),
        typeof(WizardStep),
        null);

    /// <summary>
    /// Identifies the <see cref="ValidationCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ValidationCommandProperty = BindableProperty.Create(
        nameof(ValidationCommand),
        typeof(ICommand),
        typeof(WizardStep),
        null);

    /// <summary>
    /// Identifies the <see cref="ValidationCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ValidationCommandParameterProperty = BindableProperty.Create(
        nameof(ValidationCommandParameter),
        typeof(object),
        typeof(WizardStep));

    /// <summary>
    /// Identifies the <see cref="OnEnterCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty OnEnterCommandProperty = BindableProperty.Create(
        nameof(OnEnterCommand),
        typeof(ICommand),
        typeof(WizardStep),
        null);

    /// <summary>
    /// Identifies the <see cref="OnEnterCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty OnEnterCommandParameterProperty = BindableProperty.Create(
        nameof(OnEnterCommandParameter),
        typeof(object),
        typeof(WizardStep));

    /// <summary>
    /// Identifies the <see cref="OnExitCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty OnExitCommandProperty = BindableProperty.Create(
        nameof(OnExitCommand),
        typeof(ICommand),
        typeof(WizardStep),
        null);

    /// <summary>
    /// Identifies the <see cref="OnExitCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty OnExitCommandParameterProperty = BindableProperty.Create(
        nameof(OnExitCommandParameter),
        typeof(object),
        typeof(WizardStep));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the step title.
    /// </summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the step description.
    /// </summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// Gets or sets the step icon (glyph character).
    /// </summary>
    public string? Icon
    {
        get => (string?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets whether this step can be skipped.
    /// </summary>
    public bool CanSkip
    {
        get => (bool)GetValue(CanSkipProperty);
        set => SetValue(CanSkipProperty, value);
    }

    /// <summary>
    /// Gets or sets whether this step is optional (no validation required).
    /// </summary>
    public bool IsOptional
    {
        get => (bool)GetValue(IsOptionalProperty);
        set => SetValue(IsOptionalProperty, value);
    }

    /// <summary>
    /// Gets or sets a template for the step content.
    /// </summary>
    public DataTemplate? StepTemplate
    {
        get => (DataTemplate?)GetValue(StepTemplateProperty);
        set => SetValue(StepTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets a command that validates the step.
    /// Should return true/false or set IsValid.
    /// </summary>
    public ICommand? ValidationCommand
    {
        get => (ICommand?)GetValue(ValidationCommandProperty);
        set => SetValue(ValidationCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="ValidationCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? ValidationCommandParameter
    {
        get => GetValue(ValidationCommandParameterProperty);
        set => SetValue(ValidationCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets a command executed when entering this step.
    /// </summary>
    public ICommand? OnEnterCommand
    {
        get => (ICommand?)GetValue(OnEnterCommandProperty);
        set => SetValue(OnEnterCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="OnEnterCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? OnEnterCommandParameter
    {
        get => GetValue(OnEnterCommandParameterProperty);
        set => SetValue(OnEnterCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets a command executed when leaving this step.
    /// </summary>
    public ICommand? OnExitCommand
    {
        get => (ICommand?)GetValue(OnExitCommandProperty);
        set => SetValue(OnExitCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="OnExitCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? OnExitCommandParameter
    {
        get => GetValue(OnExitCommandParameterProperty);
        set => SetValue(OnExitCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets the current status of this step.
    /// </summary>
    public WizardStepStatus Status
    {
        get => _status;
        internal set
        {
            if (_status != value)
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(IsCompleted));
                OnPropertyChanged(nameof(IsCurrent));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether this step is currently valid.
    /// </summary>
    public bool IsValid
    {
        get => _isValid;
        set
        {
            if (_isValid != value)
            {
                _isValid = value;
                OnPropertyChanged(nameof(IsValid));
            }
        }
    }

    /// <summary>
    /// Gets or sets the validation error message.
    /// </summary>
    public string? ValidationMessage
    {
        get => _validationMessage;
        set
        {
            if (_validationMessage != value)
            {
                _validationMessage = value;
                OnPropertyChanged(nameof(ValidationMessage));
            }
        }
    }

    /// <summary>
    /// Gets whether this step has been completed.
    /// </summary>
    public bool IsCompleted => Status == WizardStepStatus.Completed;

    /// <summary>
    /// Gets whether this step is the current step.
    /// </summary>
    public bool IsCurrent => Status == WizardStepStatus.Current;

    /// <summary>
    /// Gets the step index within the wizard.
    /// </summary>
    public int Index { get; internal set; }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when a validation error is detected.
    /// </summary>
    public event EventHandler<WizardStepValidationEventArgs>? ValidationFailed;

    /// <summary>
    /// Occurs when the step is entered.
    /// </summary>
    public event EventHandler? Entered;

    /// <summary>
    /// Occurs when the step is exited.
    /// </summary>
    public event EventHandler? Exited;

    #endregion

    #region Methods

    /// <summary>
    /// Validates the step.
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public virtual bool Validate()
    {
        if (IsOptional) return true;

        if (ValidationCommand != null)
        {
            var parameter = ValidationCommandParameter ?? this;
            if (ValidationCommand.CanExecute(parameter))
            {
                ValidationCommand.Execute(parameter);
            }
        }

        if (!IsValid)
        {
            ValidationFailed?.Invoke(this, new WizardStepValidationEventArgs(this, ValidationMessage));
        }

        return IsValid;
    }

    /// <summary>
    /// Called when this step becomes active.
    /// </summary>
    internal void OnEnter()
    {
        Status = WizardStepStatus.Current;
        OnEnterCommand?.Execute(OnEnterCommandParameter ?? this);
        Entered?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when leaving this step.
    /// </summary>
    /// <param name="completed">Whether the step was completed successfully.</param>
    internal void OnExit(bool completed)
    {
        if (completed)
        {
            Status = WizardStepStatus.Completed;
        }
        OnExitCommand?.Execute(OnExitCommandParameter ?? this);
        Exited?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}

/// <summary>
/// Event arguments for step validation failures.
/// </summary>
public class WizardStepValidationEventArgs : EventArgs
{
    /// <summary>
    /// Gets the step that failed validation.
    /// </summary>
    public WizardStep Step { get; }

    /// <summary>
    /// Gets the validation error message.
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WizardStepValidationEventArgs"/> class.
    /// </summary>
    public WizardStepValidationEventArgs(WizardStep step, string? message)
    {
        Step = step;
        Message = message;
    }
}
