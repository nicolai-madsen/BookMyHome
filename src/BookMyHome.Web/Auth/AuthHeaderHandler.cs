using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace BookMyHome.Web.Auth;

public class AuthHeaderHandler(IJSRuntime js) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var token = await js.InvokeAsync<string?>("sessionStorage.getItem", ct, CustomAuthStateProvider.TokenKey);
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, ct);
    }
}