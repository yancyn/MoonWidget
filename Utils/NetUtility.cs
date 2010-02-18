// include permissions namespace for security attributes
// include principal namespace for windowsidentity class
// include interopservices namespace for dllImports.
using System;
using System.Collections;
using System.Text;
using System.Net;
using System.Net.Security;
using System.Web.Mail;
using System.Security.Principal;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.DirectoryServices;
//using Microsoft.Office.Interop.Outlook;


namespace Plexus.Utils
{
    public class NetUtility
    {
        public NetUtility()
        {

        }
        #region Quaranteen Outlook feature
        /// <summary>
        /// Get a collection of addressbook from outlook.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Fail in Plexus policy
        /// Author  : yeang-shing.then
        /// Since   : 2008-07-11
        /// </remarks>
        /*public DataTable GetOutlookPhoneBook()
        {
            DataTable table = new DataTable();
            DataColumn col = new DataColumn("FullName", typeof(string));
            DataColumn col2 = new DataColumn("Email", typeof(string));
            DataColumn col3 = new DataColumn("Department", typeof(string));
            table.Columns.Add(col);
            table.Columns.Add(col2);
            table.Columns.Add(col3);

            try
            {
                Application application1 = new ApplicationClass();
                NameSpace space1 = application1.GetNamespace("MAPI");
                MAPIFolder folder1 = space1.GetDefaultFolder(OlDefaultFolders.olFolderContacts);
                for (int num1 = 1; num1 <= folder1.Items.Count; num1++)
                {
                    ContactItemClass class1 = folder1.Items[num1] as ContactItemClass;
                    DataRow row1 = table.NewRow();
                    row1["FullName"] = class1.FullName;
                    row1["Email"] = class1.Email1Address;
                    row1["Department"] = class1.Department;

                    table.Rows.Add(row1);
                }

                space1 = null;
                application1.Quit();
                application1 = null;

                table.AcceptChanges();
                return table;
            }
            catch (System.Exception ex)
            {
                if (ex.Message == "Operation aborted")
                {
                    Logger.Error(typeof(NetUtility), "User had terminated the import list process!");
                    return table;
                }

                Logger.Error(typeof(NetUtility), ex);
                return table;
            }
        }
        /// <summary>
        /// Use outlook to send an email.
        /// </summary>
        /// <param name="recipients"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        public static void SendOutlookMail(string recipients, string subject, string body)
        {
            try
            {
                ApplicationClass appClass = new ApplicationClass();

                //Create the new message
                //Here i m fully qualifing the name to avoid ambiguity
                Microsoft.Office.Interop.Outlook._MailItem myMsg = (MailItem)appClass.CreateItem(OlItemType.olMailItem);

                //Add a recipient.
                Recipient myRecip = null;
                NetUtility lcls_net = new NetUtility();
                string[] ls_Recipients = lcls_net.GetRecipientList(recipients);
                foreach (string s in ls_Recipients)
                {
                    myRecip = (Recipient)myMsg.Recipients.Add(s);
                    //substitute the emailID word by actual emailID
                    myRecip.Resolve();
                }

                //Set the basic properties.
                myMsg.Subject = subject;//"This is the subject of the test message";
                myMsg.Body = body;//"This is the text in the message.";

                //Add an attachment.
                //				String sSource;
                //				String sDisplayName;
                //				int position;
                //				int attachType;
                //				Attachment myAttach;
                //
                //				sSource = "";//enter path here of the file to be uploaded;
                //				sDisplayName = "MyFirstAttachment";
                //				position = (int)myMsg.Body.Length + 1;
                //				attachType = (int)OlAttachmentType.olByValue;
                //				myAttach = myMsg.Attachments.Add(sSource, attachType, position,sDisplayName);

                //Save and Send the message.
                myMsg.Save();
                myMsg.Display(null);
                //myMsg.Send();//send directly

                //Explicitly release objects.
                myRecip = null;
                //myAttach = null;
                myMsg = null;
                appClass = null;
            }
            catch (System.Exception ex)
            {
                Logger.Error(typeof(NetUtility), ex);
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("Done");
            }
        }
        */
        #endregion

        /// <summary>
        /// A simple send email function invoke by default local host setting (must IIS installed).
        /// </summary>
        /// <param name="mailServer">eg: localhost,PGSEPMDB001..</param>
        /// <param name="format"></param>
        /// <param name="sender"></param>
        /// <param name="receipient"></param>
        /// <param name="subject"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        /// <remarks>
        /// Only available when SMTP virtual server set authentication to anynomous access
        /// and relay restriction accept all except the list below.
        /// </remarks>
        public static bool SendMail(string mailServer, MailFormat format, string sender, string receipient, string subject, string content)
        {
            return SendMail(mailServer, format, System.Web.Mail.MailPriority.Normal, sender, receipient, subject, content);
        }
        /// <summary>
        /// A simple send email function invoke by default local host setting (must IIS installed).
        /// </summary>
        /// <param name="mailServer">eg: localhost,PGSEPMDB001..</param>
        /// <param name="format"></param>
        /// <param name="priority">High,Normal, or Low.</param>
        /// <param name="sender"></param>
        /// <param name="receipient"></param>
        /// <param name="subject"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        /// <remarks>
        /// Only available when SMTP virtual server set authentication to anynomous access and relay restriction accept all except the list below.
        /// </remarks>
        public static bool SendMail(string mailServer, MailFormat format, System.Web.Mail.MailPriority priority, string sender, string receipient, string subject, string content)
        {
            try
            {
                if (sender == "")
                {
                    //MessageBox.Show("Sender address can not be emtpy, please try again.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return false;
                }

                if (receipient == "")
                {
                    //MessageBox.Show("Receipient address can not be emtpy, please try again.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return false; ;
                }

                MailMessage message1 = new MailMessage();
                message1.From = sender;
                message1.To = receipient;
                message1.Subject = subject;
                message1.BodyFormat = format;
                message1.Body = content;
                if (!Validator.IsASCII(message1.Body))
                    message1.BodyEncoding = Encoding.UTF8;
                message1.Priority = priority;
                message1.Attachments.Clear();

                //string mailServerName = "PGSEPMDB001";//localhost
                IPHostEntry entry1 = Dns.Resolve(mailServer);
                SmtpMail.SmtpServer = entry1.HostName;
                SmtpMail.Send(message1);

                Logger.Info(typeof(NetUtility), "Email from: " + sender + " to: " + receipient + " has been sent successfully.");
                return true;
            }
            catch (System.Exception ex)
            {
                Logger.Error(typeof(NetUtility), ex);
                return false;
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
        /// <returns></returns>
        public static bool SendMail(string mailServer, string sender, string receipient, string cc, string subject, string content)
        {
            return SendMail(mailServer, sender, receipient, cc, subject, content, null);
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
        /// 2009-04-17      : change return logic[yeang-shing.then]<br/>
        /// <b>Author</b>   yeang-shing.then<br/>
        /// <b>Since</b>    2008-11-26<br/>
        /// </remarks>
        /// <returns></returns>
        public static bool SendMail(string mailServer, string sender, string receipient, string cc, string subject, string content, object attachments)
        {
            if (receipient == null && cc == null) return false;
            if (receipient.Length == 0 && cc.Length == 0) return false;

            try
            {
                MailMessage message1 = new MailMessage();
                message1.From = sender;
                message1.Cc = cc;
                message1.To = receipient;
                message1.Subject = subject;
                message1.BodyFormat = System.Web.Mail.MailFormat.Html;
                message1.Body = content;

                if (attachments != null) message1.Attachments.Add(attachments);//081128tys
                //if (!Validator.IsASCII(message1.Body))
                message1.BodyEncoding = Encoding.UTF8;

                IPHostEntry entry1 = Dns.Resolve(mailServer);
                SmtpMail.SmtpServer = entry1.HostName;
                SmtpMail.Send(message1);

                Logger.Info(typeof(NetUtility), "Email from " + sender + " to: " + receipient + " cc: " + cc + " has been sent successfully.");
                return true;
            }
            catch (System.Exception ex)
            {
                Logger.Error(typeof(NetUtility), ex);
                return false;
            }
        }
        /// <summary>
        /// Get a list of recipient (seperate by ',' or ';').
        /// </summary>
        /// <param name="recipients">Joinned names.</param>
        /// <returns></returns>
        private string[] GetRecipientList(string recipients)
        {
            string[] output = new string[] { };

            try
            {
                string[] list1 = recipients.Split(',');
                string[] list2 = recipients.Split(';');

                ArrayList al = new ArrayList();
                foreach (string s in list1)
                    al.Add(s);
                foreach (string s in list2)
                {
                    if (!al.Contains(s))
                        al.Add(s);
                }//end loops

                output = new string[al.Count];
                for (int i = 0; i < output.Length; i++)
                    output[i] = al[i].ToString();

                Logger.Info(typeof(NetUtility), "Recipient list: " + output);
                return output;
            }
            catch (System.Exception ex)
            {
                Logger.Error(typeof(NetUtility), ex);
                return output;
            }
        }
    }

    //[Assembly()]
    //[:()]
    //[SecurityPermissionAttribute(SecurityAction.RequestMinimum, UnmanagedCode = true)]
    /// <summary>
    /// Windows NT login class.
    /// </summary>
    /// <remarks>
    /// Still need javascript to handle domain name otherwise temporarily hard code.
    /// <b>author</b>yeang-shing.then<br/>
    /// <b>since</b>2008-07-04
    /// </remarks>
    /// <see>http://www.codeproject.com/KB/web-security/ASPdotnet_LoginControl.aspx</see>
    public class WinLogin
    {
        [DllImport("C:\\WINDOWS\\System32\\advapi32.dll")]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref int phToken);

        [DllImport("C:\\WINDOWS\\System32\\Kernel32.dll")]
        private static extern int GetLastError();

        /// <summary>
        /// Windows NT login for this user.
        /// </summary>
        /// <param name="domainName"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <see>http://www.google.com/codesearch?hl=en&q=WindowsIdentity+show:zsv9SxhGJsE:0T3jmn40aBI:SmF4uH0WqPg&sa=N&cd=10&ct=rc&cs_p=http://www.informit.com/content/images/0672321246/downloads/code16.zip&cs_f=Chapter+16/Principal/Principal.cs</see>
        /// <returns>True if success otherwise false.</returns>
        public static bool LogInThisUser(string domainName, string userName, string password)
        {
            bool success = false;

            try
            {
                // The Windows NT user token.
                int token1 = 0;
                // Get the user token for the specified user, machine, and password using the unmanaged LogonUser method.
                // The parameters for LogonUser are the user name, computer name, password,
                // Logon type (LOGON32_LOGON_NETWORK_CLEARTEXT), Logon provider (LOGON32_PROVIDER_DEFAULT),
                // and user token.
                bool loggedOn = LogonUser(userName, domainName, password, 3, 0, ref token1);
                // impersonate user
                IntPtr token2 = new IntPtr(token1);
                WindowsImpersonationContext mWIC = new WindowsIdentity(token2).Impersonate();

                WindowsIdentity winUser = new WindowsIdentity(token2);
                success = (winUser.Name.ToLower() == domainName.ToLower() + "\\" + userName.ToLower()) ? true : false;
                return success;
            }
            catch (System.Exception ex)
            {
                Logger.Error(typeof(WinLogin), ex);
                return false;
            }
        }
    }

    /// <summary>
    /// File explorer tool.
    /// </summary>
    public class FileTool
    {
        public FileTool()
        {
        }

        public string[] GetFileList(string path)
        {
            string[] list = new string[] { };
            DirectoryInfo d = new DirectoryInfo(path);
            FileInfo[] lFiles = d.GetFiles();

            list = new string[lFiles.Length];
            for (int i = 0; i < lFiles.Length; i++)
                list[i] = lFiles[i].Name;

            return list;
        }
        public DataTable GetFileInfo(string path)
        {
            DataTable myTable = new DataTable("FileInfo");
            DataColumn dc1 = new DataColumn("Name", typeof(string));
            DataColumn dc2 = new DataColumn("Size", typeof(string));
            DataColumn dc3 = new DataColumn("Type", typeof(string));
            DataColumn dc4 = new DataColumn("Date Modified", typeof(DateTime));
            DataColumn dc5 = new DataColumn("Date Created", typeof(DateTime));
            DataColumn dc6 = new DataColumn("Date Accessed", typeof(DateTime));
            myTable.Columns.Add(dc1);
            myTable.Columns.Add(dc2);
            myTable.Columns.Add(dc3);
            myTable.Columns.Add(dc4);
            myTable.Columns.Add(dc5);
            myTable.Columns.Add(dc6);

            try
            {
                DirectoryInfo d = new DirectoryInfo(path);
                DirectoryInfo[] ds = d.GetDirectories();
                foreach (DirectoryInfo i in ds)
                    Binding(i, path, ref myTable);

                myTable.AcceptChanges();
                return myTable;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return myTable;
            }
        }
        private void Binding(DirectoryInfo d, string path, ref DataTable table)
        {
            FileInfo[] lFiles = d.GetFiles();
            string[] roots = path.Split('\\');
            foreach (FileInfo f in lFiles)
            {
                DataRow newRow = table.NewRow();
                newRow["Name"] = ConvertToRootPath(roots[roots.Length - 1], f.DirectoryName.Replace(path, "") + "\\" + f.Name);
                newRow["Size"] = ConvertToSizeText(f.Length);
                newRow["Type"] = f.Extension.TrimStart(new char[] { '.' });
                newRow["Date Created"] = f.CreationTime;
                newRow["Date Modified"] = f.LastWriteTime;
                newRow["Date Accessed"] = f.LastAccessTime;

                table.Rows.Add(newRow);
            }//end loops
        }
        private string ConvertToRootPath(string root, string path)
        {
            string output = "";
            output = root + path;

            return output;
        }
        /// <summary>
        /// Convert to KB or MB value based on input bytes.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public string ConvertToSizeText(long size)
        {
            string output = "0 KB";
            string format = "###,###,###,###,###";
            string space = " ";
            string[] suffix = new string[4] { "KB", "MB", "GB", "TB" };
            int i = 0;//mark location of suffix
            int d = (int)size / 1000;

        Loop:
            output = d.ToString(format) + space + suffix[i];

            if (d > 1000 && i < 4)
            {
                i++;
                Console.WriteLine(i);
                d /= 1000;
                //d = (int)d/1000;
                goto Loop;
            }

            return output;
        }
        public bool WriteFileList(string[] files, string path, bool overwrite)
        {
            try
            {
                FileStream fs = null;
                if (overwrite)
                {
                    if (File.Exists(path))
                        File.Delete(path);
                }
                else
                {
                    if (File.Exists(path))
                    {
                        fs = new FileStream(path, FileMode.Append);
                        fs.ToString();
                    }
                }

                StringBuilder sb = new StringBuilder();
                if (fs != null && fs.Length > 0)
                    sb.Append(fs.ToString() + ";");
                foreach (string s in files)
                {
                    //check duplicate file name
                    sb.Append(s + ";");
                }

                if (!File.Exists(path))
                    fs = new FileStream(path, FileMode.Create);

                StreamWriter sw = new StreamWriter(fs);
                string[] mix = sb.ToString().Split(';');
                foreach (string s in mix)
                    sw.WriteLine(s);
                sw.Close();

                fs.Close();
                return true;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return false;
            }
        }

    }//end FileTool

    /// <summary>
    /// LDAP Authentication class.
    /// </summary>
    /// <see>http://www.codeproject.com/KB/system/everythingInAD.aspx</see>
    /// <see>http://www.codeproject.com/KB/system/activedirquery.aspx</see>
    /// <see>http://www.codeproject.com/KB/aspnet/ADAM_and_LDAP_ClientNet.aspx</see>
    /// <remarks>
    /// <b>Author</b>   boon-hou.wee<br/>
    /// <b>Since</b>    2009-11-20<br/>
    /// </remarks>
    public class Ldap
    {
        #region Properties
        public string HomeMdb = "homemdb";
        public string distinguishedname = "distinguishedname";
        public string countrycode = "countrycode";
        public string company = "company";
        public string lastlogoff = "lastlogoff";
        public string mailnickname = "mailnickname";
        //public string defender-violationcount = "defender-violationcount";	
        public string dscorepropagationdata = "dscorepropagationdata";
        public string msexchhomeservername = "msexchhomeservername";
        public string facsimiletelephonenumber = "facsimiletelephonenumber";
        public string msexchalobjectversion = "msexchalobjectversion";
        public string usncreated = "usncreated";
        public string objectguid = "objectguid";
        public string postalcode = "postalcode";
        public string whenchanged = "whenchanged";
        public string memberof = "memberof";
        public string adspath = "adspath";
        public string msexchuseraccountcontrol = "msexchuseraccountcontrol";
        //public string "defender-usertokendata";	
        public string accountexpires = "accountexpires";
        public string displayname = "displayname";
        public string employeenumber = "employeenumber";
        public string primarygroupid = "primarygroupid";
        public string streetaddress = "streetaddress";
        public string badpwdcount = "badpwdcount";
        public string objectclass = "objectclass";
        public string objectcategory = "objectcategory";
        public string instancetype = "instancetype";
        public string homedrive = "homedrive";
        public string samaccounttype = "samaccounttype";
        public string homedirectory = "homedirectory";
        public string whencreated = "whencreated";
        public string lastlogon = "lastlogon";
        public string L = "l";
        public string useraccountcontrol = "useraccountcontrol";
        public string Co = "co";
        public string C = "c";
        public string samaccountname = "samaccountname";
        public string Sn = "sn";
        public string GivenName = "givenname";
        public string Mail = "mail";
        public string msexchmailboxsecuritydescriptor = "msexchmailboxsecuritydescriptor";
        public string objectsid = "objectsid";
        public string lockouttime = "lockouttime";
        public string homemta = "homemta";
        public string mobile = "mobile";
        public string description = "description";
        public string msexchmailboxguid = "msexchmailboxguid";
        public string scriptpath = "scriptpath";
        public string pwdlastset = "pwdlastset";
        public string manager = "manager";
        public string logoncount = "logoncount";
        public string Cn = "cn";
        public string codepage = "codepage";
        public string name = "name";
        public string usnchanged = "usnchanged";
        public string legacyexchangedn = "legacyexchangedn";
        public string proxyaddresses = "proxyaddresses";
        public string department = "department";
        public string userprincipalname = "userprincipalname";
        public string badpasswordtime = "badpasswordtime";
        public string employeeid = "employeeid";
        public string title = "title";
        //public string "defender-lockouttime";	
        public string mdbusedefaults = "mdbusedefaults";
        public string telephonenumber = "telephonenumber";
        public string showinaddressbook = "showinaddressbook";
        public string msexchpoliciesincluded = "msexchpoliciesincluded";
        public string textencodedoraddress = "textencodedoraddress";
        public string lastlogontimestamp = "lastlogontimestamp";
        #endregion

        #region Variables
        private SearchResult _filterAttribute;
        private DirectorySearcher _search;
        #endregion

        public Ldap()
        {
        }

        #region Methods
        public bool IsAuthenticated(string domain, string username, string pwd)
        {
            string _path = "LDAP://" + domain;
            string domainAndUsername = domain + @"\" + username;
            DirectoryEntry entry = new DirectoryEntry(_path, domainAndUsername, pwd);

            try
            {
                DirectorySearcher search = new DirectorySearcher(entry);
                search.Filter = "(SAMAccountName=" + username + ")";
                SearchResult result = search.FindOne();
                if (result == null) return false;

                //fail try
                //string[] a = new string[result.Properties.Count];
                //ArrayList al = new ArrayList();
                //result.Properties.CopyTo(a, 0);
                //foreach (string s in a)
                //    System.Diagnostics.Debug.WriteLine(s);

                // Update the new path to the user in the directory
                _path = result.Path;
                _search = search;
                _filterAttribute = result;

                GetEmployeeInfo(this);
                return true;
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message);
            }
        }
        private void GetEmployeeInfo(Ldap sender)
        {
            try
            {
                sender.postalcode = (_filterAttribute.Properties["postalcode"].Count > 0) ? _filterAttribute.Properties["postalcode"][0].ToString() : string.Empty;
                sender.GivenName = (_filterAttribute.Properties["GivenName"].Count > 0) ? _filterAttribute.Properties["GivenName"][0].ToString() : string.Empty;
                sender.Sn = (_filterAttribute.Properties["Sn"].Count > 0) ? _filterAttribute.Properties["Sn"][0].ToString() : string.Empty;
                sender.displayname = (_filterAttribute.Properties["displayname"].Count > 0) ? _filterAttribute.Properties["displayname"][0].ToString() : string.Empty;
                sender.Mail = (_filterAttribute.Properties["Mail"].Count > 0) ? _filterAttribute.Properties["Mail"][0].ToString() : string.Empty;
                sender.streetaddress = (_filterAttribute.Properties["streetaddress"].Count > 0) ? _filterAttribute.Properties["streetaddress"][0].ToString() : string.Empty;
                sender.telephonenumber = (_filterAttribute.Properties["telephonenumber"].Count > 0) ? _filterAttribute.Properties["telephonenumber"][0].ToString() : string.Empty;
                sender.mobile = (_filterAttribute.Properties["mobile"].Count > 0) ? _filterAttribute.Properties["mobile"][0].ToString() : string.Empty;
                sender.facsimiletelephonenumber = (_filterAttribute.Properties["facsimiletelephonenumber"].Count > 0) ? _filterAttribute.Properties["facsimiletelephonenumber"][0].ToString() : string.Empty;
                sender.title = (_filterAttribute.Properties["title"].Count > 0) ? _filterAttribute.Properties["title"][0].ToString() : string.Empty;
                sender.department = (_filterAttribute.Properties["department"].Count > 0) ? _filterAttribute.Properties["department"][0].ToString() : string.Empty;
                sender.manager = (_filterAttribute.Properties["manager"].Count > 0) ? _filterAttribute.Properties["manager"][0].ToString() : string.Empty;

                //below code fail
                //System.Reflection.PropertyInfo[] infos = this.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                //foreach (System.Reflection.PropertyInfo info in infos)
                //{
                //    if (_filterAttribute.Properties[info.Name].Count > 0)
                //        info.SetValue(this, _filterAttribute.Properties[info.Name].ToString(), null);
                //}
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                throw ex;
            }
        }
        public Ldap GetEmployeeByName(string employeeName)
        {
            Ldap output = new Ldap();
            DirectorySearcher search = _search;
            search.Filter = "(CN=" + employeeName + ")";
            _filterAttribute = search.FindOne();
            if (_filterAttribute == null) return null;
            GetEmployeeInfo(output);
            return output;
        }
        public string[] GetOtherEmployee(string employeeName)
        {
            if (_search == null) return new string[] { };

            DirectorySearcher search = _search;
            search.Filter = "(CN=" + employeeName + ")";
            SearchResult result = search.FindOne();
            if (result == null) return new string[] { };
            return GetEmployeeInfo(result);
        }
        private string[] GetEmployeeInfo(SearchResult result)
        {
            string[] z = new string[7];
            z[0] = (result.Properties["CN"].Count > 0) ? result.Properties["CN"][0].ToString() : "";
            z[1] = (result.Properties["initials"].Count > 0) ? result.Properties["initials"][0].ToString() : "";
            z[2] = (result.Properties["mail"].Count > 0) ? result.Properties["mail"][0].ToString() : "";
            z[3] = (result.Properties["title"].Count > 0) ? result.Properties["title"][0].ToString() : "";
            z[4] = (result.Properties["department"].Count > 0) ? result.Properties["department"][0].ToString() : "";

            Regex manRegex = new Regex("^CN=([^,]+),(.*)$");
            z[5] = (result.Properties["manager"].Count > 0) ? manRegex.Replace(result.Properties["manager"][0].ToString(), "$1") : "";
            string[] x = new string[result.Properties["directreports"].Count];
            for (int i = 0; i < result.Properties["directreports"].Count; i++)
                x[i] = manRegex.Replace(result.Properties["directreports"][i].ToString(), "$1");
            z[6] = string.Join(";", x);
            string y = ShowRs(result);

            return z;
        }
        private string ShowRs(SearchResult result)
        {
            string x = "";
            foreach (string value in result.Properties.PropertyNames)
                x += value + ":" + result.Properties[value][0].ToString() + "\n";
            return x;
        }
        /// <summary>
        /// Return all domain list within company (without full name - plexus.com).
        /// </summary>
        /// <returns></returns>
        /// <seealso>http://www.highorbit.co.uk/?p=310</seealso>
        /// <remarks>
        /// <b>Author</b>	yeang-shing.then<br/>
        /// <b>Since</b>	2009-06-03<br/>
        /// </remarks>
        public static string[] GetAllDomainList()
        {
            string[] output = new string[] { };
            ArrayList al = new ArrayList();

            try
            {
                // Connect to RootDSE
                DirectoryEntry RootDSE = new DirectoryEntry("LDAP://rootDSE");

                // Retrieve the Configuration Naming Context from RootDSE
                string configNC = RootDSE.Properties["configurationNamingContext"].Value.ToString();

                // Connect to the Configuration Naming Context
                DirectoryEntry configSearchRoot = new DirectoryEntry("LDAP://" + configNC);

                // Search for all partitions where the NetBIOSName is set.
                DirectorySearcher configSearch = new DirectorySearcher(configSearchRoot);
                configSearch.Filter = ("(NETBIOSName=*)");

                // Configure search to return dnsroot and ncname attributes
                configSearch.PropertiesToLoad.Add("dnsroot");
                configSearch.PropertiesToLoad.Add("ncname");
                SearchResultCollection forestPartitionList = configSearch.FindAll();

                // Loop through each returned domain in the result collection
                foreach (SearchResult domainPartition in forestPartitionList)
                {
                    // domainName like "domain.com". ncName like "DC=domain,DC=com"
                    string domainName = domainPartition.Properties["dnsroot"][0].ToString();
                    string ncName = domainPartition.Properties["ncname"][0].ToString();
                    domainName = domainName.Replace("plexus.com", string.Empty);
                    domainName = domainName.Replace(".", string.Empty);
                    if (domainName.Length > 0) al.Add(domainName);
                }

                output = new string[al.Count];
                for (int i = 0; i < output.Length; i++)
                    output[i] = al[i].ToString();

                return output;
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(NetUtility), ex);
                throw ex;
            }
        }
        #endregion
    }
}