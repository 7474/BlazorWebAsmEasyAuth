using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace EasyAuthDemo
{
    public static class LocalStorage
    {
        public static Task<T> GetAsync<T>(IJSRuntime jsRuntime, string key)
            => jsRuntime.InvokeAsync<T>("blazorLocalStorage.get", key).AsTask();

        public static Task SetAsync(IJSRuntime jsRuntime, string key, object value)
            => jsRuntime.InvokeAsync<object>("blazorLocalStorage.set", key, value).AsTask();

        public static Task DeleteAsync(IJSRuntime jsRuntime, string key)
            => jsRuntime.InvokeAsync<object>("blazorLocalStorage.delete", key).AsTask();
    }
}