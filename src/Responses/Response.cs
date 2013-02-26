using System;

namespace FortitudeServer.Responses
{
    [JSONSerializable]
    public class Response
    {
        [Serialize( "success" )]
        public readonly bool Success;

        public Response( bool success )
        {
            Success = success;
        }
    }
}
