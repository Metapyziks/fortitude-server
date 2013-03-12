using System;

namespace FortitudeServer.Entities
{
    public enum MessageSettings : byte
    {
        Default = 0,
        BlockAll = 1
    }

    [JSONSerializable, DatabaseEntity]
    public class Message : Notification
    {
        [ForeignKey(typeof(Account)), NotNull]
        public int SenderID { get; set; }

        [Capacity(64)]
        public String Subject { get; set; }
        
        [Capacity(255)]
        public String Content { get; set; }

        public Message() { }

        public Message(int sender, int receiver, String subject, String content)
            : base(receiver, NotificationType.Message)
        {
            SenderID = sender;
            Subject = subject;
            Content = content;
        }
    }
}
