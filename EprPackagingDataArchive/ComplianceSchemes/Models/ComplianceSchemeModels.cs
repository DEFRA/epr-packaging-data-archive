namespace EprPackagingDataArchive.ComplianceSchemes.Models;

/// <summary>
/// A producer as seen from inside a scheme's membership list.
/// </summary>
public sealed record SchemeMember
{
    public required string OrganisationId { get; init; }

    public required string Name { get; init; }

    public required string ProducerSize { get; init; }

    public required string Nation { get; init; }

    public required DateOnly JoinedOn { get; init; }

    public DateOnly? LeftOn { get; init; }

    /// <summary>Active or Left, derived from the membership dates rather than stored.</summary>
    public required string Status { get; init; }
}

/// <summary>
/// Which members have reported for a period and which have not.
///
/// This answers the question a scheme actually has and which nothing in the estate exposes today.
/// Schemes currently work it out by exporting spreadsheets.
/// </summary>
public sealed record SchemeReportingStatus
{
    public required string SchemeId { get; init; }

    public required string SubmissionPeriod { get; init; }

    public required int ObligationYear { get; init; }

    public required ReportingSummary Summary { get; init; }

    public required IReadOnlyCollection<MemberReportingStatus> Members { get; init; }
}

public sealed record ReportingSummary
{
    public required int Members { get; init; }

    public required int Reported { get; init; }

    public required int NotReported { get; init; }
}

public sealed record MemberReportingStatus
{
    public required string OrganisationId { get; init; }

    public required string Name { get; init; }

    public required bool Reported { get; init; }

    public DateTimeOffset? ReportedAt { get; init; }

    public string? SubmissionId { get; init; }

    /// <summary>Null rather than zero when nothing has been reported, so the two cases stay distinguishable.</summary>
    public decimal? Tonnage { get; init; }
}

public static class MembershipStatuses
{
    public const string Active = "Active";
    public const string Left = "Left";
}
