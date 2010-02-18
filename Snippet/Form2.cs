using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Web.Mail;
using Plexus.Utils;

namespace Snippet
{
    /// <summary>
    /// Route email.
    /// </summary>
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            //string content = "We have find out alot of email been sent out were from by your location,";
            //content += " please provide a good reason to support why you have to do so if not";
            //content += " strictly action will be taken on you.";
            //content += "<p/>";
            //content += "--Plexus IT Coorperate";

            Random rand = new Random();
            string value = String.Format("{0:X}", rand.Next());
            if (value.Length > 4)
                value = value.Substring(0, 4);
            else if (value.Length == 3)
                value = "0" + value;
            else if (value.Length == 2)
                value = "00" + value;
            else if (value.Length == 1)
                value = "000" + value;
            //string content = "\u"+value;


            //refer to http://msdn.microsoft.com/en-us/library/bb311038.aspx
            string content = string.Empty;
            for (int i = 0; i < 2000; i++)
                content += Char.ConvertFromUtf32(RandomInt(ref rand));
            System.Diagnostics.Debug.WriteLine(content);

            for (int i = 0; i < 20; i++)
                SendMail("pena-mail-501.ap.plexus.com", "nobody@plexus.com", "hui-ling.tan@plexus.com", "winston.lim@plexus.com", "WAR BEGAN NOW", content, null);
        }
        public Int32 RandomInt(ref Random rand)
        {
            decimal remainder = 0m;

            try
            {
                int output = 0;
                //string s = Char.ConvertFromUtf32(output);
                //char c = '\u069B';
                remainder = rand.Next() % 32768;// Int32.MaxValue)
                return output = Convert.ToInt32(remainder - 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(remainder);
                return 32111;
            }
        }
        public bool SendMail(string mailServer, string sender, string receipient, string cc, string subject, string content, object attachments)
        {
            if (sender == null || sender == "") return false;
            if (receipient == null || receipient == "") return false;

            try
            {
                MailMessage message1 = new MailMessage();
                message1.From = sender;
                message1.Cc = cc;
                message1.To = receipient;
                message1.Subject = subject;
                message1.BodyFormat = System.Web.Mail.MailFormat.Html;
                message1.Body = content;
                if (attachments != null)
                    message1.Attachments.Add(attachments);//081128tys
                //if (!Validator.IsASCII(message1.Body))
                    message1.BodyEncoding = Encoding.UTF8;

                IPHostEntry entry1 = Dns.Resolve(mailServer);
                SmtpMail.SmtpServer = entry1.HostName;
                SmtpMail.Send(message1);

                Logger.Info(typeof(Form2), "Email from " + sender + " to: " + receipient + " cc: " + cc + " has been sent successfully.");
                return true;
            }
            catch (System.Exception ex)
            {
                Logger.Error(typeof(Form2), ex);
                return false;
            }
        }
    }
}
