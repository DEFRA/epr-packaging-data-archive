using EprPackagingDataArchive.ComplianceSchemes.Models;
using EprPackagingDataArchive.Organisations.Models;
using EprPackagingDataArchive.PackagingData.Models;

namespace EprPackagingDataArchive.Shared.Stubs;

/// <summary>
/// The single fixture set every stub provider reads from.
///
/// It is deliberately one dataset rather than per-provider fixtures, because the three providers
/// have to agree: a scheme's members must exist as organisations, and a member's reporting status
/// must match the packaging lines attributed to it. Splitting the fixtures would let them drift, and
/// a caller exercising two endpoints together would see a contradiction that no real implementation
/// could produce.
///
/// All values are invented. No real producer data appears here.
///
/// Coverage is chosen to exercise the cases that matter:
///   100123  direct producer, reports for itself, has two periods of data
///   100456  scheme member, reported by its scheme, has a resubmission
///   100987  scheme member, registered but has reported nothing
///   100999  direct producer, no scheme, no data, registration not started
///   CS-004  the scheme operator itself
/// </summary>
public static class StubDataSet
{
    /// <summary>
    /// A fixed point in time, so responses are deterministic and tests do not depend on today's date.
    /// The real providers will take this from the source system rather than a constant.
    /// </summary>
    public static readonly DateTimeOffset AsOf = new(2026, 8, 13, 9, 0, 0, TimeSpan.Zero);

    public const string SchemeId = "CS-004";
    public const string SchemeName = "Example Compliance Ltd";

    public static readonly IReadOnlyList<OrganisationResponse> Organisations =
    [
        new()
        {
            OrganisationId = "100123",
            Name = "Pop Quest Ltd",
            Type = OrganisationTypes.DirectProducer,
            ProducerSize = ProducerSizes.Large,
            Nation = Nation.England,
            CompaniesHouseNumber = "01234567",
            ComplianceScheme = null,
            Registration = new RegistrationSummary
            {
                Status = "Granted",
                ReferenceNumber = "EPR-2026-ENG-000123",
                ObligationYear = 2027
            }
        },
        new()
        {
            OrganisationId = "100456",
            Name = "Northern Packaging Ltd",
            Type = OrganisationTypes.SchemeMember,
            ProducerSize = ProducerSizes.Large,
            Nation = Nation.NorthernIreland,
            CompaniesHouseNumber = "NI045612",
            ComplianceScheme = new ComplianceSchemeMembership
            {
                SchemeId = SchemeId,
                Name = SchemeName,
                JoinedOn = new DateOnly(2026, 1, 1),
                LeftOn = null
            },
            Registration = new RegistrationSummary
            {
                Status = "Granted",
                ReferenceNumber = "EPR-2026-NIR-000456",
                ObligationYear = 2027
            }
        },
        new()
        {
            OrganisationId = "100987",
            Name = "Another Producer Ltd",
            Type = OrganisationTypes.SchemeMember,
            ProducerSize = ProducerSizes.Small,
            Nation = Nation.Scotland,
            CompaniesHouseNumber = "SC098712",
            ComplianceScheme = new ComplianceSchemeMembership
            {
                SchemeId = SchemeId,
                Name = SchemeName,
                JoinedOn = new DateOnly(2026, 4, 1),
                LeftOn = null
            },
            Registration = new RegistrationSummary
            {
                Status = "Submitted",
                ReferenceNumber = null,
                ObligationYear = 2027
            }
        },
        new()
        {
            OrganisationId = "100999",
            Name = "Quiet Trading Ltd",
            Type = OrganisationTypes.DirectProducer,
            ProducerSize = ProducerSizes.Small,
            Nation = Nation.Wales,
            CompaniesHouseNumber = "12349999",
            ComplianceScheme = null,
            Registration = new RegistrationSummary
            {
                Status = "NotStarted",
                ReferenceNumber = null,
                ObligationYear = 2027
            }
        },
        new()
        {
            OrganisationId = SchemeId,
            Name = SchemeName,
            Type = OrganisationTypes.ComplianceSchemeOperator,
            ProducerSize = null,
            Nation = Nation.England,
            CompaniesHouseNumber = "07654321",
            ComplianceScheme = null,
            Registration = null
        }
    ];

    public static readonly IReadOnlyList<SchemeMember> SchemeMembers =
    [
        new()
        {
            OrganisationId = "100456",
            Name = "Northern Packaging Ltd",
            ProducerSize = ProducerSizes.Large,
            Nation = Nation.NorthernIreland,
            JoinedOn = new DateOnly(2026, 1, 1),
            LeftOn = null,
            Status = MembershipStatuses.Active
        },
        new()
        {
            OrganisationId = "100987",
            Name = "Another Producer Ltd",
            ProducerSize = ProducerSizes.Small,
            Nation = Nation.Scotland,
            JoinedOn = new DateOnly(2026, 4, 1),
            LeftOn = null,
            Status = MembershipStatuses.Active
        },
        // A mid-year leaver, so that any "as at" logic has something to bite on.
        new()
        {
            OrganisationId = "100777",
            Name = "Departed Packaging Ltd",
            ProducerSize = ProducerSizes.Small,
            Nation = Nation.England,
            JoinedOn = new DateOnly(2025, 1, 1),
            LeftOn = new DateOnly(2026, 3, 31),
            Status = MembershipStatuses.Left
        }
    ];

    private static readonly SubmitterReference s_self100123 =
        new() { OrganisationId = "100123", Role = SubmitterRoles.Self };

    private static readonly SubmitterReference s_scheme =
        new() { OrganisationId = SchemeId, Role = SubmitterRoles.ComplianceScheme };

    public static readonly IReadOnlyList<SubmissionDetailResponse> Submissions =
    [
        new()
        {
            SubmissionId = "9f2c8e14-3a5d-4c77-9b21-8de4f0a17c60",
            Type = SubmissionTypes.PackagingData,
            SubmissionPeriod = "2026-H1",
            ObligationYear = 2027,
            Status = "AcceptedByRegulator",
            SubmittedAt = new DateTimeOffset(2026, 7, 14, 11, 2, 31, TimeSpan.Zero),
            SubmittedBy = s_self100123,
            FileName = "pom-h1-2026.csv",
            IsResubmission = false,
            Validation = new ValidationSummary { ErrorCount = 0, WarningCount = 2, RowCount = 5 },
            AvailableInWarehouse = true
        },
        new()
        {
            SubmissionId = "1b7d40aa-6c19-4f02-8e55-2ca9d3b6e731",
            Type = SubmissionTypes.PackagingData,
            SubmissionPeriod = "2025-H2",
            ObligationYear = 2026,
            Status = "AcceptedByRegulator",
            SubmittedAt = new DateTimeOffset(2026, 1, 20, 9, 45, 0, TimeSpan.Zero),
            SubmittedBy = s_self100123,
            FileName = "pom-h2-2025.csv",
            IsResubmission = false,
            Validation = new ValidationSummary { ErrorCount = 0, WarningCount = 0, RowCount = 2 },
            AvailableInWarehouse = true
        },
        new()
        {
            SubmissionId = "4e0a91c3-77bf-4d18-b6a2-5f13c8e9d024",
            Type = SubmissionTypes.PackagingData,
            SubmissionPeriod = "2026-H1",
            ObligationYear = 2027,
            Status = "SubmittedToRegulator",
            SubmittedAt = new DateTimeOffset(2026, 8, 2, 16, 20, 5, TimeSpan.Zero),
            SubmittedBy = s_scheme,
            FileName = "cs004-pom-h1-2026-v2.csv",
            IsResubmission = true,
            Validation = new ValidationSummary { ErrorCount = 0, WarningCount = 1, RowCount = 2 },
            // Deliberately false: a submission can exist before the ETL has landed it in the
            // warehouse. Callers need to be able to tell those two states apart.
            AvailableInWarehouse = false
        },
        new()
        {
            SubmissionId = "8a3b52ef-19d4-4e60-a2c8-6f90b1d43a77",
            Type = SubmissionTypes.PackagingData,
            SubmissionPeriod = "2025-H1",
            ObligationYear = 2026,
            Status = "RejectedByRegulator",
            SubmittedAt = new DateTimeOffset(2025, 7, 30, 10, 15, 0, TimeSpan.Zero),
            SubmittedBy = s_self100123,
            FileName = "pom-h1-2025.csv",
            IsResubmission = false,
            Validation = new ValidationSummary { ErrorCount = 3, WarningCount = 5, RowCount = 2 },
            AvailableInWarehouse = true
        }
    ];

    /// <summary>Maps a submission to the organisation the data is ABOUT, which is not always the submitter.</summary>
    public static readonly IReadOnlyDictionary<string, string> SubmissionSubject =
        new Dictionary<string, string>
        {
            ["9f2c8e14-3a5d-4c77-9b21-8de4f0a17c60"] = "100123",
            ["1b7d40aa-6c19-4f02-8e55-2ca9d3b6e731"] = "100123",
            ["4e0a91c3-77bf-4d18-b6a2-5f13c8e9d024"] = "100456",
            ["8a3b52ef-19d4-4e60-a2c8-6f90b1d43a77"] = "100123"
        };

    public static readonly IReadOnlyList<PackagingDataLine> PackagingLines =
    [
        Line("7c1f0001", "100123", "2026-H1", 2027, "SoldAsEmptyPackaging", "HouseholdConsumerWaste",
            "PrimaryPackaging", PackagingMaterials.Plastic, 310.40m, 98000, Nation.England, Nation.Scotland,
            "Amber", "9f2c8e14-3a5d-4c77-9b21-8de4f0a17c60", s_self100123, subtype: "PET"),
        Line("7c1f0002", "100123", "2026-H1", 2027, "SuppliedUnderYourBrand", "HouseholdConsumerWaste",
            "PrimaryPackaging", PackagingMaterials.PaperBoard, 268.75m, 54000, Nation.England, null,
            "Green", "9f2c8e14-3a5d-4c77-9b21-8de4f0a17c60", s_self100123),
        Line("7c1f0003", "100123", "2026-H1", 2027, "SoldAsEmptyPackaging", "HouseholdDrinksContainers",
            "PrimaryPackaging", PackagingMaterials.Glass, 152.04m, 41000, Nation.England, Nation.Wales,
            "Green", "9f2c8e14-3a5d-4c77-9b21-8de4f0a17c60", s_self100123),
        Line("7c1f0004", "100123", "2026-H1", 2027, "HiredOrLoaned", "NonHouseholdConsumerWaste",
            "TransitPackaging", PackagingMaterials.Aluminium, 61.00m, 12500, Nation.England, null,
            "Green", "9f2c8e14-3a5d-4c77-9b21-8de4f0a17c60", s_self100123),
        Line("7c1f0005", "100123", "2026-H1", 2027, "SuppliedUnderYourBrand", "NonHouseholdConsumerWaste",
            "SecondaryPackaging", PackagingMaterials.Steel, 50.00m, 8200, Nation.England, null,
            "Red", "9f2c8e14-3a5d-4c77-9b21-8de4f0a17c60", s_self100123),

        Line("7c1f0006", "100123", "2025-H2", 2026, "SoldAsEmptyPackaging", "HouseholdConsumerWaste",
            "PrimaryPackaging", PackagingMaterials.Plastic, 289.10m, 91000, Nation.England, null,
            "Amber", "1b7d40aa-6c19-4f02-8e55-2ca9d3b6e731", s_self100123),
        Line("7c1f0007", "100123", "2025-H2", 2026, "SuppliedUnderYourBrand", "HouseholdConsumerWaste",
            "PrimaryPackaging", PackagingMaterials.PaperBoard, 240.55m, 48000, Nation.England, null,
            "Green", "1b7d40aa-6c19-4f02-8e55-2ca9d3b6e731", s_self100123),

        // Reported BY the scheme, ABOUT the member. This is the pairing that makes a direct producer
        // and a scheme member the same shape at the organisation endpoint.
        Line("7c1f0008", "100456", "2026-H1", 2027, "SoldAsEmptyPackaging", "HouseholdConsumerWaste",
            "PrimaryPackaging", PackagingMaterials.Plastic, 88.20m, 22000, Nation.NorthernIreland, null,
            "Amber", "4e0a91c3-77bf-4d18-b6a2-5f13c8e9d024", s_scheme, subtype: "HDPE"),
        // The rejected 2025-H1 submission: one parent row, one subsidiary row with transitional units.
        Line("7c1f0010", "100123", "2025-H1", 2026, "SoldAsEmptyPackaging", "HouseholdConsumerWaste",
            "PrimaryPackaging", PackagingMaterials.Plastic, 120.00m, 30000, Nation.England, null,
            "Red", "8a3b52ef-19d4-4e60-a2c8-6f90b1d43a77", s_self100123, subtype: "PVC"),
        Line("7c1f0011", "100123", "2025-H1", 2026, "SuppliedUnderYourBrand", "HouseholdConsumerWaste",
            "SecondaryPackaging", PackagingMaterials.Glass, 40.50m, 9100, Nation.England, null,
            "Amber", "8a3b52ef-19d4-4e60-a2c8-6f90b1d43a77", s_self100123,
            transitionalUnits: 1200, subsidiaryId: "100123-S01"),

        Line("7c1f0009", "100456", "2026-H1", 2027, "SuppliedUnderYourBrand", "HouseholdDrinksContainers",
            "PrimaryPackaging", PackagingMaterials.Glass, 45.60m, 9800, Nation.NorthernIreland, null,
            "Green", "4e0a91c3-77bf-4d18-b6a2-5f13c8e9d024", s_scheme)
    ];

    private static PackagingDataLine Line(
        string lineId, string organisationId, string period, int obligationYear, string activity,
        string packagingType, string packagingClass, string material, decimal tonnage, int? units,
        string fromNation, string? toNation, string? ramRag, string submissionId,
        SubmitterReference submittedBy, string? subtype = null, int? transitionalUnits = null,
        string? subsidiaryId = null) =>
        new()
        {
            LineId = lineId,
            OrganisationId = organisationId,
            SubsidiaryId = subsidiaryId,
            SubmissionPeriod = period,
            ObligationYear = obligationYear,
            Activity = activity,
            PackagingType = packagingType,
            PackagingClass = packagingClass,
            Material = material,
            MaterialSubtype = subtype,
            Tonnage = tonnage,
            Units = units,
            TransitionalPackagingUnits = transitionalUnits,
            FromNation = fromNation,
            ToNation = toNation,
            RamRagRating = ramRag,
            SubmissionId = submissionId,
            SubmittedBy = submittedBy
        };
}
