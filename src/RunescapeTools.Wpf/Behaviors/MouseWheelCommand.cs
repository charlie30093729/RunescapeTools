using System.Windows;
using System.Windows.Input;

namespace RunescapeTools.Wpf.Behaviors;

public static class MouseWheelCommand
{
    public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
        "Command",
        typeof(ICommand),
        typeof(MouseWheelCommand),
        new PropertyMetadata(null, OnCommandChanged));

    public static void SetCommand(DependencyObject element, ICommand? value) =>
        element.SetValue(CommandProperty, value);

    public static ICommand? GetCommand(DependencyObject element) =>
        (ICommand?)element.GetValue(CommandProperty);

    private static void OnCommandChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is not UIElement element)
            return;

        if (args.OldValue is ICommand)
            element.PreviewMouseWheel -= OnPreviewMouseWheel;
        if (args.NewValue is ICommand)
            element.PreviewMouseWheel += OnPreviewMouseWheel;
    }

    private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs args)
    {
        if (sender is not DependencyObject element)
            return;

        var command = GetCommand(element);
        if (command?.CanExecute(args.Delta) != true)
            return;

        command.Execute(args.Delta);
        args.Handled = true;
    }
}
