using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Resources;

namespace KeyboardVisualizer
{
    public enum InputMethod
    {
        /// <summary>
        /// Dvorak keyboard.
        /// </summary>
        Dvorak,
        /// <summary>
        /// Changjie.
        /// </summary>
        Changjie,
        /// <summary>
        /// Pinyin.
        /// </summary>
        Pingyin,
        /// <summary>
        /// From unicode to China GB code.
        /// </summary>
        GB,
    }
    public class KeyboardWrapper
    {
        public InputMethod InputMethod { get; set; }
        private Dictionary<string, IList<string>> dictionary;
        public Dictionary<string, IList<string>> Dictionary { get { return this.dictionary; } }

        public KeyboardWrapper(InputMethod input)
        {
            this.dictionary = new Dictionary<string, IList<string>>();
            this.InputMethod = input;
            Initialize();
        }
        private void Initialize()
        {
            if (this.InputMethod == InputMethod.Changjie)
            {
                //Chinese character follow by input keys 1-*
                StreamReader streamReader = new StreamReader("changjie.txt");
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine();
                    string[] pair = line.Split(new char[] { ' ' });
                    if (pair.Length > 1)
                    {
                        try
                        {
                            if (!this.dictionary.ContainsKey(pair[pair.Length - 1]))
                                this.dictionary.Add(pair[pair.Length - 1], new List<string> { pair[0] });
                            else
                                this.dictionary[pair[pair.Length - 1]].Add(pair[0]);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(line);
                            System.Diagnostics.Debug.WriteLine(ex);
                            continue;
                        }
                    }
                }
            }
            else if (this.InputMethod == InputMethod.Pingyin)
            {
            }
            else if (this.InputMethod == InputMethod.Dvorak)
            {
            }
            else if (this.InputMethod == InputMethod.GB)
            {
                ///<seealso>http://www.dreamincode.net/code/snippet1683.htm</seealso>
                //ResourceManager manager = ResourceManager.CreateFileBasedResourceManager("gb.resx",
                //    System.AppDomain.CurrentDomain.BaseDirectory.ToString(), null);
                //manager.GetString();

                ///<seealso>http://www.java2s.com/Tutorial/CSharp/0460__GUI-Windows-Forms/ResXResourceWriterandResXResourceReader.htm</seealso>
                ///resgen gb.resources
                ResourceReader reader = new ResourceReader("gb.resources");
                IDictionaryEnumerator enumerator = reader.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    this.dictionary.Add(enumerator.Key.ToString(),
                        new List<string> { enumerator.Value.ToString() });
                }
            }
        }
        public IList<string> SearchByKey(string key)
        {
            IList<string> result = new List<string>();
            //todo: compose ambigous input based on provided from GUI.
            List<string> ambigous = new List<string> { "ab", "abu", "abt", "abjj", };
            foreach (string s in ambigous)
            {
                result = result.Union(this.dictionary.Where(f => f.Value.Contains(s))
                    .Select(f => f.Key)).ToList<string>();
            }

            return result;
        }
    }
}