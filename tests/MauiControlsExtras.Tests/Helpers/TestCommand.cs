using System.Windows.Input;

namespace MauiControlsExtras.Tests.Helpers;

/// <summary>
/// Simple ICommand implementation for verifying command execution in tests.
/// </summary>
public class TestCommand : ICommand
{
    private readonly Func<object?, bool>? _canExecute;
    private readonly Action<object?> _execute;

    public int ExecuteCount { get; private set; }
    public object? LastParameter { get; private set; }
    public bool WasExecuted => ExecuteCount > 0;

    public TestCommand(Action<object?>? execute = null, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? (_ => { });
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke(parameter) ?? true;
    }

    public void Execute(object? parameter)
    {
        LastParameter = parameter;
        ExecuteCount++;
        _execute(parameter);
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
