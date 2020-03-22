# Blazor WebAsm Easy Auth

Use EasyAuth of Azure Function App from Blazor WebAssembly application.


## Usage

### Configure App Service

- Configure any ID provider.
	- See: https://docs.microsoft.com/en-us/azure/app-service/overview-authentication-authorization#identity-providers
- And Set `allowed-external-redirect-urls` to your URL hosting Blazor app.
	- TODO Image.

- Set `WEBSITE_AUTH_PRESERVE_URL_FRAGMENT` to  `true` .
	- See: 
https://docs.microsoft.com/en-us/azure/app-service/app-service-authentication-how-to#preserve-url-fragments


### Configure Blazor WebAssembly application

DI for EasyAuth settings.

```cs
public static async Task Main(string[] args)
{
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<App>("app");

    builder.Services.AddEasyAuth(new EasyAuthConfig()
    {
        BlazorWebsiteURL = "https://<client side blazor site url>",
        AzureFunctionAuthURL = "https://<your Azure function name>.azurewebsites.net",   
    });

    await builder.Build().RunAsync();
}
```

To log in, go to the login URL for each EazyAuth ID provider.

To log out, call the EasyAuthAuthenticationStateProvider's Logout method. State changes are notified by the provider.

```razor
@inject NavigationManager _navigationManager
@inject EasyAuthNavigationHelper _navigationHelper
@inject EasyAuthAuthenticationStateProvider _easyAuthAuthenticationStateProvider

<AuthorizeView>
    <Authorizing>
        <NavLink class="nav-link" href="">Authenticating</NavLink>
    </Authorizing>
    <NotAuthorized>
        <NavLink class="nav-link" href="" @onclick="@LoginWithTwitter">
            Sign In
        </NavLink>
    </NotAuthorized>
    <Authorized>
        <NavLink class="nav-link" href="">
            <span>@context.User.Identity.Name</span>
        </NavLink>
        <NavLink class="nav-link" href="" @onclick="@Logout">
            Sign Out
        </NavLink>
    </Authorized>
</AuthorizeView>

@code {
    private void LoginWithTwitter()
    {
        _navigationManager.NavigateTo(_navigationHelper.GetLoginUrl(EasyAuthProvider.Twitter));
    }

    private async void Logout()
    {
        await _easyAuthAuthenticationStateProvider.Logout();
    }
}
```

And you can get HttpClient with Zumo authentication header set.

```razor
@attribute [Authorize]
@inject EasyAuthAuthenticationStateProvider _easyAuthAuthenticationStateProvider

@code {
    [CascadingParameter]
    private Task<AuthenticationState> authenticationStateTask { get; set; }

    protected override async Task OnInitializedAsync()
    {
        // Currently, it is necessary to wait for confirmation of the authentication status.
        await authenticationStateTask;
        var httpClient = _easyAuthAuthenticationStateProvider.GetZumoAuthedClientAsync();
    }
}
```

The rest is the same as other AuthenticationStateProvider.

See also Blazor authentication and authorization document.
https://docs.microsoft.com/en-us/aspnet/core/security/blazor/


## Thanks

This library was a great reference for this article.

https://medium.com/@hurtertyjoil/client-side-blazor-authentication-with-azure-functions-and-easyauth-c454faad657b

