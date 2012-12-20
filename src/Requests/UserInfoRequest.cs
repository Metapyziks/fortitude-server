using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

using TestServer.Entities;

namespace TestServer.Requests
{
    using AccountPred = Expression<Func<Account, bool>>;

    [RequestTypeName( "userinfo" )]
    class UserInfoRequest : Request
    {
        public override Responses.Response Respond( NameValueCollection args )
        {
            String[] usernames = null;

            if ( args[ "uname" ] != null )
                usernames = new String[] { args[ "uname" ] };
            else if ( args[ "unames" ] != null )
                usernames = args[ "unames" ].Split( new char[] { ',' },
                    StringSplitOptions.RemoveEmptyEntries );

            if ( usernames == null || usernames.Length == 0 )
                return new Responses.ErrorResponse( "no username given" );

            List<Account> users = DatabaseManager.Select( usernames.Select(
                x => (AccountPred) ( acc => acc.Username == x ) ).ToArray() );

            return new Responses.UserInfoResponse( users );
        }
    }
}
