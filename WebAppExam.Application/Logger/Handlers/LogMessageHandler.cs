using System;
using KafkaFlow;
using WebAppExam.Application.Logger.DTOs;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Repository;
using Microsoft.Extensions.Logging;

namespace WebAppExam.Application.Logger.Handlers;

public class LogMessageHandler : IMessageHandler<AuditLogMessageDTO>
{
    private readonly ILogRepository _logRepository;
    private readonly ILogger<LogMessageHandler> _logger;

    public LogMessageHandler(ILogRepository logRepository, ILogger<LogMessageHandler> logger)
    {
        _logRepository = logRepository;
        _logger = logger;
    }

    public async Task Handle(IMessageContext context, AuditLogMessageDTO messageDto)
    {
        try
        {
            _logger.LogInformation("[KAFKA AUDIT] Captured: {Action} on {EntityName} (ID: {PrimaryKey}) by {ChangedBy}", messageDto.Action, messageDto.EntityName, messageDto.PrimaryKey, messageDto.ChangedBy);

            var auditEntry = new AuditLogEntry(
                messageDto.EntityName,
                messageDto.Action,
                messageDto.PrimaryKey,
                messageDto.OldValues,
                messageDto.NewValues,
                messageDto.ChangedBy ?? "System"
            );

            await _logRepository.AddAuditLogEntryAsync(auditEntry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing audit log message");
        }
    }
}