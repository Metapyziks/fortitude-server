using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

using Nini.Ini;

namespace FortitudeServer
{
    public static class EmailManager
    {
        private static SmtpClient _sClient;
        private static String _sAdminEmail;

        public static bool IsAvaliable
        {
            get { return _sClient != null; }
        }

        public static String AdminEmail
        {
            get { return _sAdminEmail; }
        }
        
        public static void CreateClient(IniSection settings)
        {
            CreateClient( settings.GetValue( "server" ),
                Int32.Parse( settings.GetValue( "port" ) ),
                settings.GetValue( "address" ),
                settings.GetValue( "username" ),
                settings.GetValue( "password" ),
                Boolean.Parse( settings.GetValue("ssl")));
        }

        public static void CreateClient(String smtpAddress, int port, String email, String username, String password, bool ssl)
        {
            _sAdminEmail = email;

            _sClient = new SmtpClient(smtpAddress, port) {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = ssl
            };
            ServicePointManager.ServerCertificateValidationCallback =
                (s, certificate, chain, sslPolicyErrors) => true;
        }

        public static void Send(String to, String subject, String message)
        {
            try {
                _sClient.SendAsync(_sAdminEmail, to, subject, message, null);
            } catch {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Email not sent because of reasons");
                Console.ResetColor();
            }
        }
    }
}
