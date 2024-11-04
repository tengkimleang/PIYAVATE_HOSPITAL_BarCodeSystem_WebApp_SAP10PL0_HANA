using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Piyavate_Hospital.Application.Common.Interfaces;
using Piyavate_Hospital.Application.Common.Interfaces.Setting;
using Piyavate_Hospital.Domain.Common;
using Piyavate_Hospital.Infrastructure.Common.Setting;
using Piyavate_Hospital.Infrastructure.Common.Persistence;
using Piyavate_Hospital.Infrastructure.Common.QueryData;
using Piyavate_Hospital.Infrastructure.LoadConnection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Piyavate_Hospital.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, ConfigurationManager configure)
    {
        // Add connection settings to the service collection
        services.Configure<ConnectionSettings>(configure.GetSection(ConnectionSettings.SectionName));
        services.Configure<JwtSettings>(configure.GetSection(JwtSettings.SectionName));
        // Add connection provider to the service collection
        services.AddSingleton<IConnection, Connection>();
        // Add Unit of Work to the service collection
        services.AddScoped<IUnitOfWork, Connection>();
        // Add Query Data to the service collection
        services.AddControllers().AddNewtonsoftJson();
        //// Add JWT token generator to the service collection
        services.AddSingleton<IJwtRegister, JwtRegister>();
        services.AddSingleton<IConvertRecordsetToDataTable, ConvertRecordsetToDataTable>();
        // Add data provider to the service collection
        services.AddSingleton<IDataProviderRepository, DataProviderRepository>();
        services.AddSingleton<IReportLayout, ReportLayout>();
        // Add user repository to the service collection
        services.AddHostedService<LoadConnectionSapService>();
        #region ConfigureJWTToken
        var tokenvalidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configure.GetSection("JwtSetting:SecretKey").Value ?? "")),
            ValidateIssuer = false,
            ValidateAudience = false,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        services.AddSingleton(tokenvalidationParameters);
        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(x =>
        {
            x.SaveToken = true;
            x.TokenValidationParameters = tokenvalidationParameters;
        });
        #endregion
        return services;
    }
}