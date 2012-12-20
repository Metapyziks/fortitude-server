using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;

using TestServer.Entities;

namespace TestServer.Requests
{
    class RequestTypeNameAttribute : Attribute
    {
        public readonly String Name;

        public RequestTypeNameAttribute( String name )
        {
            Name = name;
        }
    }

    abstract class Request
    {
        private static Dictionary<String, Request> stRegistered;

        static Request()
        {
            stRegistered = new Dictionary<String, Request>();

            foreach ( Type type in Assembly.GetExecutingAssembly().GetTypes() )
            {
                if ( type.BaseType == typeof( Request ) )
                {
                    ConstructorInfo cons = type.GetConstructor( new Type[ 0 ] );
                    Request inst = (Request) cons.Invoke( new object[ 0 ] );

                    String name;
                    if ( type.IsDefined( typeof( RequestTypeNameAttribute ), false ) )
                        name = type.GetCustomAttribute<RequestTypeNameAttribute>().Name;
                    else
                    {
                        name = type.Name.ToLower();
                        if ( name.EndsWith( "request" ) )
                            name = name.Substring( 0, name.Length - ( "request" ).Length );
                    }

                    stRegistered.Add( name, inst );
                }
            }
        }

        public static void Register( String name, Request type )
        {
            stRegistered.Add( name, type );
        }

        public static Request Get( String name )
        {
            try
            {
                return stRegistered[ name ];
            }
            catch ( KeyNotFoundException )
            {
                return null;
            }
        }

        public bool CheckAuth( NameValueCollection args, out Account account,
            out Responses.ErrorResponse error, bool acceptSession = true )
        {
            error = new Responses.ErrorResponse( "auth error" );
            account = null;

            String sessionCode = args[ "session" ];
            String username = args[ "uname" ];
            String passwordHash = args[ "phash" ];

            if ( username == null || username.Length == 0 )
            {
                error = new Responses.ErrorResponse( "auth error: no username given" );
                return false;
            }

            if ( !Account.IsUsernameValid( username ) )
            {
                error = new Responses.ErrorResponse( "auth error: invalid username" );
                return false;
            }

            if ( acceptSession && sessionCode != null && sessionCode.Length > 0 )
            {
                if ( !AuthSession.IsCodeValid( sessionCode ) )
                {
                    error = new Responses.ErrorResponse( "auth error: invalid session code" );
                    return false;
                }
            }
            else if ( passwordHash != null && passwordHash.Length > 0 )
            {
                if ( !Account.IsPasswordHashValid( passwordHash ) )
                {
                    error = new Responses.ErrorResponse( "auth error: invalid password hash" );
                    return false;
                }
            }
            else
            {
                if ( acceptSession )
                    error = new Responses.ErrorResponse( "auth error: no password or session code given" );
                else
                    error = new Responses.ErrorResponse( "auth error: no password hash given" );
                
                return false;
            }

            account = DatabaseManager.SelectFirst<Account>( x => x.Username == username );

            if ( account == null )
            {
                error = new Responses.ErrorResponse( "auth error: unrecognised username" );
                return false;
            }

            if ( passwordHash != null && passwordHash.Length != 0 )
            {
                if ( !passwordHash.EqualsCharArray( account.PasswordHash ) )
                {
                    error = new Responses.ErrorResponse( "auth error: incorrect password hash" );
                    return false;
                }
            }
            else
            {
                AuthSession sess = AuthSession.Get( account );

                if ( sess == null || !sessionCode.EqualsCharArray( sess.SessionCode ) )
                {
                    error = new Responses.ErrorResponse( "auth error: incorrect or expired session code" );
                    return false;
                }

                sess.Refresh();
            }

            return true;
        }

        public abstract Responses.Response Respond( NameValueCollection args );
    }
}
