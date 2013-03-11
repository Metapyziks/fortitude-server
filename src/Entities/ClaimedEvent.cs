using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FortitudeServer.Entities
{
    [DatabaseEntity]
    public class ClaimedEvent
    {
        [PrimaryKey, AutoIncrement]
        public int ClaimedEventID { get; set; }

        [NotNull, ForeignKey(typeof(Account))]
        public int AccountID { get; set; }

        [NotNull, ForeignKey(typeof(SpecialEvent))]
        public int SpecialEventID { get; set; }

        [NotNull]
        public DateTime Date { get; set; }
    }
}
