using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data;
using System.Linq;
using System.IO;
using System.Text;
using System.Globalization;
using System.Configuration;

namespace Snippet
{
    static class Program
    {
        static StringBuilder sb;
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

            //20100721
            //CharacterFrequency characterFinder = new CharacterFrequency("sms.txt");
            //characterFinder.PrintOutput();

            //2010-08-24
            //SourceCounter counter = new SourceCounter(@"G:\My Projects\LeanQS\src\LeanQualityTool");
            //counter.SetSkipFolder(new string[] { ".svn", "obj", "bin", "Properties" });
            //counter.SetSkipExtension(new string[] { ".csproj" });
            //counter.Start();
            //foreach (SourceFile item in counter.Items)
            //    System.Diagnostics.Debug.WriteLine(String.Format("{0}\t{1}", item.Name, item.TotalLines));

            //requested by Nikki 2011-10-03
            FolderPrinter printer = new FolderPrinter();
            printer.Lookup(0, ConfigurationSettings.AppSettings["snapshot_uri"]);
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