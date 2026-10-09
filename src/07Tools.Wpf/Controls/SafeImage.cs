using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RunescapeTools.Wpf.Controls;

public sealed class SafeImage : Image
{
    private BitmapSource? observedDownloadingBitmap;

    private static readonly DependencyPropertyKey HasLoadedImagePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(HasLoadedImage),
            typeof(bool),
            typeof(SafeImage),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasLoadedImageProperty =
        HasLoadedImagePropertyKey.DependencyProperty;

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

    public bool HasLoadedImage => (bool)GetValue(HasLoadedImageProperty);

    public bool CollapseParentOnFailure
    {
        get => (bool)GetValue(CollapseParentOnFailureProperty);
        set => SetValue(CollapseParentOnFailureProperty, value);
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs eventArgs)
    {
        base.OnPropertyChanged(eventArgs);
        if (eventArgs.Property != SourceProperty)
            return;

        StopObservingBitmap();
        SetValue(HasLoadedImagePropertyKey, false);
        Visibility = Visibility.Visible;

        if (eventArgs.NewValue is BitmapSource bitmap)
        {
            if (bitmap.IsDownloading && !bitmap.IsFrozen)
            {
                observedDownloadingBitmap = bitmap;
                bitmap.DownloadCompleted += OnBitmapDownloadCompleted;
            }
            else
            {
                UpdateLoadedState(bitmap);
            }
        }
        else if (eventArgs.NewValue is ImageSource)
        {
            SetValue(HasLoadedImagePropertyKey, true);
        }
    }

    private void OnImageFailed(object? sender, ExceptionRoutedEventArgs eventArgs)
    {
        StopObservingBitmap();
        SetValue(HasLoadedImagePropertyKey, false);
        Visibility = Visibility.Collapsed;
        if (CollapseParentOnFailure && Parent is UIElement parent)
            parent.Visibility = Visibility.Collapsed;

        eventArgs.Handled = true;
    }

    private void OnBitmapDownloadCompleted(object? sender, EventArgs eventArgs)
    {
        if (sender is BitmapSource bitmap && ReferenceEquals(bitmap, observedDownloadingBitmap))
            UpdateLoadedState(bitmap);

        StopObservingBitmap();
    }

    private void UpdateLoadedState(BitmapSource bitmap)
    {
        SetValue(
            HasLoadedImagePropertyKey,
            bitmap.PixelWidth > 0 && bitmap.PixelHeight > 0);
    }

    private void StopObservingBitmap()
    {
        var bitmap = observedDownloadingBitmap;
        observedDownloadingBitmap = null;
        if (bitmap is not null && !bitmap.IsFrozen)
            bitmap.DownloadCompleted -= OnBitmapDownloadCompleted;
    }
}
