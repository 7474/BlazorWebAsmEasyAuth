using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace BlazorWebAsmEasyAuth
{
    public static class UserExtension
    {
        public static string AvatarIconUrl(this ClaimsPrincipal user)
        {
            // TODO 他のプロバイダにも対応する。
            return user.FindFirst("urn:twitter:profile_image_url_https")?.Value;
        }
        public static bool HasAvatarIcon(this ClaimsPrincipal user)
        {
            return !string.IsNullOrEmpty(AvatarIconUrl(user));
        }
    }
}
