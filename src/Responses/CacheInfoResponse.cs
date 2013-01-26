using System;
using System.Collections.Generic;

using TestServer.Entities;

namespace TestServer.Responses
{
    [JSONSerializable]
    public class CacheInfoResponse : Response
    {
        [Serialize("caches")]
        public readonly List<Cache> Caches;

        public CacheInfoResponse(List<Cache> caches)
            : base(true)
        {
            Caches = caches;
        }
    }
}
