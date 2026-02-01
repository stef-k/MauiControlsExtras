namespace MauiControlsExtras.Controls;

/// <summary>
/// Event arguments for wizard step changes.
/// </summary>
public class WizardStepChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the previous step.
    /// </summary>
    public WizardStep? OldStep { get; }

    /// <summary>
    /// Gets the new current step.
    /// </summary>
    public WizardStep? NewStep { get; }

    /// <summary>
    /// Gets the previous step index.
    /// </summary>
    public int OldIndex { get; }

    /// <summary>
    /// Gets the new step index.
    /// </summary>
    public int NewIndex { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WizardStepChangedEventArgs"/> class.
    /// </summary>
    public WizardStepChangedEventArgs(WizardStep? oldStep, WizardStep? newStep, int oldIndex, int newIndex)
    {
        OldStep = oldStep;
        NewStep = newStep;
        OldIndex = oldIndex;
        NewIndex = newIndex;
    }
}

/// <summary>
/// Event arguments for wizard step changing (cancelable).
/// </summary>
public class WizardStepChangingEventArgs : EventArgs
{
    /// <summary>
    /// Gets the current step.
    /// </summary>
    public WizardStep? CurrentStep { get; }

    /// <summary>
    /// Gets the target step.
    /// </summary>
    public WizardStep? TargetStep { get; }

    /// <summary>
    /// Gets the current step index.
    /// </summary>
    public int CurrentIndex { get; }

    /// <summary>
    /// Gets the target step index.
    /// </summary>
    public int TargetIndex { get; }

    /// <summary>
    /// Gets or sets whether the navigation should be cancelled.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WizardStepChangingEventArgs"/> class.
    /// </summary>
    public WizardStepChangingEventArgs(WizardStep? currentStep, WizardStep? targetStep, int currentIndex, int targetIndex)
    {
        CurrentStep = currentStep;
        TargetStep = targetStep;
        CurrentIndex = currentIndex;
        TargetIndex = targetIndex;
    }
}

/// <summary>
/// Event arguments for wizard completion.
/// </summary>
public class WizardFinishedEventArgs : EventArgs
{
    /// <summary>
    /// Gets whether the wizard was cancelled.
    /// </summary>
    public bool WasCancelled { get; }

    /// <summary>
    /// Gets all the steps in the wizard.
    /// </summary>
    public IReadOnlyList<WizardStep> Steps { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WizardFinishedEventArgs"/> class.
    /// </summary>
    public WizardFinishedEventArgs(bool wasCancelled, IReadOnlyList<WizardStep> steps)
    {
        WasCancelled = wasCancelled;
        Steps = steps;
    }
}

/// <summary>
/// Event arguments for wizard finishing (cancelable).
/// </summary>
public class WizardFinishingEventArgs : EventArgs
{
    /// <summary>
    /// Gets all the steps in the wizard.
    /// </summary>
    public IReadOnlyList<WizardStep> Steps { get; }

    /// <summary>
    /// Gets or sets whether the finish should be cancelled.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WizardFinishingEventArgs"/> class.
    /// </summary>
    public WizardFinishingEventArgs(IReadOnlyList<WizardStep> steps)
    {
        Steps = steps;
    }
}

/// <summary>
/// Event arguments for wizard cancellation (cancelable).
/// </summary>
public class WizardCancellingEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets whether the cancellation should be prevented.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Gets the current step index when cancellation was requested.
    /// </summary>
    public int CurrentIndex { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WizardCancellingEventArgs"/> class.
    /// </summary>
    public WizardCancellingEventArgs(int currentIndex)
    {
        CurrentIndex = currentIndex;
    }
}

/// <summary>
/// Navigation direction for wizard step transitions.
/// </summary>
public enum WizardNavigationDirection
{
    /// <summary>
    /// Moving forward to a higher step index.
    /// </summary>
    Forward,

    /// <summary>
    /// Moving backward to a lower step index.
    /// </summary>
    Backward
}

/// <summary>
/// Event arguments for wizard step validation (cancelable with validation errors).
/// </summary>
public class WizardStepValidatingEventArgs : EventArgs
{
    /// <summary>
    /// Gets the current step.
    /// </summary>
    public WizardStep? CurrentStep { get; }

    /// <summary>
    /// Gets the target step.
    /// </summary>
    public WizardStep? TargetStep { get; }

    /// <summary>
    /// Gets the current step index.
    /// </summary>
    public int CurrentStepIndex { get; }

    /// <summary>
    /// Gets the target step index.
    /// </summary>
    public int TargetStepIndex { get; }

    /// <summary>
    /// Gets the navigation direction.
    /// </summary>
    public WizardNavigationDirection Direction { get; }

    /// <summary>
    /// Gets or sets whether the navigation should be cancelled.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Gets the collection of validation errors to display.
    /// </summary>
    public IList<string> ValidationErrors { get; } = new List<string>();

    /// <summary>
    /// Initializes a new instance of the <see cref="WizardStepValidatingEventArgs"/> class.
    /// </summary>
    public WizardStepValidatingEventArgs(
        WizardStep? currentStep,
        WizardStep? targetStep,
        int currentStepIndex,
        int targetStepIndex)
    {
        CurrentStep = currentStep;
        TargetStep = targetStep;
        CurrentStepIndex = currentStepIndex;
        TargetStepIndex = targetStepIndex;
        Direction = targetStepIndex > currentStepIndex
            ? WizardNavigationDirection.Forward
            : WizardNavigationDirection.Backward;
    }
}
