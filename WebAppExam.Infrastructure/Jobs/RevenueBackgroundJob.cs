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

    public async Task RunDailyCalculation()
    {
        await _mediator.Send(new CalculateDailyRevenueCommand());
    }
}
