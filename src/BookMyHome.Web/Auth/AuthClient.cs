using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace BookMyHome.Web.Auth;

public record LoginResponse(string Token);   // tilpas til det, din AuthService returnerer

public class AuthClient(HttpClient http, IJSRuntime js, CustomAuthStateProvider authState)
{
    public async Task<bool> LoginAsync(string username, string password)
    {
        var response = await http.PostAsJsonAsync("auth/login", new { username, password });
        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        await js.InvokeVoidAsync("sessionStorage.setItem", CustomAuthStateProvider.TokenKey, result!.Token);
        authState.NotifyChanged();
        return true;
    }

    public async Task LogoutAsync()
    {
        await js.InvokeVoidAsync("sessionStorage.removeItem", CustomAuthStateProvider.TokenKey);
        authState.NotifyChanged();
    }
}