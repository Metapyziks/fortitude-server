using System;
using System.Collections.Generic;

using TestServer.Entities;

namespace TestServer.Responses
{
    [JSONSerializable]
    public class UserInfoResponse : Response
    {
        [Serialize( "users" )]
        public readonly List<Account> Users;

        public UserInfoResponse( List<Account> users )
            : base( true )
        {
            Users = users;
        }
    }
}
