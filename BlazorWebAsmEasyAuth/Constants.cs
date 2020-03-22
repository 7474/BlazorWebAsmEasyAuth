namespace BlazorWebAsmEasyAuth
{
    public static class Constants
    {
        public static string AuthMeEndpoint => "/.auth/me";
        public static string LogOutEndpoint => "/.auth/logout";
        public static string LogInEndpointWithoutProvider => "/.auth/login/";
        public static string PostloginRedirect => "?post_login_redirect_url=";
        public static string LoginMode => "&session_mode=token";
    }
}