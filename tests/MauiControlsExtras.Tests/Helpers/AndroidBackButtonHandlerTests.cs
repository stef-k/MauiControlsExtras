using MauiControlsExtras.Helpers;

namespace MauiControlsExtras.Tests.Helpers;

public class AndroidBackButtonHandlerTests : IDisposable
{
    public AndroidBackButtonHandlerTests()
    {
        AndroidBackButtonHandler.Reset();
    }

    public void Dispose()
    {
        AndroidBackButtonHandler.Reset();
    }

    [Fact]
    public void Register_HandleBackPress_InvokesCloseAction()
    {
        var closed = false;
        var owner = new object();

        AndroidBackButtonHandler.Register(owner, () => closed = true);

        var handled = AndroidBackButtonHandler.HandleBackPress();

        Assert.True(handled);
        Assert.True(closed);
    }

    [Fact]
    public void HandleBackPress_EmptyStack_ReturnsFalse()
    {
        var handled = AndroidBackButtonHandler.HandleBackPress();

        Assert.False(handled);
    }

    [Fact]
    public void HandleBackPress_LIFO_MostRecentFirst()
    {
        var order = new List<string>();
        var owner1 = new object();
        var owner2 = new object();
        var owner3 = new object();

        AndroidBackButtonHandler.Register(owner1, () => order.Add("first"));
        AndroidBackButtonHandler.Register(owner2, () => order.Add("second"));
        AndroidBackButtonHandler.Register(owner3, () => order.Add("third"));

        AndroidBackButtonHandler.HandleBackPress();
        AndroidBackButtonHandler.HandleBackPress();
        AndroidBackButtonHandler.HandleBackPress();

        Assert.Equal(["third", "second", "first"], order);
    }

    [Fact]
    public void Unregister_RemovesEntry()
    {
        var closed = false;
        var owner = new object();

        AndroidBackButtonHandler.Register(owner, () => closed = true);
        AndroidBackButtonHandler.Unregister(owner);

        var handled = AndroidBackButtonHandler.HandleBackPress();

        Assert.False(handled);
        Assert.False(closed);
    }

    [Fact]
    public void Unregister_NonExistentOwner_NoThrow()
    {
        var owner = new object();

        var exception = Record.Exception(() => AndroidBackButtonHandler.Unregister(owner));

        Assert.Null(exception);
    }

    [Fact]
    public void Register_SameOwner_MovesToTop()
    {
        var order = new List<string>();
        var owner1 = new object();
        var owner2 = new object();

        AndroidBackButtonHandler.Register(owner1, () => order.Add("first"));
        AndroidBackButtonHandler.Register(owner2, () => order.Add("second"));
        // Re-register owner1 — should move to top with new action
        AndroidBackButtonHandler.Register(owner1, () => order.Add("first-again"));

        AndroidBackButtonHandler.HandleBackPress();
        AndroidBackButtonHandler.HandleBackPress();

        Assert.Equal(["first-again", "second"], order);
        Assert.Equal(0, AndroidBackButtonHandler.Count);
    }

    [Fact]
    public void HandleBackPress_AfterClose_UnregisterIsIdempotent()
    {
        // Simulates: back press → Close() → ToggleDropdown() → Unregister()
        // The entry was already popped by HandleBackPress, so Unregister is a no-op
        var closeCount = 0;
        var owner = new object();

        AndroidBackButtonHandler.Register(owner, () =>
        {
            closeCount++;
            // Simulate what Close/ToggleDropdown does: calls Unregister
            AndroidBackButtonHandler.Unregister(owner);
        });

        var handled = AndroidBackButtonHandler.HandleBackPress();

        Assert.True(handled);
        Assert.Equal(1, closeCount);
        Assert.Equal(0, AndroidBackButtonHandler.Count);
    }

    [Fact]
    public void StaleWeakReference_CleanedOnNextOperation()
    {
        AndroidBackButtonHandler.Register(new object(), () => { });
        // The owner is eligible for GC since we didn't keep a reference
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // CleanupStale happens during HandleBackPress
        var handled = AndroidBackButtonHandler.HandleBackPress();

        // The stale entry may or may not be collected (GC is non-deterministic),
        // but it should not throw
        Assert.Equal(0, AndroidBackButtonHandler.Count);
    }

    [Fact]
    public void Reset_ClearsAllRegistrations()
    {
        var owner1 = new object();
        var owner2 = new object();

        AndroidBackButtonHandler.Register(owner1, () => { });
        AndroidBackButtonHandler.Register(owner2, () => { });

        AndroidBackButtonHandler.Reset();

        Assert.Equal(0, AndroidBackButtonHandler.Count);
        Assert.False(AndroidBackButtonHandler.HandleBackPress());
    }

    [Fact]
    public void MultipleRegistrations_CountIsCorrect()
    {
        var owner1 = new object();
        var owner2 = new object();
        var owner3 = new object();

        AndroidBackButtonHandler.Register(owner1, () => { });
        Assert.Equal(1, AndroidBackButtonHandler.Count);

        AndroidBackButtonHandler.Register(owner2, () => { });
        Assert.Equal(2, AndroidBackButtonHandler.Count);

        AndroidBackButtonHandler.Register(owner3, () => { });
        Assert.Equal(3, AndroidBackButtonHandler.Count);

        AndroidBackButtonHandler.HandleBackPress();
        Assert.Equal(2, AndroidBackButtonHandler.Count);
    }
}
