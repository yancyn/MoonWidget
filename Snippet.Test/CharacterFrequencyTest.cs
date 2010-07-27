using Snippet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Snippet.Test
{
    /// <summary>
    ///This is a test class for CharacterFrequencyTest and is intended
    ///to contain all CharacterFrequencyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CharacterFrequencyTest
    {
        /// <summary>
        /// A test for PrintOutput to check what Chinese character I type most frequence.
        /// </summary>
        [TestMethod()]
        public void PrintOutputTest()
        {
            string fileName = @"F:\My Projects\Snippet\Snippet.Test\bin\Debug\sms.txt";
            CharacterFrequency target = new CharacterFrequency(fileName);
            target.PrintOutput();
            Assert.IsTrue(true);
        }
    }
}