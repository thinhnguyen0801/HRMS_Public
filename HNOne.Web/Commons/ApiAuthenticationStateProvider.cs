using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace HNOne.Web.Commons
{
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly NavigationManager _nav;
        public ApiAuthenticationStateProvider(ILocalStorageService localStorage, NavigationManager nav)
        {
            _localStorage = localStorage;
            _nav = nav;
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var savedToken = await _localStorage.GetItemAsync<string>("authToken");
                if (string.IsNullOrWhiteSpace(savedToken))
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(savedToken), "jwt")));
            }
            catch (InvalidOperationException)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            keyValuePairs!.TryGetValue(nameof(ClaimTypes.Role), out object? roles);
            if (roles != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, roles.ToString() + ""));
                keyValuePairs.Remove(ClaimTypes.Role);
            }
            claims.AddRange(keyValuePairs!.Select(kvp => new Claim(kvp.Key, $"{kvp.Value}")));
            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
