using CommunityToolkit.Mvvm.ComponentModel;

namespace DemoApp.ViewModels;

public partial class AccordionDemoViewModel : BaseViewModel
{
    [ObservableProperty]
    private int _singleExpandIndex = 0;

    [ObservableProperty]
    private bool _section1Expanded = true;

    [ObservableProperty]
    private bool _section2Expanded;

    [ObservableProperty]
    private bool _section3Expanded;

    public AccordionDemoViewModel()
    {
        Title = "Accordion Demo";
    }

    partial void OnSingleExpandIndexChanged(int value)
    {
        UpdateStatus($"Expanded section index: {value}");
    }

    partial void OnSection1ExpandedChanged(bool value)
    {
        if (value) UpdateStatus("Section 1 expanded");
    }

    partial void OnSection2ExpandedChanged(bool value)
    {
        if (value) UpdateStatus("Section 2 expanded");
    }

    partial void OnSection3ExpandedChanged(bool value)
    {
        if (value) UpdateStatus("Section 3 expanded");
    }
}
