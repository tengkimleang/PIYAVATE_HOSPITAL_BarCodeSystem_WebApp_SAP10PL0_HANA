using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Piyavate_Hospital.Application;
using Piyavate_Hospital.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    // options.AddPolicy("CorsPolicy", policy =>
    // {
    //     policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:5120");
    // });
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();
// app.UseCors("CorsPolicy");
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.MapControllers();
app.Run();