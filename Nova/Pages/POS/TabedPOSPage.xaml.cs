using Newtonsoft.Json;
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
using FontAwesome.WPF;

namespace Nova.Pages.POS
{
    /// <summary>
    /// Lógica de interacción para TabedPOSPage.xaml
    /// </summary>
    public partial class TabedPOSPage : Page
    {
        public TabedPOSPage()
        {
            InitializeComponent();
        }

        //POS page loaded
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBoxStatus();
        }

        public string generateID()
        {
            return Guid.NewGuid().ToString("N");
        }

        //Load clients
        private async Task LoadClients()
        {
            bool response = await NovaAPI.APIClient.GetValues("4", DataConfig.LocalAPI);
            if (response)
            {
                Cache.Cache.ClientsDateTime = DateTime.Now;
            }
            else
            {
                string message = NovaAPI.APIClient.Message == "" ? "No hay conexión con el servidor" : NovaAPI.APIClient.Message;
                MessageBox.Show($"Error al cargar los clientes :{message}");
            }
        }


        //Load price category
        private async Task LoadPrices()
        {
            bool response = await NovaAPI.APIPrice.GetValues("4", DataConfig.LocalAPI);
            if (response)
            {
                Cache.Cache.PricesDateTime = DateTime.Now;
            }
            else
            {
                string message = NovaAPI.APIPrice.Message == "" ? "No hay conexión con el servidor" : NovaAPI.APIPrice.Message;
                MessageBox.Show($"Error al cargar los precios :{ message}");
            }
        }

        private async Task CheckAndLoad()
        {

            //Load client data -----------------------------
            if (Cache.Cache.ClientsDateTime != null)
            {
                if (Cache.Cache.ClientsDateTime <= DateTime.Now.AddMinutes(-Cache.Cache.CacheTime))
                {
                    await LoadClients();
                }
            }
            else
            {
                await LoadClients();
            }
            //-----------------------------------------------

            //Load price data -----------------------------
            if (Cache.Cache.PricesDateTime != null)
            {
                if (Cache.Cache.PricesDateTime <= DateTime.Now.AddMinutes(-Cache.Cache.CacheTime))
                {
                    await LoadPrices();
                }
            }
            else
            {
                await LoadPrices();
            }

            //-----------------------------------------------
        }

        //Control move focus
        private void MoveFocus(object sender, KeyEventArgs e)
        {
            //Close on esc press (OpenBoxPopUp)
            if (e.Key == Key.Escape)// && ((Control)sender).Name == "OpenBoxValueTX"
            {
                OpenBoxValueTX.Clear();
                OpenBoxPopUp.IsOpen = false;
                e.Handled = true;
            }

            if (e.Key == Key.Enter)
            {
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }
                e.Handled = true;
            }
        }

        //Load box status
        private async void LoadBoxStatus()
        {
            //Set loading grid visibility
            LoadingGrid.Visibility = Visibility.Visible;
            Spinner.Spin = true;

            var data = new NovaAPI.APIBoxMovements.BoxData
            {
                 id = DataConfig.WorkPointID.ToString()
            };

            //branch json data
            string requestData = JsonConvert.SerializeObject(data);

            bool response = await NovaAPI.APIBoxMovements.GetValues("1", DataConfig.LocalAPI, requestData);

            if (response)
            {
                if (NovaAPI.APIBoxMovements.status == "1")
                {
                    await CheckAndLoad();

                    BoxInformationLB.Visibility = Visibility.Visible;
                    BoxInformationLB.Content = $"Apertura de la caja realizada - {NovaAPI.APIBoxMovements.box_data[0].opendate} - por {NovaAPI.APIBoxMovements.box_data[0].username}";
                    OpenBoxBT.Visibility = Visibility.Collapsed;
                    CloseBoxBT.Visibility = Visibility.Visible;
                    POSTTab.IsEnabled = true;

                    MainFrame.Navigate(new Uri("Pages/POS/TabedPOSContent.xaml", UriKind.Relative));
                }
                else
                {
                    POSTTab.IsEnabled = false;
                    OpenBoxBT.Visibility = Visibility.Visible;
                    CloseBoxBT.Visibility = Visibility.Collapsed;
                    BoxInformationLB.Visibility = Visibility.Collapsed;
                }                
                LoadingGrid.Visibility = Visibility.Collapsed;
                Spinner.Spin = false;
            }
        }

        //New tab button selected
        private void TabItem_Selected(object sender, RoutedEventArgs e)
        {
            NewTabTab.IsSelected = false;
            e.Handled = true;
        }

        //New tab button click
        private void NewTabBT_Click(object sender, RoutedEventArgs e)
        {

            string TabID = generateID();

            //Header content
            StackPanel HeaderContent = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            TextBlock headerText = new TextBlock
            {
                Text = "Factura de venta",
                FontWeight = TabHeaderText.FontWeight
            };

            //Imageawesome icon
            ImageAwesome CloseIcon = new ImageAwesome
            {
                Foreground = new SolidColorBrush(Colors.White),
                Icon = FontAwesomeIcon.Close,
                Margin = new Thickness(3)

            };

            //Close Button
            Button CloseButton = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Style = NewTabBT.Style,
                Margin = new Thickness(10, 0, 0, 0),
                Background = (SolidColorBrush)Application.Current.TryFindResource("POSHeader"),
                Content = CloseIcon,
                Height = 15,
                Width = 15,
                Tag = TabID              

            };

            CloseButton.Click += CloseButton_Click;
           
            //Create header content text and button
            HeaderContent.Children.Add(headerText);
            HeaderContent.Children.Add(CloseButton);

            //Frame content
            Frame BillContent = new Frame
            {
                NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden,
                Source = new Uri("Pages/POS/TabedPOSContent.xaml", UriKind.Relative)
        };

            //Tab item definition
            var NewTab = new TabItem();
            NewTab.Style = TestTab.Style;
            NewTab.Background = TestTab.Background;
            NewTab.Height = 50;
            NewTab.Tag = TabID;
            NewTab.Name = null;
            NewTab.Header = HeaderContent;
            NewTab.IsSelected = true;
            NewTab.Content = BillContent;


            //Add tabitem to current tabs
            POSTTab.Items.Insert(POSTTab.Items.Count - 1 , NewTab);
            POSTTab.SelectedIndex = POSTTab.Items.Count - 1;
            NewTab.IsSelected = true;

            if (POSTTab.Items.Count == 50)
            {
                NewTabTab.IsEnabled = false;
            } else
            {
                NewTabTab.IsEnabled = true;
            }
        }

        //Close bill tab
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {           
            if (MessageBox.Show("¿Desea cerrar la ventana de facturación?","Cerrar ventana",MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                TabItem CloseTab = null;

                //Search tabs
                foreach (TabItem tab in POSTTab.Items)
                {
                    if (tab.Tag == ((Button)sender).Tag)
                    {
                        CloseTab = tab;
                    }
                }
                //Change selection if selected tab is removed
                if (POSTTab.SelectedItem == CloseTab)
                {
                    POSTTab.SelectedIndex = 0;
                }
                //Remove tab item
                POSTTab.Items.Remove(CloseTab);
            }

            //Verify tabs
            if (POSTTab.Items.Count == 6)
            {
                NewTabTab.IsEnabled = false;
            }
            else
            {
                NewTabTab.IsEnabled = true;
            }
        }
        #region BOX Logic
        //Open box BT pos
        private void OpenBoxBT_Click(object sender, RoutedEventArgs e)
        {
            OpenBoxPopUp.IsOpen = true;
            OpenBoxValueTX.Focus();
        }

        //Save open value
        private async void OpenBoxPopBT_Click(object sender, RoutedEventArgs e)
        {

            var data = new NovaAPI.APIBoxMovements.BoxData
            {
                id = DataConfig.WorkPointID.ToString(),
                openvalue = OpenBoxValueTX.Number.ToString()

            };
            string requestData = JsonConvert.SerializeObject(data);

            bool response = await NovaAPI.APIBoxMovements.GetValues("2", DataConfig.LocalAPI, requestData);

            if (response)
            {
                await CheckAndLoad();

                MainFrame.Navigate(new Uri("Pages/POS/TabedPOSContent.xaml", UriKind.Relative));

                try
                {
                    NovaAPI.APIBoxMovements.box_data[0].id = NovaAPI.APIBoxMovements.movement_id;
                }
                catch (Exception)
                {

                }
               

                //Enable tab bill controls
                POSTTab.IsEnabled = true;
                //Set BOX controls
                BoxInformationLB.Content = $"Apertura de la caja realizada - {DateTime.Now} - por {DataConfig.RealName}";
                BoxInformationLB.Visibility = Visibility.Visible;
                OpenBoxBT.Visibility = Visibility.Collapsed;
                CloseBoxBT.Visibility = Visibility.Visible;

                OpenBoxPopUp.IsOpen = false;
            }
            else
            {
                OpenBoxPopUp.IsOpen = false;
                MessageBox.Show(NovaAPI.APIBoxMovements.Message);
            }
        }

        //Open Close box popup
        private void CloseBoxBT_Click(object sender, RoutedEventArgs e)
        {
            CloseBoxPopUp.IsOpen = true;
            FinalValueTX.Focus();
        }

        #endregion

        //Close popup box
        private void ClosePopUpBox_Click(object sender, RoutedEventArgs e)
        {
            OpenBoxValueTX.Clear();
            OpenBoxPopUp.IsOpen = false;
        }

        //Close  box close popup
        private void ClosePopUpBoxBT_Click(object sender, RoutedEventArgs e)
        {
            FinalValueTX.Clear();
            CloseBoxPopUp.IsOpen = false;
        }

        //Send close box
        private async void CloseBoxValueBT_Click(object sender, RoutedEventArgs e)
        {
            var data = new NovaAPI.APIBoxMovements.BoxData
            {
                id = NovaAPI.APIBoxMovements.movement_id,
                closevalue = FinalValueTX.Number.ToString(),
                comment = CommentBox.Text

            };

            string requestData = JsonConvert.SerializeObject(data);
            bool response = await NovaAPI.APIBoxMovements.GetValues("3", DataConfig.LocalAPI, requestData);

            if (response)
            {
                CloseBoxPopUp.IsOpen = false;
                MainFrame.Source = null;
                MainFrame.Refresh();
                bool Detailresponse = await NovaAPI.APIBoxMovements.GetValues("4", DataConfig.LocalAPI, requestData);
                if (Detailresponse)
                {
                    OpendateLB.Content = NovaAPI.APIBoxMovements.opendate;
                    OpenuserLB.Content = NovaAPI.APIBoxMovements.openuser;
                    ClosedateLB.Content = NovaAPI.APIBoxMovements.closedate;
                    CloseuserLB.Content = NovaAPI.APIBoxMovements.closeuser;
                    OpenvalueLB.Content = string.Format("{0:C0}", Convert.ToInt32(NovaAPI.APIBoxMovements.openvalue));
                    ClosevalueLB.Content = string.Format("{0:C0}", Convert.ToInt32(NovaAPI.APIBoxMovements.closevalue));
                    TotalsellLB.Content = string.Format("{0:C0}", Convert.ToInt32(NovaAPI.APIBoxMovements.totalsell));
                    TotalcashLB.Content = string.Format("{0:C0}", Convert.ToInt32(NovaAPI.APIBoxMovements.cash));
                    TotalothersLB.Content = string.Format("{0:C0}", Convert.ToInt32(NovaAPI.APIBoxMovements.others));


                    BoxDetailPopUp.IsOpen = true;
                    PrintBoxDetail.Focus();
                }
                
                LoadBoxStatus();
            }
            else
            {
                CloseBoxPopUp.IsOpen = false;
                MessageBox.Show(NovaAPI.APIBoxMovements.Message);
            }
        }

        private void ExitBoxDetail_Click(object sender, RoutedEventArgs e)
        {
            BoxDetailPopUp.IsOpen = false;
        }

        private void PrintBoxDetail_Click(object sender, RoutedEventArgs e)
        {
            PrintFunctions.PrintFunctions.Print(Classes.Enums.PrintPages.BoxDetail, null);
            ExitBoxDetail.Focus();
        }
    }
}
