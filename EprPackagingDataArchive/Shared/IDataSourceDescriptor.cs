namespace EprPackagingDataArchive.Shared;

/// <summary>
/// Describes where the currently configured providers get their data, and how fresh it is.
///
/// This is a service rather than a constant because <c>AsOf</c> is a property of the source, not of
/// the clock. A stub knows its fixtures were authored at a fixed moment; the Common Data API adapter
/// will report the warehouse's last sync time; a projection will report when it last ingested. In
/// every case the honest answer to "when was this true" comes from the source, and reading the
/// current time instead would be a lie that happens to look plausible.
/// </summary>
public interface IDataSourceDescriptor
{
    /// <summary>One of <see cref="DataSourceNames"/>.</summary>
    string Name { get; }

    DateTimeOffset AsOf { get; }
}

/// <summary>
/// Describes the fixture set. The date is fixed rather than "now" so that responses are deterministic
/// and a consumer diffing two calls sees no spurious change.
/// </summary>
public sealed class StubDataSourceDescriptor : IDataSourceDescriptor
{
    public string Name => DataSourceNames.Stub;

    public DateTimeOffset AsOf => Stubs.StubDataSet.AsOf;
}
