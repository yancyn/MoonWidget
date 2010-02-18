using System;
//using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using Plexus.ERP;

namespace PlexusWPF
{
    /// <summary>
    /// Interaction logic for RulePage.xaml
    /// </summary>
    public partial class RulePage : Window
    {
        public string FileName { get; set; }
        public RulePage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <seealso>http://msdn.microsoft.com/en-us/library/system.text.encoding.ascii.aspx</seealso>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitialLayout();
        }
        private ItemCollection itemCollection;
        public ItemCollection ItemCollection
        {
            get { return this.itemCollection; }
        }
        private void InitialLayout()
        {
            AddNewRule();

            //create collection for Lookup fields
            AddComboBoxItems(this.comboBox2);

            //create collection of column
            //this.itemCollection = new ObservableCollection<string>();//new List<string>();
            //for (int i = 0; i < 26; i++)
            //{
            //    Encoding ascii = Encoding.ASCII;//hack: convert ascii code to corresponding ascii letter
            //    string s = "Column";
            //    s += ascii.GetString(new byte[] { Convert.ToByte(65 + i) });// is "A" \u0041
            //    this.itemCollection.Add(s);
            //}
        }
        private void AddNewRule()
        {
            //ExcelRuleCreator ruleCreator = new ExcelRuleCreator();
            //if (this.itemsControl1.ItemsSource != null)
            //    ruleCreator.Rules = this.itemsControl1.ItemsSource as List<DiffRule>;
            //ExcelDiffRule rule = new ExcelDiffRule();
            //rule.Id = ruleCreator.Rules.Count + 1;
            //ruleCreator.Rules.Add(rule);
            //this.itemsControl1.ItemsSource = ruleCreator.Rules;
        }
        private void AddComboBoxItems(ComboBox sender)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < 26; i++)
            {
                Encoding ascii = Encoding.ASCII;//hack: convert ascii code to corresponding ascii letter
                string s = "Column";
                s += ascii.GetString(new byte[] { Convert.ToByte(65 + i) });// is "A" \u0041
                sender.Items.Add(s);
            }
        }
        private void AND_Click(object sender, RoutedEventArgs e)
        {
            AddNewRule();
        }
    }
}
