using System;
using System.Collections.Generic;

using TestServer.Entities;

namespace TestServer.Responses
{
    [JSONSerializable]
    public class CacheInfoResponse : Response
    {
        [Serialize("caches")]
        public readonly Cache[] Caches;

        public CacheInfoResponse(params Cache[] caches)
            : base(true)
        {
            Caches = caches;
        }
    }
}
