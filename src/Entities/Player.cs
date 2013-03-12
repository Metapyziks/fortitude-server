using System;
using System.Linq;

namespace FortitudeServer.Entities
{
    [JSONSerializable, DatabaseEntity]
    public class Player
    {
        private static DateTime _sLastPayout;

        public static double PayoutInterval { get; set; }
        public static double UnitsPerCache { get; set; }

        public static bool PayoutDue
        {
            get { return (DateTime.Now - _sLastPayout).Seconds >= PayoutInterval; }
        }

        public static void Payout()
        {
            _sLastPayout = DateTime.Now;
            
            var accplys = DatabaseManager.SelectAll<Account, Player>((x, y) => x.AccountID == y.AccountID && x.Rank >= Rank.Verified);
            var caches = DatabaseManager.Select<Cache>(x => x.AccountID > 0);

            foreach (var accply in accplys) {
                if (!accply.Item1.IsVerified) continue;
                int cacheCount = caches.Count(x => x.AccountID == accply.Item1.AccountID);
                accply.Item2.Balance += 1 + (int) Math.Ceiling(UnitsPerCache * cacheCount);
                DatabaseManager.Update(accply.Item2);
            }
        }

        public static Player GetPlayer(Account acc)
        {
            return GetPlayer(acc.AccountID);
        }

        public static Player GetPlayer(int accountID)
        {
            Player ply = DatabaseManager.SelectFirst<Player>(x => x.AccountID == accountID);

            if (ply == null) {
                ply = new Player() {
                    AccountID = accountID,
                    Balance = 0
                };

                DatabaseManager.Insert(ply);
            }

            return ply;
        }

        [Serialize("accountid")]
        [PrimaryKey, ForeignKey(typeof(Account))]
        public int AccountID { get; set; }

        [Serialize("balance")]
        [NotNull]
        public int Balance { get; set; }

        [NotNull]
        public MessageSettings MessageSettings { get; set; }
    }
}
