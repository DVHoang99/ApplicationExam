using System;

namespace WebAppExam.BackgroundJobs.Services;

public interface IOutboxRetryJob
{
    Task ExecuteAsync();
}
