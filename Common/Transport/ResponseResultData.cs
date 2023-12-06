using PlayFab;

namespace KingOfDestiny.Common.Transport
{
    public class ResponseResultData
    {
        public string ErrorMessage { get; set; }
        public PlayFabError PlayfabErrorMessage { get; set; }
    }
    
    public class ResponseResultData<T> : ResponseResultData, IResponseResultData<T>
    {
        public T Data { get; set; }
    }
    
}