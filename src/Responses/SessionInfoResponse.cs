using System;

namespace FortitudeServer.Responses
{
    [JSONSerializable]
    public class SessionInfoResponse : Response
    {
        [Serialize( "code" )]
        public readonly String Code;

        public SessionInfoResponse( String code )
            : base( true )
        {
            Code = code;
        }
    }
}
