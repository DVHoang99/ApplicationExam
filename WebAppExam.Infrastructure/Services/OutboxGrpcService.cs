using Grpc.Core;
using WebAppExam.GrpcContracts.Protos;
using WebAppExam.Domain.Repository;
using WebAppExam.Domain.Enum;
using WebAppExam.Domain.Entity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace WebAppExam.Infrastructure.Services;

public class OutboxGrpcService : OutboxGrpc.OutboxGrpcBase
{
    private readonly IOutboxMessageRepository _outboxMessageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OutboxGrpcService> _logger;

    public OutboxGrpcService(IOutboxMessageRepository outboxMessageRepository, IUnitOfWork unitOfWork, ILogger<OutboxGrpcService> logger)
    {
        _outboxMessageRepository = outboxMessageRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<OutboxMessageResponse> GetOutboxMessage(OutboxMessageRequest request, ServerCallContext context)
    {
        if (!Ulid.TryParse(request.Id, out var outboxId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Outbox ID format"));
        }

        var message = await _outboxMessageRepository.GetByIdAsync(outboxId, context.CancellationToken);

        if (message == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Outbox message with ID {request.Id} not found"));
        }

        return new OutboxMessageResponse
        {
            Id = message.Id.ToString(),
            Type = message.Type,
            Content = message.Content
        };
    }

    public override async Task<UpdateStatusResponse> UpdateOutboxStatus(UpdateStatusRequest request, ServerCallContext context)
    {
        if (!Ulid.TryParse(request.Id, out var outboxId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Outbox ID format"));
        }

        var message = await _outboxMessageRepository.GetByIdAsync(outboxId, context.CancellationToken);

        if (message == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Outbox message with ID {request.Id} not found"));
        }

        var status = (OutboxMessageStatus)request.Status;
        message.UpdateStatus(status, request.Error);

        _outboxMessageRepository.Update(message);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("[gRPC] Outbox {Id} status updated to {Status}", request.Id, status);

        return new UpdateStatusResponse { Success = true };
    }
}
