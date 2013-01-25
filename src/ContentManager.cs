﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
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
        private class BinaryFile
        {
            public String MimeType { get; private set; }
            public DateTime LastUpdate { get; private set; }
            public byte[] Data { get; private set; }
            public byte[] Deflated { get; private set; }

            public BinaryFile(String path)
            {
                String ext = Path.GetExtension(path);
                switch(ext) {
                    case ".jpg":
                    case ".jpeg":
                        MimeType = "image/jpeg"; break;
                    case ".png":
                        MimeType = "image/png"; break;
                    case ".gif":
                        MimeType = "image/gif"; break;
                    case ".ico":
                        MimeType = "image/icon"; break;
                }
            }

            public void SetData(byte[] data)
            {
                Data = data;

                /*var stream = new MemoryStream();
                var compress = new DeflateStream(stream, CompressionMode.Compress, true);
                for (int i = 0; i < data.Length; i += 2048) {
                    compress.Write(data, i, Math.Min(2048, data.Length - i));
                }
                compress.Flush();

                stream.Position = 0;
                Deflated = new byte[stream.Length];
                stream.Read(Deflated, 0, (int) stream.Length);

                compress.Close();
                stream.Close();

                if (Deflated.Length == 0)*/
                    Deflated = null;
            }
        }

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
                try {
                    if (File.Exists(Path))
                        Update(File.ReadAllText(Path));
                } catch { }
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
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.IO;
using System.Reflection;

using TestServer;
using TestServer.Entities;
using TestServer.Requests;
using TestServer.Responses;

public static class {0}
{
    public static void ServeRequest(HttpListenerContext context, String body,
        NameValueCollection post, AuthSession session, StreamWriter writer,
        Dictionary<String, MethodInfo> __includes__)
    {
        var request = context.Request;
        var response = context.Response;
        var get = request.QueryString;

        Action<String> Include = path => {
            if (!path.StartsWith(""/"")) path = ""/"" + path;
            try {
                __includes__[path].Invoke(null, new object[] { context, body, post, session, writer, __includes__ } );
            } catch (Exception e) {
                writer.Write(""Failed to include "" + path + ""!<br />"");
                writer.Write(e.ToString());
            }
        };

        writer.Write(""{1}"");
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
                if (type != null) {
                    _serveMethod = type.GetMethod("ServeRequest", BindingFlags.Static | BindingFlags.Public);
                    if (_serveMethod != null) {
                        var path = FormatPath(Path);
                        if (!_sIncludes.ContainsKey(path))
                            _sIncludes.Add(path, _serveMethod);
                        else
                            _sIncludes[path] = _serveMethod;
                    }
                }
            }

            public override void ServeRequest(HttpListenerContext context)
            {
                if (_serveMethod != null) {
                    var request = context.Request;
                    var response = context.Response;
                    var get = request.QueryString;

                    var body = new StreamReader( request.InputStream ).ReadToEnd();
                    var post = HttpUtility.ParseQueryString( body );

                    AuthSession session = null;
                    if (request.Cookies["auth-uid"] != null && request.Cookies["auth-session"] != null) {
                        var uid = Int32.Parse(request.Cookies["auth-uid"].Value);
                        var sid = request.Cookies["auth-session"].Value;
                        session = AuthSession.Get(uid);
                        if (session != null && !sid.EqualsCharArray(session.SessionCode)) session = null;
                    }

                    var writer = new StreamWriter( response.OutputStream );
#if !DEBUG
                    try {
#endif
                        _serveMethod.Invoke(null, new object[] { context, body, post, session, writer, _sIncludes });
#if !DEBUG
                    } catch {
                        writer.WriteLine("Internal server error :(");
                    }
#endif
                    writer.Flush();
                }
            }

            protected override void WriteContent(StreamWriter writer)
            {
                throw new NotImplementedException();
            }
        }

        private static String _sContentDirectory;
        private static List<String> _sAllowedExtensions;

        public static String ContentDir { get { return _sContentDirectory; } }

        private static FileSystemWatcher _watcher;

        private static Dictionary<String, Page> _sPages;
        private static Dictionary<String, BinaryFile> _sContent;
        private static Dictionary<String, MethodInfo> _sIncludes;

        public static void Initialize(IniSection ini)
        {
            _sPages = new Dictionary<string, Page>();
            _sContent = new Dictionary<string, BinaryFile>();
            _sIncludes = new Dictionary<string, MethodInfo>();

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
            if (ext == ".html" || ext == ".js" || ext == ".css")
                UpdatePage(path, depth);
            else if (_sAllowedExtensions.Contains(ext))
                UpdateContent(path, depth);
        }

        private static void UpdatePage(String path, int depth = 0)
        {
            String formatted = FormatPath(path);

            if (!_sPages.ContainsKey(formatted)) {
                if (path.EndsWith(".html")) {
                    _sPages.Add(formatted, new ScriptedPage(path));
                } else {
                    _sPages.Add(formatted, new StaticPage(path));
                }

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
                var file = new BinaryFile(path);
                file.SetData(File.ReadAllBytes(path));
                _sContent.Add(formatted, file);
                Console.Write("+ ".PadLeft(2 + depth * 2));
            } else {
                if (File.Exists(path)) {
                    Console.Write("= ".PadLeft(2 + depth * 2));
                    DateTime start = DateTime.Now;
                    while ((DateTime.Now - start).TotalSeconds < 5.0) {
                        try {
                            _sContent[formatted].SetData(File.ReadAllBytes(path));
                            break;
                        } catch (IOException) {
                            Thread.Sleep(100);
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
                path = "/index";
            
            if (!path.Contains('.'))
                path += ".html";

            if (path.EndsWith(".html") || path.EndsWith(".js") || path.EndsWith(".css")) {
                if (!path.EndsWith(".html")) {
                    context.Response.AddHeader("Cache-Control", "max-age=290304000, public");
                }
                if (_sPages.ContainsKey(path)) {
                    _sPages[path].ServeRequest(context);
                    return;
                }
            } else if (_sContent.ContainsKey(path)) {
                context.Response.AddHeader("Content-Type", _sContent[path].MimeType);
                context.Response.AddHeader("Cache-Control", "max-age=290304000, public");
                if (_sContent[path].Deflated != null && context.Request.Headers["Accept-Encoding"].Contains("deflate")) {
                    context.Response.AddHeader("Content-Encoding", "deflate");
                    byte[] content = _sContent[path].Deflated;
                    context.Response.OutputStream.Write(content, 0, content.Length);
                } else {
                    byte[] content = _sContent[path].Data;
                    context.Response.OutputStream.Write(content, 0, content.Length);
                }
                return;
            }

            if (_sPages.ContainsKey("/404.html"))
                context.Response.Redirect("/404");
        }
    }
}
