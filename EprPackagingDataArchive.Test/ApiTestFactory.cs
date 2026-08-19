using System.Net.Http.Json;
using System.Text.Json;
using EprPackagingDataArchive.Shared;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EprPackagingDataArchive.Test;

/// <summary>
/// Boots the real application.
///
/// Nothing is substituted, which is the point of phase one: the stub providers are already the test
/// double, so these tests exercise real routing, real query binding, real validation and real
/// serialisation. When a live adapter arrives, the factory gains a configuration override rather
/// than a mock.
/// </summary>
public sealed class ApiTestFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Matches how the API serialises: camelCase, case-insensitive on read. Deserialising with the
    /// defaults instead would silently produce empty objects and make a broken contract look passing.
    /// </summary>
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
}

public static class HttpResponseExtensions
{
    public static async Task<Envelope<T>> ReadEnvelopeAsync<T>(
        this HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<T>>(
            ApiTestFactory.Json, cancellationToken);

        Assert.NotNull(envelope);
        return envelope;
    }
}
