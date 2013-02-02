using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        [NotNull]
        public int AccountID { get; set; }

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
    }
}
