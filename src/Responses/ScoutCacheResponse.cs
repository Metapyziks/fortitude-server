using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FortitudeServer.Responses
{
    [JSONSerializable]
    class ScoutCacheResponse : Response
    {
        private int _garrison;

        [Serialize("cacheid")]
        public int CacheID { get; set; }

        [Serialize("scouts")]
        public int Scouts { get; set; }

        [Serialize("survivors")]
        public int Survivors { get; set; }

        [Serialize("garrison")]
        public int Garrison
        {
            get { return Survivors > 0 ? _garrison : 0; }
            set { _garrison = value; }
        }

        public ScoutCacheResponse()
            : base(true) { }
    }
}
