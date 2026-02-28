using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DemoApp.Models;

// Country model for ComboBox and MultiSelect demos
public record Country(string Code, string Name, string FlagEmoji)
{
    public override string ToString() => Name;
}

// Employee model for DataGrid and BindingNavigator demos
public partial class Employee : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _department = string.Empty;

    [ObservableProperty]
    private decimal _salary;

    [ObservableProperty]
    private DateTime _hireDate;

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private string _email = string.Empty;
}

// Folder model for TreeView demo
public class FolderItem
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "folder";
    public ObservableCollection<FolderItem> Children { get; set; } = [];
    public bool IsExpanded { get; set; }

    public FolderItem() { }

    public FolderItem(string name, IEnumerable<FolderItem>? children = null)
    {
        Name = name;
        if (children != null)
            Children = new ObservableCollection<FolderItem>(children);
    }
}

// Product model for PropertyGrid demo
public partial class Product : ObservableObject
{
    [ObservableProperty]
    [property: Category("General")]
    [property: Description("The name of the product")]
    private string _name = string.Empty;

    [ObservableProperty]
    [property: Category("General")]
    [property: Description("A detailed description of the product")]
    private string _description = string.Empty;

    [ObservableProperty]
    [property: Category("Pricing")]
    [property: Description("The price of the product")]
    private decimal _price;

    [ObservableProperty]
    [property: Category("Inventory")]
    [property: Description("The quantity in stock")]
    private int _quantity;

    [ObservableProperty]
    [property: Category("General")]
    [property: Description("The product category")]
    private string _category = string.Empty;

    [ObservableProperty]
    [property: Category("Inventory")]
    [property: Description("Whether the product is available for sale")]
    private bool _isAvailable;

    [ObservableProperty]
    [property: Category("General")]
    [property: Description("The product release date")]
    private DateTime _releaseDate;

    [ObservableProperty]
    [property: Category("Appearance")]
    [property: Description("The theme color for the product display")]
    private Color _themeColor = Colors.Blue;
}

// Control info for gallery
public record ControlInfo(string Name, string Description, string Category, string Route, Color AccentColor);

// Static sample data
public static class SampleData
{
    public static List<Country> Countries =>
    [
        new("US", "United States", "\U0001F1FA\U0001F1F8"),
        new("GB", "United Kingdom", "\U0001F1EC\U0001F1E7"),
        new("DE", "Germany", "\U0001F1E9\U0001F1EA"),
        new("FR", "France", "\U0001F1EB\U0001F1F7"),
        new("IT", "Italy", "\U0001F1EE\U0001F1F9"),
        new("ES", "Spain", "\U0001F1EA\U0001F1F8"),
        new("JP", "Japan", "\U0001F1EF\U0001F1F5"),
        new("CN", "China", "\U0001F1E8\U0001F1F3"),
        new("KR", "South Korea", "\U0001F1F0\U0001F1F7"),
        new("BR", "Brazil", "\U0001F1E7\U0001F1F7"),
        new("CA", "Canada", "\U0001F1E8\U0001F1E6"),
        new("AU", "Australia", "\U0001F1E6\U0001F1FA"),
        new("IN", "India", "\U0001F1EE\U0001F1F3"),
        new("MX", "Mexico", "\U0001F1F2\U0001F1FD"),
        new("NL", "Netherlands", "\U0001F1F3\U0001F1F1"),
        new("SE", "Sweden", "\U0001F1F8\U0001F1EA"),
        new("NO", "Norway", "\U0001F1F3\U0001F1F4"),
        new("GR", "Greece", "\U0001F1EC\U0001F1F7"),
        new("PT", "Portugal", "\U0001F1F5\U0001F1F9"),
        new("PL", "Poland", "\U0001F1F5\U0001F1F1")
    ];

    public static List<Employee> Employees { get; } = GenerateEmployees(500);

    private static List<Employee> GenerateEmployees(int count)
    {
        var seeds = new (string Name, string Dept, decimal Salary, bool Active)[]
        {
            ("John Smith", "Engineering", 85000, true),
            ("Jane Doe", "Marketing", 72000, true),
            ("Bob Johnson", "Engineering", 95000, true),
            ("Alice Williams", "HR", 65000, true),
            ("Charlie Brown", "Finance", 78000, true),
            ("Diana Ross", "Engineering", 92000, true),
            ("Edward King", "Sales", 68000, true),
            ("Fiona Green", "Marketing", 75000, false),
            ("George Miller", "Engineering", 88000, true),
            ("Hannah White", "Finance", 82000, true),
            ("Ivan Black", "Engineering", 105000, true),
            ("Julia Adams", "HR", 62000, true),
            ("Kevin Lee", "Sales", 71000, true),
            ("Laura Martinez", "Marketing", 79000, true),
            ("Michael Chen", "Engineering", 98000, true),
            ("Nancy Taylor", "Finance", 85000, false),
            ("Oscar Wilson", "Engineering", 91000, true),
            ("Patricia Garcia", "HR", 68000, true),
            ("Quincy Robinson", "Sales", 74000, true),
            ("Rachel Clark", "Marketing", 77000, true),
        };

        var baseDate = new DateTime(2015, 1, 1);
        var random = new Random(42);
        return Enumerable.Range(1, count).Select(i =>
        {
            var s = seeds[(i - 1) % seeds.Length];
            return new Employee
            {
                Id = i,
                Name = i <= seeds.Length ? s.Name : $"{s.Name} #{i}",
                Department = s.Dept,
                Salary = s.Salary + (i * 500),
                HireDate = baseDate.AddDays(random.Next(0, 3650)),
                IsActive = s.Active,
                Email = $"{s.Name.ToLower().Replace(' ', '.')}.{i}@company.com",
            };
        }).ToList();
    }

    public static List<string> Departments => ["Engineering", "Marketing", "HR", "Finance", "Sales", "Operations", "IT", "Legal"];

    public static List<FolderItem> Folders =>
    [
        new("Documents", [
            new("Work", [
                new("Projects", [
                    new("Project Alpha") { Icon = "file" },
                    new("Project Beta") { Icon = "file" },
                    new("Project Gamma") { Icon = "file" }
                ]),
                new("Reports", [
                    new("Q1 Report") { Icon = "file" },
                    new("Q2 Report") { Icon = "file" },
                    new("Annual Review") { Icon = "file" }
                ]),
                new("Meetings") { Icon = "folder" }
            ]),
            new("Personal", [
                new("Photos", [
                    new("Vacation 2024") { Icon = "image" },
                    new("Family") { Icon = "image" }
                ]),
                new("Finance", [
                    new("Taxes") { Icon = "file" },
                    new("Budget") { Icon = "file" }
                ])
            ])
        ]),
        new("Downloads", [
            new("Software") { Icon = "folder" },
            new("Music") { Icon = "music" },
            new("Videos") { Icon = "video" }
        ]),
        new("Desktop", [
            new("Shortcuts") { Icon = "folder" },
            new("Temp Files") { Icon = "folder" }
        ])
    ];

    public static Product SampleProduct => new()
    {
        Name = "Premium Widget",
        Description = "A high-quality widget for all your needs",
        Price = 99.99m,
        Quantity = 150,
        Category = "Electronics",
        IsAvailable = true,
        ReleaseDate = new DateTime(2024, 6, 15),
        ThemeColor = Colors.Blue
    };

    public static List<string> Tags =>
    [
        "MAUI", ".NET", "C#", "Cross-platform", "Mobile",
        "Desktop", "iOS", "Android", "Windows", "macOS",
        "UI", "Controls", "Open Source", "MIT License"
    ];

    public static List<ControlInfo> AllControls =>
    [
        // Input Controls
        new("ComboBox", "Dropdown with search, icons, and custom templates", "Input", "ComboBoxDemo", Color.FromArgb("#2196F3")),
        new("MultiSelectComboBox", "Multi-select dropdown with chips display", "Input", "MultiSelectDemo", Color.FromArgb("#2196F3")),
        new("NumericUpDown", "Numeric input with increment/decrement buttons", "Input", "NumericUpDownDemo", Color.FromArgb("#2196F3")),
        new("TokenEntry", "Tag/token input with auto-complete", "Input", "TokenEntryDemo", Color.FromArgb("#2196F3")),
        new("RichTextEditor", "Full-featured rich text editor with toolbar", "Input", "RichTextEditorDemo", Color.FromArgb("#2196F3")),

        // Selection Controls
        new("Calendar", "Date picker with single, multiple, and range selection", "Selection", "CalendarDemo", Color.FromArgb("#4CAF50")),
        new("RangeSlider", "Dual-thumb slider for range selection", "Selection", "RangeSliderDemo", Color.FromArgb("#4CAF50")),
        new("Rating", "Star rating control with customizable icons", "Selection", "RatingDemo", Color.FromArgb("#4CAF50")),

        // Layout Controls
        new("Accordion", "Collapsible sections with single/multiple expand modes", "Layout", "AccordionDemo", Color.FromArgb("#FF9800")),
        new("TreeView", "Hierarchical data display with expand/collapse", "Layout", "TreeViewDemo", Color.FromArgb("#FF9800")),
        new("Wizard", "Step-by-step wizard with validation", "Layout", "WizardDemo", Color.FromArgb("#FF9800")),

        // Data Controls
        new("DataGridView", "Full-featured data grid with sorting, filtering, editing", "Data", "DataGridDemo", Color.FromArgb("#9C27B0")),
        new("PropertyGrid", "Object property editor with categories", "Data", "PropertyGridDemo", Color.FromArgb("#9C27B0")),
        new("BindingNavigator", "Record navigation with add/delete/save", "Data", "BindingNavigatorDemo", Color.FromArgb("#9C27B0")),

        // Navigation Controls
        new("Breadcrumb", "Navigation breadcrumb trail", "Navigation", "BreadcrumbDemo", Color.FromArgb("#00BCD4"))
    ];
}
