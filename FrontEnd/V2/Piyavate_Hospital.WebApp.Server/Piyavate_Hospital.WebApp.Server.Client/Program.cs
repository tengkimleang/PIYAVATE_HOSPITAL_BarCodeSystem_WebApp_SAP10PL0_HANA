using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;

using Piyavate_Hospital.Shared.Services;
using Piyavate_Hospital.Shared.Shared;
using Piyavate_Hospital.Shared.ViewModels;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services
        .AddFluentUIComponents()
        // .AddScoped<IFormFactor, FormFactor>()
        .AddViewModels();
// builder.Services.AddSingleton<ILoadMasterData, LoadMasterData>();
// builder.Services.AddHostedService<LoadMasterDataService>();
builder.Services.AddBlazoredLocalStorage();
await builder.Build().RunAsync();
