using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;

namespace Snippet
{
    public partial class Form1 : Form
    {
        //private DataTable table;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Start: " + System.DateTime.Now);
            KoreanCast wrapper = new KoreanCast();
            wrapper.Rename(textBox1.Text);
            System.Diagnostics.Debug.WriteLine("End: " + System.DateTime.Now);
        }

        /// <summary>
        /// Get a list of file name inside a specified directory.
        /// </summary>
        /// <param name="path">A directory location.</param>
        /// <returns>A collection of file name.</returns>
        private string[] GetFileList(string path)
        {
            string[] list = new string[] { };
            DirectoryInfo d = new DirectoryInfo(path);
            FileInfo[] lFiles = d.GetFiles();

            list = new string[lFiles.Length];
            for (int i = 0; i < lFiles.Length; i++)
                list[i] = lFiles[i].Name;

            return list;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            JawiUtils util = new JawiUtils();
            util.SplitIntoRoot(textBox2.Text.Trim(), "JawiName.xml");
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                textBox2.Text = openFileDialog1.FileName;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            JawiUtils util = new JawiUtils();
            util.DirectoryIntoRoot(textBox1.Text, "JawiName2.xml");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                #region Merge
                //DataSet dataset = new DataSet();
                //DataSet dataset2 = new DataSet();
                //dataset.ReadXml("C:\\Documents and Settings\\yeanthen\\Desktop\\JawiName.xml");
                //dataset.ReadXml("C:\\Documents and Settings\\yeanthen\\Desktop\\JawiName2.xml");
                //dataset2.Merge(dataset);//lousy without sorting
                //dataset2.AcceptChanges();
                //dataset2.WriteXml("C:\\Documents and Settings\\yeanthen\\Desktop\\JawiName3.xml");
                #endregion

                #region Sort
                DataView dataview = new DataView();
                DataSet dataset3 = new DataSet();
                dataset3.ReadXml("C:\\Documents and Settings\\yeanthen\\Desktop\\JawiName3.xml");
                dataview = dataset3.Tables[0].DefaultView;
                dataview.Sort = "rumi";

                DataTable table = dataset3.Tables[0].Clone();
                for (int i = 0; i < dataview.Count; i++)
                {
                    DataRow newRow = table.NewRow();
                    for (int j = 0; j < dataview.Table.Columns.Count; j++)
                        newRow[j] = dataview[i][j];
                    table.Rows.Add(newRow);
                }//end loops
                dataset3.Tables.RemoveAt(0);//must

                DataSet dataset4 = new DataSet();
                dataset4.Tables.Add(table);
                dataset4.AcceptChanges();
                dataset4.WriteXml("C:\\Documents and Settings\\yeanthen\\Desktop\\JawiName4.xml");
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
        }
    }//end form
}//end namespace