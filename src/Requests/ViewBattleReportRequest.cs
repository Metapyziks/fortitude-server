using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FortitudeServer.Entities;
using FortitudeServer.Responses;

namespace FortitudeServer.Requests
{
    [RequestTypeName("battlereport")]
    class ViewBattleReportRequest : Request
    {
        public override Response Respond(NameValueCollection args)
        {
            Account acc;
            ErrorResponse error;

            if (!this.CheckAuth(args, out acc, out error, true))
                return error;

            if (args["reportid"] == null) {
                return new ErrorResponse("expected a report id");
            }

            int id;
            if (!int.TryParse(args["reportid"], out id)) {
                return new ErrorResponse("invalid report id");
            }

            var report = DatabaseManager.SelectFirst<BattleReport>(x => x.AccountID == acc.AccountID && x.NotificationID == id);

            if (report == null) {
                return new ErrorResponse("invalid report id");
            }

            if (report.Status != ReadStatus.Read) {
                report.Status = ReadStatus.Read;
                DatabaseManager.Update(report);
            }

            var cache = DatabaseManager.SelectFirst<Cache>(x => x.CacheID == report.CacheID);

            if (cache == null) {
                return new ErrorResponse("the attacked cache no longer exists");
            }

            return new BattleReportResponse(cache, report);
        }
    }
}
