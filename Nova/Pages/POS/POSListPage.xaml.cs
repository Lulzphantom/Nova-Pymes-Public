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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFTextBoxAutoComplete;

namespace Nova.Pages.POS
{
    /// <summary>
    /// Lógica de interacción para POSListPage.xaml
    /// </summary>
    public partial class POSListPage : Page
    {
        string HStatus = "0";
        int Pagination = 1;

        private List<NovaAPI.APIStatus.branch> BranchList = new List<NovaAPI.APIStatus.branch>();

        public POSListPage()
        {
            InitializeComponent();

            //Set CB items branch/boxes

            for (int i = 0; i < NovaAPI.APIStatus.Branch.Count + 1; i++)
            {
                var BranchData = new NovaAPI.APIStatus.branch
                {
                    BranchID = i == 0 ? 0 : NovaAPI.APIStatus.Branch[i - 1].BranchID,
                    BranchName = i == 0 ? "  -   TODOS" : NovaAPI.APIStatus.Branch[i - 1].BranchName
                };

                BranchList.Add(BranchData);
            }
            BranchCB.ItemsSource = BranchList;

            //Set view model Class 6
            DataContext = new HViewModel(this, 6);
        }

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

        //SET H VISIBILITY
        public void SetH(bool status)
        {
            FilterGrid.Background = status == true ? (SolidColorBrush)Application.Current.TryFindResource("HBackground") : new SolidColorBrush(Colors.White);

            BranchCB.IsEnabled = status == true ? false : true;
            //Refresh page
            HStatus = status == true ? "1" : "0";
            RefreshProducts_Click(null, null);
        }

        //Set clients to searchbox
        private void SetClients()
        {
            //Set IEnumerable LIST
            IEnumerable<string> ClientList = Enumerable.Empty<string>();

            for (int i = 0; i < NovaAPI.APIClient.clients.Count; i++)
            {
                ClientList = ClientList.Concat(new[] { NovaAPI.APIClient.clients[i].name });
            }

            //Set list of clients to textbox
            BillSearchTX.SetValue(AutoCompleteBehavior.AutoCompleteItemsSource, ClientList);

            BillSearchTX.Focus();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Set date controls days
            FromDT.SelectedDate = DateTime.Now.AddDays(-31);
            ToDT.SelectedDate = DateTime.Now.AddDays(1);

            LoadTickets();
            await LoadClients();
            SetClients();
        }

        private async void LoadTickets(string Page = null, string Filter = null)
        {
            //Set loading grid visibility
            LoadingGrid.Visibility = Visibility.Visible;
            Spinner.Spin = true;

            //Try to clear existent branch list
            try
            {
                NovaAPI.APITickets.tickets.Clear();
            }
            catch (Exception) { }

            //Send request
            var Data = new NovaAPI.APITickets.DataClass();
            Data.h = HStatus;

            int parsedValue;
            if (!int.TryParse(Filter, out parsedValue))
            {
                //alfanumeric
                try
                {
                    Data.client = NovaAPI.APIClient.clients.Find(x => x.name.ToLower() == Filter.ToLower()).id;
                }
                catch (Exception)
                {

                    Data.client = Filter;
                }
                
            }
            else
            {
                //Numeric
                Data.filter = Filter;
            }

            Data.from = Page;
            //Data.ticket_box = DataConfig.WorkPointID.ToString();
            Data.ticket_branch = ((NovaAPI.APIStatus.branch)BranchCB.SelectedItem).BranchID.ToString();
            Data.from_date = FromDT.SelectedDate.Value.ToString("yyyy-MM-dd");
            Data.to_date = ToDT.SelectedDate.Value.ToString("yyyy-MM-dd");

            string requestData = JsonConvert.SerializeObject(Data);

            //Load branch 
            bool response = await NovaAPI.APITickets.GetValues("2", DataConfig.LocalAPI, requestData);
            if (response)
            {
                //Set rol data to DataGrid
                TicketsDataGrid.ItemsSource = NovaAPI.APITickets.tickets;
                TicketsDataGrid.Items.Refresh();

                TotalProducts.Content = NovaAPI.APITickets.Count;
                double Pages = (Convert.ToInt32(NovaAPI.APITickets.Count) / 15);
                double TotalPages = Math.Floor(Pages);

                SetPagination(TotalPages);
            }
            else
            {
                //On load error
                MessageBox.Show($"Se produjo un error al cargar los datos, INFO: {Environment.NewLine}{NovaAPI.APITickets.Message}");
                await Task.Delay(100);

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

        #region Pagination
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

            LPage.Content = (((Pagination - 1) * 15) + TicketsDataGrid.Items.Count).ToString();


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
                command.Background = (SolidColorBrush)Application.Current.TryFindResource("POSHeader");
                command.Click += Page_Click;
                command.Content = "«";
                PaginationStack.Children.Add(command);

                Button command2 = new Button();
                command2.Tag = Pagination - 1;
                command2.Background = (SolidColorBrush)Application.Current.TryFindResource("POSHeader");
                command2.Click += Page_Click;
                command2.Style = (Style)Application.Current.TryFindResource("PaginationCenterButton");
                command2.Content = "‹";
                PaginationStack.Children.Add(command2);
            }

            for (int i = 1; i <= TotalPages + 1; i++)
            {
                //Define button controls            
                Button page = new Button();
                page.Background = (SolidColorBrush)Application.Current.TryFindResource("POSHeader");
                page.Style = (Style)Application.Current.TryFindResource("PaginationCenterButton");

                if (i == Pagination)
                {
                    //Print current page number

                    page.Tag = (i).ToString();
                    page.Content = page.Tag;
                    page.BorderBrush = (SolidColorBrush)Application.Current.TryFindResource("POSHeader");
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
                command2.Background = (SolidColorBrush)Application.Current.TryFindResource("POSHeader");
                command2.Click += Page_Click;
                command2.Style = (Style)Application.Current.TryFindResource("PaginationCenterButton");
                command2.Content = "›";
                PaginationStack.Children.Add(command2);

                //Define button controls            
                Button command = new Button();
                command.Tag = TotalPages + 1;
                command.Background = (SolidColorBrush)Application.Current.TryFindResource("POSHeader");
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
            LoadTickets(((Pagination - 1) * 15).ToString(), BillSearchTX.Text);
        }

        #endregion

        //Cancel ticket
        private async void Cancel_Click(object sender, RoutedEventArgs e)
        {
            //Get selected ticket info
            var TicketData = NovaAPI.APITickets.tickets.Find(x => x.id == ((Button)sender).Uid.ToString());

            if (MessageBox.Show($"A continuación se anulara la factura #{TicketData.id}{Environment.NewLine}¿Desea continuar?","Anular factura", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var data = new NovaAPI.APITickets.CancelTicketClass
                {
                    branch_id = DataConfig.WorkPlaceID.ToString(),
                    status = "0",
                    ticket_h = HStatus,
                    ticket_id = TicketData.id
                };

                string requestData = JsonConvert.SerializeObject(data);

                //Load branch 
                bool response = await NovaAPI.APITickets.GetValues("3", DataConfig.LocalAPI, requestData);
                if (response)
                {
                    TicketData.ticket_status = "0";
                    TicketsDataGrid.Items.Refresh();
                }
                else
                {
                    MessageBox.Show($"Error al anular la factura: {NovaAPI.APITickets.Message}");
                }
            }
        }

        private void BillSearchTX_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Pagination = 1;
                LoadTickets(null, BillSearchTX.Text);
                e.Handled = true;
            }
        }

        private void SearchBT_Click(object sender, RoutedEventArgs e)
        {
            Pagination = 1;
            LoadTickets(null, BillSearchTX.Text);
        }

        //Refresh data
        private void RefreshProducts_Click(object sender, RoutedEventArgs e)
        {
            //Set date controls days
            FromDT.SelectedDate = DateTime.Now.AddDays(-31);
            ToDT.SelectedDate = DateTime.Now.AddDays(1);

            BillSearchTX.Clear();
            BranchCB.SelectedIndex = 0;

            Pagination = 1;

            LoadTickets();
        }

        //See bill data
        private void See_Click(object sender, RoutedEventArgs e)
        {
            var BillData = NovaAPI.APITickets.tickets.Find(x => x.id == ((Button)sender).Uid.ToString());

            BillNumberLB.Content = BillData.id;
            BillDateLB.Content = BillData.date;
            BillMethodLB.Content = BillData.payment_type == "0" ? "CONTADO" : "CREDITO";
            BillExpirationLB.Content = BillData.expiration_date;
            BillClientLB.Content = BillData.client_name;
            BillUserLB.Content = BillData.user_realname;
            BillTotalLB.Content = string.Format("{0:C0}", BillData.ticket_total_int);
            BillTotalPayment.Content = string.Format("{0:C0}", Convert.ToInt32(BillData.ticket_totalpayment)); 
            BillExchangeLB.Content = string.Format("{0:C0}", Convert.ToInt32(BillData.ticket_changepayment));
            BillLeftLB.Content = string.Format("{0:C0}", Convert.ToInt32(BillData.ticket_leftpayment));

            ProductsDataGrid.ItemsSource = BillData.items;
            ProductsDataGrid.Items.Refresh();

            //-------------------------
            Container.IsEnabled = false;
            BillDetailPopUp.IsOpen = true;
        }

        private void ExitPopUp_Click(object sender, RoutedEventArgs e)
        {
            //-------------------------
            Container.IsEnabled = true;
            BillDetailPopUp.IsOpen = false;
        }

        private void PrintBillLB_Click(object sender, RoutedEventArgs e)
        {
            var data = NovaAPI.APITickets.tickets.Find(x => x.id == BillNumberLB.Content.ToString());
            data.ticket_copy = "1";

            //Create product data object
            var Pdata = new List<NovaAPI.APITickets.ProductClass>();

            foreach (var item in data.items)
            {
                var AddData = new NovaAPI.APITickets.ProductClass()
                {
                    product_count = item.item_count,
                    product_id = item.item_id,
                    product_pricevalue = item.item_pricevalue,
                    product_tax = item.item_tax,
                    product_h = HStatus,
                    product_discountpercent = item.item_discountpercent,
                    product_discountvalue = item.item_discountvalue.ToString(),
                    product_code = item.product_code,
                    product_name = item.product_name,
                    product_total = (Convert.ToInt32(item.item_pricevalue) * Convert.ToInt32(item.item_count)).ToString(),
                    product_unity = item.product_unity

                };
                Pdata.Add(AddData);
            }

            data.products = Pdata;
            data.ticket_coti = "0";
            data.ticket_h = HStatus;


            var ClientData = NovaAPI.APIClient.clients.Find(x => x.id == data.client_id);
            if (ClientData != null)
            {
                data.client_address = ClientData.address;
                data.client_documentid = ClientData.idcomplete;
                data.client_phones = ClientData.phones;
            }

            NovaAPI.APITickets.TicketID = data.id;

            PrintFunctions.PrintFunctions.Print(Classes.Enums.PrintPages.FinalTicket, data);
        }
    }
}
