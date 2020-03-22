using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Web;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using System.Linq;
using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace BlazorWebAsmEasyAuth
{
    public class EasyAuthAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly NavigationManager _navigationManager;
        private readonly EasyAuthConfig _config;

        public EasyAuthAuthenticationStateProvider(HttpClient httpClient, IJSRuntime jsRuntime,
        NavigationManager navigationManager, EasyAuthConfig config)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            _navigationManager = navigationManager;
            _config = config;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            AuthToken token = await GetAuthToken();
            // TODO /.auth/refresh
            // https://docs.microsoft.com/ja-jp/azure/app-service/app-service-authentication-how-to#extend-session-token-expiration-grace-period
            if (token?.AuthenticationToken != null)
            {
                _httpClient.DefaultRequestHeaders.Add("X-ZUMO-AUTH", token.AuthenticationToken);
                try
                {
                    var authResponse = await _httpClient.GetStringAsync(_config.AzureFunctionAuthURL + Constants.AuthMeEndpoint);

                    //To see the response uncomment the line below
                    //Console.WriteLine(authresponse);

                    await LocalStorage.SetAsync(_jsRuntime, "authtoken", token);
                    var authInfo = JsonSerializer.Deserialize<List<AuthInfo>>(authResponse);
                    switch (authInfo[0].ProviderName)
                    {
                        case "twitter": return await GetTwitterClaims(authInfo[0]);
                        default: break;
                    }
                    // TODO 他のプロバイダ対応。
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Unable to authenticate " + e.Message);
                    _httpClient.DefaultRequestHeaders.Remove("X-ZUMO-AUTH");
                }
            }
            await LocalStorage.DeleteAsync(_jsRuntime, "authtoken");
            var identity = new ClaimsIdentity();
            return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }
        private async Task<AuthToken> GetAuthToken()
        {
            var currentUri = new Uri(_navigationManager.Uri);
            string authTokenFragment = HttpUtility.UrlDecode(currentUri.Fragment);
            if (string.IsNullOrEmpty(authTokenFragment))
            {
                return await LocalStorage.GetAsync<AuthToken>(_jsRuntime, "authtoken");
            }
            Regex getJsonRegEx = new Regex(@"\{(.|\s)*\}");
            MatchCollection matches = getJsonRegEx.Matches(authTokenFragment);
            if (matches.Count == 1)
            {
                AuthToken authToken;
                try
                {
                    authToken = JsonSerializer.Deserialize<AuthToken>(matches[0].Value);
                }
                // JsonSerializer in preview, don't know what it will thow.
                catch (Exception)
                {
                    Console.WriteLine("Error in authentication token");
                    return new AuthToken();
                }
                // Remove token.
                _navigationManager.NavigateTo(_config.BlazorWebsiteURL, false);
                return authToken;
            }
            return new AuthToken();
        }
        private Task<AuthenticationState> GetTwitterClaims(AuthInfo authInfo)
        {
            List<Claim> userClaims = new List<Claim>();
            foreach (AuthUserClaim userClaim in authInfo.UserClaims)
            {
                userClaims.Add(new Claim(userClaim.Type, userClaim.Value));
            }
            var identity = new ClaimsIdentity(userClaims, "EasyAuth");
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }
        public async Task Logout()
        {
            var authresponse = await _httpClient.GetAsync(_config.AzureFunctionAuthURL + Constants.LogOutEndpoint);
            _httpClient.DefaultRequestHeaders.Remove("X-ZUMO-AUTH");
            await LocalStorage.DeleteAsync(_jsRuntime, "authtoken");
            if (authresponse.IsSuccessStatusCode)
            {
                NotifyAuthenticationStateChanged();
            }
        }
        public void NotifyAuthenticationStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        public async Task<HttpClient> GetZumoAuthedClientAsync()
        {
            // XXX 現状は認証状態を呼び出し側で判断する必要がある。
            // なにかいいやり方はありそう。単にトークンを返却するだけでもいいかもしれない。
            //var state = await GetAuthenticationStateAsync();
            //return _httpClient.DefaultRequestHeaders.Contains("X-ZUMO-AUTH") ? _httpClient : null;
            return _httpClient;
        }
    }
}