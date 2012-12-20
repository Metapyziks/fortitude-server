using System;

using TestServer.Entities;

namespace TestServer.Responses
{
    [Serializable]
    public class UserBalanceResponse : Response
    {
        [Serialize( "balance" )]
        public readonly int Balance;

        public UserBalanceResponse( Player ply )
            : base( true )
        {
            Balance = ply.Balance;
        }
    }
}
