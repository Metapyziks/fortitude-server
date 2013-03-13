using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FortitudeServer.Entities
{
    public enum EventType : byte
    {
        Player = 0, Cache = 8,

        Register = Player | 1,
        RankChange = Player | 2,
        PlaceCache = Player | 3,
        AttackCache = Player | 4,

        CachePlaced = Cache | 1,
        CacheAttacked = Cache | 2
    }

    public class Event
    {
        [PrimaryKey, AutoIncrement]
        public int EventID { get; set; }

        [NotNull]
        public EventType Type { get; set; }

        [ForeignKey(typeof(Account), typeof(Cache))]
        public int ContextID { get; set; }

        [NotNull, ForeignKey(typeof(BattleReport), typeof(Cache))]
        public int AuxiliaryID { get; set; }

        [NotNull]
        public DateTime TimeStamp { get; set; }
    }
}
