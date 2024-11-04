using FluentValidation;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System.Reflection;
using Piyavate_Hospital.Shared.Services;

namespace Piyavate_Hospital.Shared.ViewModels;

public static class Dependencies
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        #region Add Refit Client

        // services.AddScoped<AuthenticatedHttpClientHandler>();
        services.AddRefitClient<IApiService>()
            .ConfigureHttpClient(static client =>
            {
                client.Timeout = TimeSpan.FromMinutes(10);
                client.BaseAddress = new Uri(ApiConstant.ApiUrl);
            }).AddStandardResilienceHandler(static options =>
                options.Retry = new WebOrMobileHttpRetryStrategyOptions());
        #endregion

        #region Add ViewModel

        services.AddSingleton<ApiService>();
        services.AddScoped<GoodReceptPoViewModel>();
        services.AddScoped<DeliveryOrderViewModel>();
        services.AddScoped<InventoryTransferViewModel>();
        services.AddScoped<IssueProductionOrderViewModel>();
        services.AddScoped<ReturnFromComponentViewModel>();
        services.AddScoped<ReturnViewModel>();
        services.AddScoped<GoodReturnViewModel>();
        services.AddScoped<ArCreditMemoViewModel>();
        services.AddScoped<InventoryCountingViewModel>();
        services.AddScoped<ProductionProcessViewModel>();
        services.AddScoped<ReceiptsFinishedGoodsViewModel>();
        services.AddScoped<ReturnRequestViewModel>();

        #endregion

        #region Validator

        var assembly = Assembly.GetAssembly(typeof(Dependencies));
        var validatorTypes = assembly?.GetTypes()
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>)))
            .ToList();

        foreach (var validatorType in validatorTypes!)
        {
            var interfaceType = validatorType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>));
            services.AddScoped(interfaceType, validatorType);
        }

        #endregion

        services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri(ApiConstant.ApiBase),
        });
        services.AddCascadingAuthenticationState();
        services.AddAuthorizationCore();
        services.AddScoped<CookieAuthenticationSateProvider>();
        services.AddScoped<AuthenticationStateProvider>(sp =>
            sp.GetRequiredService<CookieAuthenticationSateProvider>()
        );
        return services;
    }
}