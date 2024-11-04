using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Piyavate_Hospital.Application.Authorize;
using Piyavate_Hospital.Application.DeliveryOrder;
using Piyavate_Hospital.Application.GoodReceiptPo;
using Piyavate_Hospital.Application.GoodReturn;
using Piyavate_Hospital.Application.IssueForProductions;
using Piyavate_Hospital.Application.Layout;
using Piyavate_Hospital.Application.SaleOrder;


namespace Piyavate_Hospital.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(option => option.RegisterServicesFromAssemblyContaining(typeof(DependencyInjection)));
        var assembly = Assembly.GetAssembly(typeof(DependencyInjection));
        var validatorTypes = assembly?.GetTypes()
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>)))
            .ToList();

        foreach (var validatorType in validatorTypes!)
        {
            var interfaceType = validatorType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>));
            services.AddScoped(interfaceType, validatorType);
        }

        return services;
    }
}
