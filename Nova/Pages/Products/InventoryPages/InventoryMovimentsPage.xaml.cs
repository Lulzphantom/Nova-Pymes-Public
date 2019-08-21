using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace Nova.Pages.Products.InventoryPages
{

    //Value converter status
    public class TypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value.ToString())
            {
                case "0":
                    return "Entrada";
                case "1":
                    return "Salida (Venta)";
                case "2":
                    return "Ajuste";
                case "3":
                    return "Traslado";
                case "4":
                    return "Devolución";
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //Value converter Color
    public class TypeColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value.ToString())
            {
                case "0":
                    return new SolidColorBrush(Colors.DarkGreen);
                case "1":
                    return new SolidColorBrush(Colors.RoyalBlue);
                case "2":
                    return new SolidColorBrush(Colors.DarkOrange);
                case "3":
                    return new SolidColorBrush(Colors.DarkViolet);
                case "4":
                    return new SolidColorBrush(Colors.DarkRed);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Lógica de interacción para InventoryMovimentsPage.xaml
    /// </summary>
    public partial class InventoryMovimentsPage : Page
    {

        int Pagination = 1;

        public InventoryMovimentsPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Set date controls days
            FromDT.SelectedDate = DateTime.Now.AddDays(-31);
            ToDT.SelectedDate = DateTime.Now.AddDays(1);

            InventoryDock.IsEnabled = true;
            PaginationGrid.IsEnabled = true;

            //Set branches
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
            LoadMovements();
        }

        private async void LoadMovements(string Page = null, int BranchIndex = 0)
        {
            //Set loading grid visibility
            LoadingGrid.Visibility = Visibility.Visible;
            Spinner.Spin = true;

            //Try to clear existent list
            try
            {
                NovaAPI.APIInventoryMovements.Movements.Clear();
            }
            catch (Exception) { }

            //Set data for request
            var Data = new NovaAPI.APIInventoryMovements.RequestData
            {
                branch = NovaAPI.APIBranch.branch[BranchIndex].id,
                type = TypeCB.SelectedIndex == 0 ? null : (TypeCB.SelectedIndex - 1).ToString(),
                date_from = FromDT.SelectedDate.Value.ToString("yyyy-MM-dd"),
                date_to = ToDT.SelectedDate.Value.ToString("yyyy-MM-dd"),
                from = Page
            };

            //Create string request
            string requestData = JsonConvert.SerializeObject(Data);

            //Send request
            bool Response = await NovaAPI.APIInventoryMovements.GetValues("7", DataConfig.LocalAPI, requestData);

            if (Response)
            {
                //Set data to datagrid
                ProductsGrid.ItemsSource = NovaAPI.APIInventoryMovements.Movements;
                ProductsGrid.Items.Refresh();

                TotalProducts.Content = NovaAPI.APIInventoryMovements.Count;

                //Set pagination info
                double Pages = (Convert.ToInt32(NovaAPI.APIInventoryMovements.Count) / 15);
                double TotalPages = Math.Floor(Pages);

                SetPagination(TotalPages);

            }
            else
            {
                ProductsGrid.Items.Refresh();

                MessageBox.Show(NovaAPI.APIInventoryMovements.Message);

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

        //Search button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Pagination = 1;
            LoadMovements(null, BranchCB.SelectedIndex);
        }


        //Refresh BT
        private void RefreshProducts_Click(object sender, RoutedEventArgs e)
        {
            Pagination = 1;

            BranchCB.SelectedIndex = 0;
            TypeCB.SelectedIndex = 0;
            //Set date controls days
            FromDT.SelectedDate = DateTime.Now.AddDays(-31);
            ToDT.SelectedDate = DateTime.Now.AddDays(1);

            //request
            LoadMovements(null);
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
            LoadMovements(((Pagination - 1) * 15).ToString(), BranchCB.SelectedIndex);
        }
        
        //See details click
        private void See_Click(object sender, RoutedEventArgs e)
        {
            var Movement = NovaAPI.APIInventoryMovements.Movements.Find(x => x.id == ((Control)sender).Tag.ToString());
            //Set popup type and content
            Cache.Cache.SelectedMovement = Movement.detail_id;

            switch (Movement.type)
            {
                case "0":
                    return;
                    break;
                case "1":
                    DetailsFrame.Source = new Uri("Details/SellDetail.xaml", UriKind.Relative);                    
                    break;
                case "2":
                    return;
                    break;
                case "3":
                    return;
                    break;
                case "4":
                    return;
                default:
                    break;
            }
            //Open popup
            InventoryDock.IsEnabled = false;
            PaginationGrid.IsEnabled = false;

            MovementDetailsPopUp.IsOpen = true;
        }


        //Close popup
        private void PopUpFinishBT_Click(object sender, RoutedEventArgs e)
        {
            InventoryDock.IsEnabled = true;
            PaginationGrid.IsEnabled = true;
            MovementDetailsPopUp.IsOpen = false;
            //Clear cache value
            Cache.Cache.SelectedMovement = "0";
        }
    }
}
