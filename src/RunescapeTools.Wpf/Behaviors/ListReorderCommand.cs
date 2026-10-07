using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace RunescapeTools.Wpf.Behaviors;

public sealed record ListReorderRequest(object Item, object? BeforeItem);

/// <summary>Translates list drag handles and Alt+Up/Down into a move-before command.</summary>
public static class ListReorderCommand
{
    private const string DataFormat = "RunescapeTools.ListReorder";
    public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
        "Command", typeof(ICommand), typeof(ListReorderCommand), new PropertyMetadata(null, OnCommandChanged));

    public static readonly DependencyProperty IsDragHandleProperty = DependencyProperty.RegisterAttached(
        "IsDragHandle", typeof(bool), typeof(ListReorderCommand), new PropertyMetadata(false));

    private static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached(
        "State", typeof(DragState), typeof(ListReorderCommand));

    public static void SetCommand(DependencyObject element, ICommand? value) => element.SetValue(CommandProperty, value);
    public static ICommand? GetCommand(DependencyObject element) => (ICommand?)element.GetValue(CommandProperty);
    public static void SetIsDragHandle(DependencyObject element, bool value) => element.SetValue(IsDragHandleProperty, value);
    public static bool GetIsDragHandle(DependencyObject element) => (bool)element.GetValue(IsDragHandleProperty);

    public static IDataObject CreateDragData(ListBox source, object item)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(item);
        if (!source.Items.Contains(item))
            throw new ArgumentException("The dragged item must belong to its source list.", nameof(item));
        return new DataObject(DataFormat, new DragPayload(source, item));
    }

    private static void OnCommandChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
    {
        if (element is not ListBox list)
            return;
        (list.GetValue(StateProperty) as DragState)?.Detach();
        list.SetValue(StateProperty, args.NewValue is ICommand ? new DragState(list) : null);
    }

    private sealed record DragPayload(ListBox Source, object Item);

    private sealed class DragState
    {
        private readonly ListBox list;
        private readonly bool previousAllowDrop;
        private object? pendingItem;
        private Point startPoint;
        private bool dragging;
        private long lastScroll;
        private AdornerLayer? markerLayer;
        private InsertionMarker? marker;

        public DragState(ListBox list)
        {
            this.list = list;
            previousAllowDrop = list.AllowDrop;
            list.SetCurrentValue(UIElement.AllowDropProperty, true);
            list.PreviewMouseLeftButtonDown += OnMouseDown;
            list.PreviewMouseMove += OnMouseMove;
            list.PreviewMouseLeftButtonUp += OnMouseUp;
            list.LostMouseCapture += OnLostCapture;
            list.PreviewDragOver += OnDragOver;
            list.PreviewDragLeave += OnDragLeave;
            list.PreviewDrop += OnDrop;
            list.PreviewKeyDown += OnKeyDown;
            list.Unloaded += OnUnloaded;
        }

        public void Detach()
        {
            Reset();
            list.PreviewMouseLeftButtonDown -= OnMouseDown;
            list.PreviewMouseMove -= OnMouseMove;
            list.PreviewMouseLeftButtonUp -= OnMouseUp;
            list.LostMouseCapture -= OnLostCapture;
            list.PreviewDragOver -= OnDragOver;
            list.PreviewDragLeave -= OnDragLeave;
            list.PreviewDrop -= OnDrop;
            list.PreviewKeyDown -= OnKeyDown;
            list.Unloaded -= OnUnloaded;
            list.SetCurrentValue(UIElement.AllowDropProperty, previousAllowDrop);
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs args)
        {
            if (args.OriginalSource is not DependencyObject source
                || !IsHandle(source)
                || ItemsControl.ContainerFromElement(list, source) is not ListBoxItem container)
                return;

            var item = list.ItemContainerGenerator.ItemFromContainer(container);
            if (!CanExecute(new ListReorderRequest(item, null)))
                return;

            pendingItem = item;
            startPoint = args.GetPosition(list);
            list.CaptureMouse();
            args.Handled = true;
        }

        private void OnMouseMove(object sender, MouseEventArgs args)
        {
            if (pendingItem is null || dragging)
                return;
            if (args.LeftButton != MouseButtonState.Pressed)
            {
                Reset();
                return;
            }
            var point = args.GetPosition(list);
            if (Math.Abs(point.X - startPoint.X) < SystemParameters.MinimumHorizontalDragDistance
                && Math.Abs(point.Y - startPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;

            var item = pendingItem;
            dragging = true;
            list.ReleaseMouseCapture();
            try
            {
                DragDrop.DoDragDrop(list, CreateDragData(list, item), DragDropEffects.Move);
            }
            finally
            {
                dragging = false;
                Reset();
            }
            args.Handled = true;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs args)
        {
            if (pendingItem is null || dragging)
                return;
            Reset();
            args.Handled = true;
        }

        private void OnLostCapture(object sender, MouseEventArgs args)
        {
            if (!dragging)
                Reset();
        }

        private void OnUnloaded(object sender, RoutedEventArgs args) => Reset();

        private void OnDragOver(object sender, DragEventArgs args)
        {
            args.Effects = DragDropEffects.None;
            if (TryGetPayload(args, out var payload))
            {
                AutoScroll(args.GetPosition(list));
                var placement = FindPlacement(payload!.Item, args.GetPosition(list));
                if (CanExecute(placement.Request))
                {
                    args.Effects = DragDropEffects.Move;
                    ShowMarker(placement.Container, placement.After);
                }
            }
            if (args.Effects == DragDropEffects.None)
                ClearMarker();
            args.Handled = true;
        }

        private void OnDragLeave(object sender, DragEventArgs args) => ClearMarker();

        private void OnDrop(object sender, DragEventArgs args)
        {
            ClearMarker();
            args.Effects = DragDropEffects.None;
            if (TryGetPayload(args, out var payload))
            {
                var request = FindPlacement(payload!.Item, args.GetPosition(list)).Request;
                if (CanExecute(request))
                {
                    GetCommand(list)!.Execute(request);
                    args.Effects = DragDropEffects.Move;
                }
            }
            args.Handled = true;
        }

        private bool TryGetPayload(DragEventArgs args, out DragPayload? payload)
        {
            payload = null;
            if ((args.AllowedEffects & DragDropEffects.Move) == 0
                || !args.Data.GetDataPresent(DataFormat, false)
                || args.OriginalSource is DependencyObject source && FindAncestor<ScrollBar>(source) is not null)
                return false;
            payload = args.Data.GetData(DataFormat, false) as DragPayload;
            return payload is not null && ReferenceEquals(payload.Source, list) && list.Items.Contains(payload.Item);
        }

        private (ListReorderRequest Request, ListBoxItem? Container, bool After) FindPlacement(object item, Point point)
        {
            ListBoxItem? last = null;
            var lastIndex = -1;
            for (var index = 0; index < list.Items.Count; index++)
            {
                if (list.ItemContainerGenerator.ContainerFromIndex(index) is not ListBoxItem container)
                    continue;
                var top = container.TranslatePoint(new Point(), list).Y;
                if (point.Y < top + container.ActualHeight / 2d)
                    return (new ListReorderRequest(item, list.Items[index]), container, false);
                last = container;
                lastIndex = index;
            }

            var anchor = lastIndex >= 0 && lastIndex + 1 < list.Items.Count ? list.Items[lastIndex + 1] : null;
            return (new ListReorderRequest(item, anchor), last, true);
        }

        private void AutoScroll(Point point)
        {
            var now = Environment.TickCount64;
            if (now - lastScroll < 120 || FindVisualChild<ScrollViewer>(list) is not { } scroll)
                return;
            if (point.Y < 24)
                scroll.LineUp();
            else if (point.Y > list.ActualHeight - 24)
                scroll.LineDown();
            else
                return;
            lastScroll = now;
        }

        private void OnKeyDown(object sender, KeyEventArgs args)
        {
            var key = args.Key == Key.System ? args.SystemKey : args.Key;
            if (Keyboard.Modifiers != ModifierKeys.Alt || key is not (Key.Up or Key.Down)
                || list.SelectedItem is not { } item)
                return;
            var index = list.Items.IndexOf(item);
            var next = key == Key.Up ? index - 1 : index + 1;
            if (next >= 0 && next < list.Items.Count)
            {
                var anchorIndex = key == Key.Up ? next : next + 1;
                var request = new ListReorderRequest(item, anchorIndex < list.Items.Count ? list.Items[anchorIndex] : null);
                if (CanExecute(request))
                    GetCommand(list)!.Execute(request);
            }
            args.Handled = true;
        }

        private bool CanExecute(ListReorderRequest request) => GetCommand(list)?.CanExecute(request) == true;

        private bool IsHandle(DependencyObject source)
        {
            for (var current = source; current is not null && !ReferenceEquals(current, list); current = Parent(current))
                if (GetIsDragHandle(current))
                    return true;
            return false;
        }

        private void ShowMarker(ListBoxItem? container, bool after)
        {
            if (marker is not null && ReferenceEquals(marker.AdornedElement, container) && marker.After == after)
                return;
            ClearMarker();
            if (container is null || AdornerLayer.GetAdornerLayer(container) is not { } layer)
                return;
            markerLayer = layer;
            marker = new InsertionMarker(container, after,
                list.TryFindResource("GoldBrush") as Brush ?? Brushes.DarkGoldenrod);
            layer.Add(marker);
        }

        private void ClearMarker()
        {
            if (marker is not null)
                markerLayer?.Remove(marker);
            marker = null;
            markerLayer = null;
        }

        private void Reset()
        {
            pendingItem = null;
            if (list.IsMouseCaptured)
                list.ReleaseMouseCapture();
            ClearMarker();
        }
    }

    private sealed class InsertionMarker : Adorner
    {
        private readonly Pen pen;
        public bool After { get; }

        public InsertionMarker(UIElement element, bool after, Brush brush) : base(element)
        {
            After = after;
            pen = new Pen(brush, 2);
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var y = After ? AdornedElement.RenderSize.Height : 0;
            drawingContext.DrawLine(pen, new Point(0, y), new Point(AdornedElement.RenderSize.Width, y));
        }
    }

    private static DependencyObject? Parent(DependencyObject element) => element is Visual
        ? VisualTreeHelper.GetParent(element)
        : LogicalTreeHelper.GetParent(element);

    private static T? FindAncestor<T>(DependencyObject element) where T : DependencyObject
    {
        for (var current = element; current is not null; current = Parent(current))
            if (current is T found)
                return found;
        return null;
    }

    private static T? FindVisualChild<T>(DependencyObject element) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(element); index++)
        {
            var child = VisualTreeHelper.GetChild(element, index);
            if (child is T found)
                return found;
            if (FindVisualChild<T>(child) is { } descendant)
                return descendant;
        }
        return null;
    }
}
