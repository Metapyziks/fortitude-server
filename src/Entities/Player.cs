using System;

namespace TestServer.Entities
{
    [Serializable, DatabaseEntity]
    public class Player
    {
        public struct Fields
        {
            public const String AccountID = "AccountID";
            public const String Balance = "Balance";
        }

        public static Player GetPlayer( Account acc )
        {
            return GetPlayer( acc.AccountID );
        }

        public static Player GetPlayer( int accountID )
        {
            Player ply = DatabaseManager.SelectFirst<Player>( x => x.AccountID == accountID );

            if ( ply == null )
            {
                ply = new Player()
                {
                    AccountID = accountID,
                    Balance = 0
                };

                DatabaseManager.Insert( ply );
            }

            return ply;
        }

        [Serialize( "accountid" )]
        [PrimaryKey]
        public int AccountID { get; set; }

        [Serialize( "balance" )]
        [NotNull]
        public int Balance { get; set; }
    }
}
