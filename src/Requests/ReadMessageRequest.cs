using System.Collections.Specialized;

using FortitudeServer.Entities;
using FortitudeServer.Responses;

namespace FortitudeServer.Requests
{
    [RequestTypeName("readmessage")]
    class ReadMessageRequest : Request
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

            if (message.Status != ReadStatus.Read) {
                message.Status = ReadStatus.Read;
                DatabaseManager.Update(message);
            }

            return new ReadMessageResponse(message);
        }
    }
}
