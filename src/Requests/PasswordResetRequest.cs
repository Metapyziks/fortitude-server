using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestServer.Requests;

namespace TestServer.src.Requests
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
