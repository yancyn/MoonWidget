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

using Microsoft.Win32;
using System.Configuration;
using System.IO;
using System.Data;
using Plexus.Utils;
using Plexus.Utils.ExcelDataReader;
using Plexus.ERP;

namespace PlexusWPF
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        #region Methods and Events
        private void BrowseFile1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog().Value == true) File1.Text = openFile.FileName;
        }
        private void BrowseFile2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog().Value == true) File2.Text = openFile.FileName;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <seealso>http://www.switchonthecode.com/tutorials/wpf-tutorial-using-the-listview-part-1</seealso>
        private void Compare_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                DateTime start, end;
                ExcelDiff wrapper = new ExcelDiff();
                wrapper.IgnoreFirstRow = true;
                wrapper.File1 = new ExcelFile(this.File1.Text);
                wrapper.File2 = new ExcelFile(this.File2.Text);
                wrapper.Read();

                //diff the result and bind into gridview
                Configuration config2 = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                ExcelDiffRuleSection section = config2.GetSection("excelDiffSettings") as ExcelDiffRuleSection;
                for (int i = 0; i < section.Rules.Count; i++)
                {
                    start = DateTime.Now;
                    wrapper.Diff(section.Rules[i]);
                    end = DateTime.Now;
                    Binding binding = new Binding();
                    binding.Source = wrapper.Result;

                    ListView listView = new ListView();
                    GridView gridView = new GridView();
                    for (int j = 0; j < 5; j++)
                    {
                        GridViewColumn column = new GridViewColumn();
                        string header = "Column" + Convert.ToString(j + 1);
                        column.Header = header;
                        column.DisplayMemberBinding = new Binding(header);
                        gridView.Columns.Add(column);
                    }
                    listView.View = gridView;
                    listView.SetBinding(ListView.ItemsSourceProperty, binding);

                    TabItem tab = new TabItem();
                    tab.Header = "Result" + Convert.ToString(i + 1);
                    tab.Content = listView;
                    listView.Tag = (wrapper.Result as IEnumerable<DiffView>).Count().ToString("###,###,###,###") + " records found spent in " + end.Subtract(start).TotalSeconds + " sec";
                    this.TabControl1.Items.Add(tab);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            finally { this.Cursor = null; }
        }
        private void TabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.StatusBar.Content = ((TabControl1.Items[this.TabControl1.SelectedIndex] as TabItem).Content as ListView).Tag.ToString();
        }
        
        private void Mapping_Click(object sender, RoutedEventArgs e)
        {
            Mapping form = new Mapping();
            form.Show();
        }
        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Preference_Click(object sender, RoutedEventArgs e)
        {
            Preference form = new Preference();
            form.ShowDialog();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void About_Click(object sender, RoutedEventArgs e)
        {
            About form = new About();
            form.Show();
        }
        #endregion        
    }

    #region For Style binding purpose (no use temp)
    /// <summary>
    /// Person object
    /// </summary>
    /// <remarks>
    /// <example>
    /// <code>
    ///        List<Person> persons = new List<Person>();
    ///        persons.Add(new Person("Ali", Gender.Male));
    ///        persons.Add(new Person("Emily Tan", Gender.Female));
    ///        persons.Add(new Person("WL Hor", Gender.Male));
    ///        persons.Add(new Person("Teeying Ooi", Gender.Female));
    ///        ListView2.ItemsSource = persons;
    ///
    ///        Binding binding = new Binding();
    ///        binding.Converter = new PersonColorConverter();
    ///        ListView2.SetBinding(ListView.ForegroundProperty, binding);
    ///
    ///            <ListView x:Name="ListView2">
    ///                <ListView.View>
    ///                    <GridView>
    ///                        <GridViewColumn Header="Name" DisplayMemberBinding="{Binding Name}"/>
    ///                    </GridView>
    ///                </ListView.View>
    ///            </ListView>
    /// </code>
    /// </example>
    /// <seealso>http://bradygaster.com/post/WPF-Conditional-DataBinding.aspx</seealso>
    /// </remarks>
    public class Person
    {
        public string Name { get; set; }
        public Gender Gender { get; set; }
        public Person(string name, Gender gender)
        {
            this.Name = name;
            this.Gender = gender;
        }
    }
    public enum Gender
    {
        Male,
        Female,
    }
    public class PersonColorConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return System.Windows.Media.Brushes.Black;
            Gender gender = (Gender)value;
            if (gender == Gender.Male) return System.Windows.Media.Brushes.Blue;
            if (gender == Gender.Female) return System.Windows.Media.Brushes.Red;
            return System.Windows.Media.Brushes.Black;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}