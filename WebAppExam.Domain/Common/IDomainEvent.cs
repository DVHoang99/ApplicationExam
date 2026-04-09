using System;
using MediatR;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Domain.Entity;

public interface IDomainEvent : INotification { }
