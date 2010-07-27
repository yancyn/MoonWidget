using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Snippet
{
    /// <summary>
    /// Find the most frequence type character for English or Chinese.
    /// </summary>
    public class CharacterFrequency
    {
        //KeyValuePair<string, int> Frequencies { get; set; }
        Dictionary<string, int> Frequencies { get; set; }
        /// <summary>
        /// Constructor for import a text file only.
        /// </summary>
        /// <param name="fileName"></param>
        public CharacterFrequency(string fileName)
        {
            this.Frequencies = new Dictionary<string, int>();
            //this.Frequencies = new KeyValuePair<string,int>();
            StreamReader streamReader = new StreamReader(fileName);
            string content = streamReader.ReadToEnd();
            foreach (char character in content)
            {
                if (!this.Frequencies.ContainsKey(character.ToString()))
                    this.Frequencies.Add(character.ToString(), 1);
                else
                {
                    //int frequency = this.Frequencies[character.ToString()];
                    //this.Frequencies[character.ToString()] = frequency + 1;
                    this.Frequencies[character.ToString()]++;
                }
            }
        }
        /// <summary>
        /// Generate the dictionary output.
        /// </summary>
        public void PrintOutput()
        {
            foreach (KeyValuePair<string, int> item in this.Frequencies)
                System.Diagnostics.Debug.WriteLine(String.Format("{0}\t{1}", item.Key, item.Value));
        }
    }
}