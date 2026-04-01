using System;
using System.Linq.Expressions;
using Hangfire;
using WebAppExam.Application.Services;

namespace WebAppExam.Infrastructure.Services;

public class HangfireJobService : IJobService
{
    private readonly IBackgroundJobClient _hangfireClient;

    public HangfireJobService(IBackgroundJobClient hangfireClient)
    {
        _hangfireClient = hangfireClient;
    }

    public void Enqueue(Expression<Action> methodCall)
    {
        _hangfireClient.Enqueue(methodCall);
    }
}
