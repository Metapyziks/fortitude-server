using System;
using System.Collections.Specialized;

using FortitudeServer.Entities;
using FortitudeServer.Responses;

namespace FortitudeServer.Requests
{
    [RequestTypeName("setting")]
    class SettingRequest : Request
    {
        public override Response Respond(NameValueCollection args)
        {
            Account acc;
            ErrorResponse error;

            if (!CheckAuth(args, out acc, out error, true)) {
                return error;
            }

            String setting = args["name"];

            if (setting == null) {
                return new ErrorResponse("expected setting name");
            }

            String value = args["value"];
            var ply = Player.GetPlayer(acc);

            switch (setting) {
                case "receivemessages":
                    if (value != null) {
                        MessageSettings msgSetting = ply.MessageSettings;
                        if (!Enum.TryParse<MessageSettings>(value, out msgSetting)) {
                            return new ErrorResponse("invalid value");
                        }
                        ply.MessageSettings = msgSetting;
                        DatabaseManager.Update(ply);
                    }
                    value = ply.MessageSettings.ToString();
                    break;
                default:
                    return new ErrorResponse("invalid setting name");
            }

            return new SettingResponse(value);
        }
    }
}
