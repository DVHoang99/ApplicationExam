using System;
using System.Text.Json;
using KafkaFlow.Producers;
using MediatR;
using WebAppExam.Application.Common.Events;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Domain.Repository;

namespace WebAppExam.BackgroundJobs.Services;

public class OutboxRetryJob : IOutboxRetryJob
{
    private readonly IOutboxMessageRepository _outboxMessageRepository;
    private readonly IProducerAccessor _producerAccessor;
    private readonly ILogger<OutboxRetryJob> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public OutboxRetryJob(IOutboxMessageRepository outboxMessageRepository, IProducerAccessor producerAccessor, ILogger<OutboxRetryJob> logger, IUnitOfWork unitOfWork, IMediator mediator)
    {
        _outboxMessageRepository = outboxMessageRepository;
        _producerAccessor = producerAccessor;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task ExecuteAsync()
    {
        Console.WriteLine("--- HANGFIRE: Bắt đầu quét Outbox qua Repository ---");
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);

        var pendingMessages = await _outboxMessageRepository.GetPendingMessagesAsync(100, oneHourAgo);

        if (!pendingMessages.Any())
        {
            Console.WriteLine("Outbox is clean. No pending messages found.");
            return;
        }
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        foreach (var msg in pendingMessages)
        {
            try
            {
                Console.WriteLine("Đang xử lý lại Message ID: {Id}", msg.Id);

                Type eventType = EventRegistry.GetEventType(msg.Type);
                object eventObj = JsonSerializer.Deserialize(msg.Content, eventType, jsonOptions);

                if (eventObj != null)
                {
                    // 3. MediatR quá đỉnh: Mày ném object vào Publish, 
                    // nó sẽ tự ép về đúng kiểu và gọi INotificationHandler<T> của mày!
                    await _mediator.Publish(eventObj);
                }
                Console.WriteLine("-> THÀNH CÔNG: Đã gửi lại Message ID: {Id}", msg.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine("-> THẤT BẠI: Lỗi khi gửi lại Message ID: {Id}", msg.Id);
                // msg.Status = "Failed";
            }
        }

        Console.WriteLine("--- HANGFIRE: Hoàn tất đợt quét Outbox ---");
    }
}
