using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FortitudeServer.Responses;

namespace FortitudeServer.Entities
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

        public BattleReportResponse Attack(Player attacker, int units)
        {
            if (!HasOwner) {
                throw new Exception("Can't attack a cache with no owner");
            }

            var defender = DatabaseManager.SelectFirst<Player>(x => x.AccountID == this.AccountID);

            double weight = 0.4 + Math.Min((units - Balance) / (double) Balance, 2.0) * 0.05;

            var report = new BattleReport {
                CacheID = CacheID,
                AttackerID = attacker.AccountID,
                DefenderID = AccountID,
                AttackerInitial = units,
                AttackerSurvivors = units,
                DefenderInitial = Balance,
                DefenderSurvivors = Balance
            };

            while (report.AttackerSurvivors > 0 && report.DefenderSurvivors > 0) {
                if (Tools.CoinToss(weight)) {
                    --report.DefenderSurvivors;
                } else {
                    --report.AttackerSurvivors;
                }
            }

            report.AttackerFatalities = report.AttackerInitial - report.AttackerSurvivors;
            report.DefenderFatalities = report.DefenderInitial - report.DefenderSurvivors;

            attacker.Balance -= report.AttackerInitial;
            Balance -= report.DefenderFatalities + report.DefenderDeserters;

            if (report.AttackerSurvivors > 0) {
                report.AttackerDeserters = 0;
                report.DefenderDeserters = (int) (report.DefenderFatalities * (Tools.Random() * 0.1 + 0.1));
                Balance = report.AttackerSurvivors + report.DefenderDeserters;
                AccountID = attacker.AccountID;
            } else {
                report.DefenderDeserters = 0;
                report.AttackerDeserters = (int) (report.AttackerFatalities * (Tools.Random() * 0.1 + 0.05));
                Balance = report.DefenderSurvivors;
                defender.Balance += report.AttackerDeserters;
            }

            DatabaseManager.Update(this);
            DatabaseManager.Update(attacker);
            DatabaseManager.Update(defender);

            DatabaseManager.Insert(report);

            return new BattleReportResponse(this, report);
        }
    }
}
