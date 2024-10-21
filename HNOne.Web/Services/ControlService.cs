using HNOne.Web.Services.Interfaces;
using Microsoft.JSInterop;

namespace HNOne.Web.Services
{
    public class ProgressService : IProgressService
    {
        private IJSRuntime _jsRuntime { get; set; }
        public ProgressService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }
        public async Task Done()
        {
            await _jsRuntime!.InvokeVoidAsync("NProgress.inc");
            await _jsRuntime!.InvokeVoidAsync("NProgress.done");
        }
        public async Task SetPercent(double pPercent = 0.4) => await _jsRuntime!.InvokeVoidAsync("NProgress.set", pPercent);
        public async Task Start() => await _jsRuntime!.InvokeVoidAsync("NProgress.start");
    }

    public class LoadingService : ILoadingService
    {
        public event Action<bool>? OnShow;
        public void ShowLoading(bool pIsLoading = true) => OnShow?.Invoke(pIsLoading);
    }
}
