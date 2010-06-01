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
using System.Resources;

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
            //KeyboardWrapper keyboard = new KeyboardWrapper(InputMethod.Changjie);
            //Console.WriteLine(keyboard.Dictionary.Count);
            //foreach (string word in keyboard.SearchByKey("ab"))
            //    textBlock1.Text += word;

            //replace redundant \r and \n
            //System.IO.StreamReader streamReader = new System.IO.StreamReader("gb.txt");
            //string all = streamReader.ReadToEnd();
            //all = all.Replace("\r", "");
            //all = all.Replace("\n\n", "\n");
            //System.Diagnostics.Debug.WriteLine(all);

           
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            KeyboardWrapper keyboard = new KeyboardWrapper(InputMethod.GB);
            textBlock1.Text = keyboard.Dictionary[textBox1.Text][0];

            //ResourceManager manager = ResourceManager.CreateFileBasedResourceManager("gb.resx",
            //        System.AppDomain.CurrentDomain.BaseDirectory.ToString(), null);
            //textBlock1.Text = manager.GetString(textBox1.Text);
        }
    }
}