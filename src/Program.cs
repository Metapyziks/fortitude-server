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

using TestServer.Entities;

namespace TestServer
{
    using AccountPred = Expression<Func<Account, bool>>;

    public class Program
    {
        public static int LocalPort = 80;
        public static String ServerAddress = null;

        static bool stActive;

        static void Main(String[] args)
        {
            Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
            Console.Title = "Fortitude Server Prototype";
            stActive = true;

            String iniPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config.ini");

            String[] ownerList = null;

            if (!File.Exists(iniPath)) {
                Console.WriteLine("WARNING: config.ini not found, some features may not function");
            } else {
                IniDocument ini = new IniDocument(iniPath);
                IniSection general = ini.Sections["general"];
                ServerAddress = general.GetValue("address");
                int.TryParse(general.GetValue("localport"), out LocalPort);
                ownerList = general.GetValue("owners").Split(new char[] { ',' },
                    StringSplitOptions.RemoveEmptyEntries);

                EmailManager.CreateClient(ini.Sections["smtp"]);

                ContentManager.Initialize(ini.Sections["webserver"]);
            }

            Cache.MaxInteractionDistance = 250d;

            DatabaseManager.ConnectLocal();

            if (ownerList != null) {
                var owners = DatabaseManager.Select<Account>(ownerList.Select(
                    x => (AccountPred) (acc => acc.Email == x)).ToArray());

                foreach (var owner in owners.Where(x => x.Rank != Rank.Unverified && x.Rank != Rank.Owner)) {
                    owner.Rank = Rank.Owner;
                    DatabaseManager.Update(owner);
                }
            }

            Thread clientThread = new Thread(async () => {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://+:" + LocalPort + "/");
                listener.Start();

                Console.WriteLine("Http server started and ready for requests");

                while (stActive) {
                    Task<HttpListenerContext> ctxTask = listener.GetContextAsync();

                    while (!ctxTask.IsCompleted && stActive)
                        Thread.Sleep(10);

                    if (!stActive)
                        break;

                    ProcessRequest(await ctxTask);
                }

                listener.Stop();
            });

            clientThread.Start();

            while (stActive) {
#if DEBUG
                String[] line = Console.ReadLine().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (line.Length > 0)
                    ProcessCommand(line[0].ToLower(), line.Where((x, i) => i > 0).ToArray());
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

        static void ProcessCommand(String command, String[] args)
        {
            Responses.ErrorResponse error;
            switch (command) {
                case "stop":
                    stActive = false;
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
