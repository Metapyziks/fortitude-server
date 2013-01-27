using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

using TestServer.Entities;

namespace TestServer.Requests
{
    using AccountPred = Expression<Func<Account, bool>>;

    [RequestTypeName("userinfo")]
    class UserInfoRequest : Request
    {
        public override Responses.Response Respond(NameValueCollection args)
        {
            String[] usernames = null;
            int[] userids = null;

            if (args["uname"] != null) {
                usernames = new String[] { args["uname"] };
            } else if (args["uid"] != null) {
                userids = new int[] { Int32.Parse(args["uid"]) };
            } else if (args["unames"] != null) {
                usernames = args["unames"].Split(new char[] { ',' },
                    StringSplitOptions.RemoveEmptyEntries);
            } else if (args["uids"] != null) {
                userids = args["uids"].Split(new char[] { ',' },
                    StringSplitOptions.RemoveEmptyEntries).Select(x => Int32.Parse(x)).ToArray();
            }

            if ((usernames == null || usernames.Length == 0) && (userids == null || userids.Length == 0)) {
                return new Responses.ErrorResponse("no username given");
            }

            List<Account> users;

            if (usernames != null) {
                users = DatabaseManager.Select(usernames.Select(
                    x => (AccountPred) (acc => acc.Username == x)).ToArray());
            } else {
                users = DatabaseManager.Select(userids.Select(
                    x => (AccountPred) (acc => acc.AccountID == x)).ToArray());
            }

            return new Responses.UserInfoResponse(users);
        }
    }
}
