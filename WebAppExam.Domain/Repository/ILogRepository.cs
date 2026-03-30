using System;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Domain.Repository;

public interface ILogRepository
{
    Task AddAuditLogEntryAsync(AuditLogEntry auditEntry);
}


