using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FortitudeServer.Entities;

namespace FortitudeServer.Responses
{
    [JSONSerializable]
    class SpecialEventClaimedResponse : Response
    {
        [Serialize("event")]
        public SpecialEvent Event { get; private set; }

        public SpecialEventClaimedResponse(SpecialEvent special)
            : base(true)
        {
            Event = special;
        }
    }
}
