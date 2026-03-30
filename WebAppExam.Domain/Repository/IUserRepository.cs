using System;
using WebAppExam.Domain.Common;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Domain.Repository;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
}
