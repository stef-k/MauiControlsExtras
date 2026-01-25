using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DemoApp.Models;

// Country model for ComboBox and MultiSelect demos
public record Country(string Code, string Name, string FlagEmoji)
{
    public override string ToString() => Name;
}

// Employee model for DataGrid and BindingNavigator demos
public class Employee : INotifyPropertyChanged
{
    private int _id;
    private string _name = string.Empty;
    private string _department = string.Empty;
    private decimal _salary;
    private DateTime _hireDate;
    private bool _isActive;
    private string _email = string.Empty;

    public int Id
    {
        get => _id;
        set { _id = value; OnPropertyChanged(nameof(Id)); }
    }

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(nameof(Name)); }
    }

    public string Department
    {
        get => _department;
        set { _department = value; OnPropertyChanged(nameof(Department)); }
    }

    public decimal Salary
    {
        get => _salary;
        set { _salary = value; OnPropertyChanged(nameof(Salary)); }
    }

    public DateTime HireDate
    {
        get => _hireDate;
        set { _hireDate = value; OnPropertyChanged(nameof(HireDate)); }
    }

    public bool IsActive
    {
        get => _isActive;
        set { _isActive = value; OnPropertyChanged(nameof(IsActive)); }
    }

    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(nameof(Email)); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
public class Product : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    private decimal _price;
    private int _quantity;
    private string _category = string.Empty;
    private bool _isAvailable;
    private DateTime _releaseDate;
    private Color _themeColor = Colors.Blue;

    [Category("General")]
    [Description("The name of the product")]
    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(nameof(Name)); }
    }

    [Category("General")]
    [Description("A detailed description of the product")]
    public string Description
    {
        get => _description;
        set { _description = value; OnPropertyChanged(nameof(Description)); }
    }

    [Category("Pricing")]
    [Description("The price of the product")]
    public decimal Price
    {
        get => _price;
        set { _price = value; OnPropertyChanged(nameof(Price)); }
    }

    [Category("Inventory")]
    [Description("The quantity in stock")]
    public int Quantity
    {
        get => _quantity;
        set { _quantity = value; OnPropertyChanged(nameof(Quantity)); }
    }

    [Category("General")]
    [Description("The product category")]
    public string Category
    {
        get => _category;
        set { _category = value; OnPropertyChanged(nameof(Category)); }
    }

    [Category("Inventory")]
    [Description("Whether the product is available for sale")]
    public bool IsAvailable
    {
        get => _isAvailable;
        set { _isAvailable = value; OnPropertyChanged(nameof(IsAvailable)); }
    }

    [Category("General")]
    [Description("The product release date")]
    public DateTime ReleaseDate
    {
        get => _releaseDate;
        set { _releaseDate = value; OnPropertyChanged(nameof(ReleaseDate)); }
    }

    [Category("Appearance")]
    [Description("The theme color for the product display")]
    public Color ThemeColor
    {
        get => _themeColor;
        set { _themeColor = value; OnPropertyChanged(nameof(ThemeColor)); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

    public static List<Employee> Employees =>
    [
        new() { Id = 1, Name = "John Smith", Department = "Engineering", Salary = 85000, HireDate = new DateTime(2020, 3, 15), IsActive = true, Email = "john.smith@company.com" },
        new() { Id = 2, Name = "Jane Doe", Department = "Marketing", Salary = 72000, HireDate = new DateTime(2019, 7, 22), IsActive = true, Email = "jane.doe@company.com" },
        new() { Id = 3, Name = "Bob Johnson", Department = "Engineering", Salary = 95000, HireDate = new DateTime(2018, 1, 10), IsActive = true, Email = "bob.johnson@company.com" },
        new() { Id = 4, Name = "Alice Williams", Department = "HR", Salary = 65000, HireDate = new DateTime(2021, 5, 3), IsActive = true, Email = "alice.williams@company.com" },
        new() { Id = 5, Name = "Charlie Brown", Department = "Finance", Salary = 78000, HireDate = new DateTime(2020, 9, 18), IsActive = true, Email = "charlie.brown@company.com" },
        new() { Id = 6, Name = "Diana Ross", Department = "Engineering", Salary = 92000, HireDate = new DateTime(2017, 11, 5), IsActive = true, Email = "diana.ross@company.com" },
        new() { Id = 7, Name = "Edward King", Department = "Sales", Salary = 68000, HireDate = new DateTime(2022, 2, 14), IsActive = true, Email = "edward.king@company.com" },
        new() { Id = 8, Name = "Fiona Green", Department = "Marketing", Salary = 75000, HireDate = new DateTime(2019, 4, 28), IsActive = false, Email = "fiona.green@company.com" },
        new() { Id = 9, Name = "George Miller", Department = "Engineering", Salary = 88000, HireDate = new DateTime(2020, 6, 12), IsActive = true, Email = "george.miller@company.com" },
        new() { Id = 10, Name = "Hannah White", Department = "Finance", Salary = 82000, HireDate = new DateTime(2018, 8, 30), IsActive = true, Email = "hannah.white@company.com" },
        new() { Id = 11, Name = "Ivan Black", Department = "Engineering", Salary = 105000, HireDate = new DateTime(2015, 3, 20), IsActive = true, Email = "ivan.black@company.com" },
        new() { Id = 12, Name = "Julia Adams", Department = "HR", Salary = 62000, HireDate = new DateTime(2023, 1, 8), IsActive = true, Email = "julia.adams@company.com" },
        new() { Id = 13, Name = "Kevin Lee", Department = "Sales", Salary = 71000, HireDate = new DateTime(2021, 7, 15), IsActive = true, Email = "kevin.lee@company.com" },
        new() { Id = 14, Name = "Laura Martinez", Department = "Marketing", Salary = 79000, HireDate = new DateTime(2019, 10, 25), IsActive = true, Email = "laura.martinez@company.com" },
        new() { Id = 15, Name = "Michael Chen", Department = "Engineering", Salary = 98000, HireDate = new DateTime(2016, 12, 1), IsActive = true, Email = "michael.chen@company.com" },
        new() { Id = 16, Name = "Nancy Taylor", Department = "Finance", Salary = 85000, HireDate = new DateTime(2018, 5, 17), IsActive = false, Email = "nancy.taylor@company.com" },
        new() { Id = 17, Name = "Oscar Wilson", Department = "Engineering", Salary = 91000, HireDate = new DateTime(2019, 2, 8), IsActive = true, Email = "oscar.wilson@company.com" },
        new() { Id = 18, Name = "Patricia Garcia", Department = "HR", Salary = 68000, HireDate = new DateTime(2020, 11, 30), IsActive = true, Email = "patricia.garcia@company.com" },
        new() { Id = 19, Name = "Quincy Robinson", Department = "Sales", Salary = 74000, HireDate = new DateTime(2021, 4, 5), IsActive = true, Email = "quincy.robinson@company.com" },
        new() { Id = 20, Name = "Rachel Clark", Department = "Marketing", Salary = 77000, HireDate = new DateTime(2022, 8, 22), IsActive = true, Email = "rachel.clark@company.com" }
    ];

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
        new("MaskedEntry", "Text input with masks for phone, date, SSN, etc.", "Input", "MaskedEntryDemo", Color.FromArgb("#2196F3")),
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
