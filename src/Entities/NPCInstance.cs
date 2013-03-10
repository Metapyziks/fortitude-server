using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FortitudeServer.Entities
{
    [DatabaseEntity, Serializable]
    public class NPCInstance
    {
        [PrimaryKey]
        public int NPCInstanceID { get; set; }

        [ForeignKey(typeof(Account)), NotNull]
        public int AccountID { get; set; }

        [ForeignKey(typeof(NonPlayerCache)), NotNull]
        public int CacheID { get; set; }

        [NotNull]
        public DateTime NextAttack { get; set; }

        [NotNull]
        public int Attacks { get; set; }
    }
}
