using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace BackupManager
{
    public class EmailHelper
    {
        private readonly IConfiguration _configuration;
        private readonly Logger _logger;

        public EmailHelper(IConfiguration configuration, Logger logger)
        {
            this._configuration = configuration;
            this._logger = logger;
        }

        public void SendEmailNotification()
        {
            var SMTPServer = _configuration["SMTPServer"];
            var SMTPUsername = _configuration["SMTPUsername"];
            var SMTPPassword = _configuration["SMTPPassword"];
            var SMTPSsl = !string.IsNullOrWhiteSpace(_configuration["SMTPSsl"]) ? Convert.ToBoolean(_configuration["SMTPSsl"]) : false;
            var SMTPPort = !string.IsNullOrWhiteSpace(_configuration["SMTPPort"]) ? Convert.ToInt32(_configuration["SMTPPort"]) : 25;
            var SMTPFromEmail = _configuration["SMTPFromEmail"];
            var SMTPReportTo = _configuration["SMTPReportTo"];

            SmtpClient client = new SmtpClient(SMTPServer, SMTPPort);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(SMTPUsername, SMTPPassword);
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = SMTPSsl;
            client.Timeout = 5000;

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(SMTPFromEmail);
            mailMessage.To.Add(SMTPReportTo);
            mailMessage.IsBodyHtml = true;

            mailMessage.Body = BuildBody();
            mailMessage.Subject = "Backup Report - " + DateTime.Now.ToString("dd/MM/yy hh:mm:ss tt");


            client.Send(mailMessage);


        }

        private string BuildBody()
        {
            var sb = new StringBuilder();
            sb.Append("<style type='text/css'>");
            sb.Append("@media screen {");
            sb.Append("@font-face{");
            sb.Append("font-family:'Open Sans';");
            sb.Append("font-style:normal;");
            sb.Append("font-weight:400;");
            sb.Append("src:local('Open Sans'), local('OpenSans'), url('http://fonts.gstatic.com/s/opensans/v10/cJZKeOuBrn4kERxqtaUH3bO3LdcAZYWl9Si6vvxL-qU.woff') format('woff');");
            sb.Append("}");
            sb.Append("}");
            sb.Append("</style>");
            sb.Append("<h3>Backup Report</h3>");
            sb.Append("<ul>");
            foreach (var item in _logger.LogMessages)
            {
                sb.Append($"<li>{item}</li>");
            }

            sb.Append("</ul>");
            return sb.ToString();
        }
    }
}
