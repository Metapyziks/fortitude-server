using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

using FortitudeServer.Entities;

namespace FortitudeServer.Responses
{
    using AccountPred = Expression<Func<Account, bool>>;
    using CachePred = Expression<Func<Cache, bool>>;

    [JSONSerializable]
    public class NotificationListResponse : Response
    {
        [JSONSerializable]
        public abstract class NotificationStub
        {
            [Serialize("notificationid")]
            public abstract int NotificationID { get; }

            [Serialize("receiverid")]
            public abstract int AccountID { get; }

            [Serialize("timestamp")]
            public abstract DateTime TimeStamp { get; }

            [Serialize("status")]
            public abstract ReadStatus Status { get; }

            [Serialize("type")]
            public abstract NotificationType Type { get; }
        }

        [JSONSerializable]
        public abstract class NotificationStub<T> : NotificationStub
            where T : Notification
        {
            public T Notification { get; private set; }

            [Serialize("notificationid")]
            public override int NotificationID
            {
                get { return Notification.NotificationID;}
            }

            [Serialize("receiverid")]
            public override int AccountID
            {
                get { return Notification.AccountID; }
            }

            [Serialize("timestamp")]
            public override DateTime TimeStamp
            {
                get { return Notification.TimeStamp; }
            }

            [Serialize("status")]
            public override ReadStatus Status
            {
                get { return Notification.Status; }
            }

            [Serialize("type")]
            public override NotificationType Type
            {
                get { return Notification.Type; }
            }

            protected NotificationStub(T notification)
            {
                Notification = notification;
            }
        }

        [JSONSerializable]
        public class MessageStub : NotificationStub<Message>
        {
            [Serialize("subject")]
            public String Subject
            {
                get { return Notification.Subject; }
            }

            [Serialize("sender")]
            public String SenderName { get; set; }

            public MessageStub(Message message)
                : base(message) { }
        }

        [JSONSerializable]
        public class BattleReportStub : NotificationStub<BattleReport>
        {
            [Serialize("cache")]
            public String CacheName { get; set; }

            [Serialize("attacker")]
            public String AttackerName { get; set; }

            public BattleReportStub(BattleReport battleReport)
                : base(battleReport) { }
        }

        [Serialize("items")]
        public readonly NotificationStub[] Notifications;

        public NotificationListResponse(params Notification[] notifications)
            : base(true)
        {
            var accIds = notifications.Select(x => {
                if (x is Message) {
                    return ((Message) x).SenderID;
                } else if (x is BattleReport) {
                    return ((BattleReport) x).AttackerID;
                } else {
                    return 0; // Sorry
                }
            });

            var cacIds = notifications.Select(x => {
                if (x is BattleReport) {
                    return ((BattleReport) x).CacheID;
                } else {
                    return 0; // Really
                }
            });

            var accounts = DatabaseManager.Select<Account>(accIds.Select(
                x => (AccountPred) (account => account.AccountID == x)).ToArray());

            var caches = DatabaseManager.Select<Cache>(cacIds.Select(
                x => (CachePred) (cache => cache.CacheID == x)).ToArray());

            Notifications = notifications.Select<Notification, NotificationStub>(x => {
                if (x is Message) {
                    var stub = new MessageStub((Message) x);
                    stub.SenderName = accounts.First(y => y.AccountID == stub.Notification.SenderID).Username;
                    return stub;
                } else if (x is BattleReport) {
                    var stub = new BattleReportStub((BattleReport) x);
                    var account = accounts.FirstOrDefault(y => y.AccountID == stub.Notification.AttackerID);
                    stub.AttackerName = account != null ? account.Username : "[deleted]";
                    var cache = caches.FirstOrDefault(y => y.CacheID == stub.Notification.CacheID);
                    stub.CacheName = cache != null ? cache.Name : "[deleted]";
                    return stub;
                } else {
                    throw new Exception("Unknown notification type to serialize");
                }
            }).ToArray();
        }
    }
}
