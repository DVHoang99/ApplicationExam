using System;
using KafkaFlow;
using WebAppExam.Application.Logger.DTOs;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Logger.Handlers;

public class LogMessageHandler : IMessageHandler<AuditLogMessageDTO>
{
    private readonly ILogRepository _logRepository;

    public LogMessageHandler(ILogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task Handle(IMessageContext context, AuditLogMessageDTO messageDto)
    {
        try
        {
            Console.WriteLine($"[KAFKA AUDIT] Captured: {messageDto.Action} on {messageDto.EntityName} (ID: {messageDto.PrimaryKey}) by {messageDto.ChangedBy}");

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
            Console.WriteLine(ex.Message);
        }
    }
}