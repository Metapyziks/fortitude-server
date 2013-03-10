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
    [RequestTypeName("newsstubs")]
    class NotificationListRequest : Request
    {
        public override Responses.Response Respond(NameValueCollection args)
        {
            Account acc;
            ErrorResponse error;

            if (!CheckAuth(args, out acc, out error, true)) {
                return error;
            }

            var since = DateTime.Now;
            if (args["since"] != null) {
                long timestamp;
                if (!long.TryParse(args["since"], out timestamp)) {
                    return new ErrorResponse("invalid timestamp");
                }
                since = Tools.UnixEpoch.AddMilliseconds(timestamp).ToLocalTime();
            }

            int count = 8;
            if (args["count"] != null) {
                if (!int.TryParse(args["count"], out count)) {
                    return new ErrorResponse("invalid count");
                }
            }

            NotificationType filter = NotificationType.All;
            if (args["filter"] != null) {
                if (!Enum.TryParse<NotificationType>(args["filter"], out filter)) {
                    return new ErrorResponse("invalid filter");
                }
            }

            List<Notification> news;

            if (filter == NotificationType.All) {
                news = DatabaseManager.Select<Notification>(x => x.AccountID == acc.AccountID);
            } else {
                news = DatabaseManager.Select<Notification>(x => x.AccountID == acc.AccountID && x.Type == filter);
            }
            news = news.Where(x => x.TimeStamp < since).ToList();
            news.Reverse();
            return new NotificationListResponse(news.GetRange(0, Math.Min(count, news.Count)).ToArray());
        }
    }
}
