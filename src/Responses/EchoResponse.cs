using System;

namespace FortitudeServer.Responses
{
    [JSONSerializable]
    public class EchoResponse : Response
    {
        [Serialize( "msg" )]
        public readonly String[] Message;

        public EchoResponse( String message )
            : base( true )
        {
            Message = message.Split();
        }
    }
}
