using System.Collections.Specialized;

namespace FortitudeServer.Requests
{
    [RequestTypeName("sendpassreset")]
    class PasswordResetRequest : Request
    {
        public override Responses.Response Respond(NameValueCollection args)
        {
            EmailValidationCode valid;
            return EmailValidationCode.AttemptCreate(EmailValidationType.ResetPassword, args["email"], out valid)
                ?? new Responses.Response(true);
        }
    }
}
