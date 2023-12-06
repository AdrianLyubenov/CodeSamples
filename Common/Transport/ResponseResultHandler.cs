using System.Collections.Generic;
using KODTransportModel.Common;

namespace KingOfDestiny.Common.Transport
{
    public sealed class ResponseResultHandler : ResponseHandler
    {
        private readonly IReadOnlyList<IResponseDtoHandler> _dtoHandlers;

        public ResponseResultHandler(IReadOnlyList<IResponseDtoHandler> dtoHandlers)
        {
            _dtoHandlers = dtoHandlers;
        }
        
        public override ResponseResultData TryHandle(Response response, ResponseResultData responseResultData)
        {
            foreach (var dtoHandler in _dtoHandlers)
            {
                responseResultData = dtoHandler.TryHandle(response, responseResultData);
            }

            return responseResultData;
        }
    }
}