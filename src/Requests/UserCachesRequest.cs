using System.Linq;
using System.Collections.Specialized;

using TestServer.Entities;

namespace TestServer.Requests
{
    [RequestTypeName("caches")]
    class UserCachesRequest : Request
    {
        public override Responses.Response Respond(NameValueCollection args)
        {
            Account acc;
            Responses.ErrorResponse error;

            if (!this.CheckAuth(args, out acc, out error, true))
                return error;

            var caches = DatabaseManager.Select<Cache>(x => x.AccountID == acc.AccountID);

            return new Responses.CacheInfoResponse(caches);
        }
    }
}
