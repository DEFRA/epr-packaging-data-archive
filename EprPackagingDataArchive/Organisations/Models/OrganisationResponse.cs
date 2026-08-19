namespace EprPackagingDataArchive.Organisations.Models;

/// <summary>
/// An organisation as this API publishes it.
///
/// <c>OrganisationId</c> is the EPR organisation reference number, the same value that appears as
/// <c>organisation_id</c> in the packaging data CSV. The estate has at least three identifiers per
/// organisation; this API exposes exactly one and never an internal database id.
/// </summary>
public sealed record OrganisationResponse
{
    public required string OrganisationId { get; init; }

    public required string Name { get; init; }

    /// <summary>DirectProducer, SchemeMember or ComplianceSchemeOperator.</summary>
    public required string Type { get; init; }

    /// <summary>Large or Small. Null for a scheme operator, which has no obligation of its own.</summary>
    public string? ProducerSize { get; init; }

    public required string Nation { get; init; }

    public string? CompaniesHouseNumber { get; init; }

    /// <summary>Populated only for a SchemeMember.</summary>
    public ComplianceSchemeMembership? ComplianceScheme { get; init; }

    public RegistrationSummary? Registration { get; init; }
}

/// <summary>
/// Membership is dated on both ends because a producer can join or leave a scheme part-way through
/// an obligation year. Every scheme-level question is therefore implicitly "as at when", and this is
/// the relationship most likely to shape the eventual database choice.
/// </summary>
public sealed record ComplianceSchemeMembership
{
    public required string SchemeId { get; init; }

    public required string Name { get; init; }

    public required DateOnly JoinedOn { get; init; }

    public DateOnly? LeftOn { get; init; }
}

public sealed record RegistrationSummary
{
    /// <summary>NotStarted, Submitted, Granted, Refused, Queried or Cancelled.</summary>
    public required string Status { get; init; }

    /// <summary>Minted only once a regulator grants the registration.</summary>
    public string? ReferenceNumber { get; init; }

    public required int ObligationYear { get; init; }
}

public static class OrganisationTypes
{
    public const string DirectProducer = "DirectProducer";
    public const string SchemeMember = "SchemeMember";
    public const string ComplianceSchemeOperator = "ComplianceSchemeOperator";
}

public static class ProducerSizes
{
    public const string Large = "Large";
    public const string Small = "Small";
}
