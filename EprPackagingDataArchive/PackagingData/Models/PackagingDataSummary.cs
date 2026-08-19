namespace EprPackagingDataArchive.PackagingData.Models;

/// <summary>
/// Aggregates for one organisation over one period. Exposed as its own resource because it is what
/// most callers actually want, and computing it client-side across a paginated collection of lines
/// is a trap.
/// </summary>
public sealed record PackagingDataSummary
{
    public required string OrganisationId { get; init; }

    public required string SubmissionPeriod { get; init; }

    public required int ObligationYear { get; init; }

    public required SummaryTotals Totals { get; init; }

    public required IReadOnlyCollection<Breakdown> ByMaterial { get; init; }

    public required IReadOnlyCollection<Breakdown> ByActivity { get; init; }

    public required IReadOnlyCollection<Breakdown> ByNation { get; init; }
}

/// <summary>
/// The same summary rolled up across every member of a compliance scheme.
/// </summary>
public sealed record SchemePackagingDataSummary
{
    public required string SchemeId { get; init; }

    public required string SubmissionPeriod { get; init; }

    public required int ObligationYear { get; init; }

    /// <summary>Members counted are those in the scheme during the period, not those reporting today.</summary>
    public required int MemberCount { get; init; }

    public required int ReportingMemberCount { get; init; }

    public required SummaryTotals Totals { get; init; }

    public required IReadOnlyCollection<Breakdown> ByMaterial { get; init; }

    public required IReadOnlyCollection<Breakdown> ByNation { get; init; }
}

public sealed record SummaryTotals
{
    public required decimal Tonnage { get; init; }

    public required int LineCount { get; init; }
}

/// <summary>One row of an aggregate. <c>Key</c> is a material, activity or nation depending on context.</summary>
public sealed record Breakdown
{
    public required string Key { get; init; }

    public required decimal Tonnage { get; init; }

    public required int LineCount { get; init; }
}
