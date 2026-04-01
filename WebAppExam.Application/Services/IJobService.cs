using System;
using System.Linq.Expressions;

namespace WebAppExam.Application.Services;

public interface IJobService
{
    void Enqueue(Expression<Action> methodCall);
}
