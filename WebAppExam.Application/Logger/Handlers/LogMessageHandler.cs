using System;
using KafkaFlow;
using WebAppExam.Application.Logger.DTOs;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Logger.Handlers;

public class LogMessageHandler : IMessageHandler<LogMessageDTO>
{
    private readonly ILogRepository _logRepository;

    public LogMessageHandler(ILogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task Handle(IMessageContext context, LogMessageDTO messageDto)
    {
        try
        {
            Console.WriteLine($"[KAFKA] logged: {messageDto.Level} - {messageDto.Message}");

            var logEntry = new LogEntry(
                messageDto.Level,
                messageDto.ServiceName,
                messageDto.Message,
                messageDto.Exception);

            await _logRepository.AddAsync(logEntry);

            Console.WriteLine("[MONGO] Logged to database!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}