using Microsoft.JSInterop;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorWebAsmEasyAuth
{
    public static class LocalStorage
    {
        public static async Task<T> GetAsync<T>(IJSRuntime jsRuntime, string key) where T : class
        {
            var json = await jsRuntime.InvokeAsync<string>("localStorage.getItem", key).AsTask();
            return json == null ? null : JsonSerializer.Deserialize<T>(json);
        }

        public static Task SetAsync(IJSRuntime jsRuntime, string key, object value)
            => jsRuntime.InvokeAsync<object>("localStorage.setItem", key, JsonSerializer.Serialize(value)).AsTask();

        public static Task DeleteAsync(IJSRuntime jsRuntime, string key)
            => jsRuntime.InvokeAsync<object>("localStorage.removeItem", key).AsTask();
    }
}