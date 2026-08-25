namespace EprPackagingDataArchive.PackagingData.Models;

/// <summary>
/// The nested Get Packaging Data response: one organisation, its submissions, and the packaging
/// rows inside each submission. This is the shape the ticket describes, and it replaces the earlier
/// flat row list at the same route.
///
/// Row field names follow the warehouse vocabulary (packagingActivity, packagingMaterialWeight,
/// fromCountry) rather than this codebase's earlier shorthand, so the phase two mapping from the
/// real POM stream is mechanical.
/// </summary>
public sealed record PackagingDataReport
{
    public required OrganisationBlock Organisation { get; init; }

    public required IReadOnlyCollection<SubmissionBlock> Submissions { get; init; }
}

public sealed record OrganisationBlock
{
    public required string OrganisationId { get; init; }

    public required string Name { get; init; }

    /// <summary>Set when the organisation queried is itself a subsidiary of another producer.</summary>
    public string? ParentId { get; init; }
}

public sealed record SubmissionBlock
{
    public required string SubmissionId { get; init; }

    /// <summary>H1, H2 or P0 with the year, for example 2026-H1.</summary>
    public required string SubmissionPeriod { get; init; }

    public required string Status { get; init; }

    public required IReadOnlyCollection<PackagingRow> PackagingData { get; init; }
}

public sealed record PackagingRow
{
    public required string PackagingDataId { get; init; }

    public string? SubsidiaryId { get; init; }

    public required string PackagingActivity { get; init; }

    public required string PackagingType { get; init; }

    public required string PackagingClass { get; init; }

    public required string PackagingMaterial { get; init; }

    public string? PackagingMaterialSubtype { get; init; }

    /// <summary>Unit deliberately unstated, mirroring the upstream warehouse column. See DECISIONS.md 5.</summary>
    public required decimal PackagingMaterialWeight { get; init; }

    public int? PackagingMaterialUnits { get; init; }

    public int? TransitionalPackagingUnits { get; init; }

    public required string FromCountry { get; init; }

    public string? ToCountry { get; init; }

    public string? RamRagRating { get; init; }
}
