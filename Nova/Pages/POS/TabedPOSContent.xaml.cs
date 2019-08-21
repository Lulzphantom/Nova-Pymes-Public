using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WPFTextBoxAutoComplete;

namespace Nova.Pages.POS
{


    public class BillProductsClass
    {
        //Datagrid information
        public string product_id { get; set; }
        public string product_code { get; set; }
        public string product_name { get; set; }
        public string product_count { get; set; }
        public decimal product_value { get; set; }
        public string product_tax { get; set; }
        public string product_discount { get; set; }
        public decimal product_discountvalue { get; set; }
        public string product_type { get; set; }
        public decimal product_total { get
            {
                int count = Convert.ToInt32(product_count);

                //Sub total with discount
                decimal SubTotal = (product_value - product_discountvalue) * count;

                //Retun total value
                return SubTotal;
            }
        }
        public decimal total_discount { get { return product_discountvalue * Convert.ToInt32(product_count); } }

        //Aditional information
        public int product_selectedPrice { get; set; }
        public List<PriceClass> Product_prices { get; set; } = new List<PriceClass>();

        public class PriceClass
        {
            public string id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public string value { get; set; }
        }
    }
        
    
    public partial class TabedPOSContent : Page
    {

        string SelectedClientID = "";
        public string HStatus = "0";

        public bool CLientCredit = false;

        //Bill values
        int TotalProducts = 0;
        decimal Subtotal = 0;
        decimal IvaValue = 0;
        decimal Iva5Value = 0;
        decimal IacValue = 0;
        decimal TotalValue = 0;
        //----------------------
        //Payment values
        decimal InValue = 0;
        decimal InChangeValue = 0;
        decimal inLeftValue = 0;
        //--------------------------        
        int SelectedCatIndex = 0;
        //--------------------------
        bool EditProduct = false;
        string ProductID = "";
        bool EditPrice = false;


        public List<BillProductsClass> BillProducts;

        private NovaAPI.APITickets.TicketData TicketData = new NovaAPI.APITickets.TicketData();

        public TabedPOSContent()
        {
            InitializeComponent();
            DataContext = new POSContentViewModel(this);
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
                MessageBox.Show($"Error al cargar los clientes :{ NovaAPI.APIClient.Message}");
            }
        }

        //Control move focus
        private void MoveFocus(object sender, KeyEventArgs e)
        {           
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

        //Load page
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SetClients();

            if (BillProducts == null)
            {
                BillProducts = new List<BillProductsClass>();
                ProductsGrid.ItemsSource = BillProducts;
            }
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
            ClientFilterTX.SetValue(AutoCompleteBehavior.AutoCompleteItemsSource, ClientList);

            ClientFilterTX.Focus();
        }

        //Clear all form data and values
        private async void ClearForm()
        {
            ClientFilterTX.Clear();

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

            //Clear client data
            ClientNameLB.Content = "";
            ClientDocumentLB.Content = "";
            ClientPhonesLB.Content = "";
            ClientAddressLB.Content = "";
            ClientMailLB.Content = "";

            SelectedClientID = "";
            //Reset form
            InProductBT.IsEnabled = false;
            FinalIn.IsEnabled = false;

            string ZeroValue = string.Format("{0:C0}", 0);
            TotalProductsLB.Content = "0";
            SubtotalLB.Content = ZeroValue;
            IVALB.Content = ZeroValue;
            IACLB.Content = ZeroValue;
            TotalLB.Content = ZeroValue;

            //Product add popup
            ProductSearchTX.Clear();

            ProductCB.ItemsSource = null;
            PriceCatCB.ItemsSource = null;
            ProductCB.Items.Refresh();
            PriceCatCB.Items.Refresh();
            SelectedCatIndex = 0;

            ProductBranchCountLB.Content = "0";
            ProductBranchCountLB.Foreground = new SolidColorBrush(Colors.DarkGray);

            ProcutCountTX.Text = "1";
            ProductSellValueTX.Clear();

            BillProducts.Clear();
            ProductsGrid.Items.Refresh();

            PrintIn.Visibility = Visibility.Collapsed;
            InProductBT.Visibility = Visibility.Visible;
            FinalIn.Visibility = Visibility.Visible;

            SetClients();
        }

        //Set client information on labels
        private void SelectClient(NovaAPI.APIClient.ClientClass Data)
        {
            ClientNameLB.Content = Data.name;
            ClientDocumentLB.Content = Data.idcomplete;
            ClientPhonesLB.Content = Data.phones;
            ClientAddressLB.Content = Data.address;
            ClientMailLB.Content = Data.mail;

            CLientCredit = Data.cancredit == "1" ? true : false;
        }

        //Search client
        private void SearchClientBT_Click(object sender, RoutedEventArgs e)
        {
            //Search client value
            var Data = NovaAPI.APIClient.clients.Find(x => x.name.ToLower().Contains(ClientFilterTX.Text.ToLower()));
            
            //Clear data
            ClearForm();

            //Set Client data
            if (Data != null )
            {
                //Client found
                ClientFilterTX.Text = Data.name;
                SelectedClientID = Data.id;
                SelectClient(Data);
                InProductBT.IsEnabled = true;
                InProductBT.Focus();
            }
            else
            {
                //Client not found
                ClearForm();
                ClientFilterTX.Focus();
                ClientFilterTX.SelectAll();
            }
        }

        //Search textbox acction on enter key pressed
        private void ClientFilterTX_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchClientBT_Click(null,null);
                e.Handled = true;
            }
        }

        private void ClientFilterTX_GotFocus(object sender, object e)
        {
            ClientFilterTX.SelectAll();
        }

        //Inser product to bill / open product popup
        public void InProductBT_Click(object sender, RoutedEventArgs e)
        {
            if (BillDock.IsEnabled == true)
            {
                BillDock.IsEnabled = false;
                AddProductPopUp.IsOpen = true;
                ProductSearchTX.Focus();
            }           
        }

        //Print Bill Coti
        public void PrintCotiBT_Click(object sender, RoutedEventArgs e)
        {
            if (PrintCotiBT.IsEnabled == true)
            {

                if (BillProducts.Count == 0)
                {
                    MessageBox.Show("No se puede ralizar impresion ya que no hay productos registrados");
                    return;
                }

                //Create product data object
                var Pdata = new List<NovaAPI.APITickets.ProductClass>();

                foreach (var item in BillProducts)
                {
                    var AddData = new NovaAPI.APITickets.ProductClass()
                    {
                        product_count = item.product_count,
                        product_id = item.product_id,
                        product_pricevalue = item.product_value.ToString(),
                        product_tax = item.product_tax,
                        product_h = HStatus,
                        product_discountpercent = item.product_discount,
                        product_discountvalue = item.product_discountvalue.ToString(),
                        product_code = item.product_code,
                        product_name = item.product_name,
                        product_total = item.product_total.ToString(),
                        product_unity = item.product_type


                    };
                    Pdata.Add(AddData);
                }

                //Create ticket data object
                var data = new NovaAPI.APITickets.TicketData
                {                    
                    payment_type = "0",
                    payment_method = "0",
                    expiration_date = "",
                    ticket_total = TotalValue.ToString(),
                    ticket_iva = IvaValue.ToString(),
                    ticket_iva5 = Iva5Value.ToString(),
                    ticket_iac = IacValue.ToString(),
                    ticket_totalpayment = "0",
                    ticket_changepayment = "0",
                    ticket_leftpayment = "0",
                    ticket_comment = "",
                    ticket_h = HStatus,
                    products = Pdata,
                    client_name = ClientNameLB.Content.ToString(),
                    client_documentid = ClientDocumentLB.Content.ToString(),
                    client_address = ClientAddressLB.Content == null ? "" : ClientAddressLB.Content.ToString(),
                    client_phones = ClientPhonesLB.Content.ToString(),
                    ticket_subtotal = Subtotal.ToString(),
                    ticket_coti = "1",
                    user_realname = DataConfig.RealName
                    


                };

                PrintFunctions.PrintFunctions.Print(Classes.Enums.PrintPages.FinalTicket, data);

            }
        }

        //New client BT
        public void NewClient_Click(object sender, RoutedEventArgs e)
        {
            if (NewClient.IsEnabled == true)
            {
                BillDock.IsEnabled = false;
                CreateClientPopUp.IsOpen = true;
                ClientNameTX.Focus();
            }            
        }

        //Client exit BT
        private void ClientExitBT_Click(object sender, RoutedEventArgs e)
        {
            //Clear controls values
            ClientNameTX.Clear();
            ClientIDTX.Clear();
            ClientIDType.SelectedIndex = 0;
            ClientPhoneTX.Clear();

            //Close popup
            SaveClientBT.IsEnabled = false;

            //Set controls
            BillDock.IsEnabled = true;
            CreateClientPopUp.IsOpen = false;

            ClientFilterTX.Focus();
        }

        //Search bill data
        private void SearchBillBT_Click(object sender, RoutedEventArgs e)
        {
            BillDock.IsEnabled = false;
            BillSearchPopUp.IsOpen = true;
            BillSearchTX.Focus();
        }


        //Refresh Content pos bill page
        private void RefreshBT_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"A continuación se recargara la pagina, esto borrara los datos existentes en ella " +
                $"{Environment.NewLine}¿Desea continuar?","Recargar factura",MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                ClearForm();
            }
        }

        //On search or exit productsearchtx
        private void ProductSearchTX_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ExitInPopUp_Click(null, null);
                e.Handled = true;
            }
            if (e.Key == Key.Enter && ((Control)sender).Name != "PaymentType")
            {
                try
                {
                    //Clear list
                    ProductCB.ItemsSource = null;
                    GC.Collect();
                }
                catch (Exception) { }

                int parsedValue;
                if (!int.TryParse(ProductSearchTX.Text, out parsedValue))
                {
                    LoadProducts(ProductSearchTX.Text);
                }
                else
                {                    
                    LoadProducts(ProductSearchTX.Text);
                    ProductSearchTX.Clear();
                }
                MoveFocus(sender, e);
                e.Handled = true;
            }
        }

        //Load popup products
        private async void LoadProducts(string Filter = null)
        {

            //Clear Product data
            try
            {
                NovaAPI.APIProdructs.products.Clear();
            }
            catch (Exception) { }

            //Send request
            var Data = new NovaAPI.APIProdructs.DataClass();
            Data.filter = Filter;
            Data.h = HStatus;

            string requestData = JsonConvert.SerializeObject(Data);

            bool Response = await NovaAPI.APIProdructs.GetValues("4", DataConfig.LocalAPI, requestData);

            if (Response)
            {
                InventoryInBT.IsEnabled = false;

                ProductCB.ItemsSource = NovaAPI.APIProdructs.products;
                ProductCB.Items.Refresh();
                ProductCB.SelectedIndex = 0;                
                
                //Enabled save button
                InventoryInBT.IsEnabled = true;
            }
        }

        //Product selection change
        private void ProductCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AddProductPopUp.IsOpen == true && ProductCB.ItemsSource != null)
            {
                //Set product info
                SetProductInfo((NovaAPI.APIProdructs.ProductClass)ProductCB.SelectedItem);
            }
            
        }

        private async void SetProductInfo(NovaAPI.APIProdructs.ProductClass Data)
        {
            //Get price parameters
            var PriceData = new NovaAPI.APIPrice.PriceClass();
            PriceData.id = Data.id;
            //rol json data
            string requestData = JsonConvert.SerializeObject(PriceData);

            try
            {
                NovaAPI.APIProdructPrice.price.Clear();
            }
            catch (Exception Ex) {  }

            //Load prices data
            bool Response = await NovaAPI.APIProdructPrice.GetValues("5", DataConfig.LocalAPI, requestData);
            if (Response)
            {
                //Set product default price
                var GeneralPData = new NovaAPI.APIPrice.PriceClass()
                {
                    name = "Precio general",
                    value = Data.sellprice                    
                };
                NovaAPI.APIProdructPrice.price.Insert(0, GeneralPData);

                EditPrice = EditProduct == true ? false : true;

                //Set price categories
                PriceCatCB.ItemsSource = NovaAPI.APIProdructPrice.price;
                PriceCatCB.Items.Refresh();
               
                //PriceCatCB.SelectedIndex = PriceCatCB.SelectedItem == null ? 0 : PriceCatCB.SelectedIndex;

                int PCount = Convert.ToInt32(Data.branch_count);
                int MinPCount = Convert.ToInt32(Data.minstock);


                string PInfo = "";
                if (PCount == 0)
                {
                    PInfo = "(Sin existencias)";
                    ProductBranchCountLB.Foreground = new SolidColorBrush(Colors.DarkRed);
                    InventoryInBT.IsEnabled = false;
                }
                else if (PCount < MinPCount && MinPCount != 0)
                {
                    PInfo = "(Poca existencia)";
                    ProductBranchCountLB.Foreground = new SolidColorBrush(Colors.DarkOrange);
                    InventoryInBT.IsEnabled = true;
                }
                else
                {
                    ProductBranchCountLB.Foreground = new SolidColorBrush(Colors.DarkGreen);
                    InventoryInBT.IsEnabled = true;
                }

                ProductBranchCountLB.Content = $"{PCount} {PInfo}";

                if (PriceCatCB.SelectedItem == null)
                {
                    PriceCatCB.SelectedIndex = SelectedCatIndex;
                }
            }
        }

        //Price CB selection changed
        private void PriceCatCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AddProductPopUp.IsOpen == true && PriceCatCB.Items != null && PriceCatCB.SelectedItem != null)
            {
                SelectedCatIndex = PriceCatCB.SelectedIndex;
                SetDataPrices();
                EditPrice = true;
            }
        }

        //Set content prices and other values
        private void SetDataPrices()
        {
            string Type = "0";

            Type = ((NovaAPI.APIPrice.PriceClass)PriceCatCB.SelectedItem).type;


            if (EditPrice)
            {
                DiscountValueTX.Number = 0;
                DiscountPercentTX.Text = "0";

                //Set product price
                ProductSellValueTX.Number = Type == "0" ?
                    Convert.ToInt32(((NovaAPI.APIPrice.PriceClass)PriceCatCB.SelectedItem).value)
                    : Convert.ToInt32(((NovaAPI.APIProdructs.ProductClass)ProductCB.SelectedItem).sellprice);

                //Set product percent value
                DiscountPercentTX.Text = Type == "0" || PriceCatCB.SelectedIndex == 0 ? "0" : ((NovaAPI.APIPrice.PriceClass)PriceCatCB.SelectedItem).value;

                if (DiscountPercentTX.Text != "0")
                {
                    int Percent = Convert.ToInt32(DiscountPercentTX.Text);
                    decimal Discount = Math.Round((ProductSellValueTX.Number * Percent) / 100);
                    DiscountValueTX.Number = Discount;
                }
            }            

            //Set discount editable
            DiscountPercentTX.IsEnabled = PriceCatCB.SelectedIndex == 0 ? true : false;
            DiscountValueTX.IsEnabled = PriceCatCB.SelectedIndex == 0 ? true : false;
                     
        }

        //On DiscountPercent Change
        private void DiscountPercentTX_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void DiscountPercentTX_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DiscountPercentTX.IsFocused && DiscountPercentTX.Text.Length > 0)
            {
                int Percent = Convert.ToInt32(DiscountPercentTX.Text);

                if (Percent > 100)
                {
                    DiscountPercentTX.Text = "100";
                    DiscountPercentTX.SelectAll();
                    Percent = 100;
                }
                decimal Discount = Math.Round((ProductSellValueTX.Number * Percent) / 100);

                DiscountValueTX.Number = Discount;
            }
            else if (DiscountPercentTX.IsFocused && DiscountPercentTX.Text.Length == 0)
            {
                DiscountPercentTX.Text = "0";
                DiscountPercentTX.SelectAll();
            }
        }

        //On discount value Change
        private void DiscountValueTX_NumberChanged(object sender, EventArgs e)
        {
            if (DiscountValueTX.IsFocused)
            {
                if (DiscountValueTX.Number == 0)
                {
                    DiscountPercentTX.Text = "0";
                }
                else if (DiscountValueTX.Number > ProductSellValueTX.Number)
                {
                    DiscountValueTX.Number = ProductSellValueTX.Number;

                } else {
                    DiscountPercentTX.Text = Math.Round((DiscountValueTX.Number * 100) / ProductSellValueTX.Number).ToString();
                }
            }
        }
        //Close ProductIn Popup
        private void ExitInPopUp_Click(object sender, RoutedEventArgs e)
        {
            AddProductPopUp.IsOpen = false;
            BillDock.IsEnabled = true;
            InProductBT.Focus();

            //Clear popup data
            ProductSearchTX.Clear();
            ProductCB.ItemsSource = null;
            ProductCB.Items.Refresh();

            ProductBranchCountLB.Content = "0";
            ProductBranchCountLB.Foreground = new SolidColorBrush(Colors.DarkGray);
            PriceCatCB.ItemsSource = null;
            PriceCatCB.Items.Refresh();

            ProcutCountTX.Text = "1";
            ProductSellValueTX.Number = 0;
            DiscountPercentTX.Text = "0";
            DiscountValueTX.Number = 0;
            InventoryInBT.IsEnabled = false;

            EditProduct = false;
            ProductSearchTX.IsEnabled = true;
            ProductCB.IsEnabled = true;
        }

        //Product in to datagrid
        private void InventoryInBT_Click(object sender, RoutedEventArgs e)
        {
            if (!EditProduct)
            {
                //Get product collection data
                var PData = (NovaAPI.APIProdructs.ProductClass)ProductCB.SelectedItem;

                //Get prices collection data
                var PriceData = new List<BillProductsClass.PriceClass>();

                //Add prices to local collection
                foreach (var item in PriceCatCB.Items)
                {
                    var itemData = (NovaAPI.APIPrice.PriceClass)item;
                    var AddData = new BillProductsClass.PriceClass
                    {
                        id = itemData.id,
                        name = itemData.name,
                        type = itemData.type,
                        value = itemData.value
                    };

                    PriceData.Add(AddData);
                }

                //Set values to local collection
                var ProductData = new BillProductsClass()
                {
                    product_id = PData.id,
                    product_code = PData.code,
                    product_name = PData.name,
                    product_count = ProcutCountTX.Text,
                    product_value = ProductSellValueTX.Number,
                    product_discount = DiscountPercentTX.Text,
                    product_discountvalue = DiscountValueTX.Number,
                    product_selectedPrice = PriceCatCB.SelectedIndex,
                    Product_prices = PriceData,
                    product_tax = PData.iva == "1" ? DataConfig.ivaValue : PData.iac == "1" ? DataConfig.iacValue : PData.iva5 == "1" ? DataConfig.iva5Value : "0",
                    product_type = PData.unity_type

                };

                //Add collection to list
                BillProducts.Add(ProductData);
            }
            else
            {
                var Product = BillProducts.Find(x => x.product_id == ProductID);
                Product.product_count = ProcutCountTX.Text;
                Product.product_value = ProductSellValueTX.Number;
                Product.product_discount = DiscountPercentTX.Text;
                Product.product_discountvalue = DiscountValueTX.Number;
                Product.product_selectedPrice = PriceCatCB.SelectedIndex;
            }          

            //Close popup    
            ProductsGrid.Items.Refresh();
            SetBillValues();

            ExitInPopUp_Click(null, null);
        }

        private void DiscountPercentTX_GotKeyboardFocus(object sender, object e)
        {
            DiscountPercentTX.SelectAll();
        }


        //Set values and labels values
        private void SetBillValues()
        {
            //Set values
            TotalValue = 0;
            IacValue = 0;
            IvaValue = 0;
            Iva5Value = 0;

            TotalProducts = BillProducts.Count;
            for (int i = 0; i < TotalProducts; i++)
            {
                TotalValue += BillProducts[i].product_total;

                IacValue += BillProducts[i].product_tax == DataConfig.iacValue ? Math.Round((BillProducts[i].product_total * Convert.ToInt32(DataConfig.iacValue)) / 100) : 0;
                IvaValue += BillProducts[i].product_tax == DataConfig.ivaValue ? Math.Round((BillProducts[i].product_total * Convert.ToInt32(DataConfig.ivaValue)) / 100) : 0;
                Iva5Value += BillProducts[i].product_tax == DataConfig.iva5Value ? Math.Round((BillProducts[i].product_total * Convert.ToInt32(DataConfig.iva5Value)) / 100) : 0;
            }

            Subtotal = ((TotalValue - IacValue) - IvaValue) - Iva5Value;

            //Set labels

            TotalProductsLB.Content = TotalProducts.ToString();
            SubtotalLB.Content = string.Format("{0:C0}", Subtotal);
            IVALB.Content = string.Format("{0:C0}", IvaValue);
            IACLB.Content = string.Format("{0:C0}", IacValue);
            IVAL5B.Content = string.Format("{0:C0}", Iva5Value);
            TotalLB.Content = string.Format("{0:C0}", TotalValue);

            FinalIn.IsEnabled = TotalProducts > 0 ? true : false;
            PrintCotiBT.IsEnabled = TotalProducts > 0 ? true : false;
            MenuDelete.IsEnabled = TotalProducts > 0 ? true : false;
            MenuEdit.IsEnabled = TotalProducts > 0 ? true : false;
        }

        //Delete product from grid
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            //Get button control
            Button Control = (Button)sender;

            //Get product information
            var Product = BillProducts.Find(x => x.product_id == Control.Tag.ToString());

            MessageBox.Show(Product.product_id);

            if (MessageBox.Show($"A continuación se eliminara el producto '{Product.product_name}'{Environment.NewLine}¿Desea continuar?", "Eliminar producto", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                BillProducts.Remove(Product);
                ProductsGrid.Items.Refresh();
                SetBillValues();
            }           
        }

        //Final bill opep popup
        public void FinalIn_Click(object sender, RoutedEventArgs e)
        {
            if (BillDock.IsEnabled == true)
            {
                BillDock.IsEnabled = false;
                BillFinishPopUp.IsOpen = true;

                SetFinishBillContent();
            }            
        }

        private void SetFinishBillContent()
        {
            PaymentType.SelectedIndex = 0;

            PaymentType.IsEnabled = CLientCredit;

            PaymentMethodCB.SelectedIndex = 0;
            PaymentTX.Number = 0;
            BillDatePicker.SelectedDate = DateTime.Now.AddDays(30);
            
            CommentTX.Clear();
            if (PaymentType.IsEnabled)
            {
                PaymentType.Focus();
            }
            else
            {
                PaymentTX.Focus();
            }
            
        }

        //Finish bill popup exit click
        private void BillFinishExitBT_Click(object sender, RoutedEventArgs e)
        {
            BillDock.IsEnabled = true;
            BillFinishPopUp.IsOpen = false;

            FinalIn.Focus();
        }

        //Payment type change / datetime visibility
        private void PaymentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BillFinishPopUp.IsOpen == true)
            {
                BillDateGrid.Visibility = PaymentType.SelectedIndex == 0 ? Visibility.Collapsed : Visibility.Visible;
                UpdatePaymentValues();
            }
        }

        //Payment value changed
        private void PaymentTX_NumberChanged(object sender, EventArgs e)
        {
            if (PaymentTX.IsFocused == true)
            {
                InValue = PaymentTX.Number;
                UpdatePaymentValues();
            }
        }

        private void UpdatePaymentValues()
        {
            //Set values
            InChangeValue = TotalValue > InValue || PaymentMethodCB.SelectedIndex != 0 ? 0 : InValue - TotalValue ;
            inLeftValue = PaymentType.SelectedIndex == 0 || TotalValue < InValue ? 0 : TotalValue - InValue;
            
            //Set labels
            PaymentChange.Content = string.Format("{0:C0}", InChangeValue);
            PaymentLeft.Content = string.Format("{0:C0}", inLeftValue);

            //Verify values
            FinishBillBT.IsEnabled = PaymentType.SelectedIndex == 0 && InValue < TotalValue ? false : true;            
        }

        //Method changed
        private void PaymentMethodCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BillFinishPopUp.IsOpen == true)
            {                
                UpdatePaymentValues();
            }
        }

        //Finish ticket
        private async void FinishBillBT_Click(object sender, RoutedEventArgs e)
        {
            //Create product data object
            var Pdata = new List<NovaAPI.APITickets.ProductClass>();

            foreach (var item in BillProducts)
            {
                var AddData = new NovaAPI.APITickets.ProductClass()
                {
                    product_count = item.product_count,
                    product_id = item.product_id,
                    product_pricevalue = item.product_value.ToString(),
                    product_tax = item.product_tax,
                    product_h = HStatus,
                    product_discountpercent = item.product_discount,
                    product_discountvalue = item.product_discountvalue.ToString(),
                    product_code = item.product_code,
                    product_name = item.product_name,
                    product_total = item.product_total.ToString(),
                    product_unity = item.product_type
                    

                };
                Pdata.Add(AddData);
            }

            //Create ticket data object
            var data = new NovaAPI.APITickets.TicketData
            {

                branch_id = DataConfig.WorkPlaceID.ToString(),
                box_id = DataConfig.WorkPointID.ToString(),
                box_movement_id = NovaAPI.APIBoxMovements.movement_id,
                client_id = SelectedClientID,
                payment_type = PaymentType.SelectedIndex.ToString(),
                payment_method = PaymentMethodCB.SelectedIndex.ToString(),
                expiration_date = PaymentType.SelectedIndex != 0 ? BillDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd") : "",
                ticket_total = TotalValue.ToString(),
                ticket_iva = IvaValue.ToString(),
                ticket_iva5 = Iva5Value.ToString(),
                ticket_iac = IacValue.ToString(),
                ticket_totalpayment = InValue.ToString(),
                ticket_changepayment = InChangeValue.ToString(),
                ticket_leftpayment = inLeftValue.ToString(),
                ticket_comment = CommentTX.Text,
                ticket_h = HStatus,
                products = Pdata,
                client_name = ClientNameLB.Content.ToString(),
                client_documentid = ClientDocumentLB.Content.ToString(),
                client_address = ClientAddressLB.Content.ToString(),
                client_phones = ClientPhonesLB.Content.ToString(),
                ticket_subtotal = Subtotal.ToString()


            };

            TicketData = data;

            //Create Json string request
            string requestData = JsonConvert.SerializeObject(data);

            //Create request
            bool Response = await NovaAPI.APITickets.GetValues("1", DataConfig.LocalAPI, requestData);

            if (Response)
            {
                PrintIn.Visibility = Visibility.Visible;
                InProductBT.Visibility = Visibility.Collapsed;
                InProductBT.IsEnabled = false;
                FinalIn.Visibility = Visibility.Collapsed;
                FinalIn.IsEnabled = false;

                PrintCotiBT.IsEnabled = false;

                BillFinishExitBT_Click(null, null);

                PrintIn.Focus();
            }
            else
            {
                BillFinishExitBT_Click(null, null);
                MessageBox.Show(NovaAPI.APITickets.Message);
            }
        }

        private void ProcutCountTX_GotKeyboardFocus(dynamic sender, dynamic e)
        {
            ProcutCountTX.SelectAll();
        }


        //Print Ticket
        private void PrintIn_Click(object sender, RoutedEventArgs e)
        {
            TicketData.ticket_copy = "0";
            PrintFunctions.PrintFunctions.Print(Classes.Enums.PrintPages.FinalTicket, TicketData);
            
            //Print ticket COPY
            if (MessageBox.Show("¿Desea imprimir copia de la factura?","Imprimir", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                TicketData.ticket_copy = "1";
                PrintFunctions.PrintFunctions.Print(Classes.Enums.PrintPages.FinalTicket, TicketData);
            }

            ClearForm();
        }

        public void SetH()
        {
            if (ProductsGrid.Items.Count > 0)
            {
                MessageBox.Show("No se puede cambiar el estado de la factura cuando existen productos de otro estado (H)");
                return;
            }
            HStatus = HStatus == "0" ? "1" : "0";
            HLabel.Visibility = HStatus == "1" ? Visibility.Visible : Visibility.Collapsed;
            FilterGrid.Background = HStatus == "1" ? (SolidColorBrush)Application.Current.TryFindResource("HBackground") : new SolidColorBrush(Colors.White);

        }

        //Client name changed
        private void ClientNameTX_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveClientBT.IsEnabled = ClientNameTX.Text.Length < 5 ? false : true;
        }

        //Save client information
        private async void SaveClientBT_Click(object sender, RoutedEventArgs e)
        {
            //Get/Set client parameters
            var Data = new NovaAPI.APIClient.ClientClass();
            Data.name = ClientNameTX.Text;
            Data.type = ClientIDType.SelectedIndex.ToString();
            Data.documentid = ClientIDTX.Text;
            Data.phone = ClientPhoneTX.Text;
            Data.cancredit = "0";
            Data.address = "";
            Data.celphone = "";

            try
            {
                NovaAPI.APIClient.clients.Clear();
            }
            catch (Exception)
            {
            }

            //rol json data
            string requestData = JsonConvert.SerializeObject(Data);

            bool response;

            //Modify / Create request
            response = await NovaAPI.APIClient.GetValues("1", DataConfig.LocalAPI, requestData);

            //Request response
            if (response)
            {
                var ClientData = new NovaAPI.APIClient.ClientClass();
                ClientData.name = Data.name;
                ClientData.documentid = Data.documentid;
                ClientData.type = Data.type;
                ClientData.address = Data.address;
                ClientData.phone = Data.phone;
                ClientData.mail = Data.mail;
                ClientData.celphone = Data.celphone;
                ClientData.cancredit = Data.cancredit;
                ClientData.id = NovaAPI.APIClient.LastID;
                
                NovaAPI.APIClient.clients.Add(ClientData);

                //Set IEnumerable LIST
                IEnumerable<string> ClientList = Enumerable.Empty<string>();

                for (int i = 0; i < NovaAPI.APIClient.clients.Count; i++)
                {
                    ClientList = ClientList.Concat(new[] { NovaAPI.APIClient.clients[i].name });
                }

                //Set list of clients to textbox
                ClientFilterTX.SetValue(AutoCompleteBehavior.AutoCompleteItemsSource, ClientList);

                ClientFilterTX.Text = ClientData.name;
                ClientFilterTX.Focus();

                ClientExitBT_Click(null, null);

                SearchClientBT_Click(null, null);
            }
            else
            {
                ClientExitBT_Click(null, null);
                MessageBox.Show($"Error al crear el cliente, INFO: {Environment.NewLine}{NovaAPI.APIClient.Message}");               
            }
        }

        //Client DocumentID regex        
        private void ClientIDTX_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        //Bill search
        private async void BillSearchBT_Click(object sender, RoutedEventArgs e)
        {
            //Try to clear existent branch list
            try
            {
                NovaAPI.APITickets.tickets.Clear();
            }
            catch (Exception) { }

            //Send request
            var Data = new NovaAPI.APITickets.DataClass();
            Data.h = HStatus;
            Data.filter = BillSearchTX.Text;
            Data.from = "0";
            Data.ticket_box = DataConfig.WorkPointID.ToString();
            Data.ticket_branch = DataConfig.WorkPlaceID.ToString();

            string requestData = JsonConvert.SerializeObject(Data);

            //Load branch 
            bool response = await NovaAPI.APITickets.GetValues("2", DataConfig.LocalAPI, requestData);
            if (response)
            {
                //Set rol data to DataGrid
                TicketsDataGrid.ItemsSource = NovaAPI.APITickets.tickets;
                TicketsDataGrid.Items.Refresh();
            }
            else
            {
                BillSearchExitBT_Click(null, null);
                //On load error
                MessageBox.Show($"Se produjo un error al cargar los datos, INFO: {Environment.NewLine}{NovaAPI.APITickets.Message}");                
                return;
            }
        }

        //Search Bill BT click
        private void BillSearchExitBT_Click(object sender, RoutedEventArgs e)
        {
            BillSearchPopUp.IsOpen = false;
            BillDock.IsEnabled = true;

            BillSearchTX.Clear();            

            try
            {
                TicketsDataGrid.ItemsSource = null;
                TicketsDataGrid.Items.Refresh();
            }
            catch (Exception)
            {
              
            }

            ClientFilterTX.Focus();
        }

        //Search bill
        private void BillSearchTX_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BillSearchBT_Click(null, null);
                e.Handled = true;
            }
        }

        //Print bill copy
        private void PrintCopyBill_Click(object sender, RoutedEventArgs e)
        {
            var data = NovaAPI.APITickets.tickets.Find(x => x.id == ((Button)sender).Tag.ToString());
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


        //Bill contexMenu options
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button Control = new Button
                {
                    Tag = ((BillProductsClass)ProductsGrid.SelectedItem).product_id
                };
                Delete_Click(Control, null);
            }
            catch (Exception)
            {

            }         
        }

        private void MenuEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button Control = new Button
                {
                    Tag = ((BillProductsClass)ProductsGrid.SelectedItem).product_id
                };
                Modify_Click(Control, null);
            }
            catch (Exception)
            {

            }
        }


        //Modify product
        private void Modify_Click(object sender, RoutedEventArgs e)
        { 
            //Get button control
            Button Control = (Button)sender;
            var Product = BillProducts.Find(x => x.product_id == Control.Tag.ToString());

            if (BillDock.IsEnabled == true)
            {
                EditProduct = true;

                BillDock.IsEnabled = false;
                AddProductPopUp.IsOpen = true;

                ProductSearchTX.IsEnabled = false;
                ProductCB.IsEnabled = false;

                ProcutCountTX.Text = Product.product_count;
                ProductSellValueTX.Number = Product.product_value;
                DiscountPercentTX.Text = Product.product_discount;
                DiscountValueTX.Number = Product.product_discountvalue;
                PriceCatCB.SelectedIndex = Product.product_selectedPrice;


                ProductID = Product.product_id;

                LoadProducts(Product.product_name);
                ProcutCountTX.Focus();
            }
        }
    }
}
