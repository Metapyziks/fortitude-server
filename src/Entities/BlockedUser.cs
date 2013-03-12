namespace FortitudeServer.Entities
{
    [DatabaseEntity]
    public class BlockedUser
    {
        [PrimaryKey, AutoIncrement]
        public int BlockedUserID { get; set; }

        [ForeignKey(typeof(Account)), NotNull]
        public int BlockerID { get; set; }

        [ForeignKey(typeof(Account)), NotNull]
        public int BlockedID { get; set; }
    }
}
