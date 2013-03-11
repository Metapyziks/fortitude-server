using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FortitudeServer.Entities;

namespace FortitudeServer.Responses
{
    [JSONSerializable]
    class ReadMessageResponse : Response
    {
        public Message Message { get; private set; }

        [Serialize("senderid")]
        public int SenderID
        {
            get { return Message.SenderID; }
        }

        [Serialize("sender")]
        public String SenderName { get; private set; }

        [Serialize("subject")]
        public String Subject
        {
            get { return Message.Subject; }
        }

        [Serialize("content")]
        public String Content
        {
            get { return Message.Content; }
        }

        public ReadMessageResponse(Message message)
            : base(true)
        {
            Message = message;
            SenderName = DatabaseManager.SelectFirst<Account>(x => x.AccountID == SenderID).Username;
        }
    }
}
