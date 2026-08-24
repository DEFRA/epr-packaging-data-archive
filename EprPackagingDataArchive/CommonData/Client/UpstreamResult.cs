using System.Text.Json.Nodes;

namespace EprPackagingDataArchive.CommonData.Client;

/// <summary>
/// What came back from the warehouse, plus how the call itself went.
///
/// The diagnostics are the point. When this fails it will fail on networking, egress or auth, and
/// knowing the URL, status and elapsed time tells you which. A bare payload would not.
/// </summary>
public sealed record UpstreamResult
{
    public required UpstreamCall Upstream { get; init; }

    /// <summary>The upstream response, passed through unmapped. Null when the call failed.</summary>
    public JsonNode? Payload { get; init; }
}

public sealed record UpstreamCall
{
    public required string Method { get; init; }

    public required string Url { get; init; }

    public int? Status { get; init; }

    public required long ElapsedMs { get; init; }

    /// <summary>Populated when the call did not complete. Never contains credentials.</summary>
    public string? Error { get; init; }
}
