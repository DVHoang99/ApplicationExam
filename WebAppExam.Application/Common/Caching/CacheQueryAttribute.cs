using System;

namespace WebAppExam.Application.Common.Caching;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CacheQueryAttribute : Attribute
{
    public string Prefix { get; set; }
    public int ExpirationMinutes { get; set; }

    public CacheQueryAttribute(string prefix, int expirationMinutes = 5)
    {
        Prefix = prefix;
        ExpirationMinutes = expirationMinutes;
    }
}