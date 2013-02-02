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
    [RequestTypeName("nearbycaches")]
    class NearbyCachesRequest : Request
    {
        public override Responses.Response Respond(NameValueCollection args)
        {
            Account acc;
            ErrorResponse error;

            if (!this.CheckAuth(args, out acc, out error, true))
                return error;

            double lat, lon, radius;
            if (!Double.TryParse(args["lat"], out lat) || !Double.TryParse(args["lon"], out lon)) {
                return new ErrorResponse("invalid latitude / longitude");
            }

            if (!Double.TryParse(args["radius"], out radius) || radius < 0.0) {
                return new ErrorResponse("invalid radius");
            }

            var caches = Cache.FindNearby(lat, lon, radius).ToList();
            caches.Where(x => x.AccountID != acc.AccountID).ToList().ForEach(x => x.Balance = -1);
            return new CacheInfoResponse(caches.ToArray());
        }
    }
}
