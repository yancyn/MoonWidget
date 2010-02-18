using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Web.Mail;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.Common;
using Plexus.Utils;

namespace Snippet
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// <b>Changes</b>
    /// 2009-01-14: Add read receipient from file[yeang-shing.then].
    /// <example>
    /// <code>
    /// QuerySender sender = new QuerySender();
    /// sender.Execute();
    /// sender.Send();
    /// </code>
    /// </example>
    /// </remarks>
    public class QuerySender
    {
        #region Fields
        private const string FILE_NAME = "Result.xls";
        /// <summary>
        /// Number of set for select query.
        /// </summary>
        private int count;
        private string mailServer;
        private string sender;
        private char seperator = '\0';

        private string connectionString;
        private string provider;
        private string query;
        private string subject;
        private string receipient;
        private string cc;
        private string body;

        private string[] queries;
        private string[] subjects;
        private string[] receipients;
        private string[] ccs;
        private string[] bodies;
        /// <summary>
        /// Exported data.
        /// </summary>
        private DataTable table;
        #endregion

        /// <summary>
        /// Batch query sender
        /// </summary>
        /// <remarks>
        /// <b>author</b>   yeang-shign.then<br/>
        /// <b>since</b>    2008-11-28
        /// <example>
        /// <code>
        /// QuerySender sender = new QuerySender();
        /// sender.Send();
        /// </code>
        /// </example>
        /// </remarks>
        public QuerySender()
        {
            Initialize();
        }

        #region Methods
        private void Initialize()
        {
            query = string.Empty;
            subject = string.Empty;
            receipient = string.Empty;
            subject = string.Empty;
            cc = string.Empty;
            body = string.Empty;
            sender = System.Configuration.ConfigurationSettings.AppSettings["Sender"];
            mailServer = System.Configuration.ConfigurationSettings.AppSettings["MailServer"];

            Configuration rootWebConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (rootWebConfig.ConnectionStrings.ConnectionStrings.Count == 0) return;
            count = rootWebConfig.ConnectionStrings.ConnectionStrings.Count;

            //todo
            //connectionStrings = new string[count];
            //providers = new string[count];
            //queries = new string[count];
            //subjects = new string[count];
            //receipients = new string[count];
            //cc = new string[count];//090114tys
            //bodies = new string[count];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// <b>Changes</b>
        /// 2009-01-14: Add cc list[yeang-shing.then].
        /// </remarks>
        private void GetInfo(int index)
        {
            try
            {
                Configuration rootWebConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                connectionString = rootWebConfig.ConnectionStrings.ConnectionStrings[index].ConnectionString;
                provider = rootWebConfig.ConnectionStrings.ConnectionStrings[index].ProviderName;

                if (System.Configuration.ConfigurationSettings.AppSettings["Query" + index] != null)
                    query = System.Configuration.ConfigurationSettings.AppSettings["Query" + index];
                if (System.Configuration.ConfigurationSettings.AppSettings["Subject" + index] != null)
                    subject = System.Configuration.ConfigurationSettings.AppSettings["Subject" + index];
                if (System.Configuration.ConfigurationSettings.AppSettings["Receipient" + index] != null)
                    receipient = System.Configuration.ConfigurationSettings.AppSettings["Receipient" + index];
                if (System.Configuration.ConfigurationSettings.AppSettings["cc" + index] != null)
                    cc = System.Configuration.ConfigurationSettings.AppSettings["cc" + index];//090114tys
                if (System.Configuration.ConfigurationSettings.AppSettings["Body" + index] != null)
                    body = System.Configuration.ConfigurationSettings.AppSettings["Body" + index];

                if (System.Configuration.ConfigurationSettings.AppSettings["ReceipientFile" + index] != null)
                    GetReceipientAndCc(index, System.Configuration.ConfigurationSettings.AppSettings["ReceipientFile" + index]);
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(QuerySender), ex);
            }
        }

        /// <summary>
        /// Get receipient and cc list from text file.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fileName"></param>
        /// <remarks>
        /// <b>Refer</b>http://msdn.microsoft.com/en-us/library/system.io.streamreader.aspx
        /// </remarks>
        private void GetReceipientAndCc(int index, string fileName)
        {
            try
            {
                System.IO.StreamReader reader = System.IO.File.OpenText(fileName);
                string line = string.Empty;
                while ((line = reader.ReadLine()) != null) //while (reader.Peek() > 0)
                    System.Diagnostics.Debug.WriteLine(line);
                //receipients[index] = "";
                //cc[index] = "";
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(QuerySender), ex);
            }
        }
        private DataTable CreateTable(int columnCount)
        {
            DataTable output = new DataTable();
            for (int i = 0; i < columnCount; i++)
            {
                DataColumn column = new DataColumn("Column" + i, typeof(string));
                output.Columns.Add(column);
            }//end loops
            output.AcceptChanges();
            return output;
        }

        /// <summary>
        /// Execute logic and send.
        /// </summary>
        public void Send()
        {
            for (int i = 0; i < count; i++)
            {
                GetInfo(i);
                Execute(connectionString, provider, query);
                Send(i);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <see>http://msdn.microsoft.com/en-us/library/dd0w4a2z.aspx</see>
        /// <param name="connectionString"></param>
        /// <param name="providerName"></param>
        /// <param name="query"></param>
        private void Execute(string connectionString, string providerName, string query)
        {
            Logger.Info(typeof(QuerySender), query);

            table = new DataTable();
            DbProviderFactory provider = DbProviderFactories.GetFactory(providerName);
            DbConnection connection = provider.CreateConnection();
            DbCommand command = provider.CreateCommand();
            DbDataAdapter adapter = provider.CreateDataAdapter();

            try
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                command.Connection = connection;
                command.CommandText = query;
                adapter.SelectCommand = command;
                adapter.Fill(table);
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(QuerySender), ex);
                return;
            }
            finally
            {
                adapter.Dispose();
                command.Dispose();
                connection.Close();
            }
        }
        private void Send(int index)
        {
            try
            {
                DataManipulation lcls_data = new DataManipulation(DataManipulation.ApplicationType.Win);
                bool successFile = lcls_data.Export(DataManipulation.DataType.Excel, table, new string[] { }, FILE_NAME);
                if (successFile)
                    Logger.Info(typeof(QuerySender), "Excel exported successfully");
                else
                    Logger.Info(typeof(QuerySender), "Excel exported fail");
                System.Web.Mail.MailAttachment attachment = new System.Web.Mail.MailAttachment(Application.StartupPath + "\\" + FILE_NAME);
                SendMail(mailServer, sender, receipient, cc, subject, body, attachment);
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(QuerySender), ex);
                Logger.Error(typeof(QuerySender), "Happenned at index: " + index);
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mailServer"></param>
        /// <param name="sender"></param>
        /// <param name="receipient"></param>
        /// <param name="cc"></param>
        /// <param name="subject"></param>
        /// <param name="content"></param>
        /// <param name="attachments"></param>
        /// <remarks>
        /// <b>Changes</b><br/>
        /// 2009-08-20: Dispose the mail message object to prevent attachment in use once error happened<br/>
        /// <b>author</b>   yeang-shing.then<br/>
        /// <b>since</b>    2008-11-26
        /// </remarks>
        /// <returns></returns>
        private bool SendMail(string mailServer, string sender, string receipient, string cc,
            string subject, string content, object attachments)
        {
            if (sender == null || sender == "") return false;
            if (receipient == null || receipient == "") return false;
            MailMessage message1 = new MailMessage();

            try
            {
                message1.From = sender;
                message1.Cc = cc;
                message1.To = receipient;
                message1.Subject = subject;
                message1.BodyFormat = System.Web.Mail.MailFormat.Html;
                message1.Body = content;
                if (attachments != null)
                    message1.Attachments.Add(attachments);//081128tys
                if (!Validator.IsASCII(message1.Body))
                    message1.BodyEncoding = Encoding.UTF8;

                IPHostEntry entry1 = Dns.Resolve(mailServer);
                SmtpMail.SmtpServer = entry1.HostName;
                SmtpMail.Send(message1);

                Logger.Info(typeof(QuerySender), "Email from " + sender + " to: " + receipient + " cc: " + cc + " has been sent successfully.");
                return true;
            }
            catch (System.Exception ex)
            {
                Logger.Error(typeof(QuerySender), ex);
                return false;
            }
            finally { message1 = null; }
        }
        #endregion

    }//end class
}