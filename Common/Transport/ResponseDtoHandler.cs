using KODTransportModel.Common;

namespace KingOfDestiny.Common.Transport
{
    public abstract class ResponseDtoHandler<T> : IResponseDtoHandler where T : IResponseDto
    {
        public ResponseResultData TryHandle(Response responseResult, ResponseResultData responseResultData)
        {
            if (!(responseResult is IResponseWithResult responseWithResult)) return responseResultData;

            if (responseWithResult.Result is T resultDto)
            {
                responseResultData = HandleInternally(resultDto, responseResultData);
            }

            return responseResultData;
        }

        protected abstract ResponseResultData HandleInternally(T resultDto, ResponseResultData responseResultData);
    }
}