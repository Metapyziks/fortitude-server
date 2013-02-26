using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using FortitudeServer.Entities;

namespace FortitudeServer
{
    [TestClass]
    public class DatabaseTests
    {
        [ClassInitialize]
        public static void CreateDatabase( TestContext ctx )
        {
            DatabaseManager.ConnectLocal();
        }

        [TestMethod]
        public void CreateDatabaseTest()
        {
            Assert.IsNotNull( DatabaseManager.GetTable<Account>(), "Table not created" );
            Assert.IsNotNull( DatabaseManager.GetTable<Player>(), "Table not created" );
        }

        [TestMethod]
        public void InsertIntoTest()
        {
            for ( int i = 0; i < 1024; ++i )
            {
                String name = String.Format( "TestUser{0}", i );
                DatabaseManager.Insert( new Account()
                {
                    Username = name,
                    Email = String.Format( "test{0}@blah.com", i ),
                    PasswordHash = "0123456789abcdef0123456789abcdef".ToCharArray(),
                    RegistrationDate = DateTime.Now,
                    Rank = Rank.Verified
                } );
                Account acc = DatabaseManager.SelectFirst<Account>( x => x.Username == name );
                Assert.IsNotNull( acc, "Entity not inserted" );
            }
        }

        [TestMethod]
        public void DeleteTest()
        {
            Account acc = new Account()
            {
                Username = "TestUserB",
                Email = "testb@blah.com",
                PasswordHash = "0123456789abcdef0123456789abcdef".ToCharArray(),
                Rank = Rank.Verified
            };

            DatabaseManager.Insert( acc );
            acc = DatabaseManager.SelectFirst<Account>( x => x.Username == acc.Username );
            DatabaseManager.Delete<Account>( x => x.Username == acc.Username );
            acc = DatabaseManager.SelectFirst<Account>( x => x.Username == acc.Username );

            Assert.IsNull( acc, "Entity not deleted" );
        }

        [ClassCleanup]
        public static void CloseDatabase()
        {
            DatabaseManager.Disconnect();
            DatabaseManager.DropDatabase();
        }
    }
}
