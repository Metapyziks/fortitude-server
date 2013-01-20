using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;

using Microsoft.CSharp;

using Nini.Ini;

using TestServer.Entities;

namespace TestServer
{
    static class ContentManager
    {
        private abstract class Resource
        {
            public readonly String Path;

            protected Resource(String path)
            {
                Path = path;
                Update();
            }

            public virtual void ServeRequest(HttpListenerContext context)
            {
                WriteContent(context.Response.OutputStream);
            }

            public abstract void Update();
            public abstract void WriteContent(Stream stream);
        }

        private abstract class Page : Resource
        {
            protected Page(String path)
                : base(path) { }

            public override void WriteContent(Stream stream)
            {
                StreamWriter writer = new StreamWriter(stream);
                WriteContent(writer);
                writer.Flush();
            }

            public override void Update()
            {
                Update(File.ReadAllText(Path));
            }

            protected abstract void Update(String content);
            protected abstract void WriteContent(StreamWriter writer);
        }

        private class StaticPage : Page
        {
            public String Content { get; private set; }

            public StaticPage(String path)
                : base(path) { }

            protected override void Update(String content)
            {
                Content = content;
            }

            protected override void WriteContent(StreamWriter writer)
            {
                writer.Write(Content);
            }
        }

        private class ScriptedPage : Page
        {
#region Template
            private static readonly String _sTemplate = @"
using System;
using System.Net;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.IO;

using TestServer;
using TestServer.Entities;
using TestServer.Requests;
using TestServer.Responses;

public static class {0}
{
    public static void ServeRequest( HttpListenerContext context )
    {
        var request = context.Request;
        var response = context.Response;
        var get = request.QueryString;

        var body = new StreamReader( request.InputStream ).ReadToEnd();
        var post = HttpUtility.ParseQueryString( body );

        AuthSession session = null;
        if (request.Cookies[""auth-uid""] != null && request.Cookies[""auth-sid""] != null) {
            var uid = Int32.Parse(request.Cookies[""auth-uid""].Value);
            var sid = request.Cookies[""auth-session""].Value;
            var account = DatabaseManager.SelectFirst<Account>(x => x.AccountID == uid);
            if (account == null) return null;
            session = AuthSession.Get(account);
            if (!sid.EqualsCharArray(session.SessionCode)) session = null;
        }

        var writer = new StreamWriter( response.OutputStream );

        writer.Write( ""{1}"" );
        writer.Flush();
    }
}
";
#endregion

            private static CodeDomProvider _compiler;

            private static CompilerParameters CreateCompilerPatameters()
            {
                CompilerParameters compParams = new CompilerParameters();

                String[] allowedAssemblies = new String[]
                {
                    Assembly.GetAssembly( typeof( Math ) ).Location,
                    Assembly.GetAssembly( typeof( HttpListener ) ).Location,
                    Assembly.GetAssembly( typeof( HttpUtility ) ).Location,
                    Assembly.GetAssembly( typeof( System.Linq.Expressions.Expression ) ).Location,
                    Assembly.GetAssembly( typeof( ScriptedPage ) ).Location
                };

                compParams.ReferencedAssemblies.AddRange(allowedAssemblies);

                compParams.GenerateExecutable = false;
                compParams.GenerateInMemory = true;

                return compParams;
            }

            static ScriptedPage()
            {
                Dictionary<String, String> providerOptions = new Dictionary<string, string>()
                {
                    { "CompilerVersion", "v4.0" }
                };

                _compiler = new CSharpCodeProvider(providerOptions);
            }

            private String _className;
            private String _generatedCode;
            private Assembly _assembly;
            private MethodInfo _serveMethod;

            public ScriptedPage(String path)
                : base(path) { }

            private String FormatHTMLBlock(String block)
            {
                return block.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n")
                    .Replace("\t", "\\t").Replace("\"", "\\\"");
            }

            // TODO: Make this at least slightly acceptable
            private String GenerateCode(String content)
            {
                StringBuilder builder = new StringBuilder();
                int j = 0;
                int i = 0;
                bool script = false;
                while (i < content.Length) {
                    if (!script) {
                        if (content[i] == '<' && i < content.Length - 1 && content[i + 1] == '?') {
                            builder.Append(FormatHTMLBlock(content.Substring(j, i - j)));
                            builder.Append("\");");
                            i += 2;
                            j = i;
                            script = true;
                            continue;
                        }

                        if (content[i] == '{' && i < content.Length && content[i + 1] == '$') {
                            builder.Append(FormatHTMLBlock(content.Substring(j, i - j)));
                            j = i + 2;
                            builder.Append("\" + ");
                            while (++i < content.Length && content[i] != '}') ;
                            builder.AppendFormat("{0}.ToString() + \"", content.Substring(j, i - j));
                            j = ++i;
                        }
                    } else {
                        if (content[i] == '?' && i < content.Length - 1 && content[i + 1] == '>') {
                            builder.Append(content.Substring(j, i - j));
                            builder.Append("writer.Write(\"");
                            i += 2;
                            j = i;
                            script = false;
                            continue;
                        }

                        if (content[i] == '"')
                            while (++i < content.Length && content[i] != '"') ;
                        else if (content[i] == '\'')
                            while (++i < content.Length && content[i] != '\'') ;
                    }


                    ++i;
                }
                builder.Append(FormatHTMLBlock(content.Substring(j, i - j)));

                String formatted = FormatPath(Path);
                _className = "PageScript" + formatted.Replace('/', '_').Substring(0, formatted.IndexOf('.'));

                return _sTemplate.Replace("{0}", _className).Replace("{1}", builder.ToString());
            }

            protected override void Update(String content)
            {
                _generatedCode = GenerateCode(content);

                CompilerResults results = _compiler.CompileAssemblyFromSource(
                    CreateCompilerPatameters(), _generatedCode);

                bool borked = false;
                if (results.Errors.Count > 0) {
                    Console.WriteLine("Encountered {0} error{1} or warning{1} while compiling {2}:",
                        results.Errors.Count, results.Errors.Count != 1 ? "s" : "", Path);

                    foreach (CompilerError error in results.Errors) {
                        if (error.IsWarning)
                            Console.ForegroundColor = ConsoleColor.Yellow;
                        else {
                            Console.ForegroundColor = ConsoleColor.Red;
                            borked = true;
                        }

                        Console.WriteLine(error.ErrorText);
                    }

                    Console.ResetColor();
                }

                if (borked)
                    return;

                _assembly = results.CompiledAssembly;
                Type type = _assembly.GetType(_className);
                if (type != null)
                    _serveMethod = type.GetMethod("ServeRequest", BindingFlags.Static | BindingFlags.Public);
            }

            public override void ServeRequest(HttpListenerContext context)
            {
                if (_serveMethod != null) {
                    try {
                        _serveMethod.Invoke(null, new object[] { context });
                        return;
                    } catch {
                    }
                }
                StreamWriter writer = new StreamWriter(context.Response.OutputStream);
                writer.WriteLine("Internal server error :(");
                writer.Flush();
            }

            protected override void WriteContent(StreamWriter writer)
            {
                throw new NotImplementedException();
            }
        }

        private static String _sContentDirectory;
        private static List<String> _sAllowedExtensions;

        private static FileSystemWatcher _watcher;

        private static Dictionary<String, Page> _sPages;
        private static Dictionary<String, byte[]> _sContent;

        public static void Initialize(IniSection ini)
        {
            _sPages = new Dictionary<string, Page>();
            _sContent = new Dictionary<string, byte[]>();

            _sContentDirectory = ini.GetValue("pagesdir") ?? "res";
            _sAllowedExtensions = (ini.GetValue("allowedext") ?? "").Split(',').ToList();

            for (int i = 0; i < _sAllowedExtensions.Count; ++i)
                _sAllowedExtensions[i] = _sAllowedExtensions[i].Trim().ToLower();

            if (!Path.IsPathRooted(_sContentDirectory))
                _sContentDirectory = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, _sContentDirectory);

            Console.WriteLine("Initializing content...");

            InitializeDirectory(_sContentDirectory);

            _watcher = new FileSystemWatcher(_sContentDirectory);
            _watcher.Created += (sender, e) => {
                Console.WriteLine("Content created");
                UpdateFile(e.FullPath);
            };
            _watcher.Changed += (sender, e) => {
                Console.WriteLine("Content updated");
                UpdateFile(e.FullPath);
            };
            _watcher.Renamed += (sender, e) => {
                Console.WriteLine("Content renamed");
                UpdateFile(e.OldFullPath);
                UpdateFile(e.FullPath);
            };
            _watcher.Deleted += (sender, e) => {
                Console.WriteLine("Content removed");
                UpdateFile(e.FullPath);
            };
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            _watcher.IncludeSubdirectories = true;
            _watcher.EnableRaisingEvents = true;
        }

        private static void InitializeDirectory(string dir, int depth = 0)
        {
            String dirName = dir;
            Console.WriteLine(dirName);

            foreach (String file in Directory.GetFiles(dir))
                UpdateFile(file, depth + 1);

            foreach (String subDir in Directory.GetDirectories(dir))
                InitializeDirectory(subDir, depth + 1);
        }

        private static String FormatPath(String path)
        {
            return path.Substring(_sContentDirectory.Length).Replace('\\', '/');
        }

        private static void UpdateFile(String path, int depth = 0)
        {
            String ext = Path.GetExtension(path).ToLower();
            if (ext == ".html")
                UpdatePage(path, depth);
            else if (_sAllowedExtensions.Contains(ext))
                UpdateContent(path, depth);
        }

        private static void UpdatePage(String path, int depth = 0)
        {
            String formatted = FormatPath(path);

            if (!_sPages.ContainsKey(formatted)) {
                _sPages.Add(formatted, new ScriptedPage(path));
                Console.Write("+ ".PadLeft(2 + depth * 2));
            } else {
                if (File.Exists(path)) {
                    Console.Write("= ".PadLeft(2 + depth * 2));
                    DateTime start = DateTime.Now;
                    while ((DateTime.Now - start).TotalSeconds < 1.0) {
                        try {
                            _sPages[formatted].Update();
                            break;
                        } catch (IOException) {
                            Thread.Sleep(10);
                        }
                    }
                } else {
                    Console.Write("- ".PadLeft(2 + depth * 2));
                    _sPages.Remove(formatted);
                }
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(formatted);
            Console.ResetColor();
        }

        private static void UpdateContent(String path, int depth = 0)
        {
            String formatted = FormatPath(path);

            if (!_sContent.ContainsKey(formatted)) {
                _sContent.Add(formatted, File.ReadAllBytes(path));
                Console.Write("+ ".PadLeft(2 + depth * 2));
            } else {
                if (File.Exists(path)) {
                    Console.Write("= ".PadLeft(2 + depth * 2));
                    DateTime start = DateTime.Now;
                    while ((DateTime.Now - start).TotalSeconds < 1.0) {
                        try {
                            _sContent[formatted] = File.ReadAllBytes(path);
                            break;
                        } catch (IOException) {
                            Thread.Sleep(10);
                        }
                    }
                } else {
                    Console.Write("- ".PadLeft(2 + depth * 2));
                    _sContent.Remove(formatted);
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(formatted);
            Console.ResetColor();
        }

        public static void ServeRequest(HttpListenerContext context)
        {
            String path = context.Request.RawUrl;
            int pathEnd = path.IndexOf('?');
            if (pathEnd == -1)
                pathEnd = path.Length;
            path = path.Substring(0, pathEnd);

            if (path.Length == 1)
                path = "/index.html";

            if (!path.Contains('.'))
                path += ".html";

            if (path.EndsWith(".html")) {
                if (_sPages.ContainsKey(path)) {
                    _sPages[path].ServeRequest(context);
                    return;
                }
            } else if (_sContent.ContainsKey(path)) {
                byte[] content = _sContent[path];
                context.Response.OutputStream.Write(content, 0, content.Length);
                return;
            }

            if (_sPages.ContainsKey("/404.html"))
                context.Response.Redirect("/404.html");
        }
    }
}
