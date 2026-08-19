namespace EprPackagingDataArchive.Shared;

/// <summary>
/// Paging is applied from the first stub because a compliance scheme can have thousands of members
/// and packaging data is one row per material per activity per period. Adding it later would be a
/// breaking change for every caller.
/// </summary>
public readonly record struct PageRequest
{
    public const int DefaultSize = 50;
    public const int MaxSize = 500;

    private PageRequest(int number, int size)
    {
        Number = number;
        Size = size;
    }

    public int Number { get; }

    public int Size { get; }

    public int Skip => (Number - 1) * Size;

    /// <summary>Clamps rather than rejects: an out-of-range page size is not worth a 400 to a caller.</summary>
    public static PageRequest From(int? page, int? pageSize) =>
        new(Math.Max(page ?? 1, 1), Math.Clamp(pageSize ?? DefaultSize, 1, MaxSize));

    public IReadOnlyCollection<T> Apply<T>(IEnumerable<T> source) =>
        source.Skip(Skip).Take(Size).ToList();
}
