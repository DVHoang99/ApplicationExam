using System;
using WebAppExam.Application.Services;

namespace WebAppExam.BackgroundJobs.Services;

public class SystemCurrentUserService : ICurrentUserService
{
    public string UserId => "system";
    public string Username => "System Background Job";
}
