namespace RunescapeTools.Infrastructure.Training;

internal readonly record struct CatalogueItem
{
    public CatalogueItem(int id, string name)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A catalogue item must have a display name.", nameof(name));

        Id = id;
        Name = name;
    }

    public int Id { get; }

    public string Name { get; }
}
