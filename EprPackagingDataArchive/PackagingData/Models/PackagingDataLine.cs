using EprPackagingDataArchive.Organisations.Models;

namespace EprPackagingDataArchive.PackagingData.Models;

/// <summary>
/// One reported line of packaging placed on the market.
///
/// Field names deliberately mirror the columns of the packaging data CSV that the estate already
/// validates against, so that mapping a warehouse row onto this shape in phase two is mechanical
/// rather than interpretive.
/// </summary>
public sealed record PackagingDataLine
{
    public required string LineId { get; init; }

    public required string OrganisationId { get; init; }

    /// <summary>Set when the tonnage was reported against a subsidiary rather than the parent.</summary>
    public string? SubsidiaryId { get; init; }

    public required string SubmissionPeriod { get; init; }

    public required int ObligationYear { get; init; }

    public required string Activity { get; init; }

    public required string PackagingType { get; init; }

    public required string PackagingClass { get; init; }

    public required string Material { get; init; }

    public required decimal Tonnage { get; init; }

    public int? Units { get; init; }

    public required string FromNation { get; init; }

    public string? ToNation { get; init; }

    /// <summary>Recyclability Assessment Methodology rating: Red, Amber or Green. Drives fee modulation.</summary>
    public string? RamRagRating { get; init; }

    public required string SubmissionId { get; init; }

    public required SubmitterReference SubmittedBy { get; init; }
}

public static class PackagingMaterials
{
    public const string Aluminium = "Aluminium";
    public const string FibreComposite = "FibreComposite";
    public const string Glass = "Glass";
    public const string PaperBoard = "PaperBoard";
    public const string Plastic = "Plastic";
    public const string Steel = "Steel";
    public const string Wood = "Wood";
    public const string Other = "Other";
}
