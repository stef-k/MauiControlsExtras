using MauiControlsExtras.Controls;
using MauiControlsExtras.Tests.Helpers;

namespace MauiControlsExtras.Tests.Controls;

public class WizardStepTests
{
    [Fact]
    public void Validate_WhenOptional_ReturnsTrue()
    {
        var step = new WizardStep { IsOptional = true, IsValid = false };

        var result = step.Validate();

        Assert.True(result);
    }

    [Fact]
    public void Validate_ExecutesValidationCommand()
    {
        var step = new WizardStep();
        var cmd = new TestCommand(param =>
        {
            if (param is WizardStep s) s.IsValid = true;
        });
        step.ValidationCommand = cmd;

        step.Validate();

        Assert.True(cmd.WasExecuted);
    }

    [Fact]
    public void Validate_UsesValidationCommandParameter_WhenSet()
    {
        var step = new WizardStep();
        var cmd = new TestCommand();
        step.ValidationCommand = cmd;
        step.ValidationCommandParameter = "custom";

        step.Validate();

        Assert.Equal("custom", cmd.LastParameter);
    }

    [Fact]
    public void Validate_PassesSelfAsParameter_WhenNoParameterSet()
    {
        var step = new WizardStep();
        var cmd = new TestCommand();
        step.ValidationCommand = cmd;

        step.Validate();

        Assert.Same(step, cmd.LastParameter);
    }

    [Fact]
    public void Validate_WhenInvalid_RaisesValidationFailed()
    {
        var step = new WizardStep { IsValid = false, ValidationMessage = "Required" };
        WizardStepValidationEventArgs? args = null;
        step.ValidationFailed += (_, e) => args = e;

        step.Validate();

        Assert.NotNull(args);
        Assert.Same(step, args.Step);
        Assert.Equal("Required", args.Message);
    }

    [Fact]
    public void Validate_WhenValid_DoesNotRaiseValidationFailed()
    {
        var step = new WizardStep { IsValid = true };
        var raised = false;
        step.ValidationFailed += (_, _) => raised = true;

        step.Validate();

        Assert.False(raised);
    }

    [Fact]
    public void Validate_ReturnsFalse_WhenIsValidIsFalse()
    {
        var step = new WizardStep { IsValid = false };

        var result = step.Validate();

        Assert.False(result);
    }

    [Fact]
    public void OnEnter_SetsStatusToCurrent()
    {
        var step = new WizardStep();

        step.OnEnter();

        Assert.Equal(WizardStepStatus.Current, step.Status);
        Assert.True(step.IsCurrent);
    }

    [Fact]
    public void OnEnter_ExecutesOnEnterCommand()
    {
        var step = new WizardStep();
        var cmd = new TestCommand();
        step.OnEnterCommand = cmd;

        step.OnEnter();

        Assert.True(cmd.WasExecuted);
    }

    [Fact]
    public void OnEnter_UsesOnEnterCommandParameter_WhenSet()
    {
        var step = new WizardStep();
        var cmd = new TestCommand();
        step.OnEnterCommand = cmd;
        step.OnEnterCommandParameter = "enter-param";

        step.OnEnter();

        Assert.Equal("enter-param", cmd.LastParameter);
    }

    [Fact]
    public void OnEnter_RaisesEnteredEvent()
    {
        var step = new WizardStep();
        var raised = false;
        step.Entered += (_, _) => raised = true;

        step.OnEnter();

        Assert.True(raised);
    }

    [Fact]
    public void OnExit_WhenCompleted_SetsStatusToCompleted()
    {
        var step = new WizardStep();
        step.OnEnter();

        step.OnExit(completed: true);

        Assert.Equal(WizardStepStatus.Completed, step.Status);
        Assert.True(step.IsCompleted);
    }

    [Fact]
    public void OnExit_WhenNotCompleted_KeepsCurrentStatus()
    {
        var step = new WizardStep();
        step.OnEnter();

        step.OnExit(completed: false);

        Assert.Equal(WizardStepStatus.Current, step.Status);
    }

    [Fact]
    public void OnExit_ExecutesOnExitCommand()
    {
        var step = new WizardStep();
        var cmd = new TestCommand();
        step.OnExitCommand = cmd;

        step.OnExit(completed: true);

        Assert.True(cmd.WasExecuted);
    }

    [Fact]
    public void OnExit_RaisesExitedEvent()
    {
        var step = new WizardStep();
        var raised = false;
        step.Exited += (_, _) => raised = true;

        step.OnExit(completed: true);

        Assert.True(raised);
    }

    [Fact]
    public void Status_RaisesPropertyChanged()
    {
        var step = new WizardStep();
        var changed = new List<string>();
        step.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        step.OnEnter();

        Assert.Contains("Status", changed);
        Assert.Contains("IsCurrent", changed);
        Assert.Contains("IsCompleted", changed);
    }

    [Fact]
    public void IsValid_RaisesPropertyChanged()
    {
        var step = new WizardStep();
        var changed = false;
        step.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "IsValid") changed = true;
        };

        step.IsValid = false;

        Assert.True(changed);
    }

    [Fact]
    public void ValidationMessage_RaisesPropertyChanged()
    {
        var step = new WizardStep();
        var changed = false;
        step.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "ValidationMessage") changed = true;
        };

        step.ValidationMessage = "Error";

        Assert.True(changed);
    }

    [Fact]
    public void PropertyDefaults_AreCorrect()
    {
        var step = new WizardStep();

        Assert.Null(step.Title);
        Assert.Null(step.Description);
        Assert.Null(step.Icon);
        Assert.False(step.CanSkip);
        Assert.False(step.IsOptional);
        Assert.True(step.IsValid);
        Assert.Null(step.ValidationMessage);
        Assert.Equal(WizardStepStatus.NotVisited, step.Status);
        Assert.False(step.IsCompleted);
        Assert.False(step.IsCurrent);
    }
}
