using System.Windows;
using System.Windows.Controls;

namespace RunescapeTools.Wpf.Controls;

public sealed class SafeImage : Image
{
    public static readonly DependencyProperty CollapseParentOnFailureProperty =
        DependencyProperty.Register(
            nameof(CollapseParentOnFailure),
            typeof(bool),
            typeof(SafeImage),
            new PropertyMetadata(false));

    public SafeImage()
    {
        ImageFailed += OnImageFailed;
    }

    public bool CollapseParentOnFailure
    {
        get => (bool)GetValue(CollapseParentOnFailureProperty);
        set => SetValue(CollapseParentOnFailureProperty, value);
    }

    private void OnImageFailed(object? sender, ExceptionRoutedEventArgs eventArgs)
    {
        Visibility = Visibility.Collapsed;
        if (CollapseParentOnFailure && Parent is UIElement parent)
            parent.Visibility = Visibility.Collapsed;

        eventArgs.Handled = true;
    }
}
