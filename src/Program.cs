#if !LINUX
#define WINDOWS
#endif

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;

using Nini.Ini;

using FortitudeServer.Entities;

namespace FortitudeServer
{
    using AccountPred = Expression<Func<Account, bool>>;
    using System.Diagnostics;

    public class Program
    {
        public static int LocalPort = 80;
        public static String ServerAddress = null;

        private static bool _sActive;

        static void Main(String[] args)
        {
            Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
            Console.Title = "Fortitude Server Prototype";
            _sActive = true;

            Cache.MaxInteractionDistance = 30d;
            Cache.PlacementCost = 5;
            Cache.MinPlacementDistance = 300d;

            Player.PayoutInterval = 3.0 * 60.0 * 60.0;
            Player.UnitsPerCache = 1.0;

            String iniPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config.ini");
            String[] ownerEmails = null;

            if (!File.Exists(iniPath)) {
                Console.WriteLine("WARNING: config.ini not found, some features may not function");
            } else {
                IniDocument ini = new IniDocument(iniPath);
                IniSection general = ini.Sections["general"];

                ServerAddress = general.GetValue("address");
                int.TryParse(general.GetValue("localport"), out LocalPort);

                ownerEmails = general.GetValue("owners").Split(new char[] { ',' },
                    StringSplitOptions.RemoveEmptyEntries);

                DatabaseManager.FileName = general.GetValue("database") ?? DatabaseManager.FileName;
                EmailManager.CreateClient(ini.Sections["smtp"]);
                ContentManager.Initialize(ini.Sections["webserver"]);
            }

            DatabaseManager.ConnectLocal();

            if (ownerEmails != null) {
                Account.AddOwnerEmails(ownerEmails);
            }

            Thread clientThread = new Thread(async () => {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://+:" + LocalPort + "/");
                listener.Start();

                Console.WriteLine("Http server started and ready for requests");

                while (_sActive) {
                    Task<HttpListenerContext> ctxTask = listener.GetContextAsync();

                    while (!ctxTask.IsCompleted && _sActive) { 
                        Thread.Sleep(10);
                        if (Player.PayoutInterval > 1.0 && Player.PayoutDue) {
                            Player.Payout();
                        }
                    }

                    if (!_sActive)
                        break;

                    ProcessRequest(await ctxTask);
                }

                listener.Stop();
            });

            clientThread.Start();

            while (_sActive) {
#if DEBUG
                ProcessCommand(Console.ReadLine());
#else
                Thread.Sleep(10);
#endif
            }

            clientThread.Abort();

            while (clientThread.IsAlive)
                Thread.Sleep(10);

            DatabaseManager.Disconnect();
        }

        static void Error(String format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(format, args);
            Console.ResetColor();
        }

        static void Success(String format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(format, args);
            Console.ResetColor();
        }

        public static void Restart()
        {
            Console.Write("Attempting to restart... ");
            try {
                Process.Start("nohup", "../../update.sh >/dev/null 2>&1 &");
                Console.WriteLine("Goodbye!");
            } catch (Exception e) {
                Console.WriteLine("Failed!");
            }
        }

        public static void ProcessCommand(String command)
        {
            String[] line = command.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (line.Length > 0)
                ProcessCommand(line[0].ToLower(), line.Where((x, i) => i > 0).ToArray());
        }

        static void ProcessCommand(String command, String[] args)
        {
            Responses.ErrorResponse error;
            switch (command) {
                case "stop":
                    _sActive = false;
                    break;
                case "activate":
                    if (args.Length == 0) {
                        Error("Expected a user name");
                        break;
                    }
                    error = Account.AttemptActivate(args[0]);
                    if (error != null)
                        Error("Could not activate account: {0}", error.Message);
                    else
                        Success("Activated {0}'s account", args[0]);
                    break;
                case "promote":
                    if (args.Length == 0) {
                        Error("Expected a user name");
                        break;
                    }
                    error = Account.AttemptPromote(args[0]);
                    if (error != null)
                        Error("Could not promote account: {0}", error.Message);
                    else
                        Success("Promoted {0} to admin", args[0]);
                    break;
                case "delete":
                    if (args.Length == 0) {
                        Error("Expected an identifier");
                        break;
                    }
                    DatabaseManager.Delete<Cache>(x => x.CacheID == int.Parse(args[0]));
                    Success("Deleted cache with id {0}", args[0]);
                    break;
                case "reset":
                    if (args.Length == 0) {
                        Error("Expected a table name");
                        break;
                    }
                    args[0] = args[0].ToLower();
                    foreach(DatabaseTable table in DatabaseManager.GetTables()) {
                        if (table.Name.ToLower() == args[0]) {
                            table.Drop();
                            table.Create();
                            Success("Reset table with name {0}", args[0]);
                            return;
                        }
                    }
                    Error("Invalid table name");
                    break;
                case "findid":
                    if (args.Length < 1) {
                        Error("Expected a cache name");
                        break;
                    }
                    String cacheName = String.Join(" ", args);

                    var matches = DatabaseManager.Select<Cache>(x => x.Name == cacheName);

                    if (matches.Count == 0) {
                        Console.WriteLine("No caches found with name \"{0}\"", cacheName);
                        break;
                    }

                    Console.WriteLine("Found {0} caches with name \"{1}\"", matches.Count, cacheName);

                    foreach (var ca in matches) {
                        Console.WriteLine("#{0}: ({1},{2})", ca.CacheID, ca.Longitude, ca.Latitude);
                    }
                    break;
                case "placenpc":
                    if (args.Length < 2) {
                        Error("Expected a position");
                        break;
                    }

                    double lat, lon;
                    if (!double.TryParse(args[0], out lat)) {
                        Error("Invalid lattitude");
                        break;
                    }
                    if (!double.TryParse(args[1], out lon)) {
                        Error("Invalid longitude");
                        break;
                    }

                    NonPlayerCache cache = new NonPlayerCache {
                        Latitude = lat,
                        Longitude = lon,
                        Name = CacheNamer.GenerateRandomName(),
                        Balance = 20,
                        GrowthStyle = GrowthStyle.Linear | GrowthStyle.Slow
                    };

                    DatabaseManager.Insert(cache);
                    Success("Placed non-player cache at ({0}, {1})", args[0], args[1]);
                    break;
                case "addspecial":
                    if (args.Length < 4) {
                        Error("Expected name, address, starting balance and reward");
                        break;
                    }
                    String name = args[0];
                    String address = args[1];
                    if (!SpecialEvent.IsAddressValid(address)) {
                        Error("Invalid address");
                        break;
                    }
                    int balance, reward;
                    if (!int.TryParse(args[2], out balance)) {
                        Error("Invalid balance");
                        break;
                    }
                    if (!int.TryParse(args[3], out reward)) {
                        Error("Invalid reward");
                        break;
                    }
                    var special = new SpecialEvent {
                        Name = name,
                        MACAddress = address,
                        Balance = balance,
                        Reward = reward,
                        Expires = DateTime.Now.AddMonths(1)
                    };
                    DatabaseManager.Insert(special);
                    Success("Added special cache named {0} with address {1}, a reward of {2} points and {3} claims", args[0], args[1], args[3], args[2]);
                    break;
                case "placetest":
                    if (args.Length < 1) {
                        Error("Expected a username");
                    }

                    double latitude = 54.778324, longitude = -1.569824;

                    Account acc = DatabaseManager.SelectFirst<Account>(x => x.Username == args[0]);

                    if (acc != null) {
                        var caches = Cache.FindNearby(latitude, longitude, Cache.MinPlacementDistance);
                        DatabaseManager.Delete(caches);

                        int bal = 20;
                        if (args.Length > 1) {
                            if (!int.TryParse(args[1], out bal)) {
                                Error("Invalid balance");
                                break;
                            }
                        }

                        DatabaseManager.Insert(new Cache {
                            AccountID = acc.AccountID,
                            Balance = bal,
                            Latitude = latitude,
                            Longitude = longitude,
                            Name = CacheNamer.GenerateRandomName()
                        });

                        Success("Placed cache!");
                    } else {
                        Error("Account not found");
                    }
                    break;
                case "message":
                    if (args.Length < 3) {
                        Error("Expected a receiver, subject, and content");
                        break;
                    }
                    String receiverName = args[0];
                    String subject = args[1];
                    String content = args[2];

                    var receiver = DatabaseManager.SelectFirst<Account>(x => x.Username == receiverName);

                    if (receiver == null) {
                        Error("Invalid user name");
                        break;
                    }

                    DatabaseManager.Insert(new Message(1, receiver.AccountID, subject, content));
                    break;
                case "gibemone":
                    if (args.Length < 2) {
                        Error("Expected a username and amount");
                        break;
                    }

                    String username = args[0];
                    int amount;
                    if (!int.TryParse(args[1], out amount)) {
                        Error("Invalid amount");
                        break;
                    }

                    var a = DatabaseManager.SelectFirst<Account>(x => x.Username == username);

                    if (a == null) {
                        Error("Invalid username");
                        break;
                    }

                    var ply = Player.GetPlayer(a);
                    ply.Balance += amount;
                    DatabaseManager.Update(ply);

                    Success("Gave {0} to {1}", username, amount);
                    break;
                default:
                    Error("Unkown command \"{0}\"", command);
                    break;
            }
        }

        static void ProcessRequest(HttpListenerContext context)
        {
#if DEBUG
            Console.WriteLine("Request from " + context.Request.RemoteEndPoint.ToString() + " : " + context.Request.RawUrl);
#else
            try
            {
#endif
            if (context.Request.HttpMethod == "GET" || context.Request.HttpMethod == "POST") {
                if (context.Request.RawUrl.StartsWith("/api/"))
                    APIManager.ServeRequest(context);
                else
                    ContentManager.ServeRequest(context);
            }
#if !DEBUG
            }
            catch ( Exception e )
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine( e.GetType().Name + " thrown: " + e.Message );
                Console.ResetColor();
            }
            finally
            {
#endif
            context.Response.OutputStream.Close();
#if !DEBUG
            }
#endif
        }
    }
}
