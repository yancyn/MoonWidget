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
using System.Diagnostics;

namespace PlexusWPF
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    /// <remarks>
    /// TODO: Add auto scrolling feature
    /// </remarks>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <seealso>http://nishantrana.wordpress.com/2009/03/26/using-hyperlink-in-wpf-application/</seealso>
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
