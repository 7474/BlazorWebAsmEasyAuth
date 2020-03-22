namespace BlazorWebAsmEasyAuth
{
    public class EasyAuthConfig
    {
        public string BlazorWebsiteURL { get; set; }// => "https://<client side blazor site url>";
        public string AzureFunctionAuthURL { get; set; }// => "https://<your Azure function name>.azurewebsites.net";
    }
}