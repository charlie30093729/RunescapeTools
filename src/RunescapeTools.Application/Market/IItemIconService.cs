namespace RunescapeTools.Application.Market;

public sealed record ItemIcon(
    int ItemId,
    string WikiFileName,
    string LocalFilePath);

public interface IItemIconService
{
    Task<ItemIcon?> GetAsync(
        int itemId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, ItemIcon>> GetManyAsync(
        IEnumerable<int> itemIds,
        CancellationToken cancellationToken = default);
}
