# Wizard API Reference

Full API documentation for the `MauiControlsExtras.Controls.Wizard` control.

## Namespace

```csharp
using MauiControlsExtras.Controls;
```

## Class Definition

```csharp
public partial class Wizard : HeaderedControlBase, IKeyboardNavigable
```

## Inheritance

Inherits from [HeaderedControlBase](base-classes.md#headeredcontrolbase). See base class documentation for inherited styling and header properties.

## Interfaces

- [IKeyboardNavigable](interfaces.md#ikeyboardnavigable) - Keyboard navigation support

---

## Properties

### Core Properties

#### Steps

Gets the collection of wizard steps.

```csharp
public ObservableCollection<WizardStep> Steps { get; }
```

| Type | Bindable |
|------|----------|
| `ObservableCollection<WizardStep>` | No (collection property) |

---

#### NavigationMode

Gets or sets the navigation mode (linear or non-linear).

```csharp
public WizardNavigationMode NavigationMode { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `WizardNavigationMode` | `Linear` | Yes |

---

#### CurrentIndex (Read-only)

Gets the current step index.

```csharp
public int CurrentIndex { get; }
```

---

#### CurrentStep (Read-only)

Gets the current step.

```csharp
public WizardStep? CurrentStep { get; }
```

---

#### StepCount (Read-only)

Gets the step count.

```csharp
public int StepCount { get; }
```

---

#### CompletionPercentage (Read-only)

Gets the completion percentage (0-100).

```csharp
public double CompletionPercentage { get; }
```

---

### Navigation State Properties

#### CanGoBack (Read-only)

Gets whether the wizard can go back.

```csharp
public bool CanGoBack { get; }
```

---

#### CanGoNext (Read-only)

Gets whether the wizard can go forward.

```csharp
public bool CanGoNext { get; }
```

---

#### IsFirstStep (Read-only)

Gets whether the current step is the first step.

```csharp
public bool IsFirstStep { get; }
```

---

#### IsLastStep (Read-only)

Gets whether the current step is the last step.

```csharp
public bool IsLastStep { get; }
```

---

### Indicator Properties

#### IndicatorPosition

Gets or sets the position of the step indicator.

```csharp
public StepIndicatorPosition IndicatorPosition { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `StepIndicatorPosition` | `Top` | Yes |

---

#### IndicatorStyle

Gets or sets the style of the step indicator.

```csharp
public StepIndicatorStyle IndicatorStyle { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `StepIndicatorStyle` | `Circle` | Yes |

---

#### ShowStepNumbers

Gets or sets whether step numbers are shown.

```csharp
public bool ShowStepNumbers { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### ShowStepTitles

Gets or sets whether step titles are shown in the indicator.

```csharp
public bool ShowStepTitles { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

### Button Properties

#### ShowCancelButton

Gets or sets whether the cancel button is shown.

```csharp
public bool ShowCancelButton { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### ShowBackButton

Gets or sets whether the back button is shown.

```csharp
public bool ShowBackButton { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### BackButtonText

Gets or sets the back button text.

```csharp
public string BackButtonText { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"Back"` | Yes |

---

#### NextButtonText

Gets or sets the next button text.

```csharp
public string NextButtonText { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"Next"` | Yes |

---

#### FinishButtonText

Gets or sets the finish button text.

```csharp
public string FinishButtonText { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"Finish"` | Yes |

---

#### CancelButtonText

Gets or sets the cancel button text.

```csharp
public string CancelButtonText { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"Cancel"` | Yes |

---

#### SkipButtonText

Gets or sets the skip button text.

```csharp
public string SkipButtonText { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"Skip"` | Yes |

---

### Behavior Properties

#### ValidateOnNext

Gets or sets whether validation is performed when clicking Next.

```csharp
public bool ValidateOnNext { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### AnimateTransitions

Gets or sets whether step transitions are animated.

```csharp
public bool AnimateTransitions { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

### Appearance Properties

#### CompletedStepColor

Gets or sets the color for completed steps.

```csharp
public Color? CompletedStepColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` (uses accent) | Yes |

---

#### ErrorStepColor

Gets or sets the color for steps with errors.

```csharp
public Color? ErrorStepColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` (uses red) | Yes |

---

## Events

### StepChanged

Occurs when the current step changes.

```csharp
public event EventHandler<WizardStepChangedEventArgs>? StepChanged;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| OldStep | `WizardStep?` | Previous step |
| NewStep | `WizardStep?` | New step |
| OldIndex | `int` | Previous step index |
| NewIndex | `int` | New step index |

---

### StepChanging

Occurs before the current step changes (cancelable).

```csharp
public event EventHandler<WizardStepChangingEventArgs>? StepChanging;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| OldStep | `WizardStep?` | Previous step |
| NewStep | `WizardStep?` | Proposed new step |
| Cancel | `bool` | Set to true to cancel |

---

### Finished

Occurs when the wizard finishes.

```csharp
public event EventHandler<WizardFinishedEventArgs>? Finished;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| WasCancelled | `bool` | Whether the wizard was cancelled |
| Steps | `IReadOnlyList<WizardStep>` | All wizard steps |

---

### Finishing

Occurs before the wizard finishes (cancelable).

```csharp
public event EventHandler<WizardFinishingEventArgs>? Finishing;
```

---

### Cancelled

Occurs when the wizard is cancelled.

```csharp
public event EventHandler<WizardFinishedEventArgs>? Cancelled;
```

---

### Cancelling

Occurs before the wizard is cancelled (cancelable).

```csharp
public event EventHandler<WizardCancellingEventArgs>? Cancelling;
```

---

## Commands

### StepChangedCommand

Executed when the step changes.

```csharp
public ICommand? StepChangedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Args | `WizardStepChangedEventArgs` |

---

### FinishedCommand

Executed when the wizard finishes.

```csharp
public ICommand? FinishedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Args | `WizardFinishedEventArgs` |

---

### CancelledCommand

Executed when the wizard is cancelled.

```csharp
public ICommand? CancelledCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Args | `WizardFinishedEventArgs` |

---

## Methods

### GoNext()

Navigates to the next step.

```csharp
public bool GoNext()
```

| Returns | Description |
|---------|-------------|
| `bool` | True if navigation succeeded |

---

### GoBack()

Navigates to the previous step.

```csharp
public bool GoBack()
```

| Returns | Description |
|---------|-------------|
| `bool` | True if navigation succeeded |

---

### GoToStep(int index)

Navigates to a specific step.

```csharp
public bool GoToStep(int index)
```

| Parameter | Type | Description |
|-----------|------|-------------|
| index | `int` | The step index |

| Returns | Description |
|---------|-------------|
| `bool` | True if navigation succeeded |

---

### Finish()

Finishes the wizard.

```csharp
public bool Finish()
```

| Returns | Description |
|---------|-------------|
| `bool` | True if the wizard finished successfully |

---

### Cancel()

Cancels the wizard.

```csharp
public bool Cancel()
```

| Returns | Description |
|---------|-------------|
| `bool` | True if the wizard was cancelled successfully |

---

### SkipStep()

Skips the current step.

```csharp
public bool SkipStep()
```

| Returns | Description |
|---------|-------------|
| `bool` | True if the step was skipped successfully |

---

### Reset()

Resets the wizard to the first step.

```csharp
public void Reset()
```

---

### AddStep(WizardStep step)

Adds a step to the wizard.

```csharp
public void AddStep(WizardStep step)
```

---

### RemoveStep(WizardStep step)

Removes a step from the wizard.

```csharp
public void RemoveStep(WizardStep step)
```

---

### ClearSteps()

Clears all steps from the wizard.

```csharp
public void ClearSteps()
```

---

## Enumerations

### WizardNavigationMode

```csharp
public enum WizardNavigationMode
{
    Linear,      // Steps must be completed in order
    NonLinear    // Users can navigate to any step
}
```

### WizardStepStatus

```csharp
public enum WizardStepStatus
{
    NotVisited,   // Step has not been visited
    Current,      // Step is currently active
    Completed,    // Step has been completed
    Skipped,      // Step has been skipped
    Error         // Step has an error
}
```

### StepIndicatorPosition

```csharp
public enum StepIndicatorPosition
{
    Top,    // Step indicator at the top
    Left,   // Step indicator on the left
    None    // Step indicator hidden
}
```

### StepIndicatorStyle

```csharp
public enum StepIndicatorStyle
{
    Circle,     // Numbered circles with connecting lines
    Dot,        // Dots with connecting lines
    Progress,   // Progress bar style
    Text        // Text-only step titles
}
```

---

## Supporting Types

### WizardStep

Represents a single step in the wizard.

```csharp
public class WizardStep : ContentView, INotifyPropertyChanged
{
    // Properties
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool CanSkip { get; set; }
    public bool IsOptional { get; set; }
    public DataTemplate? StepTemplate { get; set; }
    public ICommand? ValidationCommand { get; set; }
    public ICommand? OnEnterCommand { get; set; }
    public ICommand? OnExitCommand { get; set; }

    // State
    public WizardStepStatus Status { get; }
    public bool IsValid { get; set; }
    public string? ValidationMessage { get; set; }
    public bool IsCompleted { get; }
    public bool IsCurrent { get; }
    public int Index { get; }

    // Events
    public event EventHandler<WizardStepValidationEventArgs>? ValidationFailed;
    public event EventHandler? Entered;
    public event EventHandler? Exited;

    // Methods
    public virtual bool Validate();
}
```

---

## Keyboard Shortcuts

| Key | Description |
|-----|-------------|
| Arrow Left | Previous step |
| Arrow Right | Next step |
| Page Up | Previous step |
| Page Down | Next step |
| Home | First step (non-linear mode) |
| End | Last step (non-linear mode) |
| Enter | Next step / Finish |
| Escape | Cancel wizard |

---

## Usage Examples

### Basic Wizard

```xml
<extras:Wizard FinishedCommand="{Binding OnWizardFinishedCommand}">
    <extras:WizardStep Title="Welcome">
        <Label Text="Welcome to the setup wizard!" />
    </extras:WizardStep>
    <extras:WizardStep Title="Configuration">
        <VerticalStackLayout>
            <Entry Placeholder="Enter your name" />
            <Entry Placeholder="Enter your email" />
        </VerticalStackLayout>
    </extras:WizardStep>
    <extras:WizardStep Title="Complete">
        <Label Text="Setup complete!" />
    </extras:WizardStep>
</extras:Wizard>
```

### With Validation

```xml
<extras:Wizard ValidateOnNext="True"
               FinishedCommand="{Binding FinishCommand}">
    <extras:WizardStep Title="Personal Info"
                       ValidationCommand="{Binding ValidatePersonalInfoCommand}">
        <VerticalStackLayout>
            <Entry Text="{Binding Name}" Placeholder="Full Name" />
            <Entry Text="{Binding Email}" Placeholder="Email" Keyboard="Email" />
        </VerticalStackLayout>
    </extras:WizardStep>
    <extras:WizardStep Title="Address"
                       ValidationCommand="{Binding ValidateAddressCommand}">
        <VerticalStackLayout>
            <Entry Text="{Binding Street}" Placeholder="Street" />
            <Entry Text="{Binding City}" Placeholder="City" />
        </VerticalStackLayout>
    </extras:WizardStep>
    <extras:WizardStep Title="Review">
        <Label Text="Review your information..." />
    </extras:WizardStep>
</extras:Wizard>
```

### With Skip Option

```xml
<extras:Wizard>
    <extras:WizardStep Title="Required Step">
        <Label Text="This step is required" />
    </extras:WizardStep>
    <extras:WizardStep Title="Optional Step"
                       CanSkip="True"
                       IsOptional="True">
        <Label Text="This step can be skipped" />
    </extras:WizardStep>
    <extras:WizardStep Title="Final Step">
        <Label Text="You're done!" />
    </extras:WizardStep>
</extras:Wizard>
```

### Custom Indicator Style

```xml
<extras:Wizard IndicatorStyle="Circle"
               IndicatorPosition="Top"
               ShowStepNumbers="True"
               ShowStepTitles="True"
               CompletedStepColor="#4CAF50"
               ErrorStepColor="#F44336"
               AnimateTransitions="True">
    <!-- Steps -->
</extras:Wizard>
```

### Dot Indicator

```xml
<extras:Wizard IndicatorStyle="Dot"
               ShowStepTitles="False"
               AccentColor="#2196F3">
    <!-- Steps -->
</extras:Wizard>
```

### Progress Bar Style

```xml
<extras:Wizard IndicatorStyle="Progress"
               IndicatorPosition="Top">
    <!-- Steps -->
</extras:Wizard>
```

### Non-Linear Navigation

```xml
<extras:Wizard NavigationMode="NonLinear"
               StepChangedCommand="{Binding OnStepChangedCommand}">
    <extras:WizardStep Title="Step 1" />
    <extras:WizardStep Title="Step 2" />
    <extras:WizardStep Title="Step 3" />
</extras:Wizard>
```

### Custom Button Text

```xml
<extras:Wizard BackButtonText="Previous"
               NextButtonText="Continue"
               FinishButtonText="Complete"
               CancelButtonText="Exit"
               SkipButtonText="Skip This">
    <!-- Steps -->
</extras:Wizard>
```

### Code-Behind

```csharp
// Create wizard
var wizard = new Wizard
{
    NavigationMode = WizardNavigationMode.Linear,
    IndicatorStyle = StepIndicatorStyle.Circle,
    IndicatorPosition = StepIndicatorPosition.Top,
    ValidateOnNext = true,
    AnimateTransitions = true
};

// Add steps
wizard.Steps.Add(new WizardStep
{
    Title = "Welcome",
    Description = "Get started with the setup",
    Content = new Label { Text = "Welcome!" }
});

wizard.Steps.Add(new WizardStep
{
    Title = "Configure",
    Description = "Set your preferences",
    Content = CreateConfigurationView()
});

wizard.Steps.Add(new WizardStep
{
    Title = "Finish",
    Description = "Review and complete",
    Content = new Label { Text = "All done!" }
});

// Handle events
wizard.StepChanged += (sender, args) =>
{
    Console.WriteLine($"Moved from step {args.OldIndex} to {args.NewIndex}");
};

wizard.StepChanging += (sender, args) =>
{
    // Validate before leaving current step
    if (!ValidateCurrentStep())
    {
        args.Cancel = true;
    }
};

wizard.Finished += (sender, args) =>
{
    Console.WriteLine("Wizard completed!");
    SaveConfiguration();
};

wizard.Cancelled += (sender, args) =>
{
    Console.WriteLine("Wizard cancelled");
    DiscardChanges();
};

// Programmatic navigation
wizard.GoNext();
wizard.GoBack();
wizard.GoToStep(2);
wizard.SkipStep();
wizard.Finish();
wizard.Cancel();
wizard.Reset();
```

### MVVM Pattern

```csharp
// ViewModel
public class SetupWizardViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private string _email = "";

    [RelayCommand]
    private void ValidatePersonalInfo(WizardStep step)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Name is required");

        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
            errors.Add("Valid email is required");

        step.IsValid = errors.Count == 0;
        step.ValidationMessage = errors.Count > 0
            ? string.Join("; ", errors)
            : null;
    }

    [RelayCommand]
    private void OnStepChanged(WizardStepChangedEventArgs args)
    {
        if (args.NewStep != null)
        {
            LogAnalytics("wizard_step_changed", args.NewStep.Title);
        }
    }

    [RelayCommand]
    private async Task OnFinished(WizardFinishedEventArgs args)
    {
        await _settingsService.SaveUserProfileAsync(Name, Email);
        await Shell.Current.GoToAsync("//home");
    }

    [RelayCommand]
    private async Task OnCancelled(WizardFinishedEventArgs args)
    {
        await Shell.Current.GoToAsync("//home");
    }
}
```

