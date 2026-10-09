using System.Reflection;
using System.Windows;

namespace RunescapeTools.Tests.TestSupport.Builders;

internal static class WpfDragTestData
{
    public static DragEventArgs DragEvent(IDataObject data, DependencyObject target, Point point, RoutedEvent routedEvent)
    {
        // WPF's drag-event constructor is internal. Build event data without moving the user's mouse.
        var constructor = typeof(DragEventArgs).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
            null, [typeof(IDataObject), typeof(DragDropKeyStates), typeof(DragDropEffects), typeof(DependencyObject), typeof(Point)], null)
            ?? throw new InvalidOperationException("The WPF drag-event constructor is unavailable.");
        var args = (DragEventArgs)constructor.Invoke([data, DragDropKeyStates.LeftMouseButton, DragDropEffects.Move, target, point]);
        args.RoutedEvent = routedEvent;
        return args;
    }
}
