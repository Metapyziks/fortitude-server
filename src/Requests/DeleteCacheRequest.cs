using System.Collections.Specialized;

using FortitudeServer.Responses;
using FortitudeServer.Entities;

namespace FortitudeServer.Requests
{
    [RequestTypeName("deletecache")]
    class DeleteCacheRequest : Request
    {
        public override Response Respond(NameValueCollection args)
        {
            Account acc;
            ErrorResponse error;

            if (!this.CheckAuth(args, out acc, out error, true))
                return error;

            if (acc.Rank < Rank.Admin) {
                return new ErrorResponse("you must be an administrator to perform this action");
            }

            if (args["cacheid"] == null) {
                return new ErrorResponse("expected a cache id");
            }

            int id;
            if (!int.TryParse(args["cacheid"], out id)) {
                return new ErrorResponse("invalid cache id");
            }

            var cache = DatabaseManager.SelectFirst<Cache>(x => x.CacheID == id);

            if (cache == null) {
                return new ErrorResponse("invalid mescachesage id");
            }

            DatabaseManager.Delete(cache);

            return new Response(true);
        }
    }
}
