using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Snippet
{
    /// <summary>
    /// Set option for word scales like count on word or character.
    /// </summary>
    public enum WordScalesOption
    {
        Character,
        Word,
    }
    /// <summary>
    /// A scales for weighing any latin word frequency(English, Malay etc but NOT Chinese).
    /// </summary>
    /// <remarks>
    /// <example>
    /// <code>
    /// StreamReader streamReader = new StreamReader("doc.txt");
    /// WordScales wordScales = new WordScales();
    /// wordScales.Option = WordScalesOption.Character;
    /// wordScales.Count(streamReader);
    /// foreach (KeyValuePair&lt;string, int&gt; item in wordScales.Result)
    ///     Console.WriteLine("{0}\t{1}", item.Key, item.Value);
    /// </code>
    /// </example>
    /// <b>Author</b>	yeang-shing.then<br/>
    /// <b>Since</b>	2010-02-23<br/>
    /// todo: maybe can include NOT case sensitive in calculation.
    /// </remarks>
    public class WordScales
    {
        #region Properties
        /// <summary>
        /// Gets or sets option for word weighing calculation.
        /// </summary>
        public WordScalesOption Option { get; set; }
        private Dictionary<string, int> result;
        /// <summary>
        /// Return the word count result.
        /// </summary>
        public Dictionary<string, int> Result { get { return this.result; } }
        /// <summary>
        /// A space character for to determine word by word.
        /// </summary>
        public const char WordSplitter = ' ';
        #endregion
        /// <summary>
        /// Default contructor.
        /// </summary>
        public WordScales()
        {
            //default
            this.Option = WordScalesOption.Character;
            this.result = new Dictionary<string, int>();
        }
        #region Methods
        /// <summary>
        /// Count the text stream to produce the word(character) frequency result.
        /// </summary>
        /// <param name="content"></param>
        /// <remarks>
        /// <example>
        /// <code>
        ///    WordScales wordScales = new WordScales();
        ///    wordScales.Option = WordScalesOption.Word;
        ///
        ///    string content = string.Empty;
        ///    DirectoryInfo directoryInfo = new DirectoryInfo(@"F:\jawiname");
        ///    FileInfo[] files = directoryInfo.GetFiles();
        ///    foreach (FileInfo file in files)
        ///        content += file.Name.ToLower() + " ";
        ///    wordScales.Count(content);
        ///    foreach (KeyValuePair&lt;string, int&gt; item in wordScales.Result)
        ///        Console.WriteLine("{0}\t{1}", item.Key, item.Value);
        /// </code>
        /// </example>
        /// </remarks>
        public void Count(string content)
        {
            this.result = new Dictionary<string, int>();
            if (this.Option == WordScalesOption.Character)
            {
                foreach (char c in content)
                {
                    if (!this.result.ContainsKey(c.ToString()))
                        this.result.Add(c.ToString(), 1);
                    else
                        this.result[c.ToString()]++;
                }
            }
            else if (this.Option == WordScalesOption.Word)
            {
                //we only care about human readable character which is A-Z,a-z,0-9.
                //remove punctuation
                foreach (byte b in Encoding.ASCII.GetBytes(content))
                {
                    if (!IsAlphaNumeric(b))
                    {
                        char[] nonsense = Encoding.ASCII.GetChars(new byte[] { b });
                        foreach (char c in nonsense)
                            content = content.Replace(c, WordSplitter);
                    }
                }

                string[] words = content.Split(new char[] { WordSplitter });
                foreach (string word in words)
                {
                    if (!this.result.ContainsKey(word))
                        this.result.Add(word, 1);
                    else
                        this.result[word]++;
                }
            }
        }
        /// <summary>
        /// Count the text stream to produce the word(character) frequency result.
        /// </summary>
        /// <param name="streamReader"></param>
        public void Count(StreamReader streamReader)
        {
            Count(streamReader.ReadToEnd());
        }
        /// <summary>
        /// Return true if it is an alphanumeric character.
        /// TODO: try regex approach.
        /// </summary>
        /// <param name="ascii"></param>
        /// <returns></returns>
        private bool IsAlphaNumeric(byte ascii)
        {
            if (ascii >= 48 && ascii <= 57) return true;//0-9
            if (ascii >= 65 && ascii <= 90) return true;//A-Z
            if (ascii >= 97 && ascii <= 122) return true;//a-z
            return false;
        }
        #endregion
    }
}