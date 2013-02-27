using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FortitudeServer.Entities;
using FortitudeServer.Responses;

namespace FortitudeServer.Requests
{
    [RequestTypeName("transfer")]
    class CacheTransactionRequest : LocationalRequest
    {
        public override Response Respond(System.Collections.Specialized.NameValueCollection args)
        {
            Account acc;
            ErrorResponse error;

            if (!CheckAuth(args, out acc, out error, true)) {
                return error;
            }

            double lat, lng;
            if (!CheckLocation(args, out error, out lat, out lng)) {
                return error;
            }

            int units;
            if (!Int32.TryParse(args["units"] ?? "0", out units) || units <= 0) {
                return new ErrorResponse("invalid units");
            }

            if (units == 0) {
                return new ErrorResponse("can't transfer no units");
            }

            int cacheid;
            if (!Int32.TryParse(args["cacheid"] ?? "0", out cacheid) || cacheid <= 0) {
                return new ErrorResponse("invalid cache id");
            }

            var cache = DatabaseManager.SelectFirst<Cache>(x => x.CacheID == cacheid);
            if (cache == null) {
                return new ErrorResponse("cache does not exist");
            }

            if (cache.AccountID != acc.AccountID) {
                return new ErrorResponse("you do not own the cache");
            }

            double dist = Tools.GetDistance(lat, lng, cache.Latitude, cache.Longitude);
            if (dist > Cache.MaxInteractionDistance) {
                return new ErrorResponse("too far away from cache ({0}m)", dist.ToString("F2"));
            }

            var ply = Player.GetPlayer(acc);

            if (ply.Balance < units || cache.Balance < -units) {
                return new ErrorResponse("not enough points in balance");
            }

            ply.Balance -= units;
            cache.Balance += units;

            DatabaseManager.Update(ply);
            DatabaseManager.Update(cache);

            return new Response(true);
        }
    }
}
