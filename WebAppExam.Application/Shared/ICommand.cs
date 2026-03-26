using System;
using MediatR;

namespace WebAppExam.Application.Shared;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

public interface ICommand : IRequest
{
}