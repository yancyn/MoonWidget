using KeyboardVisualizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;

namespace Keyboard.Test
{
    /// <summary>
    /// This is a test class for KeyboardWrapperTest and is intended
    /// to contain all KeyboardWrapperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class KeyboardWrapperTest
    {
        [TestMethod()]
        public void ConvertGBTextToResources()
        {
            //convert gb.txt to gb.resx
            System.IO.StreamReader reader = new System.IO.StreamReader("gb.txt");
            string result = string.Empty;
            int counter = 0;
            DateTime start = DateTime.Now;

            System.IO.StreamWriter writer = new System.IO.StreamWriter("temp.txt");
            while (!reader.EndOfStream)
            {
                counter++;
                string line = reader.ReadLine();
                string[] hold = line.Split(' ');
                string format = "<data name=\"{0}\" xml:space=\"preserve\"><value>{1}</value></data>";
                writer.WriteLine(String.Format(format, hold[1], hold[0]));
            }
            DateTime end = DateTime.Now;

            writer.Flush();
            System.Diagnostics.Debug.WriteLine(counter);
            System.Diagnostics.Debug.WriteLine(end.Subtract(start));
            System.Diagnostics.Debug.WriteLine(result);
        }
        /// <summary>
        /// A test to get GB code from a valid Chinese character.
        /// </summary>
        /// <remarks>
        /// Copy file to test deployment folder.
        /// </remarks>
        /// <seealso>http://stackoverflow.com/questions/227545/how-can-i-get-copy-to-output-directory-to-work-with-unit-tests</seealso>
        [TestMethod()]
        public void GetGBCodeTest()
        {
            KeyboardWrapper target = new KeyboardWrapper(InputMethod.GB);
            string expected = "B0A9";
            string actual = target.Dictionary["癌"][0];
            Assert.AreEqual(expected, actual);

            expected = "B1E6";
            actual = target.Dictionary["辨"][0];
            Assert.AreEqual(expected, actual);

            expected = "C5E1";
            actual = target.Dictionary["裴"][0];
            Assert.AreEqual(expected, actual);
        }
    }
}