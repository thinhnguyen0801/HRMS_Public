namespace HNOne.Web.Services.Interfaces
{
    public interface IProgressService
    {
        Task Start();
        Task SetPercent(double pPercent = 0.4);
        Task Done();
    }

    public interface ILoadingService
    {
        event Action<bool>? OnShow;
        void ShowLoading(bool pIsLoading = true);
    }
}
