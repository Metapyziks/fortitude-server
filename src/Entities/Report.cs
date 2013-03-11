using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FortitudeServer.Entities
{
    public enum ReportType : byte
    {
        Account = 1,
        Message = 2,
        Cache = 4
    }

    [DatabaseEntity]
    public class Report
    {
        [PrimaryKey, AutoIncrement]
        public int ReportID { get; set; }

        [NotNull]
        public ReportType Type { get; set; }

        [NotNull, ForeignKey(typeof(Account), typeof(Cache), typeof(Message))]
        public int ContextID { get; set; }

        [Capacity(255), NotNull]
        public String Message { get; set; }
    }
}
