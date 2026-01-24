using System.Windows.Input;

namespace MauiControlsExtras.Base.Validation;

/// <summary>
/// Interface for controls that support validation.
/// </summary>
public interface IValidatable
{
    /// <summary>
    /// Gets whether the current value is valid according to validation rules.
    /// </summary>
    bool IsValid { get; }

    /// <summary>
    /// Gets the list of current validation errors.
    /// </summary>
    IReadOnlyList<string> ValidationErrors { get; }

    /// <summary>
    /// Gets or sets the command to execute when validation is triggered.
    /// The command parameter will be the <see cref="ValidationResult"/>.
    /// </summary>
    ICommand? ValidateCommand { get; set; }

    /// <summary>
    /// Performs validation and returns the result.
    /// </summary>
    /// <returns>The validation result.</returns>
    ValidationResult Validate();
}
