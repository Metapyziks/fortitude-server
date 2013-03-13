using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FortitudeServer.Entities
{
    [JSONSerializable, DatabaseEntity]
    public class BattleReport : Notification
    {
        [ForeignKey(typeof(Cache)), NotNull]
        public int CacheID { get; set; }

        [ForeignKey(typeof(Account)), NotNull]
        public int AttackerID { get; set; }

        [NotNull]
        public int AttackerInitial { get; set; }

        [NotNull]
        public int AttackerSurvivors { get; set; }

        [NotNull]
        public int AttackerFatalities { get; set; }

        [NotNull]
        public int AttackerDeserters { get; set; }

        [ForeignKey(typeof(Account)), NotNull]
        public int DefenderID { get; set; }

        [NotNull]
        public int DefenderInitial { get; set; }

        [NotNull]
        public int DefenderSurvivors { get; set; }

        [NotNull]
        public int DefenderFatalities { get; set; }

        [NotNull]
        public int DefenderDeserters { get; set; }
        
        [CleanUpMethod]
        public void Cleanup()
        {
            DatabaseManager.Delete<Event>(x => (x.Type == EventType.AttackCache
                || x.Type == EventType.CacheAttacked) && x.AuxiliaryID == NotificationID);
        }

        public BattleReport() { }

        public BattleReport(int defenderID)
            : base(defenderID, NotificationType.BattleReport)
        {
            DefenderID = defenderID;
        }
    }
}
