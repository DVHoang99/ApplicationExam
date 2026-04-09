using System;

namespace WebAppExam.Application.Services;

public interface IHttpClientService
{
    Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        object? payload = null,
        CancellationToken cancellationToken = default);

    Task<T?> SendAsync<T>(
        HttpMethod method,
        string path,
        object? payload = null,
        CancellationToken cancellationToken = default) where T : class;

    void SetBaseAddress(string baseAddress);

    void AddDefaultHeader(string key, string value);

    void SetAuthorizationToken(string token);
}
