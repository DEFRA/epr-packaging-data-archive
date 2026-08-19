namespace EprPackagingDataArchive.Organisations.Models;

/// <summary>
/// A submission, always described from the point of view of the organisation the data is ABOUT.
///
/// <c>SubmittedBy</c> records who filed it, which is what makes a direct producer and a scheme
/// member the same shape: the URL says whose data it is, this field says who sent it.
/// </summary>
public record SubmissionResponse
{
    public required string SubmissionId { get; init; }

    /// <summary>PackagingData or Registration.</summary>
    public required string Type { get; init; }

    public required string SubmissionPeriod { get; init; }

    public required int ObligationYear { get; init; }

    public required string Status { get; init; }

    public required DateTimeOffset SubmittedAt { get; init; }

    public required SubmitterReference SubmittedBy { get; init; }

    public string? FileName { get; init; }

    public required bool IsResubmission { get; init; }
}

/// <summary>
/// The detail view adds what a caller would otherwise have to ask a second service for.
/// </summary>
public sealed record SubmissionDetailResponse : SubmissionResponse
{
    public required ValidationSummary Validation { get; init; }

    /// <summary>
    /// Whether the file has reached the reporting warehouse. The estate currently leaks this as a
    /// set of "is_file_synced_with_cosmos" style probes on the upstream API; exposing it as a field
    /// on the resource keeps the ETL lag an implementation detail rather than a caller's problem.
    /// </summary>
    public required bool AvailableInWarehouse { get; init; }
}

public sealed record ValidationSummary
{
    public required int ErrorCount { get; init; }

    public required int WarningCount { get; init; }

    public required int RowCount { get; init; }
}

public sealed record SubmitterReference
{
    public required string OrganisationId { get; init; }

    /// <summary>Self or ComplianceScheme.</summary>
    public required string Role { get; init; }
}

public static class SubmissionTypes
{
    public const string PackagingData = "PackagingData";
    public const string Registration = "Registration";
}

public static class SubmitterRoles
{
    public const string Self = "Self";
    public const string ComplianceScheme = "ComplianceScheme";
}
