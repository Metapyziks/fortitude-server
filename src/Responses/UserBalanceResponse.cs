using System;

using TestServer.Entities;

namespace TestServer.Responses
{
    [JSONSerializable]
    public class UserBalanceResponse : Response
    {
        [Serialize("balance")]
        public readonly int Balance;

        [Serialize("garrison")]
        public readonly int Garrison;

        public UserBalanceResponse(int balance, int garrison)
            : base(true)
        {
            Balance = balance;
            Garrison = garrison;
        }
    }
}
