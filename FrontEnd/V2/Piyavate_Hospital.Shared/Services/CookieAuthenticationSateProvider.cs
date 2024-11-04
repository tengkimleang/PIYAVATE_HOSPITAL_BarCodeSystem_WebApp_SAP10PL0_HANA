using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using Piyavate_Hospital.Shared.ViewModels;

namespace Piyavate_Hospital.Shared.Services;

public class CookieAuthenticationSateProvider(HttpClient httpClient) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = await GetClaimAsync();

        return new AuthenticationState(user);
    }

    public async Task<HttpResponseMessage> Login(string userName, string password)
    {
        var rs = await httpClient.PostAsJsonAsync("/api/login", new Dictionary<string, string>
        {
            { "UserName", userName },
            { "Password", password }
        });
        NotifyAuthenticationStateChanged(Task.FromResult(await GetAuthenticationStateAsync()));
        return rs;
    }

    public async Task LogOut()
    {
        var rs = await httpClient.PostAsJsonAsync("/api/logout", new Dictionary<string, string>());
        NotifyAuthenticationStateChanged(Task.FromResult(await GetAuthenticationStateAsync()));
    }

    private async Task<ClaimsPrincipal> GetClaimAsync()
    {
        var response = await httpClient.PostAsJsonAsync("/api/login", new Dictionary<string, string>());

        var content = await response.Content.ReadAsStringAsync();
        var userClaims = JsonSerializer.Deserialize<Dictionary<string, string>>(content);
        Console.WriteLine(content);
        if (userClaims == null || !userClaims.ContainsKey("token"))
        {
            return new ClaimsPrincipal();
        }
        else
        {
            return new ClaimsPrincipal(
                new ClaimsIdentity(
                    userClaims.Select(kv => new Claim(kv.Key, kv.Value)),
                    "custom"
                ));
        }
    }
}