using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using WebAppExam.Application.Auth.Services;
using WebAppExam.Application.Behaviors;
using WebAppExam.Application.Common.Events;
using WebAppExam.Application.Customers.Services;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Application.Orders.Services;
using WebAppExam.Application.OutboxMessages;
using WebAppExam.Application.OutboxMessages.Services;
using WebAppExam.Application.Products.Services;
using WebAppExam.Application.User.Services;
using WebAppExam.Domain.Events;

namespace WebAppExam.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Configure MediatR and Behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly(), typeof(TransactionBehavior<,>).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        // Register Application Services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOutboxService, OutboxService>();

        EventRegistry.Initialize(typeof(OrderCreatedEvent).Assembly);

        return services;
    }
}
