using System;
using System.Security.Cryptography.X509Certificates;
using Confluent.Kafka.Admin;
using MediatR;
using WebAppExam.Domain;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Revenue.Commands;

public class CalculateDailyRevenueCommandHandler : IRequestHandler<CalculateDailyRevenueCommand, Unit>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDailyRevenueRepository _dailyRevenueRepository;


    public CalculateDailyRevenueCommandHandler(IOrderRepository orderRepository, IDailyRevenueRepository dailyRevenueRepository)
    {
        _orderRepository = orderRepository;
        _dailyRevenueRepository = dailyRevenueRepository;
    }

    public async Task<Unit> Handle(CalculateDailyRevenueCommand request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var orders = await _orderRepository.GetByDateAsync(today);

        var totalOrders = orders.Count();

        var totalRevenue = orders.Sum(x => x.TotalAmount);

        var dailyTotal = new DailyRevenue(today, totalOrders, totalRevenue);

        await _dailyRevenueRepository.AddAsync(dailyTotal);

        return Unit.Value;
    }
}