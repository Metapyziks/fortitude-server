using System.Collections.Specialized;

using FortitudeServer.Entities;
using FortitudeServer.Responses;

namespace FortitudeServer.Requests
{
    [RequestTypeName("specialevents")]
    class SpecialEventListRequest : Request
    {
        public override Response Respond(NameValueCollection args)
        {
            Account acc;
            ErrorResponse error;

            if (!this.CheckAuth(args, out acc, out error, true))
                return error;

            var events = DatabaseManager.SelectAll<SpecialEvent>();
            return new SpecialEventListResponse(events);
        }
    }
}
