namespace RunescapeTools.Wpf.ViewModels;

public interface IPageViewModel
{
    Task LoadAsync(CancellationToken cancellationToken = default);
}
