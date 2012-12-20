using System;
using System.Collections.Generic;
using System.Collections.Specialized;

using TestServer.Entities;

namespace TestServer.Requests
{
    [RequestTypeName( "register" )]
    class RegisterRequest : Request
    {
        public override Responses.Response Respond( NameValueCollection args )
        {
            return Account.AttemptRegister( args["uname"], args["email"], args["phash"] )
                ?? new Responses.Response( true );
        }
    }
}
