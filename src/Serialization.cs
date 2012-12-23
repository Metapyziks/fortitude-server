using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace TestServer
{
    [AttributeUsage( AttributeTargets.Class )]
    class JSONSerializableAttribute : Attribute { }

    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field )]
    class SerializeAttribute : Attribute
    {
        public readonly String KeyName;

        public SerializeAttribute( String keyName )
        {
            KeyName = keyName;
        }
    }

    public static class JSONSerializer
    {
        public static String Serialize( Object obj )
        {
            StringBuilder builder = new StringBuilder();
            Serialize( obj, builder );
            return builder.ToString();
        }

        public static void Serialize( Object obj, StringBuilder builder )
        {
            if ( obj == null )
            {
                builder.Append( "null" );
                return;
            }

            if ( obj is string )
            {
                builder.AppendFormat( "\"{0}\"", ( (string) obj ).Replace( "\"", "\\\"" ) );
                return;
            }

            if ( obj is bool )
            {
                builder.Append( (bool) obj ? "true" : "false" );
                return;
            }

            if ( obj is DateTime )
            {
                builder.Append( ( (int) ( ( (DateTime) obj ).ToUniversalTime() -
                    Tools.UnixEpoch ).TotalSeconds ).ToString() );
                return;
            }

            if ( obj is Enum )
            {
                builder.AppendFormat( "\"{0}\"", ( (Enum) obj ).ToString() );
                return;
            }

            if ( obj.IsNumerical() )
            {
                builder.Append( obj.ToString() );
                return;
            }

            if ( obj is KeyValuePair<String, Object> )
            {
                KeyValuePair<String, Object> pair = (KeyValuePair<String, Object>) obj;
                builder.Append( "{\"" + pair.Key + "\":" );
                Serialize( pair.Value, builder );
                builder.Append( "}" );
                return;
            }

            Type type = obj.GetType();
            if ( type.IsDefined( typeof( JSONSerializableAttribute ), true ) )
            {
                builder.Append( "{" );
                bool first = true;
                IEnumerable<MemberInfo> members =
                    ( (IEnumerable<MemberInfo>) type.GetFields() ).Union( type.GetProperties() );
                foreach ( MemberInfo member in members )
                {
                    if ( member.IsDefined( typeof( SerializeAttribute ), true ) )
                    {
                        if ( !first ) builder.Append( "," ); else first = false;

                        SerializeAttribute attrib = member.GetCustomAttribute<SerializeAttribute>( true );
                        builder.AppendFormat( "\"{0}\":", attrib.KeyName );
                        Object val = ( member is FieldInfo ) ?
                            ( (FieldInfo) member ).GetValue( obj ) :
                            ( (PropertyInfo) member ).GetValue( obj, null );
                        Serialize( val, builder );
                    }
                }
                builder.Append( "}" );
                return;
            }

            if ( obj is IEnumerable<KeyValuePair<String, Object>> )
            {
                builder.Append( "{" );
                bool first = true;
                foreach ( KeyValuePair<String, Object> pair in (IEnumerable<KeyValuePair<String, Object>>) obj )
                {
                    if ( !first )  builder.Append( "," ); else first = false;
                    Serialize( pair, builder );
                }
                builder.Append( "}" );
                return;
            }

            if ( obj is IEnumerable<Object> )
            {
                builder.Append( "[" );
                bool first = true;
                foreach ( Object o in (IEnumerable<Object>) obj )
                {
                    if ( !first ) builder.Append( "," ); else first = false;
                    Serialize( o, builder );
                }
                builder.Append( "]" );
                return;
            }

            builder.Append( obj.ToString() );
        }
    }
}
