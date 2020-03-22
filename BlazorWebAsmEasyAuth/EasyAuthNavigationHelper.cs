using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace BlazorWebAsmEasyAuth
{
    public class EasyAuthNavigationHelper
    {
        private readonly NavigationManager _navigationManager;
        private readonly EasyAuthConfig _config;

        public EasyAuthNavigationHelper(NavigationManager navigationManager, EasyAuthConfig config)
        {
            _navigationManager = navigationManager;
            _config = config;
        }

        public string GetLoginUrl(EasyAuthProvider provider)
        {
            // TODO Top以外にも戻れるように
            return new Uri(new Uri(new Uri(
                    _config.AzureFunctionAuthURL),
                    Constants.LogInEndpointWithoutProvider),
                    provider.ToString().ToLower()).ToString()
                + Constants.PostloginRedirect
                + HttpUtility.UrlEncode(_config.BlazorWebsiteURL)
                + Constants.LoginMode;
        }
    }
}
