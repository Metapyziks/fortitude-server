using System;
using System.Reflection;

namespace FortitudeServer
{
    public static class Tools
    {
        private static Random stRand = new Random();

        public static readonly DateTime UnixEpoch =
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static bool IsDefined<T>(this MemberInfo minfo, bool inherit = false)
            where T : Attribute
        {
            return minfo.IsDefined(typeof(T), inherit);
        }

        public static T GetCustomAttribute<T>(this MemberInfo minfo, bool inherit = false)
            where T : Attribute
        {
            T[] attribs = (T[]) minfo.GetCustomAttributes(typeof(T), inherit);

            if (attribs.Length == 0)
                return null;

            return attribs[0];
        }

        public static bool IsNumerical(this Object obj)
        {
            return (obj is Byte || obj is SByte ||
                obj is UInt16 || obj is Int16 ||
                obj is UInt32 || obj is Int32 ||
                obj is UInt64 || obj is Int64 ||
                obj is Single || obj is Double || obj is Decimal);
        }

        public static bool EqualsCharArray(this String str, char[] chars)
        {
            if (str.Length != chars.Length)
                return false;

            for (int i = 0; i < chars.Length; ++i)
                if (str[i] != chars[i])
                    return false;

            return true;
        }

        public static double Random()
        {
            return stRand.NextDouble();
        }

        public static bool CoinToss(double weight = 0.5)
        {
            return stRand.NextDouble() < weight;
        }

        public static char[] GenerateHash(int length = 32)
        {
            char[] str = new char[length];
            for (int i = 0; i < length; ++i)
                str[i] = stRand.Next(16).ToString("X").ToLower()[0];

            return str;
        }

        public static bool Extends(this Type type, Type other)
        {
            if (type == other)
                return true;
            if (type.BaseType != null)
                return Extends(type.BaseType, other);
            return false;
        }

        public static double GetDistance(double latA, double lngA, double latB, double lngB)
        {
            const double radius = 6371009d;
            const double toRad = Math.PI / 180d;

            latA *= toRad;
            lngA *= toRad;
            latB *= toRad;
            lngB *= toRad;

            double dLat = (latB - latA);
            double dLng = (lngB - lngA);

            double dLatS = Math.Sin(dLat / 2d);
            double dLngS = Math.Sin(dLng / 2d);

            double a =  dLatS * dLatS + dLngS * dLngS * Math.Cos(latA) * Math.Cos(latB);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return radius * c;
        }
    }
}
