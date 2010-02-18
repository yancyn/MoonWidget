using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

namespace Snippet
{
    public class RenameFiler
    {
        private string source;
        public string Destination { get; set; }
        /// <summary>
        /// A filer clone from a single location then merge all files into a new destination
        /// with uniqued file name.
        /// </summary>
        /// <param name="source">A folder location read from.</param>
        /// <remarks>
        /// <b>Author</b>	yeang-shing.then<br/>
        /// <b>Since</b>	2010-02-18<br/>
        /// </remarks>
        public RenameFiler(string source)
        {
            this.source = source;
        }
        public void Start()
        {
            if (string.IsNullOrEmpty(this.Destination)) return;

            DirectoryInfo directoryInfo = new DirectoryInfo(this.source);
            FileInfo[] files = directoryInfo.GetFiles("*.fs");
            WriteFile(files);

            DirectoryInfo[] subDirectories = directoryInfo.GetDirectories();
            foreach (DirectoryInfo d in subDirectories)
            {
                FileInfo[] fs = d.GetFiles("*.fs");
                WriteFile(fs);
            }
        }
        /// <summary>
        /// Rename to desired naming convention.
        /// </summary>
        /// <remarks>
        /// <example>
        /// <code>
        /// RenameFiler filer = new RenameFiler(@"G:\Backups\JawiLib");
        /// filer.Destination = @"G:\Backups\JawiLib2";
        /// filer.Rename();
        /// </code>
        /// </example>
        /// </remarks>
        public void Rename()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(this.source);
            FileInfo[] files = directoryInfo.GetFiles();
            foreach (FileInfo f in files)
            {
                string fileNameOnly = f.Name.TrimEnd(new char[] { '.', 'F', 'S' });
                string newFileName = RenameJawi(fileNameOnly);
                try
                {
                    f.CopyTo(this.Destination + "\\" + newFileName + f.Extension.ToLower());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(f.Name);
                    System.Diagnostics.Debug.WriteLine(ex);
                    continue;
                }
            }
        }
        /// <summary>
        /// Rename to desired jawi name according to shortcut naming convention.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private string RenameJawi(string sender)
        {
            string result = sender.ToLower();
            result = result.Replace(".", "");
            result = result.Replace("b.", "bin");
            result = result.Replace("haji", "hj");
            result = result.Replace("hajjah", "hjh");
            result = result.Replace("hajah", "hjh");
            result = result.Replace("binti", "bt");
            result = result.Replace("binte", "bt");
            return result;
        }
        private void WriteFile(FileInfo[] files)
        {
            foreach (FileInfo f in files)
            {
                string newFileName = f.Name.ToLower();
                newFileName = newFileName.Replace(".", "");
                newFileName = this.Destination + newFileName;

                bool exist = false;
                for (int j = 0; j < 10; j++)
                {
                    int i = 1;//duplicated file indicator
                    exist = Exist(newFileName, ref i);
                    if (exist)
                        newFileName += "2";
                    else
                        break;
                }
                if (!exist) File.Copy(f.FullName, newFileName);
            }
        }
        private bool Exist(string sender, ref int copies)
        {
            bool exist = false;
            exist = File.Exists(sender);
            if (exist) copies++;
            return exist;
        }

        private DataTable CreateSchema()
        {
            DataTable output = new DataTable();
            DataColumn col1 = new DataColumn("FileName", typeof(string));
            DataColumn col2 = new DataColumn("Size", typeof(long));
            output.Columns.Add(col1);
            output.Columns.Add(col2);
            output.AcceptChanges();
            return output;
        }
        public DataTable GetFileInfo()
        {
            DataTable output = CreateSchema();
            DirectoryInfo directoryInfo = new DirectoryInfo(this.source);
            FileInfo[] files = directoryInfo.GetFiles();
            foreach (FileInfo f in files)
            {
                DataRow row = output.NewRow();
                row[0] = f.Name;
                row[1] = f.Length;
                output.Rows.Add(row);
            }
            output.AcceptChanges();
            return output;
        }
    }
}