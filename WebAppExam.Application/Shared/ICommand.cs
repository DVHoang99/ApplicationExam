using MediatR;

namespace WebAppExam.Application.Shared;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}