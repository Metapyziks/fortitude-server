using System.Linq;
using System.Collections.Specialized;

using TestServer.Entities;

namespace TestServer.Requests
{
    [RequestTypeName("userstats")]
    class UserStatsRequest : Request
    {
        public override Responses.Response Respond(NameValueCollection args)
        {
            Account acc;
            Responses.ErrorResponse error;

            if (!this.CheckAuth(args, out acc, out error, true))
                return error;

            Player ply = Player.GetPlayer(acc);
            var caches = DatabaseManager.Select<Cache>(x => x.AccountID == acc.AccountID);

            return new Responses.UserStatsResponse(ply.Balance, caches.Count, caches.Sum(x => x.Balance));
        }
    }
}
