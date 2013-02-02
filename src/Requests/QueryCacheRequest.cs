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
    [RequestTypeName("scout")]
    class QueryCacheRequest : LocationalRequest
    {
        public override Responses.Response Respond(NameValueCollection args)
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

            int cacheid;
            if (!Int32.TryParse(args["cacheid"] ?? "0", out cacheid) || cacheid <= 0) {
                return new ErrorResponse("invalid cache id");
            }

            var cache = DatabaseManager.SelectFirst<Cache>(x => x.CacheID == cacheid);
            if (cache == null) {
                return new ErrorResponse("cache does not exist");
            }

            double dist = Tools.GetDistance(lat, lng, cache.Latitude, cache.Longitude);
            if (dist > Cache.MaxInteractionDistance) {
                return new ErrorResponse("too far away from cache ({0}m)", dist.ToString("F2"));
            }

            return new CacheInfoResponse(cache);
        }
    }
}
