using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data;
using System.Linq;
using System.IO;

namespace Snippet
{
    /// <summary>
    /// Dump into a new target jawiname to a distinct jawi name.
    /// </summary>
    /// <remarks>
    /// <b>Author</b>	yeang-shing.then<br/>
    /// <b>Since</b>	2010-02-22<br/>
    /// </remarks>
    class DumpJawiName
    {
        public DumpJawiName()
        {
            DataSet oriDataSet = new DataSet();
            oriDataSet.ReadXml(@"F:\My Projects\HLGranite\JawiWeb\jawiname.xml");
            DataRow[] oriRows = oriDataSet.Tables[0].Select();


            //target folder
            DataSet newDataSet = new DataSet();
            DataTable table = oriDataSet.Tables[0].Clone();
            DirectoryInfo directoryInfo = new DirectoryInfo("F:\\jawiname");
            FileInfo[] files = directoryInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                string name = file.Name.ToLower();
                name = name.Replace("(", " ");
                name = name.Replace(")", " ");
                name = name.Replace("@", " ");
                string[] names = name.TrimEnd(new char[] { '.', 'f', 's' }).Split(' ');
                foreach (string s in names)
                {
                    //hack: due to replace above cannot works!
                    if (s.Equals("(")) continue;
                    if (s.Equals(")")) continue;
                    if (s.Equals("@")) continue;

                    DataRow row = table.NewRow();
                    row["rumi"] = s;
                    table.Rows.Add(row);
                }
            }
            table.AcceptChanges();
            DataRow[] rows = table.Select()
                .Where(f => f[0].ToString().CompareTo(string.Empty) > 0)
                .OrderBy(f => f[0]).ToArray();
            rows = rows.Distinct(new UniqueStringComparer()).ToArray();
            DataTable newTable = table.Clone();
            foreach (DataRow row in rows)
            {
                DataRow newRow = newTable.NewRow();
                newRow[0] = row[0];
                newTable.Rows.Add(newRow);
            }
            newTable.AcceptChanges();
            newDataSet.Tables.Add(newTable);
            oriDataSet.Merge(newDataSet, true);

            //start cloning..
            DataTable cloneTable = table.Clone();
            DataView dataView = oriDataSet.Tables[0].DefaultView;
            dataView.Sort = "rumi, jawi DESC";
            string hold = string.Empty;
            for (int i = 0; i < dataView.Count; i++)
            {
                if (hold != dataView[i][0].ToString())
                {
                    hold = dataView[i][0].ToString();
                    DataRow row = cloneTable.NewRow();
                    row[0] = dataView[i][0];
                    row[1] = dataView[i][1];
                    row[2] = dataView[i][2];
                    cloneTable.Rows.Add(row);
                }
            }
            cloneTable.AcceptChanges();
            DataSet cloneDataSet = new DataSet();
            cloneDataSet.Tables.Add(cloneTable);
            cloneDataSet.WriteXml("jawiname.xml");
        }
        /// <summary>
        /// A comparer to return unique first column value for a datarow.
        /// </summary>
    }

    class UniqueStringComparer : IEqualityComparer<DataRow>
    {
        #region IEqualityComparer<DataRow> Members
        public bool Equals(DataRow x, DataRow y)
        {
            return x[0].ToString().Equals(y[0].ToString());
        }
        public int GetHashCode(DataRow obj)
        {
            return obj[0].GetHashCode();
        }
        #endregion
    }
}