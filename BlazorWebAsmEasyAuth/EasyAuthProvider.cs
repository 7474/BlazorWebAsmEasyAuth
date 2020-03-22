using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorWebAsmEasyAuth
{
    // https://docs.microsoft.com/ja-jp/azure/app-service/overview-authentication-authorization#identity-providers
    public enum EasyAuthProvider
    {
        AAD,
        MicrosoftAccount,
        Facebook,
        Google,
        Twitter
    }
}
