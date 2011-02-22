using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.IO;

namespace PingWriter
{
    /// <summary>
    /// A ping writer to monitor streamyx activities.
    /// This will log into a file.
    /// </summary>
    /// <remarks>
    /// Since 2011-02-16.
    /// </remarks>
    public partial class Form1 : Form
    {
        private Configuration config;
        private string fileName = "log.csv";
        private char seperator = ',';
        public Form1()
        {
            InitializeComponent();

            config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            timer1.Interval = Convert.ToInt32(config.AppSettings.Settings["interval"].Value);
        }
        /// <summary>
        /// Get ping echo time.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <seealso>http://www.dotnetspider.com/resources/28741-Check-URL-States-Using-Ping-Command.aspx</seealso>
        public long PingTime(string url)
        {
            long time = -1;
            Ping info = new Ping();
            try
            {
                PingReply reply = info.Send(url);
                if (reply.Status == IPStatus.Success)
                    time = reply.RoundtripTime;
                return time;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return -1;
            }
        }
        /// <summary>
        /// Check whether server is alive or not.
        /// </summary>
        /// <remarks>
        /// This method is faster than call linq.DatabaseExist.
        /// </remarks>
        /// <param name="hostName"></param>
        /// <param name="port">Sql server port depends on configuration. Normally is 1433 for MSSQL.</param>
        /// <returns></returns>
        /// <see>http://www.eggheadcafe.com/community/aspnet/2/10009861/how-to-check-the-database.aspx</see>
        public bool IsServerAlive(string hostName, int port)
        {
            bool alive = false;

            try
            {
                //TCP Ping method. This rely on port setting
                IPHostEntry host = new IPHostEntry();
                host = Dns.GetHostEntry(hostName);
                if (host.AddressList.Length > 0)
                {
                    IPAddress address = host.AddressList[0];
                    TcpClient tcp = new TcpClient();
                    tcp.Connect(address, port);
                    alive = tcp.Connected;
                    tcp.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return false;
            }

            return alive;
        }
        /// <summary>
        /// Write a line into file.
        /// </summary>
        /// <param name="elapsed"></param>
        /// <seealso>http://msdn.microsoft.com/en-us/library/3zc0w663.aspx</seealso>
        private void WriteToFile(long elapsed)
        {
            StreamWriter writer = File.AppendText(fileName);
            writer.WriteLine(DateTime.Now.ToString() + seperator.ToString() + elapsed.ToString());
            writer.Close();
        }
        private void WriteToFile(bool available)
        {
            string zeroOrOne = (available) ? "1" : "0";
            StreamWriter writer = File.AppendText(fileName);
            writer.WriteLine(DateTime.Now.ToString() + seperator.ToString() + zeroOrOne);
            writer.Close();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            //bool available = IsServerAlive(config.AppSettings.Settings["url"].Value, 80);
            //WriteToFile(available);

            string url = config.AppSettings.Settings["url"].Value;
            WriteToFile(PingTime(url));
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            this.Visible = false;p
        }
    }
}