# Quick Start

This guide will help you get started with MAUI Controls Extras in just a few minutes.

## Step 1: Add the Namespace

In your XAML file, add the namespace declaration:

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:extras="clr-namespace:MauiControlsExtras.Controls;assembly=MauiControlsExtras"
             x:Class="YourApp.MainPage">
```

## Step 2: Create Your Data Model

```csharp
public class Country
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string FlagPath { get; set; }
}
```

## Step 3: Set Up Your ViewModel

```csharp
public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<Country> Countries { get; } = new()
    {
        new Country { Code = "US", Name = "United States", FlagPath = "flags/us.png" },
        new Country { Code = "UK", Name = "United Kingdom", FlagPath = "flags/uk.png" },
        new Country { Code = "CA", Name = "Canada", FlagPath = "flags/ca.png" },
        // ...
    };

    private Country? _selectedCountry;
    public Country? SelectedCountry
    {
        get => _selectedCountry;
        set
        {
            _selectedCountry = value;
            OnPropertyChanged();
        }
    }

    // INotifyPropertyChanged implementation...
}
```

## Step 4: Use the ComboBox

### Basic Usage

```xml
<extras:ComboBox ItemsSource="{Binding Countries}"
                 SelectedItem="{Binding SelectedCountry, Mode=TwoWay}"
                 DisplayMemberPath="Name"
                 Placeholder="Select a country..." />
```

### With Icons

```xml
<extras:ComboBox ItemsSource="{Binding Countries}"
                 SelectedItem="{Binding SelectedCountry, Mode=TwoWay}"
                 DisplayMemberPath="Name"
                 IconMemberPath="FlagPath"
                 Placeholder="Select a country..."
                 VisibleItemCount="6" />
```

### With Value Binding

```xml
<extras:ComboBox ItemsSource="{Binding Countries}"
                 SelectedValue="{Binding SelectedCountryCode, Mode=TwoWay}"
                 DisplayMemberPath="Name"
                 ValueMemberPath="Code"
                 Placeholder="Select a country..." />
```

## Step 5: Handle Selection Changes

You can handle selection changes in two ways:

### Via Data Binding

The `SelectedItem` or `SelectedValue` property will update automatically.

### Via Event

```xml
<extras:ComboBox x:Name="countryComboBox"
                 SelectionChanged="OnCountrySelectionChanged"
                 ... />
```

```csharp
private void OnCountrySelectionChanged(object sender, object? selectedItem)
{
    if (selectedItem is Country country)
    {
        // Handle selection
        Debug.WriteLine($"Selected: {country.Name}");
    }
}
```

> **Note**: Event handlers are shown here for simplicity. For production apps, we recommend using commands in your ViewModel with `SelectionChangedCommand` binding instead.

## Next Steps

- Explore the [ComboBox documentation](controls/combobox.md) for advanced features
- Check the [API Reference](api/combobox.md) for all available properties and methods

## Run The Demo App (Android)

The sample app in `samples/DemoApp` showcases all controls and runs on Android:

```bash
dotnet build samples/DemoApp/DemoApp.csproj -f net10.0-android
dotnet build samples/DemoApp/DemoApp.csproj -t:Run -f net10.0-android
```
