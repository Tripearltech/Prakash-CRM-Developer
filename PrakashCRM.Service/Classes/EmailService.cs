using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using Xipton.Razor.Extension;

namespace PrakashCRM.Service.Classes
{
    public class EmailService
    {
        // All this has to come from Config
        public static string ToEmail = "";
        public static string FromEmail = System.Configuration.ConfigurationManager.AppSettings["FromEmail"]; //"it@prakashchemicals.com"; 
        public static string Subject = System.Configuration.ConfigurationManager.AppSettings["Subject"]; //"Mail From PCRM";
        public static string Host = System.Configuration.ConfigurationManager.AppSettings["Host"]; //"smtp.office365.com";
        public static int Port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Port"]); //587;
        public static bool EnableSsl = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["EnableSsl"]); //true;
        public static string UserName = System.Configuration.ConfigurationManager.AppSettings["UserName"]; //"it@prakashchemicals.com";
        public static string Password = System.Configuration.ConfigurationManager.AppSettings["Password"]; //"pjymzwxmvmhldvbn";
        //private readonly C_SiteConfigRepository _SiteConfigService;
        //private readonly PCRM_Context _context;
        SmtpClient _smtpClient;
        //move this to EmailSettings.cs
        //readonly SmtpClient _smtpClient = new SmtpClient
        //{
        //    Host = Host,
        //    EnableSsl = EnableSsl,
        //    DeliveryMethod = SmtpDeliveryMethod.Network,
        //    Port = Port,
        //    UseDefaultCredentials = true,
        //    Credentials = new NetworkCredential(UserName, Password)
        //};

        public EmailService()
        {
            _smtpClient = new SmtpClient
            {
                Host = Host,
                EnableSsl = EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Port = Port,
                UseDefaultCredentials = true,
                Credentials = new NetworkCredential(UserName, Password)
            };
        }

        public void SendEmailTo(string toEmail, string emailBody, string subject)
        {
            var email = Email
                .From(FromEmail)
                .To(toEmail)
                .Subject(subject)
                .Body(emailBody)
                .UsingClient(_smtpClient)
                .Send();

            email.Dispose();
        }

        public void SendEmail(string toEmail, string ccEmail, string subject, string emailBody)
        {
            var email = Email
                .From(FromEmail)
                .To(toEmail)
                .CC(ccEmail)
                .Subject(subject)
                .Body(emailBody)
                .UsingClient(_smtpClient)
                .Send();

            email.Dispose();
        }
        public void SendEmail1(string femail, string toEmail, string ccEmail, string subject, string emailBody)
        {
            var email = Email
                .From(femail)
                .To(toEmail)
                .CC(ccEmail)
                .Subject(subject)
                .Body(emailBody)
                .UsingClient(_smtpClient)
                .Send();

            email.Dispose();
        }

        public void SendAttachmentEmailTo(string toEmail, string subject, Attachment attachment, string emailBody)
        {
            var email = Email
                .From(FromEmail)
                .To(toEmail)
                .Subject(subject)
                .Body(emailBody)
                .Attach(attachment)
                .UsingClient(_smtpClient)
                .Send();

            email.Dispose();
        }
        public void SendAttachmentEmail(string toEmail, string ccEmail, string bccEmail, string subject, Attachment attachment, string emailBody)
        {
            var email = Email
                .From(FromEmail)
                .To(toEmail)
                .CC(ccEmail)
                .BCC(bccEmail)
                .Subject(subject)
                .Body(emailBody)
                .Attach(attachment)
                .UsingClient(_smtpClient)
                .Send();

            email.Dispose();
        }
        public void SendEmailWithHTMLBody(string toEmail, string ccEmail, string bccEmail, string subject, string emailBody)
        {
            //var email = Email
            //    .From(FromEmail)
            //    .To(toEmail)
            //    .CC(ccEmail)
            //    .BCC(bccEmail)
            //    .Subject(subject)
            //    .Body(emailBody, true)
            //    .UsingClient(_smtpClient)
            //    .Send();

            //email.Dispose();

            MailMessage message = new MailMessage();
            SmtpClient cmt = new SmtpClient(Host, Port);
            cmt.Credentials = new NetworkCredential(UserName, Password);
            MailAddress from = new MailAddress(FromEmail, "");

            message.Subject = subject;
            message.From = from;
            message.To.Add(toEmail.Trim().TrimEnd(','));
            message.CC.Add(ccEmail.Trim());
            message.IsBodyHtml = true;
            message.Body = emailBody;
            cmt.EnableSsl = true;
            cmt.DeliveryMethod = SmtpDeliveryMethod.Network;
            cmt.Send(message);

        }
    }
}