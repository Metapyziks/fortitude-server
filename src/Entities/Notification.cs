using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FortitudeServer.Entities
{
    public enum NotificationType : byte
    {
        Message = 1,
        BattleReport = 2,
        All = Message | BattleReport
    }

    public enum ReadStatus : byte
    {
        Unread = 0,
        Read = 1
    }

    [JSONSerializable, DatabaseEntity]
    public class Notification
    {
        [Serialize("notificationid")]
        [PrimaryKey, AutoIncrement]
        public int NotificationID { get; set; }

        [Serialize("receiverid")]
        [ForeignKey(typeof(Account))]
        public int AccountID { get; set; }

        [Serialize("timestamp")]
        [NotNull]
        public DateTime TimeStamp { get; set; }

        [Serialize("type")]
        [NotNull]
        public NotificationType Type { get; set; }

        [Serialize("status")]
        [NotNull]
        public ReadStatus Status { get; set; }

        public Notification() { }

        protected Notification(int accountID, NotificationType type)
        {
            AccountID = accountID;
            TimeStamp = DateTime.Now;
            Type = type;
            Status = ReadStatus.Unread;
        }
    }
}
