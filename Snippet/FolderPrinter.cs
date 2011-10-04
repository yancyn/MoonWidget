using System;
using System.Text;
using System.IO;

namespace Snippet
{
    /// <summary>
    /// Lookup a folder then print the file name with location.
    /// </summary>
    /// <remarks>
    /// Requested by Nikki at 2011-10-04.
    /// <example>
    /// <code>
    /// FolderPrinter printer = new FolderPrinter();
    /// printer.Lookup(0, ConfigurationSettings.AppSettings["snapshot_uri"]);
    /// </code>
    /// </example>
    /// </remarks>
    public class FolderPrinter
    {
        private StringBuilder sb;
        public FolderPrinter()
        {
            sb = new StringBuilder();
        }
        private void Print(int indent, DirectoryInfo info)
        {
            string tab = "|-";
            for (int i = 0; i < indent; i++)
                tab += "-";
            sb.AppendLine(string.Format(tab + "[{0}]", info.FullName));
            //System.Diagnostics.Debug.WriteLine(string.Format(tab + "[{0}]", info.FullName));//info.Name

            try
            {
                foreach (FileInfo fileInfo in info.GetFiles())
                    sb.AppendLine(tab + fileInfo.Name);
                //System.Diagnostics.Debug.WriteLine(tab + fileInfo.Name);
                foreach (DirectoryInfo directoryInfo in info.GetDirectories())
                    Print(indent++, directoryInfo);
            }
            catch (System.UnauthorizedAccessException ex)
            {
                sb.AppendLine("... no permission");
                return;
            }
        }
        private void Print()
        {
            StreamWriter writer = new StreamWriter("result.txt");
            writer.Write(sb.ToString());
            writer.Close();
        }
        /// <summary>
        /// Start lookup the folder.
        /// </summary>
        /// <remarks>TODO: if unauthorized folder?</remarks>
        /// <param name="indent"></param>
        /// <param name="path"></param>
        public void Lookup(int indent, string path)
        {
            sb.AppendLine(string.Format("[{0}]", path));
            //System.Diagnostics.Debug.WriteLine(string.Format("[{0}]", path));

            try
            {
                DirectoryInfo info = new DirectoryInfo(path);
                foreach (FileInfo fileInfo in info.GetFiles())
                    sb.AppendLine(fileInfo.Name);
                //System.Diagnostics.Debug.WriteLine(tab + fileInfo.Name);

                foreach (DirectoryInfo directoryInfo in info.GetDirectories())
                    Print(indent++, directoryInfo);
            }
            //catch (System.Security.SecurityException ex)
            catch (System.UnauthorizedAccessException ex)
            {
                sb.AppendLine("... no permission");
                return;
            }

            Print();
        }
    }
}