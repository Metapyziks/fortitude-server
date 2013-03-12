using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

using System.Text.RegularExpressions;

namespace FortitudeServer.Entities
{
    using AccountPred = Expression<Func<Account, bool>>;

    public enum Rank : byte
    {
        Unverified = 0,
        Verified = 1,
        Admin = 3,
        Owner = 7
    }

    [JSONSerializable, DatabaseEntity]
    public class Account
    {
        private static List<String> _sOwnerEmails = new List<string>();

        public static void AddOwnerEmails(params String[] emails)
        {
            _sOwnerEmails.AddRange(emails);

            var owners = DatabaseManager.Select<Account>(emails.Select(
                    x => (AccountPred) (acc => acc.Email == x)).ToArray());

            foreach (var owner in owners.Where(x => x.Rank != Rank.Unverified && x.Rank != Rank.Owner)) {
                owner.Rank = Rank.Owner;
                DatabaseManager.Update(owner);
            }
        }

        private static readonly Regex _sUsernameRegex;
        private static readonly Regex _sEmailRegex;
        private static readonly Regex _sPasswordHashRegex;

        private static readonly IEnumerable<String> _sReservedNames;

        static Account()
        {
            _sUsernameRegex = new Regex("^[a-zA-Z0-9_-]([ a-zA-Z0-9_-]{1,31})$");
            _sEmailRegex = new Regex("^[a-z0-9._%-]+@[a-z0-9.-]+\\.[a-z]{2,4}$");
            _sPasswordHashRegex = new Regex("^[0-9a-f]{32}$");

            _sReservedNames = new[] { "admin", "owner", "outlaw", "outlaws", "unowned" };
        }

        public static bool IsUsernameValid(String username)
        {
            username = username.ToLower();
            if (_sReservedNames.Contains(username)) return false;

            return _sUsernameRegex.IsMatch(username);
        }

        public static bool IsEmailValid(String email)
        {
            if (email.Length > 64)
                return false;

            return _sEmailRegex.IsMatch(email);
        }

        public static bool IsPasswordHashValid(String hash)
        {
            return _sPasswordHashRegex.IsMatch(hash);
        }

        public static Responses.ErrorResponse AttemptRegister(String uname, String email, String phash)
        {
            if (uname == null || uname.Length == 0)
                return new Responses.ErrorResponse("no username given");

            if (!Account.IsUsernameValid(uname))
                return new Responses.ErrorResponse("invalid username");

            if (phash == null || phash.Length == 0)
                return new Responses.ErrorResponse("no password hash given");

            phash = phash.ToLower();

            if (!Account.IsPasswordHashValid(phash))
                return new Responses.ErrorResponse("invalid password hash");

            if (email == null || email.Length == 0)
                return new Responses.ErrorResponse("no email address given");

            email = email.ToLower();

            if (!Account.IsEmailValid(email))
                return new Responses.ErrorResponse("invalid email address");

            List<Account> clashes = DatabaseManager.Select<Account>(x => x.Username == uname || x.Email == email);

            if (clashes.Count > 0) {
                if (clashes[0].Username == uname)
                    return new Responses.ErrorResponse("username already in use");
                else
                    return new Responses.ErrorResponse("email already in use");
            }

            Account account = new Account() {
                Username = uname,
                PasswordHash = phash.ToCharArray(),
                Email = email,
                Rank = Rank.Unverified,
                RegistrationDate = DateTime.Now,
                AvatarID = 2130837506
            };

            DatabaseManager.Insert(account);

            account = DatabaseManager.Select<Account>(x => x.Email == email)[0];
            EmailValidationCode.Create(EmailValidationType.Activate, account).SendEmail(account);

            return null;
        }

        public static Responses.ErrorResponse AttemptActivate(String username)
        {
            Account account = DatabaseManager.SelectFirst<Account>(x => x.Username == username);

            if (account == null) {
                return new Responses.ErrorResponse("username not recognised");
            }

            return account.Activate();
        }

        public static Responses.ErrorResponse AttemptActivate(String email, String code)
        {
            if (email == null || email.Length == 0)
                return new Responses.ErrorResponse("no email address given");

            if (!Account.IsEmailValid(email))
                return new Responses.ErrorResponse("invalid email address");

            if (code == null || code.Length == 0)
                return new Responses.ErrorResponse("no activation code given");

            if (!Account.IsPasswordHashValid(code))
                return new Responses.ErrorResponse("invalid activation code");

            Account account = DatabaseManager.SelectFirst<Account>(x => x.Email == email);

            if (account == null)
                return new Responses.ErrorResponse("email address not recognised");

            return account.Activate(code);
        }

        public static Responses.ErrorResponse AttemptPromote(String username)
        {
            Account account = DatabaseManager.SelectFirst<Account>(x => x.Username == username);

            if (account == null)
                return new Responses.ErrorResponse("username not recognised");

            return account.Promote();
        }

        public static Responses.ErrorResponse AttemptLogin(String username, String passwordHash, out AuthSession session)
        {
            session = null;

            if (username == null || username.Length == 0) {
                return new Responses.ErrorResponse("no username given");
            }

            if (!Account.IsUsernameValid(username)) {
                return new Responses.ErrorResponse("invalid username or password");
            }

            if (passwordHash != null && passwordHash.Length > 0) {
                if (!Account.IsPasswordHashValid(passwordHash)) {
                    return new Responses.ErrorResponse("invalid username or password");
                }
            } else {
                return new Responses.ErrorResponse("auth error: no password given");
            }

            var account = DatabaseManager.SelectFirst<Account>(x => x.Username == username);

            if (account == null || !passwordHash.EqualsCharArray(account.PasswordHash)) {
                return new Responses.ErrorResponse("invalid username or password");
            }

            session = AuthSession.Get(account);
            if (session == null || session.IsExpired)
                session = AuthSession.Create(account);

            return null;
        }

        [Serialize("accountid")]
        [PrimaryKey, AutoIncrement]
        public int AccountID { get; set; }

        [Serialize("uname")]
        [Capacity(32), Unique]
        public String Username { get; set; }

        [Capacity(32), FixedLength, NotNull]
        public char[] PasswordHash { get; set; }

        [Capacity(64), Unique]
        public String Email { get; set; }

        [Serialize("joindate")]
        [NotNull]
        public DateTime RegistrationDate { get; set; }

        [Serialize("rank")]
        [NotNull]
        public Rank Rank { get; set; }

        [Serialize("avatarid")]
        [NotNull]
        public int AvatarID { get; set; }

        public bool IsVerified
        {
            get { return (Rank & Rank.Verified) == Rank.Verified; }
        }

        public bool IsAdmin
        {
            get { return (Rank & Rank.Admin) == Rank.Admin; }
        }

        public bool IsOwner
        {
            get { return (Rank & Rank.Owner) == Rank.Owner; }
        }

        [CleanUpMethod]
        public void Cleanup()
        {
            if (AccountID <= 0) return;
            
            var caches = DatabaseManager.Select<Cache>(x => x.AccountID == AccountID);
            if (caches.Count == 0) return;

            foreach (var cache in caches) {
                cache.AccountID = 0;
                cache.Balance = 0;
                DatabaseManager.Update(cache);
            }

            DatabaseManager.Delete<Message>(x => x.AccountID == AccountID);
            DatabaseManager.Delete<BattleReport>(x => x.AccountID == AccountID);
            DatabaseManager.Delete<NPCInstance>(x => x.AccountID == AccountID);
            DatabaseManager.Delete<Player>(x => x.AccountID == AccountID);
            DatabaseManager.Delete<BlockedUser>(x => x.BlockerID == AccountID);
            DatabaseManager.Delete<ClaimedEvent>(x => x.AccountID == AccountID);
            DatabaseManager.Delete<Report>(x => x.AccountID == AccountID);
        }

        public Responses.ErrorResponse Activate(String code = null)
        {
            if (IsVerified)
                return new Responses.ErrorResponse("account already activated");

            EmailValidationCode request = EmailValidationCode.Get(EmailValidationType.Activate, this);

            if (code != null && (request == null || !code.EqualsCharArray(request.Code)))
                return new Responses.ErrorResponse("incorrect activation code");

            if (request != null)
                request.Remove();

            if (_sOwnerEmails.Exists(x => x == Email)) {
                Rank = Rank.Owner;
            } else {
                Rank = Rank.Verified;
            }
            
            DatabaseManager.Update(this);
            return null;
        }

        public Responses.ErrorResponse Promote()
        {
            if (IsAdmin)
                return new Responses.ErrorResponse("account already promoted");

            Rank = Rank.Admin;
            DatabaseManager.Update(this);

            return null;
        }

        public bool HasBlocked(Account account)
        {
            return Player.GetPlayer(account).MessageSettings == MessageSettings.BlockAll ||
                DatabaseManager.SelectFirst<BlockedUser>(x => x.BlockerID == AccountID &&
                    x.BlockedID == account.AccountID) != null;
        }
    }
}
