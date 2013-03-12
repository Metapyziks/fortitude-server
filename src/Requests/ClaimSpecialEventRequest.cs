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
    [RequestTypeName("claimspecial")]
    class ClaimSpecialEventRequest : Request
    {
        public override Response Respond(NameValueCollection args)
        {
            Account acc;
            ErrorResponse error;

            if (!this.CheckAuth(args, out acc, out error, true))
                return error;

            String address = args["address"];

            if (address == null) {
                return new ErrorResponse("expected a MAC address");
            }

            if (!SpecialEvent.IsAddressValid(address)) {
                return new ErrorResponse("invalid MAC address");
            }

            var special = DatabaseManager.SelectFirst<SpecialEvent>(x => x.MACAddress == address);

            if (special == null) {
                return new ErrorResponse("there is no special event for this address, perhaps you are too late");
            }

            if (DateTime.Now > special.Expires || special.Balance <= 0) {
                DatabaseManager.Delete(special);
                return new ErrorResponse("special event expired");
            }

            if (DatabaseManager.SelectFirst<ClaimedEvent>(x => x.AccountID == acc.AccountID
                && x.SpecialEventID == special.SpecialEventID) != null) {
                return new ErrorResponse("event alread claimed");
            }

            --special.Balance;
            special.Expires = DateTime.Now.AddMonths(1);

            var player = Player.GetPlayer(acc);
            player.Balance += special.Reward;
            DatabaseManager.Update(player);

            if (special.Balance <= 0) {
                DatabaseManager.Delete(special);
            } else {
                DatabaseManager.Insert(new ClaimedEvent {
                    AccountID = acc.AccountID,
                    SpecialEventID = special.SpecialEventID,
                    Date = DateTime.Now
                });
            }

            DatabaseManager.Update(special);

            return new SpecialEventClaimedResponse(special);
        }
    }
}
