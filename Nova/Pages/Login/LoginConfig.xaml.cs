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

namespace Nova.Pages.Login
{
    /// <summary>
    /// Lógica de interacción para LoginConfig.xaml
    /// </summary>
    public partial class LoginConfig : Page
    {
        

        public LoginConfig()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Setup configuration loaded

            //API path
            APIIPTextbox.Text = DataConfig.LocalAPI;

            try
            {
                BranchCB.Items.Clear();
            }
            catch (Exception) { }

            //Branch
            if (NovaAPI.APIStatus.Success != null)
            {
                BranchCB.ItemsSource = NovaAPI.APIStatus.Branch;
                BranchCB.SelectedIndex = 0;
            }
            else
            {
                //CHECK API STATUS
                try
                {
                    NovaAPI.APIStatus.Branch.Clear(); //Clear data
                }
                catch (Exception){}

                bool status = await NovaAPI.APIStatus.GetValues(APIIPTextbox.Text);

                if (status)
                {
                    //API CONNECTED
                    APIIPTextbox.BorderBrush = (SolidColorBrush)Application.Current.TryFindResource("PassBrush");
                    BranchCB.ItemsSource = NovaAPI.APIStatus.Branch;
                    BranchCB.SelectedIndex = 0;
                }
                else
                {
                    //API DISCONNECTED
                    APIIPTextbox.BorderBrush = (SolidColorBrush)Application.Current.TryFindResource("ErrorBrush");
                }
            }

            //Updates and remote
            UpdatesCB.IsChecked = DataConfig.LocalUpdates;
            CloudCB.IsChecked = DataConfig.CloudUpdates;

            CheckConfig.IsEnabled = true;
        }

        private async void CheckConfig_Click(object sender, RoutedEventArgs e)
        {
            CheckConfig.IsEnabled = false;

            try
            {
                NovaAPI.APIStatus.Branch.Clear();
            }
            catch (Exception) { }

            //CHECK API STATUS
            bool status = await NovaAPI.APIStatus.GetValues(APIIPTextbox.Text);

            if (status)
            {
                //API CONNECTED
                APIIPTextbox.BorderBrush = (SolidColorBrush)Application.Current.TryFindResource("PassBrush");
                try
                {
                    BranchCB.Items.Clear();
                }
                catch (Exception){ }

                BranchCB.ItemsSource = NovaAPI.APIStatus.Branch;
                BranchCB.SelectedIndex = 0;                
            } else
            {
                //API DISCONNECTED
                APIIPTextbox.BorderBrush = (SolidColorBrush)Application.Current.TryFindResource("ErrorBrush");
            }

            CheckConfig.IsEnabled = true;
        }

        /// <summary>
        /// SAVE CONFIG BUTTON
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveBT_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //CONFIGURATIONS PARAMETERS
                DataConfig.LocalAPI = APIIPTextbox.Text;
                DataConfig.WorkPlaceID = NovaAPI.APIStatus.Branch[BranchCB.SelectedIndex].BranchID;
                DataConfig.WorkPlaceLabel = $"{NovaAPI.APIStatus.OwnerName} - {NovaAPI.APIStatus.Branch[BranchCB.SelectedIndex].BranchName}";
                DataConfig.LocalUpdates = UpdatesCB.IsChecked.Value;
                DataConfig.CloudUpdates = CloudCB.IsChecked.Value;
                DataConfig.WorkPointID = WorkPointCB.SelectedItem != null ? ((NovaAPI.APIStatus.box)WorkPointCB.SelectedItem).BoxID : 0;                

                //Save config file
                DataConfig.SaveConfig();
            }
            catch (Exception)
            {
                SaveBT.Background = (SolidColorBrush)Application.Current.TryFindResource("ErrorBrush");
                return;
            }           

            SaveBT.Background = (SolidColorBrush)Application.Current.TryFindResource("PassBrush");
        }

        private void BranchCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Data = new List<NovaAPI.APIStatus.box>();

            WorkPointCB.ItemsSource = Data;           

            try
            {
                for (int i = 0; i < NovaAPI.APIStatus.BoxData.Count; i++)
                {
                    if (NovaAPI.APIStatus.BoxData[i].BoxBranch == NovaAPI.APIStatus.Branch[BranchCB.SelectedIndex].BranchID.ToString())
                    {
                        Data.Add(NovaAPI.APIStatus.BoxData[i]);
                    }
                }
            }
            catch (Exception ex) { }

            WorkPointCB.Items.Refresh();
        }
    }
}
