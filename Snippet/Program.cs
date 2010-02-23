using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data;
using System.Linq;
using System.IO;

namespace Snippet
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new frmPing());//080426tys

            //tracker#10712 - Master Item owned by lc.ho@plexus.com
            //ExcelWrapper wrapper = new ExcelWrapper();
            //wrapper.Execute();

            //100218tys
            //RenameFiler filer = new RenameFiler(@"F:\JawiName");
            //filer.Destination = @"G:\Backups\JawiLib2";
            //filer.Rename();
            //DataSet dataSet = new DataSet();
            //dataSet.Tables.Add(filer.GetFileInfo());
            //dataSet.WriteXml("result.xml");

            //20100223tys
            //WordScales wordScales = new WordScales();
            //wordScales.Option = WordScalesOption.Word;

            //string content = string.Empty;
            //DirectoryInfo directoryInfo = new DirectoryInfo(@"F:\jawiname");
            //FileInfo[] files = directoryInfo.GetFiles();
            //foreach (FileInfo file in files)
            //    content += file.Name.ToLower() + " ";
            ////StreamReader streamReader = new StreamReader("doc.txt");
            //wordScales.Count(content);
            //foreach (KeyValuePair<string, int> item in wordScales.Result)
            //    Console.WriteLine("{0}\t{1}", item.Key, item.Value);
        }

        /// <summary>
        /// Return date time from a timestamp value of UNIX or Oracle
        /// </summary>
        /// <param name="timestamp">A timestamp value of UNIX or Oracle data</param>
        /// <returns>A .NET or MSSQL date time format</returns>
        /// <remarks>
        /// <b>Author</b>   yeang-shing.then<br/>
        /// <b>Since</b>    2009-09-10<br/>
        /// </remarks>
        public static DateTime ToDateTime(double timestamp)
        {
            DateTime result = new DateTime(1970, 1, 1);
            return result.AddSeconds(timestamp);
        }
    }
}