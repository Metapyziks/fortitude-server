using System;
using System.Collections.Generic;

using System.Text.RegularExpressions;

namespace TestServer.Entities
{
    public enum Rank : byte
    {
        Unverified = 0,
        Verified = 1,
        Admin = 3,
        Owner = 7
    }

    [Serializable, DatabaseEntity]
    public class Account
    {
        private static readonly Regex stUsernameRegex;
        private static readonly Regex stEmailRegex;
        private static readonly Regex stPasswordHashRegex;

        static Account()
        {
            stUsernameRegex = new Regex( "^[a-zA-Z0-9_-]([ a-zA-Z0-9_-]{1,31})$" );
            stEmailRegex = new Regex( "^[a-z0-9._%-]+@[a-z0-9.-]+\\.[a-z]{2,4}$" );
            stPasswordHashRegex = new Regex( "^[0-9a-f]{32}$" );
        }

        public static bool IsUsernameValid( String username )
        {
            return stUsernameRegex.IsMatch( username );
        }

        public static bool IsEmailValid( String email )
        {
            if ( email.Length > 64 )
                return false;

            return stEmailRegex.IsMatch( email );
        }

        public static bool IsPasswordHashValid( String hash )
        {
            return stPasswordHashRegex.IsMatch( hash );
        }

        public static Responses.ErrorResponse AttemptRegister( String uname, String email, String phash )
        {
            if ( uname == null || uname.Length == 0 )
                return new Responses.ErrorResponse( "no username given" );

            if ( !Account.IsUsernameValid( uname ) )
                return new Responses.ErrorResponse( "invalid username" );

            if ( phash == null || phash.Length == 0 )
                return new Responses.ErrorResponse( "no password hash given" );

            phash = phash.ToLower();

            if ( !Account.IsPasswordHashValid( phash ) )
                return new Responses.ErrorResponse( "invalid password hash" );

            if ( email == null || email.Length == 0 )
                return new Responses.ErrorResponse( "no email address given" );

            email = email.ToLower();

            if ( !Account.IsEmailValid( email ) )
                return new Responses.ErrorResponse( "invalid email address" );

            List<Account> clashes = DatabaseManager.Select<Account>( x => x.Username == uname || x.Email == email );

            if ( clashes.Count > 0 )
            {
                if ( clashes[0].Username == uname )
                    return new Responses.ErrorResponse( "username already in use" );
                else
                    return new Responses.ErrorResponse( "email already in use" );
            }

            Account account = new Account()
            {
                Username = uname,
                PasswordHash = phash.ToCharArray(),
                Email = email,
                Rank = Rank.Unverified,
                RegistrationDate = DateTime.Now
            };

            DatabaseManager.Insert( account );

            account = DatabaseManager.Select<Account>( x => x.Email == email )[0];
            ActivationCode.Create( account ).SendEmail( account );

            return null;
        }

        public static Responses.ErrorResponse AttemptActivate( String username )
        {
            Account account = DatabaseManager.SelectFirst<Account>( x => x.Username == username );

            if ( account == null )
                return new Responses.ErrorResponse( "username not recognised" );

            return account.Activate();
        }

        public static Responses.ErrorResponse AttemptActivate( String email, String code )
        {
            if ( email == null || email.Length == 0 )
                return new Responses.ErrorResponse( "no email address given" );

            if ( !Account.IsEmailValid( email ) )
                return new Responses.ErrorResponse( "invalid email address" );

            if ( code == null || code.Length == 0 )
                return new Responses.ErrorResponse( "no activation code given" );

            if ( !Account.IsPasswordHashValid( code ) )
                return new Responses.ErrorResponse( "invalid activation code" );

            Account account = DatabaseManager.SelectFirst<Account>( x => x.Email == email );

            if ( account == null )
                return new Responses.ErrorResponse( "email address not recognised" );

            return account.Activate( code );
        }

        public static Responses.ErrorResponse AttemptPromote( String username )
        {
            Account account = DatabaseManager.SelectFirst<Account>( x => x.Username == username );

            if ( account == null )
                return new Responses.ErrorResponse( "username not recognised" );

            return account.Promote();
        }

        [Serialize( "accountid" )]
        [PrimaryKey, AutoIncrement]
        public int AccountID { get; set; }

        [Serialize( "uname" )]
        [Capacity( 32 ), Unique]
        public String Username { get; set; }

        [Capacity( 32 ), FixedLength, NotNull]
        public char[] PasswordHash { get; set; }

        [Capacity( 64 ), Unique]
        public String Email { get; set; }

        [Serialize( "joindate" )]
        [NotNull]
        public DateTime RegistrationDate { get; set; }

        [Serialize( "rank" )]
        [NotNull]
        public Rank Rank { get; set; }

        public bool IsVerified
        {
            get { return ( Rank & Rank.Verified ) == Rank.Verified; }
        }

        public bool IsAdmin
        {
            get { return ( Rank & Rank.Admin ) == Rank.Admin; }
        }

        public bool IsOwner
        {
            get { return ( Rank & Rank.Owner ) == Rank.Owner; }
        }

        public Responses.ErrorResponse Activate( String code = null )
        {
            if ( IsVerified )
                return new Responses.ErrorResponse( "account already activated" );

            ActivationCode request = ActivationCode.Get( this );

            if ( code != null && ( request == null || !code.EqualsCharArray( request.Code ) ) )
                return new Responses.ErrorResponse( "incorrect activation code" );

            if( request != null )
                request.Remove();

            Rank = Rank.Verified;
            DatabaseManager.Update( this );

            return null;
        }

        public Responses.ErrorResponse Promote()
        {
            if ( IsAdmin )
                return new Responses.ErrorResponse( "account already promoted" );

            Rank = Rank.Admin;
            DatabaseManager.Update( this );

            return null;
        }
    }
}
