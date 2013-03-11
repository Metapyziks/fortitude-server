using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FortitudeServer.Entities;
using FortitudeServer.Responses;

namespace FortitudeServer.Requests
{
    [RequestTypeName("blockedusers")]
    class BlockedUserListRequest : Request
    {
        public override Response Respond(NameValueCollection args)
        {
            Account acc;
            ErrorResponse error;

            if (!CheckAuth(args, out acc, out error, true)) {
                return error;
            }

            var blocked = DatabaseManager.Select<Account, BlockedUser>((y, x) =>
                x.BlockedID == y.AccountID, (y, x) => x.BlockerID == acc.AccountID);

            return new UserInfoResponse(blocked.Select(x => x.Item1).ToList());
        }
    }
}
