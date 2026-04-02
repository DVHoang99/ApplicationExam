using System;

namespace WebAppExam.Application.Common.Caching;

public interface ICacheLockService
{
    Task<List<string>> AcquireMultipleLocksAsync(IEnumerable<string> lockKeys, string lockToken, TimeSpan expiry);
    Task ReleaseMultipleLocksAsync(IEnumerable<string> lockKeys, string lockToken);
}
