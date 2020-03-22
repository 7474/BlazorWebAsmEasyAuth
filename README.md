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

T.B.D.


## Thanks

This library was a great reference for this article.

https://medium.com/@hurtertyjoil/client-side-blazor-authentication-with-azure-functions-and-easyauth-c454faad657b

