namespace MauiControlsExtras.ContextMenu;

/// <summary>
/// Provides platform-specific context menu rendering using native menu controls.
/// </summary>
public class ContextMenuService : IContextMenuService
{
    private static ContextMenuService? _current;

    /// <summary>
    /// Gets the singleton instance of the context menu service.
    /// </summary>
    public static ContextMenuService Current => _current ??= new ContextMenuService();

    /// <inheritdoc />
    public async Task ShowAsync(View anchor, IList<ContextMenuItem> items, Point? position = null)
    {
        if (items == null || items.Count == 0)
            return;

        var visibleItems = items.Where(i => i.IsVisible).ToList();
        if (visibleItems.Count == 0)
            return;

#if WINDOWS
        await ShowWindowsMenuAsync(anchor, visibleItems, position);
#elif MACCATALYST
        await ShowMacMenuAsync(anchor, visibleItems, position);
#elif IOS
        await ShowIOSMenuAsync(anchor, visibleItems, position);
#elif ANDROID
        await ShowAndroidMenuAsync(anchor, visibleItems, position);
#else
        await ShowFallbackMenuAsync(anchor, visibleItems);
#endif
    }

    /// <inheritdoc />
    public async Task ShowAtAsync(IList<ContextMenuItem> items, Point screenPosition)
    {
        // For screen-positioned menus, we need to find a suitable anchor view
        var page = GetCurrentPage();
        if (page is ContentPage contentPage && contentPage.Content is View content)
        {
            await ShowAsync(content, items, screenPosition);
        }
    }

    /// <inheritdoc />
    public void Dismiss()
    {
#if WINDOWS
        DismissWindowsMenu();
#elif MACCATALYST
        DismissMacMenu();
#elif IOS
        DismissIOSMenu();
#elif ANDROID
        DismissAndroidMenu();
#endif
    }

    private static Page? GetCurrentPage()
    {
        if (Application.Current?.Windows is { Count: > 0 } windows)
        {
            return windows[0].Page;
        }
        return null;
    }

#if WINDOWS
    private async Task ShowWindowsMenuAsync(View anchor, IList<ContextMenuItem> items, Point? position)
    {
        await anchor.Dispatcher.DispatchAsync(() =>
        {
            if (anchor.Handler?.PlatformView is not Microsoft.UI.Xaml.FrameworkElement element)
                return;

            var menuFlyout = new Microsoft.UI.Xaml.Controls.MenuFlyout();

            foreach (var item in items)
            {
                var flyoutItem = CreateWindowsFlyoutItem(item);
                if (flyoutItem != null)
                {
                    menuFlyout.Items.Add(flyoutItem);
                }
            }

            if (position.HasValue)
            {
                var pos = new Windows.Foundation.Point(position.Value.X, position.Value.Y);
                menuFlyout.ShowAt(element, pos);
            }
            else
            {
                menuFlyout.ShowAt(element);
            }
        });
    }

    private Microsoft.UI.Xaml.Controls.MenuFlyoutItemBase? CreateWindowsFlyoutItem(ContextMenuItem item)
    {
        if (item.IsSeparator)
        {
            return new Microsoft.UI.Xaml.Controls.MenuFlyoutSeparator();
        }

        if (item.HasSubItems)
        {
            var subMenu = new Microsoft.UI.Xaml.Controls.MenuFlyoutSubItem
            {
                Text = item.Text ?? string.Empty,
                IsEnabled = item.IsEnabled
            };

            if (!string.IsNullOrEmpty(item.IconGlyph))
            {
                subMenu.Icon = new Microsoft.UI.Xaml.Controls.FontIcon
                {
                    Glyph = item.IconGlyph,
                    FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets")
                };
            }

            foreach (var subItem in item.SubItems.Where(s => s.IsVisible))
            {
                var flyoutSubItem = CreateWindowsFlyoutItem(subItem);
                if (flyoutSubItem != null)
                {
                    subMenu.Items.Add(flyoutSubItem);
                }
            }

            return subMenu;
        }

        var flyoutItem = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem
        {
            Text = item.Text ?? string.Empty,
            IsEnabled = item.IsEnabled
        };

        if (!string.IsNullOrEmpty(item.IconGlyph))
        {
            flyoutItem.Icon = new Microsoft.UI.Xaml.Controls.FontIcon
            {
                Glyph = item.IconGlyph,
                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets")
            };
        }

        if (!string.IsNullOrEmpty(item.KeyboardShortcut))
        {
            flyoutItem.KeyboardAcceleratorTextOverride = item.KeyboardShortcut;
        }

        flyoutItem.Click += (s, e) => item.Execute();

        return flyoutItem;
    }

    private void DismissWindowsMenu()
    {
        // MenuFlyout auto-dismisses on click or loss of focus
    }
#endif

#if MACCATALYST
    private async Task ShowMacMenuAsync(View anchor, IList<ContextMenuItem> items, Point? position)
    {
        await anchor.Dispatcher.DispatchAsync(() =>
        {
            if (anchor.Handler?.PlatformView is not UIKit.UIView view)
                return;

            var menuElements = items.Select(CreateMacMenuItem).Where(m => m != null).ToArray();
            var menu = UIKit.UIMenu.Create(menuElements!);

            // For Mac Catalyst, we use UIContextMenuInteraction
            // Remove any existing context menu interactions
            foreach (var existing in view.Interactions.OfType<UIKit.UIContextMenuInteraction>().ToArray())
            {
                view.RemoveInteraction(existing);
            }

            var interaction = new UIKit.UIContextMenuInteraction(new MacContextMenuDelegate(menu));
            view.AddInteraction(interaction);

            // Trigger the context menu programmatically if possible
            // Note: UIContextMenuInteraction requires user gesture to trigger
        });
    }

    private UIKit.UIMenuElement? CreateMacMenuItem(ContextMenuItem item)
    {
        if (item.IsSeparator)
        {
            // Return an inline menu with no items to act as a separator
            return UIKit.UIMenu.Create(string.Empty, null, UIKit.UIMenuIdentifier.None, UIKit.UIMenuOptions.DisplayInline, Array.Empty<UIKit.UIMenuElement>());
        }

        if (item.HasSubItems)
        {
            var subElements = item.SubItems
                .Where(s => s.IsVisible)
                .Select(CreateMacMenuItem)
                .Where(m => m != null)
                .ToArray();

            return UIKit.UIMenu.Create(item.Text ?? string.Empty, null, UIKit.UIMenuIdentifier.None, (UIKit.UIMenuOptions)0, subElements!);
        }

        UIKit.UIImage? image = null;
        if (!string.IsNullOrEmpty(item.IconGlyph))
        {
            image = UIKit.UIImage.GetSystemImage(item.IconGlyph);
        }

        var attributes = item.IsEnabled
            ? (UIKit.UIMenuElementAttributes)0  // None
            : UIKit.UIMenuElementAttributes.Disabled;

        return UIKit.UIAction.Create(
            item.Text ?? string.Empty,
            image,
            null,
            _ => item.Execute());
    }

    private void DismissMacMenu()
    {
        // Context menu dismisses automatically
    }

    private class MacContextMenuDelegate : UIKit.UIContextMenuInteractionDelegate
    {
        private readonly UIKit.UIMenu _menu;

        public MacContextMenuDelegate(UIKit.UIMenu menu)
        {
            _menu = menu;
        }

        public override UIKit.UIContextMenuConfiguration? GetConfigurationForMenu(
            UIKit.UIContextMenuInteraction interaction,
            CoreGraphics.CGPoint location)
        {
            return UIKit.UIContextMenuConfiguration.Create(
                null,
                null,
                _ => _menu);
        }
    }
#endif

#if IOS && !MACCATALYST
    private UIKit.UIAlertController? _activeAlert;

    private async Task ShowIOSMenuAsync(View anchor, IList<ContextMenuItem> items, Point? position)
    {
        await anchor.Dispatcher.DispatchAsync(() =>
        {
            var viewController = GetViewController(anchor);
            if (viewController == null)
                return;

            var alert = UIKit.UIAlertController.Create(
                null, null,
                UIKit.UIAlertControllerStyle.ActionSheet);

            foreach (var item in items)
            {
                if (item.IsSeparator)
                    continue; // iOS doesn't support separators in action sheets

                if (item.HasSubItems)
                {
                    // For submenus, show a button that opens another action sheet
                    var capturedItem = item;
                    var action = UIKit.UIAlertAction.Create(
                        $"{item.Text} →",
                        UIKit.UIAlertActionStyle.Default,
                        _ => ShowSubMenu(viewController, capturedItem.SubItems.ToList()));
                    alert.AddAction(action);
                }
                else
                {
                    var capturedItem = item;
                    var action = UIKit.UIAlertAction.Create(
                        item.Text ?? string.Empty,
                        UIKit.UIAlertActionStyle.Default,
                        _ => capturedItem.Execute());
                    action.Enabled = item.IsEnabled;
                    alert.AddAction(action);
                }
            }

            alert.AddAction(UIKit.UIAlertAction.Create(
                "Cancel",
                UIKit.UIAlertActionStyle.Cancel,
                null));

            // For iPad, we need to configure the popover
            if (alert.PopoverPresentationController != null && anchor.Handler?.PlatformView is UIKit.UIView nativeView)
            {
                alert.PopoverPresentationController.SourceView = nativeView;
                if (position.HasValue)
                {
                    alert.PopoverPresentationController.SourceRect = new CoreGraphics.CGRect(
                        position.Value.X, position.Value.Y, 1, 1);
                }
                else
                {
                    alert.PopoverPresentationController.SourceRect = nativeView.Bounds;
                }
            }

            _activeAlert = alert;
            viewController.PresentViewController(alert, true, null);
        });
    }

    private void ShowSubMenu(UIKit.UIViewController viewController, IList<ContextMenuItem> items)
    {
        var alert = UIKit.UIAlertController.Create(
            null, null,
            UIKit.UIAlertControllerStyle.ActionSheet);

        foreach (var item in items.Where(i => i.IsVisible && !i.IsSeparator))
        {
            var capturedItem = item;
            var action = UIKit.UIAlertAction.Create(
                item.Text ?? string.Empty,
                UIKit.UIAlertActionStyle.Default,
                _ => capturedItem.Execute());
            action.Enabled = item.IsEnabled;
            alert.AddAction(action);
        }

        alert.AddAction(UIKit.UIAlertAction.Create(
            "Cancel",
            UIKit.UIAlertActionStyle.Cancel,
            null));

        viewController.PresentViewController(alert, true, null);
    }

    private static UIKit.UIViewController? GetViewController(View view)
    {
        var page = GetCurrentPage();
        if (page?.Handler?.PlatformView is UIKit.UIView nativeView)
        {
            return nativeView.Window?.RootViewController;
        }
        return null;
    }

    private void DismissIOSMenu()
    {
        _activeAlert?.DismissViewController(true, null);
        _activeAlert = null;
    }
#endif

#if ANDROID
    private Android.Widget.PopupMenu? _activePopupMenu;

    private async Task ShowAndroidMenuAsync(View anchor, IList<ContextMenuItem> items, Point? position)
    {
        await anchor.Dispatcher.DispatchAsync(() =>
        {
            if (anchor.Handler?.PlatformView is not Android.Views.View nativeView)
                return;

            var context = nativeView.Context;
            if (context == null)
                return;

            var popupMenu = new Android.Widget.PopupMenu(context, nativeView);
            var menu = popupMenu.Menu;
            if (menu == null)
                return;

            int itemId = 0;
            var itemMap = new Dictionary<int, ContextMenuItem>();

            foreach (var item in items)
            {
                if (item.IsSeparator)
                {
                    // Android uses groups for separators
                    continue;
                }

                if (item.HasSubItems)
                {
                    var subMenu = menu.AddSubMenu(0, itemId, 0, new Java.Lang.String(item.Text ?? string.Empty));
                    if (subMenu != null)
                    {
                        foreach (var subItem in item.SubItems.Where(s => s.IsVisible && !s.IsSeparator))
                        {
                            itemId++;
                            var subMenuItem = subMenu.Add(0, itemId, 0, new Java.Lang.String(subItem.Text ?? string.Empty));
                            if (subMenuItem != null)
                            {
                                subMenuItem.SetEnabled(subItem.IsEnabled);
                                itemMap[itemId] = subItem;
                            }
                        }
                    }
                    itemId++;
                }
                else
                {
                    var menuItem = menu.Add(0, itemId, 0, new Java.Lang.String(item.Text ?? string.Empty));
                    if (menuItem != null)
                    {
                        menuItem.SetEnabled(item.IsEnabled);
                        itemMap[itemId] = item;
                    }
                    itemId++;
                }
            }

            popupMenu.MenuItemClick += (s, e) =>
            {
                if (e.Item != null && itemMap.TryGetValue(e.Item.ItemId, out var clickedItem))
                {
                    clickedItem.Execute();
                }
            };

            _activePopupMenu = popupMenu;
            popupMenu.Show();
        });
    }

    private void DismissAndroidMenu()
    {
        _activePopupMenu?.Dismiss();
        _activePopupMenu = null;
    }
#endif

#if !(WINDOWS || MACCATALYST || IOS || ANDROID)
    private async Task ShowFallbackMenuAsync(View anchor, IList<ContextMenuItem> items)
    {
        // Fallback to DisplayActionSheet for unsupported platforms
        var page = GetCurrentPage();
        if (page == null)
            return;

        var visibleItems = items.Where(i => i.IsVisible && !i.IsSeparator && !i.HasSubItems).ToList();
        var buttons = visibleItems.Select(i => i.Text ?? string.Empty).ToArray();

        var result = await page.DisplayActionSheet(null, "Cancel", null, buttons);

        if (!string.IsNullOrEmpty(result) && result != "Cancel")
        {
            var selectedItem = visibleItems.FirstOrDefault(i => i.Text == result);
            selectedItem?.Execute();
        }
    }
#endif
}
