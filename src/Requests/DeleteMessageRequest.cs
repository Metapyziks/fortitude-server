using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FortitudeServer.Responses;
using FortitudeServer.Entities;

namespace FortitudeServer.Requests
{
    [RequestTypeName("deletemessage")]
    class DeleteMessageRequest : Request
    {
        public override Response Respond(NameValueCollection args)
        {
            Account acc;
            ErrorResponse error;

            if (!this.CheckAuth(args, out acc, out error, true))
                return error;

            if (args["messageid"] == null) {
                return new ErrorResponse("expected a message id");
            }

            int id;
            if (!int.TryParse(args["messageid"], out id)) {
                return new ErrorResponse("invalid message id");
            }

            var message = DatabaseManager.SelectFirst<Message>(x => x.AccountID == acc.AccountID && x.NotificationID == id);

            if (message == null) {
                return new ErrorResponse("invalid message id");
            }

            DatabaseManager.Delete(message);

            return new Response(true);
        }
    }
}
