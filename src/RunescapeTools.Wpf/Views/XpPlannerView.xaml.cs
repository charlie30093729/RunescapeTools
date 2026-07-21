using System.Windows;
using System.Windows.Controls;

namespace RunescapeTools.Wpf.Views;

public partial class XpPlannerView : UserControl
{
    public XpPlannerView()
    {
        InitializeComponent();
    }

    private void SkillIconFailed(object sender, ExceptionRoutedEventArgs e)
    {
        if (sender is Image image)
            image.Visibility = Visibility.Collapsed;
    }
}
