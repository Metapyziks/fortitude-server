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
            EmailManager.Send(account.Email, "TestServer account activation", String.Format(
@"Hey {0},

To finish the registration process just click this link: {1}activate?email={2}&code={3}

Have fun!", account.Username, Program.ServerAddress, account.Email, new String(Code)));
        }
    }
}
