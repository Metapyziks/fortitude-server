using System;
using System.Collections.Specialized;

using TestServer.Entities;

namespace TestServer.Requests
{
    [RequestTypeName( "session" )]
    class StartSessionRequest : Request
    {
        public override Responses.Response Respond( NameValueCollection args )
        {
            Account account;
            Responses.ErrorResponse error;

            if ( CheckAuth( args, out account, out error, false ) )
            {
                AuthSession sess = AuthSession.Create( account );
                return new Responses.SessionInfoResponse(
                    new String( sess.SessionCode ) );
            }

            return error;
        }
    }
}
