using HelpDesk.UI.Infrastructure.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Xml.Linq;

namespace HelpDesk.UI.Infrastructure.Concrete
{
    public class EmailSender : IEmailSender
    {
        private readonly XElement name;
        private readonly XElement address;
        private readonly XElement host;
        private readonly XElement port;
        private readonly XElement ssl;
        private readonly XElement login;
        private readonly XElement password;

        public EmailSender()
        {
            XDocument xml;
            try
            {
                xml = XDocument.Load(HttpContext.Current.Server.MapPath("~/App_Data/Settings.xml"));
            }
            catch
            {
                xml = null;
            }

            XElement settings = xml != null ? xml.Element("Settings") : null;
            XElement emailSettings = settings != null ? settings.Element("EmailSettings") : null;
            name = emailSettings != null ? emailSettings.Element("Name") : null;
            address = emailSettings != null ? emailSettings.Element("Address") : null;
            host = emailSettings != null ? emailSettings.Element("Host") : null;
            port = emailSettings != null ? emailSettings.Element("Port") : null;
            ssl = emailSettings != null ? emailSettings.Element("Ssl") : null;
            login = emailSettings != null ? emailSettings.Element("Login") : null;
            password = emailSettings != null ? emailSettings.Element("Password") : null;
        }

        public void SendEmail(string receiverEmail, string subject, string content)
        {
            using (MailMessage msg = new MailMessage())
            {
                msg.From = new MailAddress($"{name.Value} <{address.Value}>");
                msg.To.Add(receiverEmail);
                msg.Subject = subject;
                msg.Body = content;
                msg.IsBodyHtml = true;

                SmtpClient client = new SmtpClient();
                client.UseDefaultCredentials = true;
                client.Host = host.Value;
                client.Port = int.Parse(port.Value);
                client.EnableSsl = bool.Parse(ssl.Value);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = new NetworkCredential(login.Value, password.Value);
                client.Timeout = 20000;

                client.Send(msg);
            }
        }
    }
}