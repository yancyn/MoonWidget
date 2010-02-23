using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KeyboardVisualizer
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            KeyboardWrapper keyboard = new KeyboardWrapper(InputMethod.Changjie);
            //foreach (KeyValuePair<string, IList<string>> item in keyboard.Dictionary)
            //{
            //    string value = string.Empty;
            //    foreach (string s in item.Value)
            //        value += s + ',';
            //    Console.WriteLine("{0}: {1}", item.Key, value.TrimEnd(new char[] { ',' }));
            //}
            Console.WriteLine(keyboard.Dictionary.Count);
            foreach (string word in keyboard.SearchByKey("ab"))
                textBlock1.Text += word;
        }
    }
}
