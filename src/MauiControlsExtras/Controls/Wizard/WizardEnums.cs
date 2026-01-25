namespace MauiControlsExtras.Controls;

/// <summary>
/// Specifies the navigation mode for the wizard.
/// </summary>
public enum WizardNavigationMode
{
    /// <summary>
    /// Steps must be completed in order.
    /// </summary>
    Linear,

    /// <summary>
    /// Users can navigate to any step regardless of completion.
    /// </summary>
    NonLinear
}

/// <summary>
/// Specifies the status of a wizard step.
/// </summary>
public enum WizardStepStatus
{
    /// <summary>
    /// Step has not been visited.
    /// </summary>
    NotVisited,

    /// <summary>
    /// Step is currently active.
    /// </summary>
    Current,

    /// <summary>
    /// Step has been completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Step has been skipped.
    /// </summary>
    Skipped,

    /// <summary>
    /// Step has an error that needs attention.
    /// </summary>
    Error
}

/// <summary>
/// Specifies the position of the step indicator.
/// </summary>
public enum StepIndicatorPosition
{
    /// <summary>
    /// Step indicator is at the top.
    /// </summary>
    Top,

    /// <summary>
    /// Step indicator is at the left side.
    /// </summary>
    Left,

    /// <summary>
    /// Step indicator is hidden.
    /// </summary>
    None
}

/// <summary>
/// Specifies the style of the step indicator.
/// </summary>
public enum StepIndicatorStyle
{
    /// <summary>
    /// Shows numbered circles connected by lines.
    /// </summary>
    Circle,

    /// <summary>
    /// Shows dots connected by lines.
    /// </summary>
    Dot,

    /// <summary>
    /// Shows progress bar style indicator.
    /// </summary>
    Progress,

    /// <summary>
    /// Shows text-only step titles.
    /// </summary>
    Text
}
