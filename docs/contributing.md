# Contributing

Thank you for your interest in contributing to MauiControlsExtras! This document provides guidelines and instructions for contributing.

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- Visual Studio 2022 (17.8+) or JetBrains Rider
- .NET MAUI workload installed

### Setting Up the Development Environment

1. Fork the repository
2. Clone your fork:
   ```bash
   git clone https://github.com/YOUR_USERNAME/MauiControlsExtras.git
   ```
3. Open the solution in your IDE
4. Build to restore packages:
   ```bash
   dotnet build
   ```

## Development Guidelines

### Code Style

- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments to public APIs
- Keep methods focused and small (single responsibility)

### Control Development

When creating new controls:

1. Inherit from `StyledControlBase` for consistent styling support
2. Implement keyboard navigation via `IKeyboardNavigable` interface
3. Support clipboard operations via `IClipboardSupport` when applicable
4. Follow MVVM pattern with bindable properties and commands
5. Include accessibility support

See [Control Development Guide](ControlDevelopmentGuide.md) for detailed instructions.

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Controls | PascalCase | `DataGridView` |
| Properties | PascalCase | `SelectedItem` |
| Events | PascalCase, past tense | `ItemSelected` |
| Commands | PascalCase + Command | `SelectItemCommand` |
| Private fields | _camelCase | `_selectedIndex` |

## Submitting Changes

### Branch Naming

- Features: `feature/<short-description>`
- Bug fixes: `fix/<short-description>`
- Documentation: `docs/<short-description>`

### Commit Messages

Follow conventional commits format:

```
type(scope): description

[optional body]

[optional footer]
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

Examples:
```
feat(DataGrid): add column reordering support
fix(TreeView): correct keyboard navigation in nested nodes
docs: update RichTextEditor usage examples
```

### Pull Request Process

1. Create a feature branch from `main`
2. Make your changes following the guidelines above
3. Ensure the project builds without warnings
4. Update documentation if needed
5. Create a pull request with a clear description
6. Link any related issues

### Pull Request Template

```markdown
## Summary
Brief description of changes

## Changes
- Change 1
- Change 2

## Testing
How to test these changes

## Related Issues
Fixes #123
```

## Documentation

- Update control documentation in `docs/controls/`
- Use consistent formatting (see existing docs)
- Include code examples for common scenarios
- Document all public properties, events, and commands

## Reporting Issues

### Bug Reports

Include:
- .NET MAUI version
- Target platform(s) affected
- Steps to reproduce
- Expected vs actual behavior
- Code sample or minimal reproduction

### Feature Requests

Include:
- Use case description
- Proposed API design
- Examples of similar implementations (if any)

## Code of Conduct

- Be respectful and inclusive
- Provide constructive feedback
- Focus on the code, not the person
- Help newcomers learn

## Questions?

Open a discussion or issue on GitHub if you have questions about contributing.

---

Thank you for contributing to MauiControlsExtras!
