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
    [RequestTypeName("sendmessage")]
    class SendMessageRequest : Request
    {
        public override Response Respond(NameValueCollection args)
        {
            Account acc;
            ErrorResponse error;

            if (!this.CheckAuth(args, out acc, out error, true))
                return error;

            if (args["receiverid"] == null && args["receiver"] == null) {
                return new ErrorResponse("expected a receiver id");
            }

            String subject = args["subject"];
            String content = args["content"];

            if (subject == null || subject.Length == 0) {
                return new ErrorResponse("expected a subject");
            }

            if (content == null || content.Length == 0) {
                return new ErrorResponse("expected a content body");
            }

            Account receiver;

            if (args["receiverid"] != null) {
                int receiverid;
                if (!int.TryParse(args["receiverid"], out receiverid)) {
                    return new ErrorResponse("invalid receiver id");
                }

                receiver = DatabaseManager.SelectFirst<Account>(x => x.AccountID == receiverid);
            } else {
                receiver = DatabaseManager.SelectFirst<Account>(x => x.Username == args["receiver"]);
            }

            if (receiver == null) {
                return new ErrorResponse("invalid receiver id");
            }

            if (receiver.HasBlocked(acc)) {
                return new ErrorResponse("the receiver has disabled receiving messages");
            }

            if (acc.HasBlocked(receiver)) {
                return new ErrorResponse("you cannot send messages to a blocked user");
            }

            DatabaseManager.Insert(new Message(acc.AccountID, receiver.AccountID, subject, content));

            return new Response(true);
        }
    }
}
