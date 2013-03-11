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
        public static NPCInstance GetInstance(int cacheID, int accountID)
        {
            NPCInstance instance = DatabaseManager.SelectFirst<NPCInstance>(x => x.CacheID == cacheID && x.AccountID == accountID);

            if (instance == null) {
                instance = new NPCInstance() {
                    CacheID = cacheID,
                    AccountID = accountID
                };

                DatabaseManager.Insert(instance);
            }

            return instance;
        }

        [PrimaryKey, AutoIncrement]
        public int NPCInstanceID { get; set; }

        [ForeignKey(typeof(Account)), NotNull]
        public int AccountID { get; set; }

        [ForeignKey(typeof(NonPlayerCache)), NotNull]
        public int CacheID { get; set; }

        [NotNull]
        public DateTime NextAttack { get; set; }

        [NotNull]
        public int Attacks { get; set; }

        public NPCInstance()
        {
            NextAttack = DateTime.Now;
            Attacks = 0;
        }
    }
}
