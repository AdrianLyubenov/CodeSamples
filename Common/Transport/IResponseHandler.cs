using KODTransportModel.Common;

namespace KingOfDestiny.Common.Transport
{
    public interface IResponseHandler
    {
        ResponseResultData TryHandle(Response response, ResponseResultData responseResultData);
    }
}