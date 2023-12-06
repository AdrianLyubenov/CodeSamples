using KODTransportModel.Common;

namespace KingOfDestiny.Common.Transport
{
    public abstract class ResponseHandler : IResponseHandler
    {
        public abstract ResponseResultData TryHandle(Response response, ResponseResultData responseResultData);
    }
}