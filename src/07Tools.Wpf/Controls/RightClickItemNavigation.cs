using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RunescapeTools.Wpf.Controls;

public static class RightClickItemNavigation
{
    public static readonly DependencyProperty TargetItemsControlProperty =
        DependencyProperty.RegisterAttached(
            "TargetItemsControl",
            typeof(ItemsControl),
            typeof(RightClickItemNavigation),
            new PropertyMetadata(null, OnTargetItemsControlChanged));

    public static void SetTargetItemsControl(DependencyObject element, ItemsControl? value) =>
        element.SetValue(TargetItemsControlProperty, value);

    public static ItemsControl? GetTargetItemsControl(DependencyObject element) =>
        (ItemsControl?)element.GetValue(TargetItemsControlProperty);

    public static bool ScrollToItem(ItemsControl targetItemsControl, object item)
    {
        ArgumentNullException.ThrowIfNull(targetItemsControl);
        ArgumentNullException.ThrowIfNull(item);

        targetItemsControl.UpdateLayout();
        if (targetItemsControl.ItemContainerGenerator.ContainerFromItem(item)
            is not FrameworkElement targetContainer)
        {
            return false;
        }

        var scrollViewer = FindAncestor<ScrollViewer>(targetItemsControl);
        if (scrollViewer is null)
            return false;

        var horizontalOffset = scrollViewer.HorizontalOffset;
        var rowPosition = targetContainer.TranslatePoint(new Point(), scrollViewer);
        scrollViewer.ScrollToVerticalOffset(
            Math.Max(0, scrollViewer.VerticalOffset + rowPosition.Y));
        scrollViewer.ScrollToHorizontalOffset(horizontalOffset);
        return true;
    }

    private static void OnTargetItemsControlChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is not ItemsControl sourceItemsControl)
            return;

        sourceItemsControl.PreviewMouseRightButtonUp -= OnPreviewMouseRightButtonUp;
        if (args.NewValue is ItemsControl)
            sourceItemsControl.PreviewMouseRightButtonUp += OnPreviewMouseRightButtonUp;
    }

    private static void OnPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ItemsControl sourceItemsControl
            || GetTargetItemsControl(sourceItemsControl) is not { } targetItemsControl
            || e.OriginalSource is not DependencyObject originalSource
            || ItemsControl.ContainerFromElement(sourceItemsControl, originalSource)
                is not FrameworkElement sourceContainer
            || sourceContainer.DataContext is not { } item)
        {
            return;
        }

        if (ScrollToItem(targetItemsControl, item))
            e.Handled = true;
    }

    private static T? FindAncestor<T>(DependencyObject descendant)
        where T : DependencyObject
    {
        for (var current = descendant; current is not null; current = VisualTreeHelper.GetParent(current))
        {
            if (current is T ancestor)
                return ancestor;
        }

        return null;
    }
}
