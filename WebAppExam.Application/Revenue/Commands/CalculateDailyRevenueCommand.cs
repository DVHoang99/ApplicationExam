using System;
using System.Windows.Input;
using MediatR;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Revenue.Commands;

public class CalculateDailyRevenueCommand : ICommand<Unit>;

