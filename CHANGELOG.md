# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed

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
