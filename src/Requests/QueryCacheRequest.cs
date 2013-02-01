using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestServer.Entities;

namespace TestServer.Requests
{
    class QueryCacheRequest : LocationalRequest
    {
        public override Responses.Response Respond(System.Collections.Specialized.NameValueCollection args)
        {
            Account acc;
            Responses.ErrorResponse error;

            if (!this.CheckAuth(args, out acc, out error, true)) {
                return error;
            }

            double lat, lng;
            if (!this.CheckLocation(args, out error, out lat, out lng)) {
                return error;
            }

            int cacheid;
            if (!int.TryParse(args["cacheid"] ?? "0", out cacheid)) {
                return new Responses.ErrorResponse("invalid cache id");
            }

            var cache = DatabaseManager.SelectFirst<Cache>(x => x.CacheID == cacheid);
            if (cache == null) {
                return new Responses.ErrorResponse("cache does not exist");
            }

            double dist = Tools.GetDistance(lat, lng, cache.Latitude, cache.Longitude);
            if (dist > Cache.MaxInteractionDistance) {
                return new Responses.ErrorResponse("too far away from cache ({0})", dist.ToString("F2"));
            }

            return new Responses.CacheInfoResponse(new List<Cache>(){ cache });
        }
    }
}
