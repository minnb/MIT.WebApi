using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace VCM.Common.Helpers
{
    public class EmailHelper
    {
        private readonly string _Host;
        private readonly int _Port;
        private readonly string _UserName;
        private readonly string _Password;
        public EmailHelper(
            string host,
            int port,
            string userName,
            string password
            )
        {
            _Host = host;
            _Port = port;
            _UserName = userName;
            _Password = password;
        }
        public bool SendingEmail(string subject, string fromEmail, string toEmail, string bodyHtml)
        {
            try
            {
                var smtp = new SmtpClient
                {
                    Host = _Host,
                    Port = _Port,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = true,
                    Credentials = new NetworkCredential(_UserName, _Password)
                };

                string[] toEmailArr = toEmail.Split(';');

                MailAddressCollection TO_addressList = new MailAddressCollection();

                foreach (var e in toEmailArr)
                {
                    MailAddress mytoAddress = new MailAddress(e);
                    TO_addressList.Add(mytoAddress);
                }

                using (var message = new MailMessage()
                {
                    From = new MailAddress(_UserName, "WINMART"),
                    Subject = subject,
                    Body = bodyHtml,
                    IsBodyHtml = true
                })
                {
                    message.To.Add(TO_addressList.ToString());
                    ServicePointManager.ServerCertificateValidationCallback = delegate
                    (object s, X509Certificate certificate, X509Chain chain,
                    SslPolicyErrors sslPolicyErrors)
                    { return true; };
                    //smtp.EnableSsl = true;
                    smtp.Send(message);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
