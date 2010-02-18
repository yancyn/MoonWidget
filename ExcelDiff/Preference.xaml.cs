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
using System.Windows.Shapes;
using System.Configuration;
using Plexus.ERP;

namespace PlexusWPF
{
    /// <summary>
    /// Interaction logic for Preference.xaml
    /// </summary>
    public partial class Preference : Window
    {
        public Preference()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ///hack: only work after release. Not valid in debug mode
            ///<seealso>http://www.kylirhorton.com/?tag=wpf</seealso>
            ///<seealso>http://weblogs.asp.net/vblasberg/archive/2005/10/27/428738.aspx</seealso>
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["IgnoreFirstRow"].Value = "false";
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            System.Diagnostics.Debug.WriteLine("Customized app config");
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = "Statuses.xml";
            Configuration conf = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            foreach (string s in conf.AppSettings.Settings.AllKeys)
                System.Diagnostics.Debug.WriteLine(s);

            #region App Section
            //read from existing
            //ConfigurationSection section = ExcelDiffSetting.GetConfig();
            ////foreach(int? i in (section as ExcelDiffSetting).Keys)
            ////System.Diagnostics.Debug.WriteLine(i);
            //System.Diagnostics.Debug.WriteLine((section as ExcelDiffSetting).Key);

            ////create a new setting for replace the old value
            //ExcelDiffSetting newSetting = new ExcelDiffSetting();
            //newSetting.Id = 2;
            //newSetting.Key = 30;// s = new List<int?>() { 2, 25, 47 };
            //newSetting.Lookup = 13;

            ////config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //config.Sections.Remove("excelDiffSettings");
            //config.Sections.Add("excelDiffSettings", newSetting);
            //config.Save();
            #endregion

            Configuration config2 = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ExcelDiffRuleSection section = config2.GetSection("excelDiffSettings") as ExcelDiffRuleSection;
            foreach (int i in section.Rules[0].Keys)
                System.Diagnostics.Debug.WriteLine(i);
            System.Diagnostics.Debug.WriteLine(section.Rules[0].Type);
        }
    }
}