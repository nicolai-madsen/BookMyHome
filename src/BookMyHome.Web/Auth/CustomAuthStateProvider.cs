using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace BookMyHome.Web.Auth;

public class CustomAuthStateProvider(IJSRuntime js) : AuthenticationStateProvider
{
    public const string TokenKey = "authToken";
    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await js.InvokeAsync<string?>("sessionStorage.getItem", TokenKey);
        if (string.IsNullOrWhiteSpace(token))
            return Anonymous;

        var claims = JwtParser.ParseClaim(token);
        if (JwtParser.IsExpired(claims))
        {
            await js.InvokeVoidAsync("sessionStorage.removeItem", TokenKey);
            return Anonymous;
        }

        // "jwt" = authenticationType. Uden den regnes brugeren som IKKE logget ind.
        var identity = new ClaimsIdentity(claims, "jwt", nameType: "unique_name", roleType: "role");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyChanged() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}