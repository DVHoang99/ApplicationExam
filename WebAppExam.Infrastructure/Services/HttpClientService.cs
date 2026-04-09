using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using WebAppExam.Application.Services;

namespace WebAppExam.Infrastructure.Services;

public class HttpClientService : IHttpClientService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public HttpClientService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public void SetBaseAddress(string baseAddress)
    {
        _httpClient.BaseAddress = new Uri(baseAddress);
    }

    public void AddDefaultHeader(string key, string value)
    {
        _httpClient.DefaultRequestHeaders.Add(key, value);
    }

    public void SetAuthorizationToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        object? payload = null,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(method, path, payload);

        try
        {
            return await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new BadHttpRequestException($"HTTP request failed: {ex.Message}", ex);
        }
    }

    public async Task<T?> SendAsync<T>(
        HttpMethod method,
        string path,
        object? payload = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var response = await SendAsync(method, path, payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return default;

        var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
        var node = JsonNode.Parse(jsonString);
        return node?["data"].Deserialize<T>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, object? payload)
    {
        var request = new HttpRequestMessage(method, path);

        // Add internal API key if configured
        var internalKey = _configuration["InternalSettings:ApiKey"];
        if (!string.IsNullOrEmpty(internalKey))
        {
            request.Headers.Add("X-Internal-Key", internalKey);
        }

        // Add authorization header from current context
        var context = _httpContextAccessor.HttpContext;
        if (context != null && context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            request.Headers.TryAddWithoutValidation("Authorization", authHeader.ToString());
        }

        // Add payload if provided
        if (payload != null)
        {
            request.Content = JsonContent.Create(payload);
        }

        return request;
    }
}
