using System;
using System.Collections.Specialized;
using System.Linq;

using FortitudeServer.Entities;
using FortitudeServer.Responses;

namespace FortitudeServer.Requests
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

            var cache = new Cache {
                AccountID = acc.AccountID,
                Balance = units,
                Latitude = lat,
                Longitude = lng,
                Name = CacheNamer.GenerateRandomName()
            };

            DatabaseManager.Update(ply);
            DatabaseManager.Insert(cache);

            cache = DatabaseManager.Select<Cache>(x => x.AccountID == acc.AccountID
                && x.Name == cache.Name).Last();

            DatabaseManager.Insert(new Event(EventType.CachePlaced, cache.CacheID));
            DatabaseManager.Insert(new Event(EventType.PlaceCache, acc.AccountID, cache.CacheID));

            return new CacheInfoResponse(cache);
        }
    }
}
