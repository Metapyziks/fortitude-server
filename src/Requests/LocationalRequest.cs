using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

using TestServer.Entities;

namespace TestServer.Requests
{
    abstract class LocationalRequest : Request
    {
        private static readonly byte[] _sSalt = new byte[] { 0x2a, 0x1e, 0x97, 0xab, 0x2b, 0xb1, 0x3c };
        private static readonly char[] _sCharMap;

        static LocationalRequest()
        {
            _sCharMap = new char[16];
            for (int i = 0; i < 16; ++i) {
                _sCharMap[i] = i.ToString("X")[0];
            }
        }

        public bool CheckLocation(NameValueCollection args, out Responses.ErrorResponse error,
            out double lat, out double lng)
        {
            error = new Responses.ErrorResponse("invalid location info");
            lat = 0d;
            lng = 0d;

            String latitude = args["lat"] ?? "";
            String longitude = args["lng"] ?? "";

            String timestampStr = args["time"];

            int timestamp;
            if (!int.TryParse(timestampStr, out timestamp)) {
                return false;
            }

            String locationHash = (args["hash"] ?? "").ToLower();

            if (!Account.IsPasswordHashValid(locationHash)) {
                return false;
            }

            var bytes = new byte[latitude.Length + longitude.Length + timestampStr.Length + _sSalt.Length];
            Array.Copy(_sSalt, 0, bytes, 0, 2);
            UnicodeEncoding.UTF8.GetBytes(latitude, 0, latitude.Length, bytes, 2);
            Array.Copy(_sSalt, 2, bytes, 2 + latitude.Length, 2);
            UnicodeEncoding.UTF8.GetBytes(longitude, 0, longitude.Length, bytes, 4 + latitude.Length);
            Array.Copy(_sSalt, 4, bytes, 4 + latitude.Length + longitude.Length, 1);
            UnicodeEncoding.UTF8.GetBytes(timestampStr, 0, timestampStr.Length, bytes, 5 + latitude.Length + longitude.Length);
            Array.Copy(_sSalt, 5, bytes, 5 + latitude.Length + longitude.Length + timestampStr.Length, 2);

            var md5 = new MD5CryptoServiceProvider();
            var hash = String.Join("", md5.ComputeHash(bytes).Select(x => x.ToString("X2"))).ToLower();

            if (hash != locationHash) {
                return false;
            }

            return double.TryParse(latitude, out lat) && double.TryParse(longitude, out lng);
        }
    }
}
