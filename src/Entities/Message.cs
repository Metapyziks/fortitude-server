using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FortitudeServer.Entities
{
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
