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

        [Serialize("balance")]
        [NotNull]
        public int Balance { get; set; }
    }
}
