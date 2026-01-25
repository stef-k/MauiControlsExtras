# Architecture

For the complete architecture documentation, see [ARCHITECTURE.md](../ARCHITECTURE.md) in the repository root.

## Quick Links

- [Base Classes](../ARCHITECTURE.md#base-classes)
- [Interfaces](../ARCHITECTURE.md#interfaces)
- [Theming](../ARCHITECTURE.md#theming)
- [Keyboard/Mouse/Focus Support](../ARCHITECTURE.md#keyboardmousefocus-support-mandatory)
- [Control Patterns](../ARCHITECTURE.md#control-patterns)

## Overview

MAUI Controls Extras follows a layered architecture:

```
┌─────────────────────────────────────────────────────┐
│                    Controls Layer                    │
│  ComboBox, DataGridView, TreeView, NumericUpDown... │
├─────────────────────────────────────────────────────┤
│                   Base Classes Layer                 │
│  StyledControlBase, TextStyledControlBase, etc.     │
├─────────────────────────────────────────────────────┤
│                  Interfaces Layer                    │
│  IKeyboardNavigable, IClipboardSupport, IUndoRedo   │
├─────────────────────────────────────────────────────┤
│                   Services Layer                     │
│  IPrintService, Behaviors, Converters               │
├─────────────────────────────────────────────────────┤
│                   Theming Layer                      │
│  MauiControlsExtrasTheme, Light/Dark themes         │
└─────────────────────────────────────────────────────┘
```

## Key Principles

1. **All controls inherit from a base class** that provides theming support
2. **Events have corresponding commands** for MVVM support
3. **Keyboard/mouse support is mandatory** for desktop platforms
4. **Theme-aware styling** through `Effective*` properties

For detailed implementation guidance, see the [Control Development Guide](ControlDevelopmentGuide.md).
