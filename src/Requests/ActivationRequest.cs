using System;
using System.Collections.Specialized;

using TestServer.Entities;

namespace TestServer.Requests
{
    [RequestTypeName( "activate" )]
    class ActivationRequest : Request
    {
        public override Responses.Response Respond( NameValueCollection args )
        {
            return Account.AttemptActivate( args["email"], args["code"] )
                ?? new Responses.Response( true );
        }
    }
}
