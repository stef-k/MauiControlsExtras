namespace MauiControlsExtras.Base.Validation;

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public readonly struct ValidationResult
{
    /// <summary>
    /// Gets whether the validation passed.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the collection of validation error messages.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// Gets the first error message, or null if validation passed.
    /// </summary>
    public string? FirstError => Errors.Count > 0 ? Errors[0] : null;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationResult"/> struct.
    /// </summary>
    /// <param name="isValid">Whether validation passed.</param>
    /// <param name="errors">Collection of error messages.</param>
    public ValidationResult(bool isValid, IReadOnlyList<string>? errors = null)
    {
        IsValid = isValid;
        Errors = errors ?? Array.Empty<string>();
    }

    /// <summary>
    /// Gets a successful validation result.
    /// </summary>
    public static ValidationResult Success => new(true);

    /// <summary>
    /// Creates a failed validation result with the specified error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed validation result.</returns>
    public static ValidationResult Failure(string error) =>
        new(false, new[] { error });

    /// <summary>
    /// Creates a failed validation result with multiple error messages.
    /// </summary>
    /// <param name="errors">The error messages.</param>
    /// <returns>A failed validation result.</returns>
    public static ValidationResult Failure(IReadOnlyList<string> errors) =>
        new(false, errors);

    /// <summary>
    /// Combines multiple validation results into one.
    /// The combined result is valid only if all input results are valid.
    /// </summary>
    /// <param name="results">The validation results to combine.</param>
    /// <returns>A combined validation result.</returns>
    public static ValidationResult Combine(params ValidationResult[] results)
    {
        var allErrors = new List<string>();
        foreach (var result in results)
        {
            if (!result.IsValid)
            {
                allErrors.AddRange(result.Errors);
            }
        }

        return allErrors.Count > 0
            ? new ValidationResult(false, allErrors)
            : Success;
    }
}
