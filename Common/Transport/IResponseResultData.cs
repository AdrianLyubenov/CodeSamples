using PlayFab;

namespace KingOfDestiny.Common.Transport
{
    public interface IResponseResultData<out T>
    {
        T Data { get; }
        string ErrorMessage { get; }
        PlayFabError PlayfabErrorMessage { get; }
    }
}