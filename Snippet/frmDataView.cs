using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace Snippet
{
    public partial class frmDataView : Form
    {
        private DataView dataview;
        public frmDataView()
        {
            InitializeComponent();
        }

        #region Methods
        private void InitialLayout()
        {
            textBox2.Text = "";
            comboBox1.Items.Clear();
            listBox1.Items.Clear();
            button3.Text = "\u03A3";//sigma
            label2.Text = "0 counts";
        }
        private void BindDataView(string fileName)
        {
            DataSet dataset = new DataSet();

            try
            {
                dataset.ReadXml(fileName);
                dataview = dataset.Tables[0].DefaultView;
                dataGridView1.DataSource = dataview;
                dataview.Sort = "";
                Filter("");
                CheckDuplicate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());
                return;
            }
        }
        private void CheckDuplicate()
        {
            if (dataview == null) return;

            string hold = dataview[0][0].ToString();
            DataGridViewCellStyle style = new DataGridViewCellStyle();
            style.ForeColor = Color.Red;

            for (int i = 1; i < dataview.Count; i++)
            {
                //is duplicate
                if (hold == dataview[i][0].ToString())
                {
                    System.Diagnostics.Debug.WriteLine(dataview[i][0].ToString());
                    dataGridView1[0,i].Style = style;
                }
                else
                    hold = dataview[i][0].ToString();
            }//end loops

            if (hold == dataview[dataview.Count - 1][0].ToString())
            {
                System.Diagnostics.Debug.WriteLine(dataview[dataview.Count - 1][0].ToString());
                dataGridView1[0,dataview.Count - 1].Style = style;
            }

        }
        private void Filter(string query)
        {
            try
            {
                dataview.RowFilter = query;
                label1.Text = dataview.Count + " rows count";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());
                return;
            }
        }
        private decimal Sum(string columnName)
        {
            decimal output = 0m;

            try
            {
                for (int i = 0; i < dataview.Count; i++)
                    output += (dataview[i][columnName] == DBNull.Value) ? 0 : Convert.ToDecimal(dataview[i][columnName]);

                return output;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());
                return output;
            }
        }
        private void BindColumnList()
        {
            if (dataview == null) return;

            comboBox1.Items.Clear();
            for (int i = 0; i < dataview.Table.Columns.Count; i++)
                comboBox1.Items.Add(dataview.Table.Columns[i].ColumnName);
        }
        private int Distinct(string columnName)
        {
            int count = 0;
            if (dataview == null) return count;

            try
            {
                listBox1.Items.Clear();
                string hold = string.Empty;

                dataview.Sort = columnName;
                for (int i = 0; i < dataview.Count; i++)
                {
                    if (hold != dataview[i][columnName].ToString())
                    {
                        hold = dataview[i][columnName].ToString();
                        listBox1.Items.Add(hold);
                        count++;
                    }
                }//end loops

                label2.Text = count + " counts";
                return count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());
                return count;
            }
        }
        #endregion

        #region Events
        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
                InitialLayout();

                Cursor.Current = Cursors.WaitCursor;
                BindDataView(openFileDialog1.FileName);
                Cursor.Current = Cursors.Default;
                BindColumnList();                
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Filter(textBox2.Text);
            Cursor.Current = Cursors.Default;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            button3.Text = Sum(comboBox1.Text).ToString();
            Cursor.Current = Cursors.Default;
        }
        private void button4_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Distinct(comboBox1.Text);
            Cursor.Current = Cursors.Default;
        }
        #endregion

   

       

    }//end winform
}//end namespace since 20080210tys