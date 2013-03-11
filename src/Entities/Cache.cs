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

        public bool HasOwner { get { return AccountID != 0; } }
        public bool IsNPC { get { return AccountID == -1; } }

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

        public Response Scout(Player ply, int units)
        {
            NonPlayerCache npc = null;
            NPCInstance instance = null;
            if (IsNPC) {
                npc = DatabaseManager.SelectFirst<NonPlayerCache>(x => x.CacheID == CacheID);
                instance = NPCInstance.GetInstance(CacheID, ply.AccountID);

                if (instance.NextAttack > DateTime.Now) {
                    return new ErrorResponse("the camp is still in ruins");
                }
            }

            int survivors = Enumerable.Range(0, units).Count(x => Tools.CoinToss(0.8));
            ply.Balance -= units - survivors;

            DatabaseManager.Update(ply);

            if (IsNPC) {
                return new ScoutCacheResponse {
                    CacheID = CacheID,
                    Scouts = units,
                    Survivors = survivors,
                    Garrison = NonPlayerCache.FindNextGarrisonSize(Balance, instance.Attacks, npc.GrowthStyle)
                };
            } else {
                return new ScoutCacheResponse {
                    CacheID = CacheID,
                    Scouts = units,
                    Survivors = survivors,
                    Garrison = Balance
                };
            }
        }

        public Response Attack(Player attacker, int units)
        {
            if (!HasOwner) {
                throw new Exception("Can't attack a cache with no owner");
            }

            double weight = 0.4 + Math.Min((units - Balance) / (double) Balance, 2.0) * 0.05;

            NonPlayerCache npc = null;
            NPCInstance instance = null;
            if (IsNPC) {
                npc = DatabaseManager.SelectFirst<NonPlayerCache>(x => x.CacheID == CacheID);
                instance = NPCInstance.GetInstance(CacheID, attacker.AccountID);

                if (instance.NextAttack > DateTime.Now) {
                    return new ErrorResponse("the camp is still in ruins");
                }
            }

            int balance = IsNPC
                    ? NonPlayerCache.FindNextGarrisonSize(Balance, instance.Attacks, npc.GrowthStyle)
                    : Balance;

            var report = new BattleReport (AccountID) {
                CacheID = CacheID,
                AttackerID = attacker.AccountID,
                AttackerInitial = units,
                AttackerSurvivors = units,
                DefenderInitial = balance,
                DefenderSurvivors = balance
            };

            attacker.Balance -= report.AttackerInitial;

            while (report.AttackerSurvivors > 0 && report.DefenderSurvivors > 0) {
                if (Tools.CoinToss(weight)) {
                    --report.DefenderSurvivors;
                } else {
                    --report.AttackerSurvivors;
                }
            }

            report.AttackerFatalities = report.AttackerInitial - report.AttackerSurvivors;
            report.DefenderFatalities = report.DefenderInitial - report.DefenderSurvivors;

            if (report.AttackerSurvivors > 0) {
                report.AttackerDeserters = 0;
                if (!IsNPC) {
                    report.DefenderDeserters = (int) (report.DefenderFatalities * (Tools.Random() * 0.1 + 0.1));
                    Balance = report.AttackerSurvivors + report.DefenderDeserters;
                    AccountID = attacker.AccountID;
                } else {
                    report.DefenderDeserters = report.DefenderInitial / 2 + report.AttackerFatalities;
                    attacker.Balance += report.DefenderDeserters;
                    ++instance.Attacks;
                    instance.NextAttack = DateTime.Now.AddSeconds(30.0);
                    DatabaseManager.Update(instance);
                }
            } else {
                report.DefenderDeserters = 0;
                report.AttackerDeserters = (int) (report.AttackerFatalities * (Tools.Random() * 0.1 + 0.05));

                if (!IsNPC) {
                    Balance = report.DefenderSurvivors;
                    var defender = DatabaseManager.SelectFirst<Player>(x => x.AccountID == this.AccountID);
                    defender.Balance += report.AttackerDeserters;
                    DatabaseManager.Update(defender);
                }
            }

            if (IsNPC) {
                DatabaseManager.Update(this);
                DatabaseManager.Insert(report);
            }

            DatabaseManager.Update(attacker);

            return new BattleReportResponse(this, report);
        }
    }
}
