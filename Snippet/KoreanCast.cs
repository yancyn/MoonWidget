using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Data;

namespace Snippet
{
    public class KoreanCast
    {
        public DataTable table;
        public KoreanCast()
        {
            InitialTable();
        }

        public void Rename(string sourcePath)
        {
            try
            {
                table.Clear();

                System.IO.DirectoryInfo dinfo = new System.IO.DirectoryInfo(sourcePath);
                System.IO.FileInfo[] finfos = dinfo.GetFiles();

                foreach (System.IO.FileInfo f in finfos)
                {
                    string fileNameOnly = GetFileNameOnly(f.Name);
                    string newFileName = CastTo(fileNameOnly);

                    string newLocation = sourcePath + "2" + "\\" + newFileName + f.Extension;
                    if (!System.IO.File.Exists(newLocation))
                    {
                        f.CopyTo(newLocation);
                        //f.Delete();
                    }
                }//end loops

                SaveToXml("KoreanCompany2.xml");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return;
            }
        }
        public string GetFileNameOnly(string sender)
        {
            string output = sender;
            int i = sender.LastIndexOf(".");

            output = sender.Substring(0, i);
            return output;
        }
        /// <summary>
        /// Case file name into format [Company Name][space][OCN#][space][Street No].extension.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public string CastTo(string sender)
        {
            string output = sender;

            try
            {
                string company = string.Empty;
                string koreancom = string.Empty;
                int ocn = 0;
                string no = string.Empty;
                int c1 = -1;
                int c2 = -1;
                int k1 = -1;
                int k2 = -1;
                int o1 = -1;
                int o2 = -1;
                int n1 = -1;
                int n2 = -1;

                //odd case
                int k3 = sender.IndexOf("(주)");//mean (Ltd)

                int i = -1;
                foreach (char c in sender)
                {
                    i++;
                    //System.Diagnostics.Debug.Write(c);
                    #region get english name index
                    if (c1 == -1 && char.IsLetter(c))
                        c1 = i;
                    //if (c1 > -1 && char.IsLetter(c))
                    //    c2 = i;
                    #endregion

                    #region get ocn index
                    if (o1 == -1 && char.IsNumber(c))
                        o1 = i;
                    if (o2 == -1 && o1 > -1 && !char.IsNumber(c))
                        o2 = i - 1;
                    #endregion

                    #region get korean name index
                    if (k3 == -1)
                    {
                        if (k1 == -1 && char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherLetter)
                            k1 = i;
                        //if (k2 == -1 && k1 > -1 && char.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.OtherLetter)
                        //    k2 = i - 1;
                    }
                    else
                    {
                        //in this situation  k2-4 > k1
                        if (k1 == -1 && char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherLetter)
                            k1 = Math.Min(i, k3);
                        //if (i > k1 + 3)
                        //{
                        //    if (k2 > -1 && char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherLetter)
                        //        k2 = -1;
                        //    if (k2 == -1 && k1 > -1 && char.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.OtherLetter)
                        //        k2 = i - 1;
                        //}
                    }
                    #endregion

                    #region get street no index
                    if (c1 > -1 && k1 > -1 && o2 > -1)
                    {
                        if (n1 == -1 && char.IsNumber(c))
                            n1 = i;
                    }
                    #endregion

                    #region when there is last char
                    if (i == sender.Length - 1)
                    {
                        if (c1 > -1 && o1 > -1) c2 = Math.Min(o1, k1) - 1;
                        if (o2 == -1) o2 = i;

                        if (o1 > k1) //if ocn behind korean name
                            k2 = o1 - 1;
                        else if (n1 > k1) //if street no behind korean name
                            k2 = n1 - 2;
                        else
                            k2 = i;

                        if (n1 > -1)
                            n2 = i - 1;
                    }
                    #endregion

                }//end loops

                if (c1 > -1 && c2 > -1) company = sender.Substring(c1, c2 - c1 + 1);
                if (k1 > -1 && k2 > -1) koreancom = sender.Substring(k1, k2 - k1 + 1);
                ocn = Convert.ToInt32(sender.Substring(o1, o2 - o1 + 1));
                if (n1 > -1 && n2 > -1) no = sender.Substring(n1, n2 - n1 + 1);

                if (company.CompareTo(string.Empty) > 0 && koreancom.CompareTo(string.Empty) > 0 && ocn > 0)
                {
                    if (no.Length == 0)
                        //output = company.Trim() + " " + koreancom.Trim() + " " + ocn.ToString();//must have IIS 6 above
                        //output = company.Trim() + " " + ocn.ToString();//due to IIS 5.1
                        output = ocn.ToString() + "_" + company.Trim();//due to IIS 5.1
                    else
                        //output = company.Trim() + " " + koreancom.Trim() + " " + ocn.ToString() + " [" + no.Trim() + "]";
                        output = ocn.ToString() + "_" + company.Trim() + " [" + no.Trim() + "]";
                    AddIntoTable(company.Trim(), koreancom.Trim(), ocn, no.Trim());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(sender);
                System.Diagnostics.Debug.WriteLine(ex);
                return output;
            }

            return output;
        }
        public bool IsInteger(string val)
        {
            if (val.Trim().Length == 0) return false;

            /* foreach(char c in val)
            {
                bool lb_Valid = false;
                if(c.ToString() == "0" || c.ToString() == "1" || c.ToString() == "2"
                    || c.ToString() == "3" || c.ToString() == "4" || c.ToString() == "5"
                    || c.ToString() == "6" || c.ToString() == "7" || c.ToString() == "8"
                    || c.ToString() == "9" )
                {
                    lb_Valid = true;
                    continue;
                }

                if(!lb_Valid)
                    return false;//not a valid integer found
            }//end loops string

            return true; */

            Regex objNotIntPattern = new Regex("[^0-9-]");
            Regex objIntPattern = new Regex("^-[0-9]+$|^[0-9]+$");
            return !objNotIntPattern.IsMatch(val) && objIntPattern.IsMatch(val);
        }

        private void InitialTable()
        {
            table = new DataTable("Company");
            DataColumn col = new DataColumn("CompanyName", typeof(System.String));
            DataColumn col2 = new DataColumn("NLSCompanyName", typeof(System.String));
            DataColumn col3 = new DataColumn("OCN", typeof(System.Int64));
            DataColumn col4 = new DataColumn("OSN", typeof(System.Int64));

            DataColumn col5 = new DataColumn("Address1", typeof(System.String));
            DataColumn col6 = new DataColumn("Address2", typeof(System.String));
            DataColumn col7 = new DataColumn("Address3", typeof(System.String));
            DataColumn col8 = new DataColumn("Address4", typeof(System.String));
            DataColumn col9 = new DataColumn("City", typeof(System.String));
            DataColumn col10 = new DataColumn("State", typeof(System.String));
            DataColumn col11 = new DataColumn("NLSAddress1", typeof(System.String));
            DataColumn col12 = new DataColumn("NLSAddress2", typeof(System.String));
            DataColumn col13= new DataColumn("NLSAddress3", typeof(System.String));
            DataColumn col14= new DataColumn("NLSAddress4", typeof(System.String));
            DataColumn col15= new DataColumn("NLSCity", typeof(System.String));
            DataColumn col16 = new DataColumn("NLSState", typeof(System.String));
            table.Columns.Add(col);
            table.Columns.Add(col2);
            table.Columns.Add(col3);
            table.Columns.Add(col4);
            table.Columns.Add(col5);
            table.Columns.Add(col6);
            table.Columns.Add(col7);
            table.Columns.Add(col8);
            table.Columns.Add(col9);
            table.Columns.Add(col10);
            table.Columns.Add(col11);
            table.Columns.Add(col12);
            table.Columns.Add(col13);
            table.Columns.Add(col14);
            table.Columns.Add(col15);
            table.Columns.Add(col16);
            table.AcceptChanges();
        }
        private void AddIntoTable(string company, string koreanCom, int ocn, string no)
        {
            DataRow row = table.NewRow();
            row["CompanyName"] = company;
            row["NLSCompanyName"] = koreanCom;
            row["OCN"] = ocn;
            row["Address1"] = no;
            table.Rows.Add(row);
        }
        public bool SaveToXml(string fileName)
        {
            try
            {
                //table.AcceptChanges();
                DataSet dataset = new DataSet();
                if (System.IO.File.Exists(fileName))
                    dataset.ReadXml(fileName, XmlReadMode.InferTypedSchema);
                else
                    dataset.Tables.Add(table);

                dataset.Merge(table, true, MissingSchemaAction.Ignore);
                dataset.AcceptChanges();

                dataset.WriteXml(fileName);
                dataset.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return false;
            }

            return true;
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
    }//end class
}
