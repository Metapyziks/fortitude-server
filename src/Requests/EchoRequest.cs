using System.Collections.Specialized;

namespace TestServer.Requests
{
    [RequestTypeName( "echo" )]
    class EchoRequest : Request
    {
        public override Responses.Response Respond( NameValueCollection args )
        {
            if ( args[ "msg" ] != null )
                return new Responses.EchoResponse( args[ "msg" ] );

            return new Responses.EchoResponse( "null" );
        }
    }
}
