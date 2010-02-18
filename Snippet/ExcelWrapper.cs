using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Data;
using System.Net;
using System.Web.Mail;
using System.Configuration;
using System.Windows.Forms;
using System.Threading;
using Plexus.Utils;

namespace Snippet
{
    /// <summary>
    /// Wrapper text file into excel format.
    /// </summary>
    /// <remarks>
    /// TODO: 1. use thread to process sending email to resolve smtp send error, or
    /// 2. replace the old smtp send method
    /// </remarks>
    public class ExcelWrapper : ConfigurationSection
    {
        #region Fields
        private string outputFile;
        private DataTable table;
        private DataView dataView;
        private string emailSeperator = ";";
        #endregion

        #region Properties
        /// <summary>
        /// Exchange mail server name.
        /// </summary>
        [ConfigurationProperty("mailserver")]
        public string MailServer
        {
            get { return this["mailserver"] as string; }
        }
        [ConfigurationProperty("username")]
        public string UserName
        {
            get { return this["username"] as string; }
        }
        [ConfigurationProperty("password")]
        public string Password
        {
            get { return this["password"] as string; }
        }
        /// <summary>
        /// Sender email address.
        /// </summary>
        [ConfigurationProperty("sender")]
        public string Sender
        {
            get { return this["sender"] as string; }
        }
        /// <summary>
        /// Input text file format.
        /// </summary>
        [ConfigurationProperty("sourceFormat")]
        public string SourceFormat
        {
            get { return this["sourceFormat"] as string; }
        }
        /// <summary>
        /// Output file stored location. If blank mean current executable location.
        /// </summary>
        [ConfigurationProperty("output")]
        public string Output
        {
            get { return this["output"] as string; }
        }
        /// <summary>
        /// Delimitor file text file.
        /// </summary>
        [ConfigurationProperty("seperator")]
        public string Seperator
        {
            get { return this["seperator"] as string; }
        }
        /// <summary>
        /// Lookup to email column in the text file.
        /// </summary>
        [ConfigurationProperty("to")]
        public string To
        {
            get { return this["to"] as string; }
        }
        /// <summary>
        /// Lookup cc email column in the text file.
        /// </summary>
        [ConfigurationProperty("cc")]
        public string Cc
        {
            get { return this["cc"] as string; }
        }
        /// <summary>
        /// Blind copy to sender.
        /// </summary>
        [ConfigurationProperty("bcc")]
        public string Bcc
        {
            get { return this["bcc"] as string; }
        }
        /// <summary>
        /// Lookup to email column in the text file.
        /// </summary>
        [ConfigurationProperty("tomanager")]
        public string ToManager
        {
            get { return this["tomanager"] as string; }
        }
        /// <summary>
        /// Group by column to lookup cc list.
        /// </summary>
        [ConfigurationProperty("groupby")]
        public string GroupBy
        {
            get { return this["groupby"] as string; }
        }
        /// <summary>
        /// Starting column for export.
        /// </summary>
        [ConfigurationProperty("readfromcolumn")]
        public string ReadFromColumn
        {
            get { return this["readfromcolumn"] as string; }
        }
        /// <summary>
        /// End of column for export.
        /// </summary>
        [ConfigurationProperty("readtocolumn")]
        public string ReadToColumn
        {
            get { return this["readtocolumn"] as string; }
        }
        /// <summary>
        /// Subject of email to send out.
        /// </summary>
        [ConfigurationProperty("subject")]
        public string Subject
        {
            get { return this["subject"] as string; }
        }
        /// <summary>
        /// Content to in email.
        /// </summary>
        [ConfigurationProperty("body")]
        public string Body
        {
            get { return this["body"] as string; }
        }
        /// <summary>
        /// Content to in email for cc list.
        /// </summary>
        [ConfigurationProperty("bodycc")]
        public string BodyCc
        {
            get { return this["bodycc"] as string; }
        }
        /// <summary>
        /// Content to in email for Manager list.
        /// </summary>
        /// <remarks>
        /// <b>Author</b>   yeang-shing.then<br/>
        /// <b>Since</b>    2009-02-26<br/>
        /// </remarks>
        [ConfigurationProperty("bodymanager")]
        public string BodyManager
        {
            get { return this["bodymanager"] as string; }
        }
        #endregion

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks>
        /// <example>
        /// <code>
        ///  ExcelWrapper wrapper = new ExcelWrapper();
        ///  wrapper.Read();
        ///  wrapper.Send();
        /// </code>
        /// </example>
        /// </remarks>
        public ExcelWrapper()
        {
            table = new DataTable();
        }

        #region Methods
        /// <summary>
        /// Execute logic.
        /// </summary>
        /// <remarks>
        /// Pseudo Code<br/>
        /// <code>
        /// Read();
        /// Process();
        /// for loop receipient list then Send();
        /// </code>
        /// <b>Author</b>   yeang-shing.then<br/>
        /// <b>Since</b>    2009-02-12<br/>
        /// </remarks>
        public void Execute()
        {
            Read();
            Process();
        }
        /// <summary>
        /// Read from SourceFormat input file (text) and process into data table.
        /// </summary>
        /// <remarks>
        /// <b>Changes</b><br/>
        /// 2009-02-02: move to a new folder then delete it. refer to LC Ho email[yeang-shing.then]<br/>
        /// 2009-02-02: change export to .xls to .csv. LCHo request[yeang-shing.then]<br/>
        /// 2009-04-03: change export from .csv to .xls. LCHo request[yeang-shing.then]<br/>
        /// <b>Refer</b>http://msdn.microsoft.com/en-us/library/system.io.streamreader.aspx<br/>
        /// </remarks>
        public void Read()
        {
            string fileName = string.Empty;
            string newFolder = string.Empty;

            try
            {
                //get a flexible file name with current date time format in MMYY or other.
                fileName = GetCurrentFileName(ExcelWrapper.GetConfig().SourceFormat);
                if (!File.Exists(fileName))
                {
                    Logger.Info(typeof(ExcelWrapper), fileName + " not found");
                    return;
                }

                StreamReader reader = File.OpenText(fileName);
                string line = string.Empty;
                bool isFistLine = false;
                while ((line = reader.ReadLine()) != null)
                {
                    if (table.Columns.Count == 0 && line != null) isFistLine = true;
                    if (isFistLine)
                    {
                        CreateTable(line);
                        isFistLine = false;
                    }
                    else
                        DumpData(line);
                }//end loops
                reader.Close();
                this.table.AcceptChanges();

                //move to a new folder then delete it //090202tys refer to LC Ho email
                newFolder = GetNewFolder(GetFolderOnly(fileName));
                FileInfo info = new FileInfo(fileName);
                info.CopyTo(newFolder + "\\" + GetFileOnly(fileName));//090217tys
                info.Delete();
                Logger.Info(typeof(ExcelWrapper), fileName + " has been moved");
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(ExcelWrapper), ex);
                throw ex;
            }
        }
        /// <summary>
        /// Process to get the collection of receipient, and cc name list, then export, and send it out.
        /// </summary>
        /// <remarks>
        /// <b>Changes</b><br/>
        /// 2009-02-17: Change loop logic to take care last row of data.[yeang-shing.then]<br/>
        /// <b>Author</b>   yeang-shing.then<br/>
        /// <b>Since</b>    2009-02-17<br/>
        /// </remarks>
        private void Process()
        {
            if (this.table == null) return;
            if (this.table.Rows.Count == 0) return;

            try
            {
                string previousTo = "";
                string to = "";
                string previousCc = "";//090226tys
                string cc = "";
                ArrayList copyList = new ArrayList();//090212tys

                dataView = this.table.DefaultView;
                dataView.Sort = (ExcelWrapper.GetConfig().GroupBy == ExcelWrapper.GetConfig().To) ? //hack 090116tys
                    ExcelWrapper.GetConfig().GroupBy.Replace(' ', '_') + "," + ExcelWrapper.GetConfig().Cc.Replace(' ', '_') :
                    ExcelWrapper.GetConfig().GroupBy.Replace(' ', '_') + "," + ExcelWrapper.GetConfig().To.Replace(' ', '_');
                for (int i = 1; i < dataView.Count; i++)
                {
                    to = dataView[i][ExcelWrapper.GetConfig().To.Replace(' ', '_')].ToString();
                    cc = dataView[i][ExcelWrapper.GetConfig().Cc.Replace(' ', '_')].ToString();
                    previousTo = dataView[i - 1][ExcelWrapper.GetConfig().To.Replace(' ', '_')].ToString();
                    previousCc = dataView[i - 1][ExcelWrapper.GetConfig().Cc.Replace(' ', '_')].ToString();
                    if (previousCc != "" && !copyList.Contains(previousCc))
                        copyList.Add(previousCc);//090212tys

                    //090226tys capture send to no body and no cc list
                    if ((to == string.Empty && cc != previousCc)
                        && (previousTo == string.Empty && previousCc == string.Empty))
                    {
                        Process(previousTo, copyList);
                        previousCc = string.Empty;
                        copyList = new ArrayList();
                    }

                    if (to != previousTo) //090226tys receipient, or cc can be empty
                    {
                        Process(previousTo, copyList);
                        previousCc = string.Empty;
                        copyList = new ArrayList();
                    }
                }//end loops

                to = dataView[dataView.Count - 1][ExcelWrapper.GetConfig().To.Replace(' ', '_')].ToString();
                cc = dataView[dataView.Count - 1][ExcelWrapper.GetConfig().Cc.Replace(' ', '_')].ToString();
                if (cc != "" && !copyList.Contains(cc))
                    copyList.Add(cc);
                Process(to, copyList);
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(ExcelWrapper), ex);
                throw ex;
            }
        }
        /// <summary>
        /// Process export excel and send out email
        /// </summary>
        /// <param name="to">Recipient address</param>
        /// <param name="cc">Copy and Bcc address</param>
        /// <remarks>
        /// <b>Author</b>	yeang-shing.then<br/>
        /// <b>Since</b>	2009-08-20<br/>
        /// </remarks>
        private void Process(string to, ArrayList cc)
        {
            if (Export(to, cc))
                Send(to, cc);
        }
        /// <summary>
        /// Selected certain columns to be exported only.
        /// </summary>
        /// <remarks>
        /// <b>Changes</b><br/>
        /// 2009-02-02: LC.Ho request change export to .csv [yeang-shing.then]<br/>
        /// 2009-02-12: LC.Ho request export to corresponded receipient and cc name appear only to certain data row[yeang-shing.then]<br/>
        /// 2009-03-02: Add filtering supplier='' and buyer=''[yeang-shing.then]<br/>
        /// 2009-04-03: Change export from .cvs to .xls[yeang-shing.then]<br/>
        /// </remarks>
        private bool Export(string receipient, ArrayList ccs)
        {
            bool exported = false;//090226tys
            try
            {
                //filtering by receipient, and cc row only.
                DataTable outputTable = this.table.Copy();
                DataView outputView = outputTable.DefaultView;
                string query = ExcelWrapper.GetConfig().To.Replace(' ', '_') + "='" + receipient + "'";
                if (ccs.Count == 0)
                    query += " AND " + ExcelWrapper.GetConfig().Cc.Replace(' ', '_') + "=''";//090302tys
                else
                {
                    query += " AND " + ExcelWrapper.GetConfig().Cc.Replace(' ', '_') + " IN(";
                    for (int i = 0; i < ccs.Count; i++)
                        query += "'" + ccs[i].ToString() + "',";
                    query = query.TrimEnd(new char[] { ',' });
                    query += ")";
                }

                Logger.Info(typeof(ExcelWrapper), query);
                try
                {
                    outputView.RowFilter = query;
                }
                catch (Exception ex)
                {
                    Logger.Error(typeof(ExcelWrapper), ex);
                    File.Delete(this.outputFile);
                    return false;
                }

                //clone to a new table for exporting.
                DataTable newTable = outputTable.Clone();
                for (int i = 0; i < outputView.Count; i++)
                {
                    DataRow row = newTable.NewRow();
                    for (int j = 0; j < outputTable.Columns.Count; j++)
                        row[j] = outputView[i][j];
                    newTable.Rows.Add(row);
                }//end loops
                newTable.AcceptChanges();


                //remove unneccessary column
                for (int i = newTable.Columns.Count - 1; i >= 0; i--)
                {
                    if (i > Convert.ToInt32(ExcelWrapper.GetConfig().ReadToColumn) - 1
                        || i < Convert.ToInt32(ExcelWrapper.GetConfig().ReadFromColumn) - 1)
                        newTable.Columns.RemoveAt(i);
                }//end loops
                newTable.AcceptChanges();
                Logger.Info(typeof(ExcelWrapper), "row in table: " + newTable.Rows.Count);

                //start exporting
                this.outputFile = Application.StartupPath + "\\" + ExcelWrapper.GetConfig().Output;//hack 020212tys
                DataManipulation lcls_data = new DataManipulation(DataManipulation.ApplicationType.Win);
                exported = lcls_data.Export(DataManipulation.DataType.Excel, newTable, new string[] { }, this.outputFile);//090403tys
                Logger.Info(typeof(ExcelWrapper), "Excel exported " + exported);
                return exported;
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(ExcelWrapper), ex);
                File.Delete(this.outputFile);
                return false;
            }
        }
        /// <summary>
        /// Attach the excel file and send out.
        /// </summary>
        /// <remarks>
        /// <b>Changes</b><br/>
        /// 2009-02-20: provide a second email content if there are no receipient but only cc[yeang-shing.then]<br/>
        /// </remarks>
        public void Send(string receipient, ArrayList ccs)
        {
            System.Web.Mail.MailAttachment attachment = new System.Web.Mail.MailAttachment(this.outputFile);//check
            try
            {
                string to = receipient;//090226tys
                string body = (receipient.Length == 0) ? ExcelWrapper.GetConfig().BodyCc : ExcelWrapper.GetConfig().Body;
                if (receipient.Length == 0 && ccs.Count == 0)
                {
                    to = ExcelWrapper.GetConfig().ToManager;
                    body = ExcelWrapper.GetConfig().BodyManager;
                }

                SendMail(ExcelWrapper.GetConfig().MailServer,
                    ExcelWrapper.GetConfig().Sender,
                    to,
                    JoinNames(ccs),
                    ExcelWrapper.GetConfig().Bcc,
                    ExcelWrapper.GetConfig().Subject,
                    body,
                    attachment);
                attachment = null;
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(ExcelWrapper), ex);
                return;
            }
            //key 090820tys: release holding resource
            finally { File.Delete(this.outputFile); }
        }
        /// <summary>
        /// Send email through exchange server.
        /// </summary>
        /// <param name="mailServer"></param>
        /// <param name="sender"></param>
        /// <param name="receipient"></param>
        /// <param name="cc"></param>
        /// <param name="bcc"></param>
        /// <param name="subject"></param>
        /// <param name="content"></param>
        /// <param name="attachments"></param>
        /// <remarks>
        /// 2009-02-02: Force send external email. Refer http://www.experts-exchange.com/Programming/Languages/.NET/ASP.NET/Q_22409059.html[yeang-shing.then]<br/>
        /// 2009-08-20: release attachment once it fail to send to prevent file in use[yeang-shing.then]<br/>
        /// <b>author</b>   yeang-shing.then<br/>
        /// <b>since</b>    2008-11-26<br/>
        /// </remarks>
        /// <returns></returns>
        private bool SendMail(string mailServer, string sender, string receipient, string cc, string bcc,
            string subject, string content, object attachments)
        {
            if (sender == null || receipient == null || cc == null || bcc == null) return false;//090220tys
            if (sender == string.Empty && receipient == string.Empty && cc == string.Empty && bcc == string.Empty) return false;//090220tys
            MailMessage message1 = new MailMessage();

            try
            {
                message1.From = sender;
                message1.To = receipient;
                message1.Cc = cc;
                message1.Bcc = bcc;//090123tys
                message1.Subject = subject;
                message1.BodyFormat = System.Web.Mail.MailFormat.Html;
                message1.Body = content;
                if (attachments != null) message1.Attachments.Add(attachments);//081128tys
                if (!Validator.IsASCII(message1.Body)) message1.BodyEncoding = Encoding.UTF8;
                message1.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", "1"); //basic authentication
                message1.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername", ExcelWrapper.GetConfig().UserName); //set your username here
                message1.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword", ExcelWrapper.GetConfig().Password); //set your password here

                IPHostEntry entry1 = Dns.Resolve(mailServer);
                SmtpMail.SmtpServer = entry1.HostName;
                SmtpMail.Send(message1);

                Logger.Info(typeof(ExcelWrapper), "Email from " + sender + " to: " + receipient + " cc: " + cc + " has been sent successfully.");
                Logger.Info(typeof(ExcelWrapper), "bcc: " + bcc);
                return true;
            }
            catch (System.Exception ex)
            {
                Logger.Error(typeof(ExcelWrapper), ex);
                return false;
            }
            finally
            {
                message1.Attachments.RemoveAt(0);
                message1 = null;
            }
        }
        #endregion

        #region Functions
        /// <summary>
        /// Get a current file name append with date time value.
        /// TODO: cater more cases. Use regex.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private string GetCurrentFileName(string sender)
        {
            string output = sender;
            string[] formats = new string[]
            {
                "MMYYYY","MMyyyy","mmyyyy","mmYYYY",
                "DDMMYY","DDMMyy","DDmmyy","ddmmyy","ddmmYY","ddMMYY",
                "DDYYMM","DDYYmm","DDyymm","ddyymm","ddyyMM","ddYYMM",
                "MMDDYY","mmddyy",
                "MMYYDD","mmyydd",
                "MMYY","MMyy","mmyy","mmYY",
                "YYMM","YYmm","yymm","yyMM",
            };
            foreach (string s in formats)
                output = output.Replace(s, DateTime.Now.ToString(s));

            return output;
        }
        /// <summary>
        /// Get the new instance of this class.
        /// </summary>
        /// <returns></returns>
        public static ExcelWrapper GetConfig()
        {
            return ConfigurationManager.GetSection("ExcelWrapper") as ExcelWrapper;//hack
        }
        /// <summary>
        /// Create a blank table with the columns correspond to given text format.
        /// </summary>
        /// <param name="columns"></param>
        private void CreateTable(string columns)
        {
            try
            {
                table = new DataTable();
                string[] splits = columns.Split(ExcelWrapper.GetConfig().Seperator.ToCharArray());//hack
                //for (int i = Convert.ToInt32(ExcelWrapper.GetConfig().ReadFromColumn)-1;
                //    i <= Math.Min(Convert.ToInt32(ExcelWrapper.GetConfig().ReadToColumn)-1, splits.Length);
                //    i++)
                for (int i = 0; i < splits.Length; i++)
                {
                    //column name without space to prevent from exporting to excel error
                    DataColumn column = new DataColumn(splits[i].Replace(' ', '_'), typeof(string));
                    table.Columns.Add(column);
                }//end loops
                table.AcceptChanges();
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(ExcelWrapper), ex);
            }
        }
        /// <summary>
        /// Dump data from SourceFormat text file into a new created table.
        /// </summary>
        /// <param name="sender"></param>
        private void DumpData(string sender)
        {
            try
            {
                string[] splits = sender.Split(ExcelWrapper.GetConfig().Seperator.ToCharArray());
                DataRow row = table.NewRow();
                //for (int i = Convert.ToInt32(ExcelWrapper.GetConfig().ReadFromColumn) - 1;
                //    i <= Math.Min(Convert.ToInt32(ExcelWrapper.GetConfig().ReadToColumn) - 1, splits.Length);
                //    i++)
                for (int i = 0; i < Math.Min(splits.Length, table.Columns.Count); i++)
                {
                    if (i > splits.Length - 1)
                        row[i] = "";
                    else
                        row[i] = splits[i];
                }//end loops
                table.Rows.Add(row);
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(ExcelWrapper), ex);
            }
        }
        /// <summary>
        /// Join the receipient or cc email name list.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private string JoinNames(string[] sender)
        {
            string output = string.Empty;
            foreach (string s in sender)
                output += s + ";";
            output = output.TrimEnd(new char[] { ';' });

            return output;
        }
        /// <summary>
        /// Join the receipient or cc email name list.
        /// </summary>
        /// <param name="sender">ArrayList in string</param>
        /// <returns></returns>
        /// <reamrks>
        /// <b>Author</b>   yeang-shing.then<br/>
        /// <b>Since</b>    2009-02-12<br/>
        /// </reamrks>
        private string JoinNames(ArrayList sender)
        {
            string output = string.Empty;
            for (int i = 0; i < sender.Count; i++)
                output += sender[i].ToString() + ";";
            output = output.TrimEnd(new char[] { ';' });

            return output;
        }
        /// <summary>
        /// Set the new output file name located in a directory.
        /// According to yyyymm format and file name is ddHHmm
        /// </summary>
        /// <remarks>
        /// <b>Changes</b><br/>
        /// 2009-02-12: change new folder must under working path.<br/>
        /// 2009-02-20: new folder must depend on source file.<br/>
        /// </remarks>
        private string GetNewFolder(string workingPath)
        {
            string output = "";
            try
            {
                string newFolderName = DateTime.Now.ToString("yyyyMM");
                string newFileName = DateTime.Now.ToString("ddHHmm");
                //check it is a valid working path if not place on start up application folder
                if (workingPath.Length == 0) //090220tys
                    workingPath = Application.StartupPath;
                DirectoryInfo directoryInfo = new DirectoryInfo(workingPath);
                if (!Directory.Exists(workingPath + "\\" + newFolderName))
                    Directory.CreateDirectory(workingPath + "\\" + newFolderName);
                //FileInfo[] filesInfo = directoryInfo.GetFiles();
                output = workingPath + "\\" + newFolderName;// +"\\" + newFileName + ".xls";//090202tys

                return output;
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(ExcelWrapper), ex);
                return output;
            }

        }
        /// <summary>
        /// Return file name with extension only (without the fullpath).
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        /// <remarks>
        /// <b>Author</b>   yeang-shing.then<br/>
        /// <b>Since</b>    2009-02-17<br/>
        /// </remarks>
        private string GetFileOnly(string sender)
        {
            string output = string.Empty;
            string[] holds = sender.Split(new char[] { '\\', '/' });
            output = holds[holds.Length - 1];
            return output;
        }
        /// <summary>
        /// Get the physical path where file located despite of the file name.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        /// <remarks>
        /// <b>Author</b>   yeang-shing.then<br/>
        /// <b>Since</b>    2009-02-20<br/>
        /// </remarks>
        private string GetFolderOnly(string sender)
        {
            string output = string.Empty;
            int index = sender.LastIndexOf('.');
            string[] holds = sender.Split(new char[] { '\\', '/' });
            for (int i = 0; i < holds.Length - 1; i++)
                output += holds[i] + "\\";
            return output;
        }
        #endregion
    }
    //last updated 20090212tys
}//end namespace since 20090115tys