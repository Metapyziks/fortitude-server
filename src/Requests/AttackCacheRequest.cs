using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TestServer.Entities;
using TestServer.Responses;

namespace TestServer.Requests
{
    [RequestTypeName("attack")]
    class AttackCacheRequest : LocationalRequest
    {
        public override Response Respond(NameValueCollection args)
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

            var ply = Player.GetPlayer(acc);
            if (ply.Balance < units) {
                return new ErrorResponse("not enough points in balance");
            }

            int cacheid;
            if (!Int32.TryParse(args["cacheid"] ?? "0", out cacheid) || cacheid <= 0) {
                return new ErrorResponse("invalid cache id");
            }

            var cache = DatabaseManager.SelectFirst<Cache>(x => x.CacheID == cacheid);
            if (cache == null) {
                return new ErrorResponse("cache does not exist");
            }

            if (cache.AccountID == acc.AccountID) {
                return new ErrorResponse("unable to attack a cache owned by you");
            }

            if (!cache.HasOwner) {
                return new ErrorResponse("can't attack a cache with no owner");
            }

            double dist = Tools.GetDistance(lat, lng, cache.Latitude, cache.Longitude);
            if (dist > Cache.MaxInteractionDistance) {
                return new ErrorResponse("too far away from cache ({0}m)", dist.ToString("F2"));
            }

            return cache.Attack(ply, units);
        }
    }
}
