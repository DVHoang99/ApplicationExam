using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Configuration;
using WebAppExam.Domain.Common;

namespace WebAppExam.Infrastructure.Common.Interceptors;

public class InternalApiKeyInterceptor : Interceptor
{
    private readonly string _apiKey;

    public InternalApiKeyInterceptor(IConfiguration configuration)
    {
        _apiKey = configuration[Constants.ConfigKeys.InternalApiKeyConfigPath] 
                  ?? throw new InvalidOperationException($"Configuration {Constants.ConfigKeys.InternalApiKeyConfigPath} is missing.");
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var metadata = context.Options.Headers ?? new Metadata();
        metadata.Add(Constants.HttpHeader.InternalKeyHeader, _apiKey);

        var newOptions = context.Options.WithHeaders(metadata);
        var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, newOptions);

        return continuation(request, newContext);
    }
}
