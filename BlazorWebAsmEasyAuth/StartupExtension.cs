using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorWebAsmEasyAuth
{
    public static class StartupExtension
    {
        public static IServiceCollection AddEasyAuth(this IServiceCollection services, EasyAuthConfig config)
        {
            services.AddSingleton(config);
            services.AddScoped<EasyAuthNavigationHelper>();
            services.AddScoped<EasyAuthAuthenticationStateProvider>();
            services.AddScoped<AuthenticationStateProvider>(
                provider => provider.GetRequiredService<EasyAuthAuthenticationStateProvider>()
            );
            // https://github.com/dotnet/aspnetcore/issues/18733
            // Call AddAuthorizationCore after register AuthenticationStateProvider.
            services.AddOptions();
            services.AddAuthorizationCore();
            return services;
        }
    }
}
