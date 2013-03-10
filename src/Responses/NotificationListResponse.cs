using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FortitudeServer.Entities;

namespace FortitudeServer.Responses
{
    [JSONSerializable]
    public class NotificationListResponse : Response
    {
        [Serialize("items")]
        public readonly Notification[] Notifications;

        public NotificationListResponse(params Notification[] notifications)
            : base(true)
        {
            Notifications = notifications;
        }
    }
}
