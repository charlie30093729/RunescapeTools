using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace RunescapeTools.Wpf.Behaviors;

public static class SelectionChangedCommand
{
    public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
        "Command",
        typeof(ICommand),
        typeof(SelectionChangedCommand),
        new PropertyMetadata(null, OnCommandChanged));

    public static void SetCommand(DependencyObject element, ICommand? value) =>
        element.SetValue(CommandProperty, value);

    public static ICommand? GetCommand(DependencyObject element) =>
        (ICommand?)element.GetValue(CommandProperty);

    private static void OnCommandChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs eventArgs)
    {
        if (dependencyObject is not Selector selector)
            return;

        if (eventArgs.OldValue is ICommand)
            selector.SelectionChanged -= OnSelectionChanged;
        if (eventArgs.NewValue is ICommand)
            selector.SelectionChanged += OnSelectionChanged;
    }

    private static void OnSelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
    {
        if (sender is not Selector selector)
            return;

        var command = GetCommand(selector);
        var selectedItem = selector.SelectedItem;
        if (command?.CanExecute(selectedItem) == true)
            command.Execute(selectedItem);
    }
}
