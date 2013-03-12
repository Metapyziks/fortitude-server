using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FortitudeServer.Entities
{
    [DatabaseEntity]
    public class Announcement
    {
        [PrimaryKey, AutoIncrement]
        public int AnnouncementID { get; set; }

        [Capacity(32), NotNull]
        public String Title { get; set; }

        [Capacity(65535), NotNull]
        public String Content { get; set; }

        [NotNull]
        public DateTime Date { get; set; }
    }
}
