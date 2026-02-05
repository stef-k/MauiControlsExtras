# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **DataGrid**: `IContextMenuSupport` interface implementation on DataGridView (#162)
  - Explicit `IContextMenuSupport.ContextMenuOpening` event (avoids naming conflict with legacy event)
  - `ShowContextMenu(Point?)` overload resolves focused cell context
- **Clipboard**: `IClipboardSupport` implemented on ComboBox, MultiSelectComboBox, MaskedEntry, and NumericUpDown (#163)
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
- **MaskedEntry**: Input mask control (#3)
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
| 1.0.0 | TBD | Initial release with 14 enterprise controls |

## Contributing

See [CONTRIBUTING](contributing.md) for guidelines on how to contribute to this project.
