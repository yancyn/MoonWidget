using System;
using System.IO;
using System.Collections.Generic;

namespace Snippet
{
    /// <summary>
    /// A utility to count how many line in the file and total files under the folder.
    /// Display statistic for a software project.
    /// </summary>
    /// <remarks>
    /// 2010-08-24: yeang-shing.then
    /// </remarks>
    public class SourceCounter
    {
        private List<SourceFile> items;
        public List<SourceFile> Items { get { return this.items; } }
        private string path;
        private List<string> skippedFolders;
        private List<string> skippedFiles;
        private List<string> skippedExtensions;

        public SourceCounter(string path)
        {
            this.path = path;
            this.items = new List<SourceFile>();
            this.skippedFolders = new List<string>();
            this.skippedFiles = new List<string>();
            this.skippedExtensions = new List<string>();
        }

        public void SetSkipFolder(string[] folders)
        {
            this.skippedFolders = new List<string>();
            foreach (string folder in folders)
                this.skippedFolders.Add(folder);
        }
        public void SetSkipFile(string[] files)
        {
            this.skippedFiles = new List<string>();
            foreach (string file in files)
                this.skippedFiles.Add(file);
        }
        public void SetSkipExtension(string[] extensions)
        {
            this.skippedExtensions = new List<string>();
            foreach (string extension in extensions)
                this.skippedExtensions.Add(extension);
        }
        public void Start()
        {
            Read(this.path);
        }
        private void Read(string path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (this.skippedFolders.Contains(directoryInfo.Name)) return;

            FileInfo[] fileInfos = directoryInfo.GetFiles();
            foreach (FileInfo info in fileInfos)
            {
                if (this.skippedExtensions.Contains(info.Extension)) continue;
                if (this.skippedFiles.Contains(info.Name)) continue;

                string[] contents = File.ReadAllLines(info.FullName);
                this.items.Add(new SourceFile(info.FullName, contents.Length));
            }

            DirectoryInfo[] directoriesInfo = directoryInfo.GetDirectories();
            foreach (DirectoryInfo info in directoriesInfo)
            {
                if (this.skippedFolders.Contains(info.Name)) continue;
                Read(info.FullName);
            }
        }
    }
    public class SourceFile
    {
        private string name;
        public string Name { get { return this.name; } }
        private int totalLines;
        public int TotalLines { get { return this.totalLines; } }
        public SourceFile(string name, int totalLines)
        {
            this.name = name;
            this.totalLines = totalLines;
        }
    }
}