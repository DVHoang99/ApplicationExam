using System;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Domain.Repository;

public interface ILogRepository
{
    Task AddAsync(LogEntry logEntry);
}
