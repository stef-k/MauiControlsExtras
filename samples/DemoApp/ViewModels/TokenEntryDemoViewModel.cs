using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DemoApp.Models;

namespace DemoApp.ViewModels;

public partial class TokenEntryDemoViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<string> _basicTokens = [];

    [ObservableProperty]
    private ObservableCollection<string> _emailTokens = [];

    [ObservableProperty]
    private ObservableCollection<string> _tagTokens = [];

    [ObservableProperty]
    private ObservableCollection<string> _skillTokens = [];

    public List<string> AvailableTags => SampleData.Tags;

    public List<string> AvailableSkills =>
    [
        "C#", "JavaScript", "Python", "Java", "TypeScript",
        "React", "Angular", "Vue.js", ".NET", "Node.js",
        "SQL", "MongoDB", "Docker", "Kubernetes", "AWS",
        "Azure", "Git", "REST API", "GraphQL", "Microservices"
    ];

    public TokenEntryDemoViewModel()
    {
        Title = "TokenEntry Demo";

        // Initialize with some default tokens
        TagTokens = new ObservableCollection<string>(["MAUI", ".NET", "C#"]);
    }

    [RelayCommand]
    private void TokenAdded(string token)
    {
        UpdateStatus($"Token added: {token}");
    }

    [RelayCommand]
    private void TokenRemoved(string token)
    {
        UpdateStatus($"Token removed: {token}");
    }
}
