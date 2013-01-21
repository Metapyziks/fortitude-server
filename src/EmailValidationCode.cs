using System;
using System.Collections.Generic;

using TestServer.Entities;

namespace TestServer
{
    public enum EmailValidationType
    {
        Activate,
        ResetPassword
    }

    public class EmailValidationCode
    {
        public const double ExpirationTime = 60.0 * 60.0 * 24.0;

        private static Dictionary<EmailValidationType, Dictionary<Int32, EmailValidationCode>> stCodes;

        static EmailValidationCode()
        {
            stCodes = new Dictionary<EmailValidationType, Dictionary<int, EmailValidationCode>>();
            foreach (EmailValidationType etype in Enum.GetValues(typeof(EmailValidationType))) {
                stCodes[etype] = new Dictionary<int, EmailValidationCode>();
            }
        }

        public static EmailValidationCode Create(EmailValidationType type, Account account)
        {
            if (type == EmailValidationType.Activate && account.IsVerified)
                return null;

            EmailValidationCode code = new EmailValidationCode() {
                Type = type,
                AccountID = account.AccountID,
                Code = Tools.GenerateHash(),
                SentDate = DateTime.Now
            };

            if (stCodes[type].ContainsKey(account.AccountID))
                stCodes[type][account.AccountID] = code;
            else
                stCodes[type].Add(account.AccountID, code);

            return code;
        }

        public static Responses.ErrorResponse AttemptCreate(EmailValidationType type,
            String email, out EmailValidationCode valid)
        {
            valid = null;

            if (email == null || email.Length == 0)
                return new Responses.ErrorResponse("no email address given");

            if (!Account.IsEmailValid(email))
                return new Responses.ErrorResponse("invalid email address");

            Account account = DatabaseManager.SelectFirst<Account>(x => x.Email == email);

            if (account == null)
                return new Responses.ErrorResponse("invalid email address");

            valid = EmailValidationCode.Create(type, account);
            valid.SendEmail(account);

            return null;
        }

        public static EmailValidationCode Get(EmailValidationType type, Account account)
        {
            if (stCodes[type].ContainsKey(account.AccountID)) {
                EmailValidationCode code = stCodes[type][account.AccountID];

                if (!code.IsExpired)
                    return code;

                stCodes[type].Remove(account.AccountID);
            }

            return null;
        }

        public static Responses.ErrorResponse AttemptGet(EmailValidationType type,
            String email, String code, out EmailValidationCode valid)
        {
            valid = null;

            if (email == null || email.Length == 0)
                return new Responses.ErrorResponse("no email address given");

            if (!Account.IsEmailValid(email))
                return new Responses.ErrorResponse("invalid email address");

            if (code == null || code.Length == 0)
                return new Responses.ErrorResponse("no validation code given");

            if (!Account.IsPasswordHashValid(code))
                return new Responses.ErrorResponse("invalid validation code");

            var account = DatabaseManager.SelectFirst<Account>(x => x.Email == email);

            if (account == null)
                return new Responses.ErrorResponse("invalid email address");

            valid = EmailValidationCode.Get(type, account);

            if (valid == null || !code.EqualsCharArray(valid.Code)) {
                valid = null;
                return new Responses.ErrorResponse("invalid validation code");
            }

            if (valid.IsExpired) {
                valid = null;
                return new Responses.ErrorResponse("expired validation code");
            }

            return account.Activate(code);
        }

        public static void Remove(EmailValidationType type, Account account)
        {
            Remove(type, account.AccountID);
        }

        public static void Remove(EmailValidationType type, int accountID)
        {
            if (stCodes[type].ContainsKey(accountID))
                stCodes[type].Remove(accountID);
        }

        public EmailValidationType Type { get; private set; }
        public int AccountID { get; private set; }
        public char[] Code { get; private set; }
        public DateTime SentDate { get; private set; }

        public bool IsExpired
        {
            get { return (SentDate - DateTime.Now).TotalSeconds >= ExpirationTime; }
        }

        public void Remove()
        {
            Remove(Type, AccountID);
        }

        public void SendEmail(Account account)
        {
            var subject = string.Empty;
            switch (Type) {
                case EmailValidationType.Activate:
                    subject = "Foritude account activation"; break;
                case EmailValidationType.ResetPassword:
                    subject = "Foritude password reset"; break;
            }

            EmailManager.Send(account.Email, subject, String.Format(
@"Hey {0},

To finish the registration process just click this link: {1}{2}?email={3}&code={4}

Have fun!", account.Username, Program.ServerAddress, Type.ToString().ToLower(), account.Email, new String(Code)));
        }
    }
}
