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
    [RequestTypeName("setblocked")]
    class SetBlockedRequest : Request
    {
        public override Response Respond(NameValueCollection args)
        {
            Account acc;
            ErrorResponse error;

            if (!this.CheckAuth(args, out acc, out error, true))
                return error;

            if (args["accountid"] == null && args["account"] == null) {
                return new ErrorResponse("expected an account id");
            }

            if (args["block"] == null) {
                return new ErrorResponse("expected a block value");
            }

            bool shouldBlock;
            if (!bool.TryParse(args["block"], out shouldBlock)) {
                return new ErrorResponse("invalid block value");
            }

            Account account;

            if (args["accountid"] != null) {
                int accountid;
                if (!int.TryParse(args["accountid"], out accountid)) {
                    return new ErrorResponse("invalid account id");
                }

                account = DatabaseManager.SelectFirst<Account>(x => x.AccountID == accountid);
            } else {
                account = DatabaseManager.SelectFirst<Account>(x => x.Username == args["account"]);
            }

            if (account == null) {
                return new ErrorResponse("invalid account id");
            }

            var block = DatabaseManager.SelectFirst<BlockedUser>(x =>
                x.BlockerID == acc.AccountID && x.BlockedID == account.AccountID);

            if (block == null) {
                if (shouldBlock) {
                    DatabaseManager.Insert(new BlockedUser {
                        BlockerID = acc.AccountID,
                        BlockedID = account.AccountID
                    });
                } else {
                    return new ErrorResponse("user already unblocked");
                }
            } else {
                if (shouldBlock) {
                    return new ErrorResponse("user already blocked");
                } else {
                    DatabaseManager.Delete(block);
                }
            }

            return new Response(true);
        }
    }
}
