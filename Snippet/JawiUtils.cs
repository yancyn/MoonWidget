using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;

namespace Snippet
{
    public class JawiUtils
    {
        public JawiUtils()
        {
        }

        /// <summary>
        /// Make jawi name split into disticnt root word together with jawi word only.
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="outputFileName"></param>
        /// <returns>Rows count otherwise -1 if error found.</returns>
        public int SplitIntoRoot(string sourceFile, string outputFileName)
        {
            int count = 0;
            DataSet dataset = new DataSet();

            try
            {
                if (!System.IO.File.Exists(sourceFile)) return count;
                dataset.ReadXml(sourceFile);
                if (dataset.Tables.Count == 0) return count;

                #region Split into ArrayList
                string[] names = new string[] { };//holding rumi purpose
                string[] jawis = new string[] { };//holding jawi purpose
                ArrayList list = new ArrayList();
                //ArrayList list = new ArrayList();

                System.Diagnostics.Debug.WriteLine(System.DateTime.Now.ToString() + " start dump into table..");
                DataTable table = CreateJawiRootSchema();
                foreach (DataRow row in dataset.Tables[0].Rows)
                {
                    names = row["name"].ToString().ToLower().Split(new char[] { ' ' });
                    jawis = row["jawi"].ToString().ToLower().Split(new char[] { ' ' });
                    if (names.Length == 1) continue;
                    if (jawis.Length == 1) continue;
                    if (names.Length != jawis.Length) continue;//data imcomplete

                    for (int i = 0; i < names.Length; i++)
                    {
                        //exception case
                        if (names[i].Equals("bin")) continue;
                        if (names[i].Equals("bt")) continue;
                        if (names[i].Equals("hj")) continue;
                        if (names[i].Equals("hjh")) continue;

                        //System.Diagnostics.Debug.WriteLine(names[i] + ", " + jawis[i]);
                        JawiWord jawi = new JawiWord(names[i], jawis[i]);
                        list.Add(jawi);
                    }//end loops
                }//end loops

                System.Diagnostics.Debug.WriteLine(System.DateTime.Now.ToString() + " start sorting..");
                BubbleSort(list);
                System.Diagnostics.Debug.WriteLine(System.DateTime.Now.ToString() + " end sorting.");
                #endregion

                #region clean up for distinct, not duplicate
                System.Diagnostics.Debug.WriteLine(System.DateTime.Now.ToString() + " start cleaning..");
                string hold = string.Empty;
                for (int i = 1; i < list.Count; i++)
                {
                    hold = (list[i - 1] as JawiWord).Rumi;
                    if (!hold.Equals((list[i] as JawiWord).Rumi))
                    {
                        DataRow newRow = table.NewRow();
                        newRow["rumi"] = (list[i - 1] as JawiWord).Rumi;
                        newRow["jawi"] = (list[i - 1] as JawiWord).Jawi;
                        table.Rows.Add(newRow);
                        count++;
                    }
                }//end loops
                System.Diagnostics.Debug.WriteLine(System.DateTime.Now.ToString() + " done clean up.");
                #endregion

                table.AcceptChanges();
                dataset = new DataSet();
                dataset.Tables.Add(table);
                dataset.WriteXml(outputFileName);

                return count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return -1;
            }
            finally { dataset.Dispose(); }
        }
        public int DirectoryIntoRoot(string sourceDirectory, string outputFileName)
        {
            int count = 0;
            DataSet dataset = new DataSet();

            try
            {
                System.Diagnostics.Debug.WriteLine(System.DateTime.Now.ToString() + " start dump..");
                string[] origin = GetFileList(sourceDirectory);
                if (origin.Length == 0) return count;

                ArrayList list = new ArrayList();
                foreach (string s in origin)
                {
                    string[] names = s.ToLower().TrimEnd(new char[] { 's', 'f', '.' }).Split(new char[] { ' ' });
                    if (names.Length == 1) continue;
                    for (int i = 0; i < names.Length; i++)
                    {
                        //exception case
                        if (names[i].Equals("bin")) continue;
                        if (names[i].Equals("bt")) continue;
                        if (names[i].Equals("hj")) continue;
                        if (names[i].Equals("hjh")) continue;

                        JawiWord jawi = new JawiWord(names[i], string.Empty);
                        list.Add(jawi);
                    }//end loops
                }//end loops

                BubbleSort(list);

                #region clean up for distinct, not duplicate
                System.Diagnostics.Debug.WriteLine(System.DateTime.Now.ToString() + " start cleaning..");
                DataTable table = CreateJawiRootSchema();
                string hold = string.Empty;
                for (int i = 1; i < list.Count; i++)
                {
                    hold = (list[i - 1] as JawiWord).Rumi;
                    if (!hold.Equals((list[i] as JawiWord).Rumi))
                    {
                        DataRow newRow = table.NewRow();
                        newRow["rumi"] = (list[i - 1] as JawiWord).Rumi;
                        newRow["jawi"] = (list[i - 1] as JawiWord).Jawi;
                        table.Rows.Add(newRow);
                        count++;
                    }
                }//end loops
                System.Diagnostics.Debug.WriteLine(System.DateTime.Now.ToString() + " done clean up.");
                #endregion

                table.AcceptChanges();
                dataset = new DataSet();
                dataset.Tables.Add(table);
                dataset.WriteXml(outputFileName);

                return count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return -1;
            }
            finally { dataset.Dispose(); }
        }
        private DataTable CreateJawiRootSchema()
        {
            DataTable output = new DataTable("rootname");

            DataColumn col1 = new DataColumn("rumi", typeof(string));
            DataColumn col2 = new DataColumn("jawi", typeof(string));
            output.Columns.Add(col1);
            output.Columns.Add(col2);
            output.AcceptChanges();

            return output;
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
        /// <summary>
        /// Sort the element of an array with buble sort.
        /// </summary>
        /// <param name="sender"></param>
        public void BubbleSort(ArrayList sender)
        {
            for (int i = 1; i < sender.Count; i++)
            {
                for (int j = 0; j < sender.Count - 1; j++)
                {
                    if (sender[j].GetType() == typeof(JawiWord))
                    {
                        if((sender[j] as JawiWord).Rumi.CompareTo((sender[j+1] as JawiWord).Rumi)>0)
                        //if (sender[j].CompareTo(sender[j + 1]) > 0)
                            Swap(sender, j, j + 1);
                    }
                }//end loops
            }//end loops
        }
        private void Swap(ArrayList sender, int first, int second)
        {
            if (sender[first].GetType() == typeof(JawiWord))
            {
                JawiWord jawi = sender[first] as JawiWord;
                sender[first] = sender[second];
                sender[second] = jawi;
            }        
        }

    }//end class

    public class JawiWord
    {
        private string rumi;
        public string Rumi
        {
            get { return this.rumi; }
            set { this.rumi = value; }
        }
        private string jawi;
        public string Jawi
        {
            get { return this.jawi; }
            set { this.jawi = value; }
        }
        public JawiWord(string rumi, string jawi)
        {
            this.rumi = rumi;
            this.jawi = jawi;
        }
    }//end jawi class
}
