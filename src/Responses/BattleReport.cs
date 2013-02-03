using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TestServer.Entities;

namespace TestServer.Responses
{
    [JSONSerializable]
    public class BattleReport : Response
    {
        [JSONSerializable]
        public class UnitReport
        {
            [Serialize("initial")]
            public int Initial { get; set; }

            [Serialize("survivors")]
            public int Survivors { get; set; }

            [Serialize("fatalities")]
            public int Fatalities { get; set; }

            [Serialize("deserters")]
            public int Deserters { get; set; }
        }

        [Serialize("attackerid")]
        public int AttackerID { get; set; }

        [Serialize("defenderid")]
        public int DefenderID { get; set; }

        [Serialize("cache")]
        public Cache Cache { get; set; }

        [Serialize("attackers")]
        public UnitReport Attackers { get; set; }

        [Serialize("defenders")]
        public UnitReport Defenders { get; set; }

        [Serialize("victory")]
        public bool IsVictory { get; set; }

        public BattleReport()
            : base(true) { }
    }
}
