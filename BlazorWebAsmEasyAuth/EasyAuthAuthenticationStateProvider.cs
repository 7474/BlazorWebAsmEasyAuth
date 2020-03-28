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

        private DateTimeOffset estimatedSessionExpireTime;

        public EasyAuthAuthenticationStateProvider(HttpClient httpClient, IJSRuntime jsRuntime,
        NavigationManager navigationManager, EasyAuthConfig config)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            _navigationManager = navigationManager;
            _config = config;
            estimatedSessionExpireTime = DateTimeOffset.MinValue;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            AuthToken token = await GetAuthToken();
            // TODO /.auth/refresh
            // https://docs.microsoft.com/ja-jp/azure/app-service/app-service-authentication-how-to#extend-session-token-expiration-grace-period
            if (token?.AuthenticationToken != null)
            {
                await SetSessionToken(token);
                try
                {
                    var authResponse = await _httpClient.GetStringAsync(_config.AzureFunctionAuthURL + Constants.AuthMeEndpoint);

                    if (_config.EnableLogToConsole)
                    {
                        Console.WriteLine(authResponse);
                    }

                    var authInfos = JsonSerializer.Deserialize<List<AuthInfo>>(authResponse);
                    var authenticationState = await ToAuthenticationState(authInfos);
                    return authenticationState;
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
        private void UpdateSessionExpireTime()
        {
            estimatedSessionExpireTime = DateTimeOffset.UtcNow.AddSeconds(Constants.SessionTokenExpiresSec);
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
        private Task<AuthenticationState> ToAuthenticationState(IEnumerable<AuthInfo> authInfos)
        {
            // TODO Provider �ŗL�̏������v�肻���Ȃ珈������
            // ���ƁA������ʓI�ȃN���[���͋�������
            var userClaims = authInfos.SelectMany(
                authInfo => authInfo.UserClaims.Select(
                    userClaim => new Claim(userClaim.Type, userClaim.Value)));
            var identity = new ClaimsIdentity(userClaims, "EasyAuth");
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }
        private bool ShouldRefreshSession()
        {
            return DateTimeOffset.UtcNow > estimatedSessionExpireTime.AddSeconds(-1);
        }
        private async Task RefreshSession()
        {
            try
            {
                // Cookie�ł̃Z�b�V������GET����ZUMO-AUTH�̏ꍇ��POST�炵��
                var emptyContent = new StringContent("");
                var refreshResponse = await _httpClient.PostAsync(_config.AzureFunctionAuthURL + Constants.RefreshEndpoint, emptyContent);
                // �Z�b�V�������L���ȊԂ�Twitter�Ȃǃ��t���b�V���̊T�O���Ȃ��ꍇ��BadRequest���Ċ����B�ǂ������B
                // TODO �a�ʂƂ�
                if (refreshResponse.IsSuccessStatusCode)
                {
                    var refreshResponseContent = await refreshResponse.Content.ReadAsStringAsync();
                    var authToken = JsonSerializer.Deserialize<AuthToken>(refreshResponseContent);
                    if (!string.IsNullOrEmpty(authToken.AuthenticationToken))
                    {
                        await SetSessionToken(authToken);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to refresh session " + e.Message);
                await InvalidateSessionToken();
                NotifyAuthenticationStateChanged();
            }
        }
        public async Task Logout()
        {
            var authresponse = await _httpClient.GetAsync(_config.AzureFunctionAuthURL + Constants.LogOutEndpoint);
            await InvalidateSessionToken();
            if (authresponse.IsSuccessStatusCode)
            {
                NotifyAuthenticationStateChanged();
            }
        }
        private async Task SetSessionToken(AuthToken token)
        {
            _httpClient.DefaultRequestHeaders.Add("X-ZUMO-AUTH", token.AuthenticationToken);
            await LocalStorage.SetAsync(_jsRuntime, "authtoken", token);
            UpdateSessionExpireTime();
        }
        private async Task InvalidateSessionToken()
        {
            _httpClient.DefaultRequestHeaders.Remove("X-ZUMO-AUTH");
            await LocalStorage.DeleteAsync(_jsRuntime, "authtoken");
        }
        public void NotifyAuthenticationStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        public async Task<HttpClient> GetZumoAuthedClientAsync()
        {
            if (ShouldRefreshSession())
            {
                await RefreshSession();
            }

            // XXX ����͔F�؏�Ԃ��Ăяo�����Ŕ��f����K�v������B
            // �Ȃɂ����������͂��肻���B�P�Ƀg�[�N����ԋp���邾���ł�������������Ȃ��B
            //var state = await GetAuthenticationStateAsync();
            //return _httpClient.DefaultRequestHeaders.Contains("X-ZUMO-AUTH") ? _httpClient : null;
            return _httpClient;
        }
    }
}