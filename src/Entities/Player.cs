using System;

namespace TestServer.Entities
{
    [JSONSerializable, DatabaseEntity]
    public class Player
    {
        public static Player GetPlayer(Account acc)
        {
            return GetPlayer(acc.AccountID);
        }

        public static Player GetPlayer(int accountID)
        {
            Player ply = DatabaseManager.SelectFirst<Player>(x => x.AccountID == accountID);

            if (ply == null) {
                ply = new Player() {
                    AccountID = accountID,
                    Balance = 0
                };

                DatabaseManager.Insert(ply);
            }

            return ply;
        }

        [Serialize("accountid")]
        [PrimaryKey, ForeignKey(typeof(Account))]
        public int AccountID { get; set; }

        [Serialize("balance")]
        [NotNull]
        public int Balance { get; set; }
    }
}
