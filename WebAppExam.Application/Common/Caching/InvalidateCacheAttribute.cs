using System;

namespace WebAppExam.Application.Common.Caching;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class InvalidateCacheAttribute : Attribute
{
    public string[] Prefixes { get; set; }

    public InvalidateCacheAttribute(params string[] prefixes)
    {
        Prefixes = prefixes;
    }
}
