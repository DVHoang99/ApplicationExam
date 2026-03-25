using MediatR;
using WebAppExam.Application.Orders.DTOs;

namespace WebAppExam.Application.Orders.Queries;

public class GetOrderListQuery() : IRequest<List<OrderDto>>;