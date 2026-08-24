using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using EprPackagingDataArchive.Config;
using Microsoft.Extensions.Options;

namespace EprPackagingDataArchive.CommonData.Client;

public sealed class CommonDataApiClient(
    HttpClient httpClient,
    IOptions<CommonDataApiOptions> options,
    ILogger<CommonDataApiClient> logger) : ICommonDataApiClient
{
    private readonly CommonDataApiOptions _options = options.Value;

    public Task<UpstreamResult> GetLastSyncTimeAsync(CancellationToken cancellationToken) =>
        SendAsync(new HttpRequestMessage(HttpMethod.Get, "api/submission-events/get-last-sync-time"),
            cancellationToken);

    public Task<UpstreamResult> GetPomSummaryAsync(
        string organisationReference, int pageSize, CancellationToken cancellationToken)
    {
        // Upstream takes a POST body even though this is a read. UserId and DecisionsDelta are
        // regulator-context fields; they are sent empty because this is not a regulator caller.
        var body = new
        {
            OrganisationReference = organisationReference,
            PageSize = pageSize,
            PageNumber = 1
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "api/submissions/pom/summary")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };

        return SendAsync(request, cancellationToken);
    }

    public async Task<UpstreamResult> GetPomSampleAsync(
        int relativeYear, int take, CancellationToken cancellationToken)
    {
        var capped = Math.Clamp(take, 1, _options.MaxStreamRows);
        var path = $"api/paycal/poms/stream?RelativeYear={relativeYear}";
        var url = Absolute(path);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            AddAuth(request);

            // ResponseHeadersRead so we can stop after N rows rather than buffering a whole year.
            using var response = await httpClient.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await FailureFromResponseAsync("GET", url, response, stopwatch, cancellationToken);
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            var rows = new JsonArray();
            while (rows.Count < capped && await reader.ReadLineAsync(cancellationToken) is { } line)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                rows.Add(JsonNode.Parse(line));
            }

            stopwatch.Stop();
            logger.LogInformation("Sampled {Count} POM rows for relative year {Year}", rows.Count, relativeYear);

            return new UpstreamResult
            {
                Upstream = new UpstreamCall
                {
                    Method = "GET",
                    Url = url,
                    Status = (int)response.StatusCode,
                    ElapsedMs = stopwatch.ElapsedMilliseconds
                },
                Payload = new JsonObject
                {
                    ["rowsSampled"] = rows.Count,
                    ["cappedAt"] = capped,
                    ["rows"] = rows
                }
            };
        }
        catch (Exception ex)
        {
            return Failure("GET", url, stopwatch, ex);
        }
    }

    private async Task<UpstreamResult> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var method = request.Method.Method;
        var url = Absolute(request.RequestUri?.ToString() ?? string.Empty);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            AddAuth(request);
            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await FailureFromResponseAsync(method, url, response, stopwatch, cancellationToken);
            }

            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            stopwatch.Stop();

            return new UpstreamResult
            {
                Upstream = new UpstreamCall
                {
                    Method = method,
                    Url = url,
                    Status = (int)response.StatusCode,
                    ElapsedMs = stopwatch.ElapsedMilliseconds
                },
                Payload = string.IsNullOrWhiteSpace(raw) ? null : JsonNode.Parse(raw)
            };
        }
        catch (Exception ex)
        {
            return Failure(method, url, stopwatch, ex);
        }
        finally
        {
            request.Dispose();
        }
    }

    private void AddAuth(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(_options.AuthToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AuthToken);
        }
    }

    private string Absolute(string path) =>
        Uri.TryCreate(path, UriKind.Absolute, out _)
            ? path
            : $"{_options.BaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";

    private static async Task<UpstreamResult> FailureFromResponseAsync(
        string method, string url, HttpResponseMessage response, Stopwatch stopwatch, CancellationToken cancellationToken)
    {
        stopwatch.Stop();
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        return new UpstreamResult
        {
            Upstream = new UpstreamCall
            {
                Method = method,
                Url = url,
                Status = (int)response.StatusCode,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                // Truncated: an upstream error page can be a full HTML document.
                Error = body.Length > 2000 ? body[..2000] + " ...(truncated)" : body
            }
        };
    }

    private UpstreamResult Failure(string method, string url, Stopwatch stopwatch, Exception ex)
    {
        stopwatch.Stop();
        logger.LogWarning(ex, "Common Data API call failed: {Method} {Url}", method, url);

        return new UpstreamResult
        {
            Upstream = new UpstreamCall
            {
                Method = method,
                Url = url,
                Status = null,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Error = $"{ex.GetType().Name}: {ex.Message}"
            }
        };
    }
}
