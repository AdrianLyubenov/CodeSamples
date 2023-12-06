using KODTransportModel.Common;

namespace KingOfDestiny.Common.Transport
{
    public interface IResponseDtoHandler
    {
        ResponseResultData TryHandle(Response responseResult, ResponseResultData responseResultData);
    }
}