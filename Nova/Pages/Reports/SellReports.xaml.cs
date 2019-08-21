using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Nova.Pages.Reports
{
    /// <summary>
    /// Lógica de interacción para SellReports.xaml
    /// </summary>
    public partial class SellReports : Page
    {
        public SellReports()
        {
            Cache.Cache.SelectedSellReport = "0";

            InitializeComponent();            
        }

        private void InventoryTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Check tab selection
            if (e.Source is TabControl && Page.IsLoaded == true)
            {
                if (BoxTab.IsSelected)
                {
                    Cache.Cache.SelectedSellReport = "0";
                    BoxReportsFrame.Source = new Uri("SellReportsPages/Page.xaml", UriKind.Relative);

                } else if (SellTab.IsSelected)
                {
                    Cache.Cache.SelectedSellReport = "1";
                    SellReportsFrame.Source = new Uri("SellReportsPages/Page.xaml", UriKind.Relative);
                }
            }           
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //SELECT BoxTab
            BoxReportsFrame.Source = new Uri("SellReportsPages/Page.xaml", UriKind.Relative);
        }
    }
}
