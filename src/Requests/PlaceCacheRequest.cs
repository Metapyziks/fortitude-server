using System;
using System.Collections.Specialized;
using System.Linq;

using TestServer.Entities;
using TestServer.Responses;

namespace TestServer.Requests
{
    [RequestTypeName("placecache")]
    class PlaceCacheRequest : LocationalRequest
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
            if (!Int32.TryParse(args["units"] ?? "0", out units)) {
                return new ErrorResponse("invalid units");
            }

            if (units < Cache.PlacementCost) {
                return new ErrorResponse("not enough units, {0} needed", Cache.PlacementCost);
            }

            var ply = Player.GetPlayer(acc);

            if (ply.Balance < units) {
                return new ErrorResponse("not enough points in balance");
            }

            if (Cache.FindNearby(lat, lng, Cache.MinPlacementDistance).Count() > 0) {
                return new ErrorResponse("too close to another cache");
            }

            ply.Balance -= units;
            units -= Cache.PlacementCost - 1;
            DatabaseManager.Update(ply);

            var cache = new Cache {
                AccountID = acc.AccountID,
                Balance = units,
                Latitude = lat,
                Longitude = lng,
                Name = CacheNamer.GenerateRandomName()
            };
            DatabaseManager.Insert(cache);

            return new CacheInfoResponse(cache);
        }
    }
}
