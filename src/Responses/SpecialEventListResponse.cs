using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FortitudeServer.Entities;

namespace FortitudeServer.Responses
{
    [JSONSerializable]
    public class SpecialEventListResponse : Response
    {
        [Serialize("addresses")]
        public readonly String[] Addresses;

        public SpecialEventListResponse(List<SpecialEvent> users)
            : base(true)
        {
            Addresses = users.Select(x => x.MACAddress).ToArray();
        }
    }
}
