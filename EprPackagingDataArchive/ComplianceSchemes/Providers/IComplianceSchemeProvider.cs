using EprPackagingDataArchive.ComplianceSchemes.Models;

namespace EprPackagingDataArchive.ComplianceSchemes.Providers;

/// <summary>
/// The port for compliance scheme data. See <see cref="Organisations.Providers.IOrganisationProvider"/>
/// for the reasoning behind the shape of these interfaces.
/// </summary>
public interface IComplianceSchemeProvider
{
    Task<bool> ExistsAsync(
        string schemeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SchemeMember>> GetMembersAsync(
        string schemeId,
        MemberQuery query,
        CancellationToken cancellationToken = default);

    Task<SchemeReportingStatus> GetReportingStatusAsync(
        string schemeId,
        string submissionPeriod,
        CancellationToken cancellationToken = default);
}

public sealed record MemberQuery
{
    /// <summary>When set, returns membership as it stood on that date rather than today.</summary>
    public DateOnly? AsAt { get; init; }

    public string? Status { get; init; }
}
