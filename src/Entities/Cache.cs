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
        /// <param name="radius">Measured in metres</param>
        public static IEnumerable<Cache> FindNearby(double lat, double lon, double radius)
        {
            double bound = (radius / 6371009.0) * 180.0 / Math.PI;
            var caches = DatabaseManager.Select<Cache>(x =>
                x.Latitude >= lat - bound && x.Latitude <= lat + bound &&
                x.Longitude >= lon - bound && x.Longitude <= lon + bound);
            return caches.Where(x => {
                double dlat = lat - x.Latitude;
                double dlon = lon - x.Longitude;
                return dlat * dlat + dlon * dlon <= bound * bound;
            });
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
