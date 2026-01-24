using MauiControlsExtras.Base.Validation;

namespace MauiControlsExtras.Tests;

/// <summary>
/// Tests for the <see cref="ValidationResult"/> struct.
/// </summary>
public class ValidationResultTests
{
    [Fact]
    public void Success_IsValid_ReturnsTrue()
    {
        var result = ValidationResult.Success;

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Null(result.FirstError);
    }

    [Fact]
    public void FailureWithSingleError_IsValid_ReturnsFalse()
    {
        var errorMessage = "Selection is required";
        var result = ValidationResult.Failure(errorMessage);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal(errorMessage, result.FirstError);
    }

    [Fact]
    public void FailureWithMultipleErrors_ContainsAllErrors()
    {
        var errors = new[] { "Error 1", "Error 2", "Error 3" };
        var result = ValidationResult.Failure(errors);

        Assert.False(result.IsValid);
        Assert.Equal(3, result.Errors.Count);
        Assert.Equal("Error 1", result.FirstError);
        Assert.Contains("Error 2", result.Errors);
        Assert.Contains("Error 3", result.Errors);
    }

    [Fact]
    public void Constructor_WithValidTrue_CreatesSuccessResult()
    {
        var result = new ValidationResult(true);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Constructor_WithValidFalseAndErrors_CreatesFailureResult()
    {
        var errors = new[] { "Error" };
        var result = new ValidationResult(false, errors);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void Constructor_WithNullErrors_UsesEmptyArray()
    {
        var result = new ValidationResult(true, null);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Combine_AllSuccess_ReturnsSuccess()
    {
        var result1 = ValidationResult.Success;
        var result2 = ValidationResult.Success;
        var result3 = ValidationResult.Success;

        var combined = ValidationResult.Combine(result1, result2, result3);

        Assert.True(combined.IsValid);
        Assert.Empty(combined.Errors);
    }

    [Fact]
    public void Combine_OneFailure_ReturnsFailure()
    {
        var result1 = ValidationResult.Success;
        var result2 = ValidationResult.Failure("Error from result 2");
        var result3 = ValidationResult.Success;

        var combined = ValidationResult.Combine(result1, result2, result3);

        Assert.False(combined.IsValid);
        Assert.Single(combined.Errors);
        Assert.Equal("Error from result 2", combined.FirstError);
    }

    [Fact]
    public void Combine_MultipleFailures_CombinesAllErrors()
    {
        var result1 = ValidationResult.Failure("Error 1");
        var result2 = ValidationResult.Success;
        var result3 = ValidationResult.Failure(new[] { "Error 2", "Error 3" });

        var combined = ValidationResult.Combine(result1, result2, result3);

        Assert.False(combined.IsValid);
        Assert.Equal(3, combined.Errors.Count);
        Assert.Contains("Error 1", combined.Errors);
        Assert.Contains("Error 2", combined.Errors);
        Assert.Contains("Error 3", combined.Errors);
    }

    [Fact]
    public void Combine_EmptyResults_ReturnsSuccess()
    {
        var combined = ValidationResult.Combine();

        Assert.True(combined.IsValid);
        Assert.Empty(combined.Errors);
    }
}
