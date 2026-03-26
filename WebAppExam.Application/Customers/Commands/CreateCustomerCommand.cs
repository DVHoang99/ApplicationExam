using System;
using MediatR;
using WebAppExam.Application.Shared;
using WebAppExam.Domain;
using WebAppExam.Domain.Repository;
using WebAppExam.Infrastructure.UnitOfWork;

namespace WebAppExam.Application.Customers.Command;

public class CreateCustomerCommand : ICommand<Ulid>
{
    public string CustomerName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}