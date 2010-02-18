using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data;

namespace Snippet
{
    public partial class frmPing : Form
    {
        /// <summary>
        /// Get the server prefix, server full name, ip, lost percentage, and average response time only.
        /// </summary>
        public frmPing()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Read a source file.
        /// </summary>
        /// <param name="filePath">Specify a file name with full path.</param>
        /// <see>http://msdn2.microsoft.com/en-us/library/9kstw824.aspx</see>
        /// <seealso>
        /// http://msdn2.microsoft.com/en-us/library/db5x7c0d.aspx
        /// http://msdn2.microsoft.com/en-us/library/system.io.streamreader.aspx
        /// </seealso>
        private void ReadFile(string filePath)
        {
            System.IO.StreamReader reader = System.IO.File.OpenText(filePath);
            //or reader.ReadToEnd() for whole textfile
            while (reader.Peek() > 0)
            {
                string line = reader.ReadLine();
                System.Diagnostics.Debug.WriteLine(reader.ReadLine());
            }
        }
        /// <summary>
        /// If is a new header block.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns>True if is a header otherwise false.</returns>
        private bool IsHeader(string sender)
        {
            //means
            //if (sender.IndexOf("Ping") > -1)
            //    return true;
            //else
            //    return false;
            return (sender.IndexOf("Pinging") > -1) ? true : false;
        }
        /// <summary>
        /// Get the rest of info. Server name, ip, etc.
        /// </summary>
        /// <param name="sender">DataRow to carry on</param>
        /// <param name="source">Input text line.</param>
        private void GetInfo(DataRow sender, string source)
        {
            try
            {
                string prefix = string.Empty;//is better use string.Empty than ""
                string serverName = string.Empty;
                string ip = string.Empty;
                int lostValue = 0;
                int average = 0;

                if (source.IndexOf("Pinging") > -1)
                {
                    int index = source.IndexOf("Pinging");
                    source = source.Replace("Pinging ", "");//get rid of "Ping "
                    int firstDot = source.IndexOf('.');
                    prefix = source.Substring(0, firstDot);

                    //if there is a 'rpm' server type
                    if (prefix.IndexOf('-') > -1)
                        prefix = prefix.Replace("-rpm", "");

                    //get ip address
                    int start = source.IndexOf('[');
                    int end = source.IndexOf(']');
                    ip = source.Substring(start + 1, end - start - 1);

                    //get server full name
                    serverName = source.Substring(0, start - 1);
                }
                else if (source.IndexOf("%") > -1)
                {
                    int index = source.IndexOf("%");
                    int start = source.IndexOf("(");
                    lostValue = Convert.ToInt32(source.Substring(start + 1, index - start - 1));
                }
                else if (source.IndexOf("Average") > -1)
                {
                    int index = source.LastIndexOf("=");
                    int end = source.LastIndexOf("ms");
                    average = Convert.ToInt32(source.Substring(index + 1, end - index - 1));
                }

                if (prefix != "") sender["Prefix"] = prefix;
                if (serverName != "") sender["ServerName"] = serverName;
                if (ip != "") sender["IP"] = ip;
                if (lostValue > 0) sender["PacketLost"] = lostValue;
                if (average > 0) sender["TripAverage"] = average;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
        }
        /// <summary>
        /// Get a DataTable.
        /// </summary>
        /// <param name="fileName">Source of file.</param>
        /// <returns></returns>
        private DataTable Manipulate(string fileName)
        {
            DataTable table = new DataTable();

            try
            {
                //create table schema
                DataColumn col1 = new DataColumn("Prefix", typeof(string));
                DataColumn col2 = new DataColumn("ServerName", typeof(string));
                DataColumn col3 = new DataColumn("IP", typeof(string));
                DataColumn col4 = new DataColumn("PacketLost", typeof(int));
                DataColumn col5 = new DataColumn("TripAverage", typeof(int));
                table.Columns.Add(col1);
                table.Columns.Add(col2);
                table.Columns.Add(col3);
                table.Columns.Add(col4);
                table.Columns.Add(col5);

                //for generate row in datatable purpose
                DataRow r = table.NewRow();//default declaration
                bool isEndBlock = false;//mark if there is an end of block

                //read from source file
                System.IO.StreamReader reader = System.IO.File.OpenText(fileName);
                while (reader.Peek() > 0)
                {
                    string line = reader.ReadLine();
                    if (IsHeader(line))
                    {
                        //if there not empty DataRow then add into table
                        //if (r["IP"].ToString().CompareTo(string.Empty) > 0)
                        if (r != null)
                            table.Rows.Add(r);
                        r = table.NewRow();
                        r["Prefix"] = string.Empty;
                        r["ServerName"] = string.Empty;
                        r["IP"] = string.Empty;
                        r["PacketLost"] = 0;
                        r["TripAverage"] = 0;
                    }

                    //get neccessary info, may do nothing also
                    GetInfo(r, line);
                }//end loops

                //add last row
                if (r != null)
                    table.Rows.Add(r);

                table.AcceptChanges();
                return table;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return table;
            }
        }
        /// <summary>
        /// Writo a file into csv format with seperator comma.
        /// </summary>
        /// <param name="table"></param>
        private void ExportToCSV(DataTable table)
        {
            try
            {
                string seperator = ",";
                System.IO.StreamWriter writer = new System.IO.StreamWriter("pingresult.csv");
                writer.WriteLine("Prefix" + seperator + "ServerName" + seperator + "IP" + seperator + "PacketLost" + seperator + "TripAverage");
                foreach (DataRow r in table.Rows)
                    writer.WriteLine(r["Prefix"].ToString() + seperator + r["ServerName"].ToString() + seperator + r["IP"] + seperator + r["PacketLost"].ToString() + seperator + r["TripAverage"].ToString());
                writer.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
                DataTable table = Manipulate(openFileDialog1.FileName);
                dataGridView1.DataSource = table;
                ExportToCSV(table);
            }
        }
    }
}//end namepace since 20080426tys