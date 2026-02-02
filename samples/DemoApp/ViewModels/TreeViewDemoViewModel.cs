using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DemoApp.Models;

namespace DemoApp.ViewModels;

public partial class TreeViewDemoViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<FolderItem> _folders = [];

    [ObservableProperty]
    private ObservableCollection<FolderItem> _interactiveFolders = [];

    [ObservableProperty]
    private ObservableCollection<FolderItem> _badgeFolders = [];

    [ObservableProperty]
    private ObservableCollection<FolderItem> _contextMenuFolders = [];

    [ObservableProperty]
    private ObservableCollection<FolderItem> _styledFolders = [];

    [ObservableProperty]
    private FolderItem? _selectedItem;

    [ObservableProperty]
    private bool _showCheckBoxes;

    [ObservableProperty]
    private bool _showLines;

    public TreeViewDemoViewModel()
    {
        Title = "TreeView Demo";
        InitializeSampleData();
    }

    private void InitializeSampleData()
    {
        // Basic TreeView data
        Folders = new ObservableCollection<FolderItem>(SampleData.Folders);

        // Interactive features data (separate copy for checkboxes demo)
        InteractiveFolders = new ObservableCollection<FolderItem>(CreateInteractiveFolders());

        // Badge folders (simpler structure for badge demo)
        BadgeFolders = new ObservableCollection<FolderItem>(CreateBadgeFolders());

        // Context menu folders (separate copy)
        ContextMenuFolders = new ObservableCollection<FolderItem>(CreateContextMenuFolders());

        // Styled folders (separate copy for styling demo)
        StyledFolders = new ObservableCollection<FolderItem>(CreateStyledFolders());
    }

    private static List<FolderItem> CreateInteractiveFolders()
    {
        return
        [
            new("Source Code", [
                new("src", [
                    new("Components", [
                        new("Button.cs") { Icon = "file" },
                        new("TextBox.cs") { Icon = "file" },
                        new("ListView.cs") { Icon = "file" }
                    ]),
                    new("Services", [
                        new("AuthService.cs") { Icon = "file" },
                        new("DataService.cs") { Icon = "file" }
                    ]),
                    new("Models", [
                        new("User.cs") { Icon = "file" },
                        new("Product.cs") { Icon = "file" }
                    ])
                ]),
                new("tests", [
                    new("UnitTests") { Icon = "folder" },
                    new("IntegrationTests") { Icon = "folder" }
                ])
            ]),
            new("Documentation", [
                new("API Reference") { Icon = "file" },
                new("User Guide") { Icon = "file" },
                new("Changelog") { Icon = "file" }
            ])
        ];
    }

    private static List<FolderItem> CreateBadgeFolders()
    {
        return
        [
            new("Inbox", [
                new("Unread", [
                    new("Meeting invite") { Icon = "file" },
                    new("Project update") { Icon = "file" },
                    new("Review request") { Icon = "file" }
                ]),
                new("Flagged", [
                    new("Important memo") { Icon = "file" },
                    new("Action required") { Icon = "file" }
                ])
            ]) { Icon = "folder" },
            new("Sent", [
                new("Report submission") { Icon = "file" },
                new("Status update") { Icon = "file" }
            ]) { Icon = "folder" },
            new("Drafts", [
                new("Meeting notes draft") { Icon = "file" }
            ]) { Icon = "folder" },
            new("Archive") { Icon = "folder" }
        ];
    }

    private static List<FolderItem> CreateContextMenuFolders()
    {
        return
        [
            new("Project Root", [
                new("Configuration", [
                    new("appsettings.json") { Icon = "file" },
                    new("launchSettings.json") { Icon = "file" }
                ]),
                new("Controllers", [
                    new("HomeController.cs") { Icon = "file" },
                    new("ApiController.cs") { Icon = "file" },
                    new("AuthController.cs") { Icon = "file" }
                ]),
                new("Views", [
                    new("Home") { Icon = "folder" },
                    new("Shared") { Icon = "folder" }
                ])
            ]),
            new("Resources", [
                new("Images") { Icon = "folder" },
                new("Styles") { Icon = "folder" },
                new("Fonts") { Icon = "folder" }
            ])
        ];
    }

    private static List<FolderItem> CreateStyledFolders()
    {
        return
        [
            new("Design System", [
                new("Colors", [
                    new("Primary") { Icon = "file" },
                    new("Secondary") { Icon = "file" },
                    new("Accent") { Icon = "file" }
                ]),
                new("Typography", [
                    new("Headings") { Icon = "file" },
                    new("Body Text") { Icon = "file" }
                ]),
                new("Spacing", [
                    new("Margins") { Icon = "file" },
                    new("Padding") { Icon = "file" }
                ])
            ]),
            new("Components", [
                new("Buttons") { Icon = "folder" },
                new("Forms") { Icon = "folder" },
                new("Cards") { Icon = "folder" }
            ])
        ];
    }

    partial void OnSelectedItemChanged(FolderItem? value)
    {
        if (value is not null)
            UpdateStatus($"Selected: {value.Name}");
    }
}
