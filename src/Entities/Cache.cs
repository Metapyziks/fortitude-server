using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TestServer.Responses;

namespace TestServer.Entities
{
    [JSONSerializable, DatabaseEntity]
    public class Cache
    {
        public static int PlacementCost { get; internal set; }
        public static double MaxInteractionDistance { get; internal set; }
        public static double MinPlacementDistance { get; internal set; }

        /// <param name="radius">Measured in metres</param>
        public static IEnumerable<Cache> FindNearby(double lat, double lon, double radius)
        {
            double bound = (radius / 6371009.0) * 180.0 / Math.PI;
            var caches = DatabaseManager.Select<Cache>(x =>
                x.Latitude >= lat - bound && x.Latitude <= lat + bound &&
                x.Longitude >= lon - bound && x.Longitude <= lon + bound);
            return caches.Where(x => Tools.GetDistance(lat, lon, x.Latitude, x.Longitude) <= radius);
        }

        [Serialize("cacheid")]
        [PrimaryKey, AutoIncrement]
        public int CacheID { get; set; }

        [Serialize("ownerid")]
        [ForeignKey(typeof(Account))]
        public int AccountID { get; set; }

        public bool HasOwner { get { return AccountID > 0; } }

        [Serialize("name")]
        [NotNull, Capacity(32)]
        public String Name { get; set; }

        [Serialize("latitude")]
        [NotNull, Capacity(10,7)]
        public double Latitude { get; set; }

        [Serialize("longitude")]
        [NotNull, Capacity(10, 7)]
        public double Longitude { get; set; }

        [Serialize("garrison")]
        [NotNull]
        public int Balance { get; set; }

        public BattleReport Attack(Player attacker, int units)
        {
            if (!HasOwner) {
                throw new Exception("Can't attack a cache with no owner");
            }

            double weight = 0.4 + Math.Min((units - Balance) / (double) Balance, 2.0) * 0.05;

            var report = new BattleReport {
                Cache = this,
                AttackerID = attacker.AccountID,
                DefenderID = AccountID,
                Attackers = new BattleReport.UnitReport {
                    Initial = units,
                    Survivors = units
                },
                Defenders = new BattleReport.UnitReport {
                    Initial = Balance,
                    Survivors = Balance
                }
            };

            while (report.Attackers.Survivors > 0 && report.Defenders.Survivors > 0) {
                if (Tools.CoinToss(weight)) {
                    --report.Defenders.Survivors;
                } else {
                    --report.Attackers.Survivors;
                }
            }

            report.Attackers.Fatalities = report.Attackers.Initial - report.Attackers.Survivors;
            report.Defenders.Fatalities = report.Defenders.Initial - report.Defenders.Survivors;

            attacker.Balance -= report.Attackers.Fatalities;
            Balance -= report.Defenders.Fatalities;

            report.IsVictory = report.Attackers.Survivors > 0;
            if (report.IsVictory) {
                report.Attackers.Deserters = 0;
                report.Defenders.Deserters = (int) (report.Defenders.Fatalities * (Tools.Random() * 0.1 + 0.1));
                Balance = report.Attackers.Survivors + report.Defenders.Deserters;
                AccountID = attacker.AccountID;
            } else {
                report.Defenders.Deserters = 0;
                report.Attackers.Deserters = (int) (report.Attackers.Fatalities * (Tools.Random() * 0.1 + 0.05));
                Balance = report.Defenders.Survivors + report.Attackers.Deserters;
            }

            DatabaseManager.Update(this);
            DatabaseManager.Update(attacker);

            return report;
        }
    }
}
