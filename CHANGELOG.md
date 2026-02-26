# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed

- **Theming**: Controls now respond to MAUI `RequestedThemeChanged`, fixing theme-dependent properties not updating when toggling `UserAppTheme` at runtime (#258)

## [3.1.1] - 2026-02-26

### Added

- **Docs**: Centralized AOT & Trimming guide (`docs/aot.md`) with Func-based property reference table, code examples, and links to per-control sections

## [3.1.0] - 2026-02-26

### Added

- **ComboBox**: `SelectedIndex` bindable property for position-based selection (#243)
- **MultiSelectComboBox**: `SelectedIndices` bindable property for position-based multi-selection (#244)

### Removed

- **DataGrid**: Removed `DataGridColumn.SortIndicator` public property — sort indicator is now managed internally (#252)

### Fixed

- **AOT/Trimming**: Replaced DataGrid sort indicator `SetBinding` + `SortIndicatorConverter` with direct `PropertyChanged` subscription to eliminate CLR-property binding hazard (#240)
- **AOT/Trimming**: Added `[DynamicDependency]` annotations on all base class `Effective*` CLR properties (`StyledControlBase`, `TextStyledControlBase`, `ListStyledControlBase`, `NavigationControlBase`, `HeaderedControlBase`, `AnimatedControlBase`) to prevent getter trimming (#240)
- **AOT/Trimming**: Added `[DynamicDependency]` annotations for `CurrentBorderColor` and `EffectiveStepIndicatorBackgroundColor` CLR properties on Accordion, Wizard, Breadcrumb, NumericUpDown, Rating, and TokenEntry (#240)
- **AOT/Trimming**: Added `[DynamicDependency]` on `PropertyEditorBase.CreateValueBinding` for `PropertyItem.Value` binding (#240)
- **AOT/Trimming**: Added `[Preserve(AllMembers = true)]` on `InvertedBoolConverter`, `MauiAssetImageConverter`, and `FuncDisplayConverter` to protect against trimming (#240)
- **AOT/Trimming**: Added `x:DataType="x:String"` to TokenEntry suggestion `DataTemplate` for compiled bindings (#240)
- **AOT/Trimming**: Added XML doc `<remarks>` warnings on `DisplayMemberPath` and `IconMemberPath` (ComboBox, MultiSelectComboBox) advising AOT-safe alternatives (#240)
- **AOT/Trimming**: Added `[DynamicDependency]` annotations for CLR properties on RangeSlider, DataGridView, BindingNavigator, RichTextEditor, and PropertyGrid (#251)
- **Breadcrumb**: Removed dead `EffectiveBackgroundColor` binding targeting a non-existent property (#253)
- **DataGrid**: Fixed virtualization crash on Windows debug builds caused by async handler re-attachment race during row recycling (#237)
- **DataGrid**: Page size picker text is now horizontally centered (#239)
- **DataGrid**: Header text color now reacts to `ForegroundColor` property changes without requiring a full refresh (#238)
- **ComboBox**: Selected item now displays correctly when `DisplayMemberPath`/`DisplayMemberFunc` is set after `SelectedItem` (#235)
- **ComboBox**: Dropdown now highlights the currently selected item when opened instead of always highlighting the first item (#236)
- **ComboBox**: `SelectItemByIndex(-1)` no longer triggers `ClearCommand` and popup close as side effects (#246)

## [3.0.0] - 2026-02-25

### Added

- **AOT/Trimming**: Enabled `IsTrimmable`, `IsAotCompatible`, and `EnableTrimAnalyzer` in the library project
- **ComboBox**: `DisplayMemberFunc`, `ValueMemberFunc`, `IconMemberFunc` — AOT-safe alternatives to string-based property paths
- **MultiSelectComboBox**: `DisplayMemberFunc` — AOT-safe alternative to `DisplayMemberPath`
- **TreeView**: `DisplayMemberFunc`, `ChildrenFunc`, `IconMemberFunc`, `IsExpandedFunc`, `HasChildrenFunc` — AOT-safe alternatives to string-based property paths
- **DataGridColumn**: `CellValueFunc`, `CellValueSetter` — AOT-safe alternatives to reflection via `PropertyPath`
- **DataGridComboBoxColumn**: `DisplayMemberFunc`, `SelectedValueFunc` — AOT-safe alternatives to string-based property paths
- **PropertyGrid**: `RegisterMetadata<T>()` / `RegisterMetadata(Type, ...)` for AOT-safe property discovery via `PropertyMetadataRegistry`
- **Helpers**: `PropertyAccessor` — centralized, cached reflection helper with `[RequiresUnreferencedCode]` annotations (replaces scattered `GetPropertyValue()` methods)
- **Helpers**: `PropertyMetadataRegistry` / `PropertyMetadataEntry` for registering AOT-safe property metadata

### Changed

- **Breaking:** **PropertyGrid**: `PropertyItem.PropertyInfo` changed from `public` to `internal` — use the `GetValue`/`SetValue` methods or register metadata via `PropertyMetadataRegistry` instead
- **Breaking:** **PropertyGrid**: `PropertyItem` constructors changed to `internal` — use `PropertyMetadataRegistry` for AOT-safe property item creation

## [2.1.8] - 2026-02-24

### Fixed

- **DataGrid**: Column header text and icon colors now update immediately on Light↔Dark theme switch (#231)
- **DataGrid**: Pagination grid layout fixed — lastPageButton no longer overlaps nextPageButton (#231)
- **DataGrid**: Page-size picker centered correctly (#231)
- **DataGrid**: Data cell text colors refresh on theme change — non-selected cells no longer retain stale colors (#231)

## [2.1.7] - 2026-02-23

### Fixed

- **DataGrid**: Filter popup now correctly preserves the "(Empty)" checkbox state when reopening after filtering null values (#217)
  - Null cell values are represented internally via a sentinel object for `HashSet`/`Dictionary` compatibility
  - Select All / Clear actions now include null entries
- **DataGrid**: Corrected `ShouldSuppressContextMenu` doc comment to accurately describe per-cell (not per-grid) suppression behavior

## [2.1.6] - 2026-02-23

### Fixed

- **DataGrid**: Context menu long-press now works reliably on all platforms (#223)
  - iOS/Android: Replaced unreliable `PanGestureRecognizer` timer with native long-press handlers (`UILongPressGestureRecognizer` / `View.LongClick`)
  - Windows: Added `Holding` event for touch long-press alongside existing right-click support
  - macOS: Added `UILongPressGestureRecognizer` alongside existing secondary-click support
  - Context menu is suppressed for all edit control types when a cell is in edit mode, simplifying the previous per-type approach

## [2.1.5] - 2026-02-23

### Fixed

- **DataGrid**: Cell editing now works when virtualization is active (#222, #227)
  - Tap-to-edit and double-tap-to-edit correctly resolve the data item from virtualized rows
  - Edit commit updates the underlying data source even when rows are recycled

## [2.1.4] - 2026-02-23

### Fixed

- **DataGrid**: `RefreshData()` and `ItemsSource` reassignment now visually update the grid in default mode (no pagination, no virtualization) (#221)
  - Added explicit `InvalidateMeasure()` on data grids after row rebuild to ensure layout re-measurement

## [2.1.3] - 2026-02-23

### Fixed

- **DataGrid**: Filter icon touch target enlarged to meet 44×44pt minimum (#220)
  - Icon wrapped in transparent `Border` with `MinimumWidthRequest = 44` / `MinimumHeightRequest = 44`
  - `TapGestureRecognizer` attached to the wrapper for full-area tap detection
  - Icon `FontSize` increased from 12 to 14 for improved legibility

## [2.1.2] - 2026-02-23

### Fixed

- **DataGrid**: Filter popup now shows cascading/progressive values — only distinct values from the currently filtered dataset are shown when multiple column filters are active (#219)
  - When Column A is filtered, Column B's popup shows only values present in filtered rows (Excel-like behavior)
  - A column's own filter is excluded so users can still see and modify their previous selection

## [2.1.1] - 2026-02-23

### Fixed

- **ComboBox**: Software keyboard no longer appears on iOS/Android when `IsSearchVisible="False"` (#216)
  - Hidden keyboard-capture entry focus is now restricted to desktop platforms (`WINDOWS` / `MACCATALYST`) via preprocessor guard
  - No behavioral change on Windows or macOS — keyboard navigation continues to work when search is hidden

## [2.1.0] - 2026-02-22

### Added

- **ComboBox**: Anchor-based popup placement for standalone `PopupMode` (#213)
  - Self-hosting fallback: when no external handler subscribes to `PopupRequested`, the ComboBox automatically shows an anchored overlay popup
  - New `PopupPlacement` property (`Auto`, `Bottom`, `Top`) for preferred popup positioning
  - New `PopupOverlayHelper` internal helper for reusable anchor-based overlay logic
  - `ComboBoxPopupContent` gains `AnchorView`, `ShowAnchored()`, and `Hide()` for manual usage
  - `ComboBoxPopupRequestEventArgs` gains `PreferredPlacement` property (backwards compatible)

## [2.0.0] - 2026-02-21

### Added

- **MVVM Command Parity**: Added missing command/command-parameter pairs for user actions that previously had event-only coverage
  - `Wizard`: `StepChangingCommand`, `FinishingCommand`, `CancellingCommand`
  - `ComboBox`: `PopupRequestedCommand`
  - `Calendar`: `DateSelectingCommand`
  - `PropertyGrid`: `PropertyChangingCommand`
  - `DataGridView`: `ColumnResizingCommand`, `ExportingCommand`, `ContextMenuItemsOpeningCommand`
- **Documentation**: Added desktop+mobile screenshot pairs to all control docs, `README.md`, and docs home
  - Mobile image coverage is complete for 15/15 controls

### Fixed

- **Architecture Conformance**: `StyledControlBase` now auto-attaches `KeyboardBehavior` for controls implementing `IKeyboardNavigable`
  - Ensures keyboard input is wired consistently across interactive controls inheriting styled bases
  - `DataGridView` now configures the shared behavior (`HandleTabKey = true`) instead of adding a duplicate behavior
- **Theming Lifecycle**: `StyledControlBase` now safely manages theme event subscription across handler detach/reattach
- **Theme Notifications**: `AnimatedControlBase` now raises `PropertyChanged` for effective animation properties on theme changes
- **Base Class Alignment**: `AccordionItem`, `WizardStep`, `ComboBoxPopupContent`, and `DataGridFilterPopup` now inherit from `StyledControlBase`
- **XAML Convention**: Added `x:Name=\"thisControl\"` to `Accordion` and `Wizard` root elements
- **PropertyGrid**: Added cancelable pre-change pipeline (`PropertyItem.ValueChanging`) used by `PropertyGrid.PropertyValueChanging`/`PropertyChangingCommand`
- **Documentation**: Reconciled architecture doc drift (default corner radius/border thickness, removed undocumented `InfoColor`/`FocusBackgroundColor` references)
- **Demo Docs**: Added Android demo run steps in `README.md` and `docs/quickstart.md`

### Known Issues

- **Clipboard**: Mobile clipboard bridge to fire `IClipboardSupport` commands when users perform Copy/Cut/Paste via native context menus on Android and iOS (#189)
  - Android: Intercepts `ActionMode` callbacks on `AppCompatEditText` to detect clipboard actions
  - iOS/Mac Catalyst: Observes `UIPasteboard.ChangedNotification` and text changes to detect operations
  - Affected controls: ComboBox, MultiSelectComboBox, NumericUpDown

### Added

- **Android Back Button**: Back button now closes open ComboBox/MultiSelectComboBox dropdowns and context menus (#175)
  - Stack-based (LIFO) handling supports nested popups — most recently opened closes first
  - No impact on other platforms; Escape key behavior unchanged
- **Hover**: Reusable `HoverBehavior` for desktop hover feedback via `PointerGestureRecognizer` (#171)
  - Applied to NumericUpDown buttons, BindingNavigator buttons, Calendar navigation and day cells, Rating icons
  - Theme-aware: defaults to `ControlsTheme.HoverColor`, overridable per-instance
  - No-op on touch-only platforms (Android/iOS)
- **ComboBox**: `IContextMenuSupport` interface with long-press and right-click context menus (#176)
- **MultiSelectComboBox**: `IContextMenuSupport` interface with long-press and right-click context menus (#176)
- **DataGrid**: `IContextMenuSupport` interface implementation on DataGridView (#162)
  - Explicit `IContextMenuSupport.ContextMenuOpening` event (avoids naming conflict with legacy event)
  - `ShowContextMenu(Point?)` overload resolves focused cell context
- **Clipboard**: `IClipboardSupport` implemented on ComboBox, MultiSelectComboBox, and NumericUpDown (#163)
  - `CanCopy`, `CanCut`, `CanPaste` state properties reflect current control state
  - `Copy()`, `Cut()`, `Paste()` programmatic methods delegate to underlying text input
  - `CopyCommand`, `CutCommand`, `PasteCommand` bindable properties for MVVM
  - Keyboard shortcuts Ctrl+C, Ctrl+X, Ctrl+V registered on all four controls
  - NumericUpDown respects `IsReadOnly` for `CanCut`/`CanPaste`
- **Tests**: Per-control command execution and event raising tests covering AccordionItem, WizardStep, BreadcrumbItem, ContextMenuItem, DataGridColumn, PropertyItem, ToolbarConfig, and all EventArgs (#187)
- **Tests**: Base class and theme system test coverage for `StyledControlBase`, `TextStyledControlBase`, `HeaderedControlBase`, `NavigationControlBase`, `ListStyledControlBase`, `AnimatedControlBase`, `ControlsTheme`, and `MauiControlsExtrasTheme` (#165)
- **Focus**: Focus border visuals for keyboard navigation on Rating, Accordion, Breadcrumb, and Wizard (#169)
  - `CurrentBorderColor` property switches between `EffectiveFocusBorderColor` (focused) and `EffectiveBorderColor` (unfocused)
  - Rating: tapping a star also shows the focus ring
- **NumericUpDown**: Mouse wheel support to increment/decrement value when focused (#168)
- **Rating**: Mouse wheel support to adjust rating when focused (#168)
- **RangeSlider**: Mouse wheel support to adjust active thumb when focused (#168)
- **RichTextEditor**: `ISelectable` interface implementation (#164)
  - `HasSelection`, `IsAllSelected`, `SupportsMultipleSelection` state properties from cached JS bridge state
  - `SelectAll()`, `ClearSelection()`, `GetSelection()`, `SetSelection()` programmatic methods
  - `SelectAllCommand`, `ClearSelectionCommand` bindable properties for MVVM
  - Explicit `ISelectable.SelectionChanged` event (avoids conflict with existing `RichTextSelectionChangedEventArgs` event)
  - Ctrl+A keyboard shortcut for Select All
- **CommandParameters**: 89 missing `{Action}CommandParameter` bindable properties added across 8 controls (#161)
  - DataGridView (26), Accordion (8), AccordionItem (2), Calendar (8), Breadcrumb (8), Wizard (7), WizardStep (3), BindingNavigator (9), PropertyGrid (6), RichTextEditor (12)

### Fixed

- **Keyboard**: Arrow key names now use `"Arrow*"` convention consistently across all controls (#174)
  - Fixes broken arrow key navigation in TreeView, DataGrid, ComboBox, MultiSelectComboBox, NumericUpDown, Rating, and TokenEntry
  - Aligns `GetMacKeyCommands` in `KeyboardBehavior` with the same naming convention
- **Touch Targets**: Increased touch targets to meet 44dp minimum on TokenEntry, TreeView, and Calendar (#172)
- **Theme**: Derived base classes now notify Effective properties on theme change (#158)
  - `TextStyledControlBase`, `HeaderedControlBase`, `NavigationControlBase`, `ListStyledControlBase` override `OnThemeChanged`
- **Wizard**: Resolved `ContentProperty` conflict preventing render by building visual tree in code (#150)
- **ItemTemplate**: Added missing `ItemTemplate` BindableProperty to ComboBox and MultiSelectComboBox; fixed demo page crashes from invalid properties

### Changed

- **Wizard**: Now inherits from `NavigationControlBase` instead of `HeaderedControlBase` (#103)
  - Adds: `ActiveColor`, `InactiveColor`, `VisitedColor`, `DisabledNavigationColor`, `ActiveBackgroundColor`, `ShowNavigationIndicator`, `NavigationIndicatorColor`, `NavigationIndicatorThickness`
  - Adds: `StepIndicatorBackgroundColor`, `StepIndicatorPadding`, `StepTitleFontSize`, `StepTitleFontAttributes`
  - **Breaking**: `CompletedStepColor` removed, use `VisitedColor` instead
  - **Breaking**: `ErrorStepColor` default fallback changed from `Colors.Red` to theme `ErrorColor`
  - **Breaking**: Header properties (`HeaderBackgroundColor`, `HeaderPadding`, `HeaderFontSize`, etc.) no longer inherited; use new `StepIndicator*` / `StepTitle*` properties
- **DemoApp**: Wizard demo now validates terms acceptance before finishing, showing a warning if unchecked

### Added

- **Breadcrumb**: `NavigatingCommand` and `NavigatingCommandParameter` bindable properties for MVVM support on the `Navigating` event (#160)
- **TokenEntry**: Clipboard support with `IClipboardSupport` and `IContextMenuSupport` interface implementations (#109)
  - Copy/Cut/Paste operations for selected tokens
  - Keyboard shortcuts (Ctrl+C/X/V on Windows, ⌘C/X/V on Mac)
  - Right-click context menu on desktop, long-press on mobile
  - Visual token selection with distinct styling (bold text, accent border)
  - `PasteDelimiters` property for configurable clipboard text splitting
  - `Copying`, `Cutting`, `Pasting`, `Pasted` events with cancellation support
  - `CopyCommand`, `CutCommand`, `PasteCommand` for MVVM
  - Paste validates tokens against MaxTokens, AllowDuplicates, MaxTokenLength, and ValidationFunc
- **TreeView**: Context menu support with `IContextMenuSupport` interface implementation
  - `ShowDefaultContextMenu` property to enable built-in menu items (Expand, Collapse, Expand All, Collapse All)
  - `ContextMenuItems` collection for custom menu items
  - `ContextMenuOpening` event for dynamic menu customization
  - `ShowContextMenu()` and `ShowContextMenuAsync()` methods for programmatic display
  - Default menu includes "Select All Children" / "Deselect All Children" when checkboxes enabled
  - Platform support: Windows (right-click), macOS (secondary click), mobile (long-press)
- **TreeViewContextMenuOpeningEventArgs**: Event args with `Node`, `DataItem`, `Level`, `IsLeafNode`, `ParentNode` properties

### Changed

- **DemoApp**: TreeViewDemoPage now showcases all TreeView features
  - Basic TreeView with icons and selection status display
  - Interactive features section with checkbox/lines toggles and expand/collapse buttons
  - Custom ItemTemplate section with badge counts
  - Context menu demonstration with custom actions
  - Styling options with accent color and border width pickers

### Fixed

- **DataGridView**: Column headers and data columns now align correctly on initial load (#98)
  - Auto-width columns previously measured independently causing misalignment
  - Added automatic column width synchronization after layout completes
- **TreeView**: Fixed multiple issues with selection and display (#99)
  - Selection background now uses theme-aware colors (dark theme compatible)
  - Fixed double selection caused by CollectionView's native selection conflicting with manual selection
  - Fixed items disappearing during expand/collapse by using DisplayText binding instead of Content views
  - Added TextColor property to TreeViewNode for proper text coloring when selected

### Changed

- **Calendar**: Now inherits from `HeaderedControlBase` instead of `StyledControlBase` (#96)
  - Adds: `HeaderBackgroundColor`, `HeaderTextColor`, `HeaderFontSize`, `HeaderFontAttributes`, `HeaderFontFamily`, `HeaderPadding`
  - Month/year navigation header can now be styled via properties
- **Accordion**: Now inherits from `HeaderedControlBase` instead of `StyledControlBase` (#95)
  - Adds: `HeaderFontAttributes`, `HeaderFontFamily`, `HeaderHeight`, `HeaderBorderColor`, `HeaderBorderThickness`
  - **Breaking**: Default `HeaderFontSize` changed from 14 to 16
  - **Breaking**: Default `HeaderPadding` changed from (12,10) to (12,8)
  - **Breaking**: Headers are now bold by default (`HeaderFontAttributes = Bold`)

### Added

- **ComboBox**: `IsSearchVisible` property to show/hide the search input in dropdowns (#91)
- **Context Menu System**: Platform-specific native context menu support with `IContextMenuSupport` interface
  - Windows: MenuFlyout with FontIcon support
  - macOS: UIMenu via UIContextMenuInteraction
  - iOS: UIAlertController action sheet
  - Android: PopupMenu
- **DataGridView**: `ContextMenuItems` collection for custom menu items in XAML or code
- **DataGridView**: `ContextMenuItemsOpening` event for dynamic menu customization
- **DataGridView**: `ShowContextMenuAsync()` method for programmatic context menu display
- **ComboBox**: `PopupMode` property for external popup handling in constrained containers
- **ComboBox**: `PopupRequested` event for parent container popup management
- **ComboBox**: `ComboBoxPopupContent` control for rendering popup overlays
- **DataGridView**: ComboBoxColumn now uses library ComboBox with filtering support (#84)
- **DataGridView**: DatePickerColumn for date editing with native DatePicker (#61)
- **DataGridView**: TimePickerColumn for time editing with native TimePicker (#62)
- **DataGridView**: Sort indicator (⇅) visible when column is sortable (#69)
- **DataGridView**: ESC key cancels edit on Windows desktop (#68)
- **DataGridView**: F2 key enters edit mode on Windows desktop (#74)
- **RichTextEditor**: Dark theme support with dynamic switching (#40)
- **RichTextEditor**: Local/bundled Quill.js support for offline use (#37)
- **Calendar**: Date picker control with single, multiple, and range selection (#16)
- **Breadcrumb**: Hierarchical navigation control (#15)
- **Accordion**: Expandable/collapsible sections control (#14)
- **BindingNavigator**: Data navigation toolbar (#13)
- **Wizard**: Step-by-step wizard/stepper control (#12)
- **PropertyGrid**: Property editor similar to Visual Studio (#11)
- **RichTextEditor**: WYSIWYG HTML/Markdown editor (#10)
- **DataGridView**: Enterprise-grade data grid with sorting, filtering, grouping (#26)
- **Rating**: Star rating control (#7)
- **TreeView**: Hierarchical tree view control (#6)
- **RangeSlider**: Dual-thumb range slider (#5)
- **TokenEntry**: Tag/token input control (#4)
- **MultiSelectComboBox**: Multi-select dropdown (#2)
- **NumericUpDown**: Numeric spinner control (#1)

### Changed

- **DataGridView**: Entry edit mode now allows native TextBox context menu (Cut/Copy/Paste/Select All)
- **DataGridView**: Entry edit commits are now deterministic - commit on Enter, Escape to cancel, or clicking another cell (removed time-based Unfocused handler)
- All controls now support keyboard navigation (#27)
- All controls now support mouse interactions (#27)

### Fixed

- **Calendar**: Range selection now works correctly - second click completes the range instead of resetting
- **DataGridView**: Filter icon now distinct from sort arrows (⫶ vs ▲/▼) (#63)
- **DataGridView**: Feature toggle checkboxes now update UI correctly (#64)
- **DataGridView**: Selection performance with targeted visual updates (#52, #58)
- **DataGridView**: Edit trigger default and dark theme text contrast (#57, #59)
- **DataGridView**: Type conversion when committing cell edits (#55)
- **DataGridView**: Picker/DatePicker/TimePicker columns now stay open when dropdown opens (#77)
- **DataGridView**: F2/ESC/arrow keys now work after cell tap (grid receives focus) (#80)
- **DataGridView**: Right-click context menu now works on Windows desktop using native handlers (#85)
- **DataGridView**: Native context menu no longer disappears immediately when right-clicking Entry in edit mode (#87)
- **DataGridView**: Edit mode commit timing issues with context menu interactions resolved

### Known Issues

- Documentation GitHub Pages deployment with .nojekyll file (#35)

## [1.0.0] - Initial Release

### Added

- Initial control library structure
- Base control infrastructure
- Docsify documentation site

---

## Version History

| Version | Date | Highlights |
|---------|------|------------|
| 3.1.0 | 2026-02-26 | SelectedIndex/SelectedIndices features, AOT/trimming fixes, DataGrid virtualization crash fix, ComboBox selection fixes |
| 3.0.0 | 2026-02-25 | AOT/trimming safety for all controls, Func-based property accessors, PropertyMetadataRegistry (#232, #233) |
| 2.1.8 | 2026-02-24 | DataGrid theme-reactive headers, pagination layout, picker centering, cell text colors (#231) |
| 2.1.7 | 2026-02-23 | DataGrid filter popup null-value checkbox preservation (#217) |
| 2.1.6 | 2026-02-23 | DataGrid context menu native long-press on all platforms (#223) |
| 2.1.5 | 2026-02-23 | DataGrid cell editing with virtualization enabled (#222, #227) |
| 2.1.4 | 2026-02-23 | DataGrid RefreshData visual update in default mode (#221) |
| 2.1.3 | 2026-02-23 | DataGrid filter icon 44pt touch target fix (#220) |
| 2.1.2 | 2026-02-23 | DataGrid cascading/progressive filter popup values (#219) |
| 2.1.1 | 2026-02-23 | ComboBox: no software keyboard on iOS/Android when IsSearchVisible=false (#216) |
| 2.1.0 | 2026-02-22 | ComboBox anchor-based popup placement for standalone PopupMode (#213) |
| 2.0.0 | 2026-02-21 | 15 enterprise controls, keyboard/clipboard/undo-redo, dark theme, MVVM parity |
| 1.0.0 | — | Initial release with ComboBox |

## Contributing

See [CONTRIBUTING](contributing.md) for guidelines on how to contribute to this project.
