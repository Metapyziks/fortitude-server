using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FortitudeServer.Entities;

namespace FortitudeServer.Responses
{
    [JSONSerializable]
    public class BattleReportResponse : Response
    {
        public BattleReport BattleReport { get; set; }

        [Serialize("attackerid")]
        public int AttackerID { get { return BattleReport.AttackerID; } }

        [Serialize("defenderid")]
        public int DefenderID { get { return BattleReport.DefenderID; } }

        [Serialize("cache")]
        public Cache Cache { get; set; }

        [Serialize("attackerinitial")]
        public int AttackerInitial { get { return BattleReport.AttackerInitial; } }
        [Serialize("attackersurvivors")]
        public int AttackerSurvivors { get { return BattleReport.AttackerSurvivors; } }
        [Serialize("attackerfatalities")]
        public int AttackerFatalities { get { return BattleReport.AttackerFatalities; } }
        [Serialize("attackerdeserters")]
        public int AttackerDeserters { get { return BattleReport.AttackerDeserters; } }

        [Serialize("defenderinitial")]
        public int DefenderInitial { get { return BattleReport.DefenderInitial; } }
        [Serialize("defendersurvivors")]
        public int DefenderSurvivors { get { return BattleReport.DefenderSurvivors; } }
        [Serialize("defenderfatalities")]
        public int DefenderFatalities { get { return BattleReport.DefenderFatalities; } }
        [Serialize("defenderdeserters")]
        public int DefenderDeserters { get { return BattleReport.DefenderDeserters; } }

        [Serialize("victory")]
        public bool IsVictory { get { return BattleReport.DefenderSurvivors > 0; } }

        public BattleReportResponse(Cache cache, BattleReport report)
            : base(true)
        {
            Cache = cache;
            BattleReport = report;
        }
    }
}
