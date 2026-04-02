using System;
using MediatR;

namespace WebAppExam.Application.Features.Orders.Events;

public record OrderCreatedNotification(
        Ulid OrderId,
        int Amount,
        DateTime CreatedAt
    ) : INotification;