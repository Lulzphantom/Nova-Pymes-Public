using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Nova.Pages.Products.InventoryPages
{
    /// <summary>
    /// Lógica de interacción para InventoryLowPage.xaml
    /// </summary>
    public partial class InventoryLowPage : Page
    {
        public InventoryLowPage()
        {
            InitializeComponent();
        }


        int Pagination = 1;

        //Low products page loaded
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Set loading grid visibility
            LoadingGrid.Visibility = Visibility.Visible;
            Spinner.Spin = true;

            //Clear branch information
            try
            {
                NovaAPI.APIBranch.branch.Clear();
            }
            catch (Exception) { }

            //Request branch information
            bool BranchRequest = await NovaAPI.APIBranch.GetValues("4", DataConfig.LocalAPI, null);

            if (!BranchRequest)
            {
                MessageBox.Show($"Error al cargar datos {NovaAPI.APIBranch.Message}");
                return;
            }
            //Set branch combobox
            BranchCB.ItemsSource = NovaAPI.APIBranch.branch;

            //Load products data to datagrid
            LoadProducts();

        }

        private async void LoadProducts(string Page = null, int BranchIndex = 0)
        {
            //Set loading grid visibility
            LoadingGrid.Visibility = Visibility.Visible;
            Spinner.Spin = true;

            //Try to clear existent list
            try
            {
                NovaAPI.APIInventory.LowProductsData.products.Clear();
            }
            catch (Exception) { }

            //Set data for request
            var Data = new NovaAPI.APIInventory.LowProductRequestData
            {
                branch_id = NovaAPI.APIBranch.branch[BranchIndex].id,
                low_point = LowPointTX.Text,
                from = Page
            };

            //Create string request
            string requestData = JsonConvert.SerializeObject(Data);

            //Send request
            bool Response = await NovaAPI.APIInventory.GetValues("6", DataConfig.LocalAPI, requestData, true);

            if (Response)
            {
                //Set data to datagrid
                ProductsGrid.ItemsSource = NovaAPI.APIInventory.LowProductsData.products;
                ProductsGrid.Items.Refresh();

                TotalProducts.Content = NovaAPI.APIInventory.LowProductsData.Count;

                //Set pagination info
                double Pages = (Convert.ToInt32(NovaAPI.APIInventory.LowProductsData.Count) / 15);
                double TotalPages = Math.Floor(Pages);

                SetPagination(TotalPages);

            } else
            {
                ProductsGrid.Items.Refresh();

                MessageBox.Show(NovaAPI.APIInventory.LowProductsData.Message);

                //Set loading grid visibility
                LoadingGrid.Visibility = Visibility.Collapsed;
                Spinner.Spin = false;
                return;
            }

            await Task.Delay(100);

            //Set loading grid visibility
            LoadingGrid.Visibility = Visibility.Collapsed;
            Spinner.Spin = false;
        }

        /// <summary>
        /// Pagination system
        /// </summary>
        /// <param name="TotalPages"></param>
        private void SetPagination(double TotalPages)
        {

            if (Pagination == 1)
            {
                FPage.Content = "1";
            }
            else
            {
                FPage.Content = ((Pagination - 1) * 15).ToString();
            }

            LPage.Content = (((Pagination - 1) * 15) + ProductsGrid.Items.Count).ToString();


            bool previousPageIsEllipsis = false;

            try
            {
                PaginationStack.Children.Clear();
            }
            catch (Exception)
            {
            }

            if (TotalPages < 1)
            {
                return;
            }

            if (TotalPages > 1 && Pagination > 1)
            {
                //Define button controls            
                Button command = new Button();
                command.Tag = "1";
                command.Background = (SolidColorBrush)Application.Current.TryFindResource("InventoryHeader");
                command.Click += Page_Click;
                command.Content = "«";
                PaginationStack.Children.Add(command);

                Button command2 = new Button();
                command2.Tag = Pagination - 1;
                command2.Background = (SolidColorBrush)Application.Current.TryFindResource("InventoryHeader");
                command2.Click += Page_Click;
                command2.Style = (Style)Application.Current.TryFindResource("PaginationCenterButton");
                command2.Content = "‹";
                PaginationStack.Children.Add(command2);
            }

            for (int i = 1; i <= TotalPages + 1; i++)
            {
                //Define button controls            
                Button page = new Button();
                page.Background = (SolidColorBrush)Application.Current.TryFindResource("InventoryHeader");
                page.Style = (Style)Application.Current.TryFindResource("PaginationCenterButton");

                if (i == Pagination)
                {
                    //Print current page number

                    page.Tag = (i).ToString();
                    page.Content = page.Tag;
                    page.BorderBrush = (SolidColorBrush)Application.Current.TryFindResource("InventoryHeader");
                    page.Foreground = new SolidColorBrush(Colors.White);
                    page.ToolTip = $"Pagina {page.Tag}";
                    PaginationStack.Children.Add(page);

                    previousPageIsEllipsis = false;
                }
                else
                {
                    if (i == 1
                        || i == 2
                        || i == Pagination - 2
                        || i == Pagination - 1
                        || i == Pagination + 1
                        || i == Pagination + 2
                        || i == Pagination - 1
                        || i == Pagination)
                    {
                        page.Tag = (i).ToString();
                        page.Content = page.Tag;
                        page.ToolTip = $"Pagina {page.Tag}";
                        page.Click += Page_Click;

                        PaginationStack.Children.Add(page);
                        previousPageIsEllipsis = false;
                    }
                    else
                    {
                        if (previousPageIsEllipsis)
                        {
                            //an ellipsis was already added. Do not add it again. Do nothing.
                            continue;
                        }
                        else
                        {
                            //Print an ellipsis

                            page.Content = "...";
                            page.IsEnabled = false;

                            PaginationStack.Children.Add(page);
                            previousPageIsEllipsis = true;
                        }
                    }
                }
            }


            if (Pagination < TotalPages)
            {

                Button command2 = new Button();
                command2.Tag = Pagination + 1;
                command2.Background = (SolidColorBrush)Application.Current.TryFindResource("InventoryHeader");
                command2.Click += Page_Click;
                command2.Style = (Style)Application.Current.TryFindResource("PaginationCenterButton");
                command2.Content = "›";
                PaginationStack.Children.Add(command2);

                //Define button controls            
                Button command = new Button();
                command.Tag = TotalPages + 1;
                command.Background = (SolidColorBrush)Application.Current.TryFindResource("InventoryHeader");
                command.Click += Page_Click;
                command.Content = "»";
                PaginationStack.Children.Add(command);
            }

            var FirstBT = (Button)PaginationStack.Children[0];
            FirstBT.Style = (Style)Application.Current.TryFindResource("PaginationLeftButton");

            var LastBT = (Button)PaginationStack.Children[PaginationStack.Children.Count - 1];
            LastBT.Style = (Style)Application.Current.TryFindResource("PaginationRightButton");
        }

        //Pagination buttons
        private void Page_Click(object sender, RoutedEventArgs e)
        {
            Button page = (Button)sender;
            Pagination = Convert.ToInt32(page.Tag.ToString());
            LoadProducts(((Pagination - 1) * 15).ToString(), BranchCB.SelectedIndex);
        }

        //Search button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Pagination = 1;
            LoadProducts(null, BranchCB.SelectedIndex);
        }

        private void LowPointTX_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void RefreshProducts_Click(object sender, RoutedEventArgs e)
        {
            Pagination = 1;
            //Clear data
            BranchCB.SelectedIndex = 0;
            LowPointTX.Text = "5";

            //Refresh
            LoadProducts();
        }
    }
}
