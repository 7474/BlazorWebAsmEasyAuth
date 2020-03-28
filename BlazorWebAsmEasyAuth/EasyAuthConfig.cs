namespace BlazorWebAsmEasyAuth
{
    public class EasyAuthConfig
    {
        /// <summary>
        /// e.g. "https://<client side blazor site url>"
        /// </summary>
        public string BlazorWebsiteURL { get; set; }
        /// <summary>
        /// e.g. "https://<your Azure function name>.azurewebsites.net"
        /// </summary>
        public string AzureFunctionAuthURL { get; set; }

        public bool EnableLogToConsole { get; set; }
    }
}