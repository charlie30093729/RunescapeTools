using System.Windows;
using System.Windows.Controls;

namespace RunescapeTools.Wpf.Views;

public partial class ProfileView : UserControl
{
    public ProfileView()
    {
        InitializeComponent();
    }

    private void SkillIconFailed(object sender, ExceptionRoutedEventArgs e)
    {
        if (sender is Image { Parent: Border imageContainer })
            imageContainer.Visibility = Visibility.Collapsed;
    }
}
