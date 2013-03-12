using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FortitudeServer.Responses
{
    [JSONSerializable]
    class SettingResponse : Response
    {
        [Serialize("value")]
        public String Value { get; private set; }

        public SettingResponse(String value)
            : base(true)
        {
            Value = value;
        }
    }
}
