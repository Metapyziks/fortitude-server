using System;

using FortitudeServer.Entities;

namespace FortitudeServer.Responses
{
    [JSONSerializable]
    public class UserStatsResponse : Response
    {
        [Serialize("balance")]
        public readonly int Balance;

        [Serialize("cachecount")]
        public readonly int Caches;

        [Serialize("garrison")]
        public readonly int Garrison;

        public UserStatsResponse(int balance, int caches, int garrison)
            : base(true)
        {
            Balance = balance;
            Caches = caches;
            Garrison = garrison;
        }
    }
}
