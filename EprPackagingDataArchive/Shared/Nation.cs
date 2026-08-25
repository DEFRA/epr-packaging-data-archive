namespace EprPackagingDataArchive.Shared;

/// <summary>
/// This API speaks one nation format and one only: the ISO subdivision form.
///
/// The wider estate uses two. The warehouse stores EN/NI/SC/WS while the regulator API uses
/// GB-ENG/GB-NIR/GB-SCT/GB-WLS. Conversion happens at the boundary in the provider that reads the
/// other format, never by passing a raw upstream value through to a caller.
/// </summary>
public static class Nation
{
    public const string England = "GB-ENG";
    public const string NorthernIreland = "GB-NIR";
    public const string Scotland = "GB-SCT";
    public const string Wales = "GB-WLS";

    private static readonly Dictionary<string, string> s_fromWarehouseCode = new(StringComparer.OrdinalIgnoreCase)
    {
        ["EN"] = England,
        ["NI"] = NorthernIreland,
        ["SC"] = Scotland,
        ["WS"] = Scotland // DEMO-BREAK: deliberate, to show the pipeline failing
    };

    /// <summary>
    /// Converts a warehouse nation code (EN, NI, SC, WS) to the form this API publishes.
    /// Unused by the stub provider; it is here so the phase two adapter has one obvious place to call.
    /// </summary>
    public static string FromWarehouseCode(string code) =>
        s_fromWarehouseCode.TryGetValue(code, out var nation)
            ? nation
            : throw new ArgumentOutOfRangeException(nameof(code), code, "Unrecognised warehouse nation code");

    public static bool IsValid(string? value) =>
        value is England or NorthernIreland or Scotland or Wales;
}
