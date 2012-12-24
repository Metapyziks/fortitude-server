using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using TestServer.Entities;

namespace TestServer
{
    public class AuthSession
    {
        public const double ExpirationTime = 60.0 * 15.0;

        private static readonly Regex stCodeRegex = new Regex( "^[0-9a-f]{16}$" );

        private static Dictionary<Int32, AuthSession> stSessions = new Dictionary<int, AuthSession>();

        public static AuthSession Create( Account account )
        {
            AuthSession sess = new AuthSession()
            {
                AccountID = account.AccountID,
                SessionCode = Tools.GenerateHash( 16 ),
                LastRefresh = DateTime.Now
            };

            if ( stSessions.ContainsKey( account.AccountID ) )
                stSessions[ account.AccountID ] = sess;
            else
                stSessions.Add( account.AccountID, sess );

            return sess;
        }

        public static AuthSession Get( Account account )
        {
            if ( stSessions.ContainsKey( account.AccountID ) )
            {
                AuthSession sess = stSessions[ account.AccountID ];

                if ( sess.IsExpired )
                    stSessions.Remove( account.AccountID );

                return sess;
            }

            return null;
        }

        public static void Remove( Account account )
        {
            Remove( account.AccountID );
        }

        public static void Remove( int accountID )
        {
            if ( stSessions.ContainsKey( accountID ) )
                stSessions.Remove( accountID );
        }

        public static bool IsCodeValid( String code )
        {
            return stCodeRegex.IsMatch( code );
        }

        public int AccountID { get; private set; }
        public DateTime LastRefresh { get; private set; }
        public char[] SessionCode { get; private set; }

        public bool IsExpired
        {
            get { return ( LastRefresh - DateTime.Now ).TotalSeconds >= ExpirationTime; }
        }

        public void Refresh()
        {
            LastRefresh = DateTime.Now;
        }
    }
}
