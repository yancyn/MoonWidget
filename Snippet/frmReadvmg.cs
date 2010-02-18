using System;
//using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Snippet
{
    public partial class frmReadvmg : Form
    {
        public frmReadvmg()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
                Vmg vmg = new Vmg(textBox2.Text.Trim(), textBox1.Text);
                dataGridView1.DataSource = vmg.Table;
            }
        }


    }//end form

    public class Vmg
    {
        private string folderPath;
        public string FolderPath
        {
            get { return this.folderPath; }
        }
        private string sender;
        public string Sender
        {
            get { return this.sender; }
        }
        private TimeSpan timeZone;
        private DataTable table;
        public DataTable Table
        {
            get { return this.table; }
            set { this.table = value; }
        }

        public Vmg(string sender, string path)
        {
            this.sender = sender;
            this.folderPath = path;
            this.timeZone = System.TimeZone.CurrentTimeZone.GetUtcOffset(System.DateTime.Now);
        
            CreateSchema();
            Read();
            Save();
        }
        private void CreateSchema()
        {
            this.table = new DataTable("sms");
            DataColumn col1 = new DataColumn("From", typeof(String));
            DataColumn col2 = new DataColumn("To", typeof(String));
            DataColumn col3 = new DataColumn("At", typeof(DateTime));
            DataColumn col4 = new DataColumn("Content", typeof(String));
            this.table.Columns.Add(col1);
            this.table.Columns.Add(col2);
            this.table.Columns.Add(col3);
            this.table.Columns.Add(col4);

            this.table.AcceptChanges();
        }
        private void Read()
        {
            System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(this.folderPath);
            System.IO.FileInfo[] lFiles = d.GetFiles();
            foreach (System.IO.FileInfo f in lFiles)
            {
                System.IO.StreamReader objReader = new System.IO.StreamReader(f.FullName);
                string sLine = string.Empty;
                string output = string.Empty;
                while (sLine != null)
                {
                    sLine = objReader.ReadLine();
                    if (sLine != null)
                        output += sLine;
                }
                objReader.Close();

                output = output.Replace("\0", "");//key
                Convert(output);
            }//end loops

            this.table.AcceptChanges();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <remarks>
        /// ZX-MESSAGE-TYPE:
        /// SUBMITBEGIN = sent
        /// DELIVERBEGIN = received
        /// <br/>
        /// TEL:+60127799600END:
        /// TEL:END:
        /// <br/>
        /// VCARDBEGIN:VENVBEGIN:VBODY
        /// END:VBODYEND:VENVEND:VENVEND:VMSG
        /// </remarks>
        private void Convert(string sender)
        {
            DataRow newRow = this.table.NewRow();
            string key = "SUBMITBEGIN";
            int i = -1;
            int j = -1;
            if (sender.IndexOf(key) > -1)
            {
                newRow["From"] = this.sender;
                key = "TEL:";
                i = sender.IndexOf(key);
                newRow["To"] = Chop(sender.Substring(i + key.Length, sender.Length - i - key.Length), "END");
            }
            else
            {
                newRow["To"] = this.sender;
                key = "TEL:";
                i = sender.IndexOf(key);
                newRow["From"] = Chop(sender.Substring(i + key.Length, sender.Length - i - key.Length), "END");
            }

            key = "VCARDBEGIN:VENVBEGIN:VBODY";
            i = sender.IndexOf(key);

            key = "END:VBODYEND:VENVEND:VENVEND:VMSG";
            j = sender.IndexOf(key);

            //Date time format fit in 19 length
            string content = sender.Substring(i + 19, sender.Length - i - 19 - 15 - j);
            newRow["At"] = System.Convert.ToDateTime(content.Substring(5, 19)).Add(this.timeZone);
            newRow["Content"] = content.TrimStart(new char[] { 'D', 'a', 't', 'e', ':' });

            this.table.Rows.Add(newRow);
        }
        private string Chop(string sender, string key)
        {
            string output = string.Empty;
            int i = sender.IndexOf(key);
            output = sender.Substring(0, i);

            return output;
        }
        public bool Save()
        {
            bool success = false;
            DataSet dataset = new DataSet();
            DataTable hold = this.table.Clone();
            DataView dataview = this.table.DefaultView;

            try
            {
                dataview.Sort = "At";
                for (int i = 0; i < dataview.Count; i++)
                {
                    DataRow newRow = hold.NewRow();
                    newRow["From"] = dataview[i]["From"];
                    newRow["To"] = dataview[i]["To"];
                    newRow["At"] = dataview[i]["At"];
                    newRow["Content"] = dataview[i]["Content"];
                    hold.Rows.Add(newRow);
                }//end loops
                hold.AcceptChanges();
                dataset.Tables.Add(hold);
                dataset.WriteXml("sms.xml");

                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return success;
            }
            finally
            {
                dataset.Dispose();
                dataview.Dispose();
                hold.Dispose();
            }
        }
    }//end class
}//end namespace