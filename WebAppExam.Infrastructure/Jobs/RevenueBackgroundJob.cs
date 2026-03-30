using System;
using MediatR;
using WebAppExam.Application.Revenue.Commands;

namespace WebAppExam.Infrastructure.Jobs;

public class RevenueBackgroundJob
{
    private readonly ISender _mediator;

    public RevenueBackgroundJob(ISender mediator)
    {
        _mediator = mediator;
    }

    // Hangfire calls this entry point
    public async Task RunDailyCalculation()
    {
        // Simply trigger the Use Case through MediatR
        await _mediator.Send(new CalculateDailyRevenueCommand());
    }
}
