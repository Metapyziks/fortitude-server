using System;

namespace TestServer.Responses
{
    [JSONSerializable]
    public class ErrorResponse : Response
    {
        [Serialize( "error" )]
        public readonly String Message;

        public ErrorResponse( String message )
            : base( false )
        {
            Message = message;
        }

        public ErrorResponse( String format, params Object[] args )
            : base( false )
        {
            Message = String.Format( format, args );
        }
    }
}
