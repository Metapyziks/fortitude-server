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

            if (args["receiverid"] == null) {
                return new ErrorResponse("expected a receiver id");
            }

            int receiverid;
            if (!int.TryParse(args["receiverid"], out receiverid)) {
                return new ErrorResponse("invalid receiver id");
            }

            String subject = args["subject"];
            String content = args["content"];

            if (subject == null || subject.Length == 0) {
                return new ErrorResponse("expected a subject");
            }

            if (content == null || content.Length == 0) {
                return new ErrorResponse("expected a content body");
            }

            var account = DatabaseManager.SelectFirst<Account>(x => x.AccountID == receiverid);

            if (account == null) {
                return new ErrorResponse("invalid receiver id");
            }

            DatabaseManager.Insert(new Message(acc.AccountID, receiverid, subject, content));

            return new Response(true);
        }
    }
}
