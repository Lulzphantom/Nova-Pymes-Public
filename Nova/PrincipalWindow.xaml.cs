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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Nova
{
    /// <summary>
    /// Lógica de interacción para PrincipalWindow.xaml
    /// </summary>
    public partial class PrincipalWindow : Window
    {
        public PrincipalWindow()
        {
            InitializeComponent();
            DataContext = new PincipalViewModel();

            Application.Current.MainWindow = this;

            NavigationCommands.BrowseBack.InputGestures.Clear();
            NavigationCommands.BrowseForward.InputGestures.Clear();

            //SET MENU
            Menu1.Visibility = NovaAPI.APIPermissions.RolData[0].permissions.consult == 1 ? Visibility.Visible : Visibility.Collapsed;
            Menu2.Visibility = NovaAPI.APIPermissions.RolData[1].permissions.consult == 1 ? Visibility.Visible : Visibility.Collapsed;
            Menu3.Visibility = NovaAPI.APIPermissions.RolData[2].permissions.consult == 1 ? Visibility.Visible : Visibility.Collapsed;
            Menu4.Visibility = NovaAPI.APIPermissions.RolData[3].permissions.consult == 1 ? Visibility.Visible : Visibility.Collapsed;
            Menu6.Visibility = NovaAPI.APIPermissions.RolData[4].permissions.consult == 1 ? Visibility.Visible : Visibility.Collapsed;
            Menu7.Visibility = NovaAPI.APIPermissions.RolData[5].permissions.consult == 1 ? Visibility.Visible : Visibility.Collapsed;
            Menu5.Visibility = NovaAPI.APIPermissions.RolData[6].permissions.consult == 1 ? Visibility.Visible : Visibility.Collapsed;
        }


        //Menu navigation
        private void Navigate(string uid)
        {
            switch (uid)
            {
                case "1": //Dashboard
                    PageFrame.Source = new Uri("Pages/DashboardPage.xaml", UriKind.Relative);
                    break;
                case "7": //Bill
                    PageFrame.Source = new Uri("Pages/POS/TabedPOSPage.xaml", UriKind.Relative);
                    break;
                case "8": //Bill details
                    PageFrame.Source = new Uri("Pages/POS/POSListPage.xaml", UriKind.Relative);
                    break;
                case "9": //Clients
                    PageFrame.Source = new Uri("Pages/Clients/ClientsPage.xaml", UriKind.Relative);
                    break;
                case "20":
                    PageFrame.Source = new Uri("Pages/Configuration/BranchPage.xaml", UriKind.Relative);
                    break;
                case "21":
                    PageFrame.Source = new Uri("Pages/Configuration/UserRolPage.xaml", UriKind.Relative);
                    break;
                case "22":
                    PageFrame.Source = new Uri("Pages/Configuration/UsersPage.xaml", UriKind.Relative);
                    break;
                case "23":
                    PageFrame.Source = new Uri("Pages/Configuration/ConfigurationPage.xaml", UriKind.Relative);
                    break;
                case "11":
                    PageFrame.Source = new Uri("Pages/Products/SuppliersPage.xaml", UriKind.Relative);
                    break;
                case "12":
                    PageFrame.Source = new Uri("Pages/Products/ProductCategoryPage.xaml", UriKind.Relative);
                    break;
                case "13":
                    PageFrame.Source = new Uri("Pages/Products/InventoryPage.xaml", UriKind.Relative);
                    break;
                case "14": //expends
                   
                    break;
                case "15": //Credits report
                   
                    break;
                case "16": //Sell reports
                    PageFrame.Source = new Uri("Pages/Reports/SellReports.xaml", UriKind.Relative);
                    break;
                case "17": //Expend reports
                    
                    break;
                case "18":
                    
                    break;
                default:
                    break;
            }
        }

        //Menu selected item
        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;

            foreach (TreeViewItem items in PMenu.Items)
            {
                //Collapse non selected menu item
                items.IsExpanded = items.IsSelected == false ? false : true;
            }              
            
            Navigate(item.Uid);         
        }


        //Menu visibility toggle
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MenuContainer.Opacity == 1)
            {
                ToggleImage.Icon = FontAwesome.WPF.FontAwesomeIcon.ToggleOff;
                ToggleBT.ToolTip = "Mostrar menú";
                MenuContainer.BeginStoryboard((Storyboard)Application.Current.TryFindResource("LateralOut"));
            }
            else
            {
                ToggleImage.Icon = FontAwesome.WPF.FontAwesomeIcon.ToggleOn;
                ToggleBT.ToolTip = "Ocultar menú";
                MenuContainer.BeginStoryboard((Storyboard)Application.Current.TryFindResource("LateralIn"));
            }
            
        }

        //LogOut
        private async void TreeViewItem_Selected_1(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Desea cerrar sesion?","Cerrar sesion", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                bool loginstatus = await NovaAPI.APILoginData.GetValues(DataConfig.Username, "", DataConfig.LocalAPI);

                MainWindow Login = new MainWindow();
                Login.Show();
                Close();
            }            
        }
    }

    
}
