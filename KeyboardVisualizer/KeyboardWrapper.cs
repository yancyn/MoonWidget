using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KeyboardVisualizer
{
    public enum InputMethod
    {
        Dvorak,
        Changjie,
        Pingyin,
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
                            if (!this.Dictionary.ContainsKey(pair[pair.Length - 1]))
                                this.Dictionary.Add(pair[pair.Length - 1], new List<string> { pair[0] });
                            else
                                this.Dictionary[pair[pair.Length - 1]].Add(pair[0]);
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