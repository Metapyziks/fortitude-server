using System;
using System.Collections.Generic;
using System.Linq;

using FortitudeServer.Entities;

namespace FortitudeServer.Responses
{
    [JSONSerializable]
    public class UserInfoResponse : Response
    {
        [Serialize("users")]
        public readonly Object Users;

        public UserInfoResponse(List<Account> users, List<int> cacheCounts = null)
            : base(true)
        {
            if (cacheCounts == null) {
                Users = users;
            } else {
                Users = users.Select((x, i) =>
                    new SerializedTuple { x, new SerializedNamedValue("caches", cacheCounts[i]) } );
            }
        }
    }
}
