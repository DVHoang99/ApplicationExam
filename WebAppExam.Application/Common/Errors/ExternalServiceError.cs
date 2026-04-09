using FluentResults;

namespace WebAppExam.Application.Common.Errors;

public class ExternalServiceError : Error
{
    public int? StatusCode { get; }
    public string ServiceName { get; }

    public ExternalServiceError(string serviceName, int? statusCode = null, string? message = null)
        : base(message ?? $"External service '{serviceName}' returned an error")
    {
        ServiceName = serviceName;
        StatusCode = statusCode;
        Metadata.Add("ServiceName", serviceName);
        if (statusCode.HasValue)
            Metadata.Add("StatusCode", statusCode.Value);
    }

    public static ExternalServiceError InventoryServiceError(int statusCode, string details)
    {
        return new ExternalServiceError("Inventory Service", statusCode, $"Inventory Service error ({statusCode}): {details}");
    }

    public static ExternalServiceError WarehouseServiceError(int statusCode, string details)
    {
        return new ExternalServiceError("Warehouse Service", statusCode, $"Warehouse Service error ({statusCode}): {details}");
    }
}
