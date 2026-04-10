using System;
using System.Reflection;
using MediatR;

namespace WebAppExam.Application.Common.Events;

public static class EventRegistry
{
    private static readonly Dictionary<string, Type> _eventTypes = new();

    public static void Initialize(Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(INotification).IsAssignableFrom(t));

        foreach (var type in types)
        {
            _eventTypes[type.Name] = type;
        }
    }

    public static Type GetEventType(string messageType)
    {
        if (_eventTypes.TryGetValue(messageType, out var type))
        {
            return type;
        }

        throw new Exception($"Không tìm thấy Event Type nào có tên là: {messageType}");
    }
}
