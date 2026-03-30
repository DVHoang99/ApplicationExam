using System;
using System.Security.Claims;
using WebAppExam.Application.Services;

namespace WebAppExam.API.Services;

public class CurrentUserService : ICurrentUserService 
{
    private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
        
        public string Username => _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
}
