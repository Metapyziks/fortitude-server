using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FortitudeServer.Entities
{
    [DatabaseEntity, JSONSerializable]
    public class SpecialEvent
    {
        private static readonly Regex _sAddressRegex;

        static SpecialEvent()
        {
            String part = "([a-zA-Z0-9]{2})";
            _sAddressRegex = new Regex("^" + part + "(:" + part + "){5,7}?$");
        }

        public static bool IsAddressValid(String address)
        {
            return _sAddressRegex.IsMatch(address);
        }

        [Serialize("eventid")]
        [PrimaryKey, AutoIncrement]
        public int SpecialEventID { get; set; }

        [Serialize("name")]
        [Capacity(32), NotNull]
        public String Name { get; set; }

        [Serialize("balance")]
        [NotNull]
        public int Balance { get; set; }

        [Serialize("reward")]
        [NotNull]
        public int Reward { get; set; }

        [Serialize("macaddress")]
        [Capacity(24), NotNull]
        public String MACAddress { get; set; }

        [Serialize("expires")]
        [NotNull]
        public DateTime Expires { get; set; }

        [CleanUpMethod]
        public void Cleanup()
        {
            DatabaseManager.Delete<ClaimedEvent>(x => x.SpecialEventID == SpecialEventID);
        }
    }
}
