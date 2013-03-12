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
    [RequestTypeName("sendreport")]
    class SendReportRequest : Request
    {
        public override Response Respond(NameValueCollection args)
        {
            Account acc;
            ErrorResponse error;

            if (!CheckAuth(args, out acc, out error, true)) {
                return error;
            }

            if (args["type"] == null) {
                return new ErrorResponse("expected type");
            }

            ReportType type;
            if (!Enum.TryParse<ReportType>(args["type"], out type)) {
                return new ErrorResponse("invalid type");
            }

            String body = args["message"];
            if (body == null) {
                return new ErrorResponse("expected message");
            }

            if (args["contextid"] == null) {
                return new ErrorResponse("expected context id");
            }

            int id;
            if (!int.TryParse(args["contextid"], out id)) {
                return new ErrorResponse("invalid context id");
            }

            switch (type) {
                case ReportType.Account:
                    var account = DatabaseManager.SelectFirst<Account>(x => x.AccountID == id);
                    if (account == null) {
                        return new ErrorResponse("invalid context id");
                    }
                    if (account.AccountID == acc.AccountID) {
                        return new ErrorResponse("you cannot report yourself");   
                    }
                    break;
                case ReportType.Cache:
                    var cache = DatabaseManager.SelectFirst<Cache>(x => x.CacheID == id);
                    if (cache == null) {
                        return new ErrorResponse("invalid context id");
                    }
                    break;
                case ReportType.Message:
                    var message = DatabaseManager.SelectFirst<Message>(x => x.NotificationID == id);
                    if (message == null) {
                        return new ErrorResponse("invalid context id");
                    }
                    if (message.SenderID == acc.AccountID) {
                        return new ErrorResponse("you cannot report a message sent by yourself");   
                    }
                    break;
                default:
                    return new ErrorResponse("invalid type");
            }

            if (DatabaseManager.SelectFirst<Report>(x => x.Type == type &&
                x.AccountID == acc.AccountID && x.ContextID == id) != null) {
                return new ErrorResponse("you have already reported this {0}", type);
            }

            DatabaseManager.Insert(new Report {
                AccountID = acc.AccountID,
                Type = type,
                ContextID = id,
                Message = body
            });

            return new Response(true);
        }
    }
}
