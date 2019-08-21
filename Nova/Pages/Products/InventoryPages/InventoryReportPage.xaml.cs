using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Nova.Pages.Products.InventoryPages
{
    /// <summary>
    /// Lógica de interacción para InventoryInPage.xaml
    /// </summary>
    public partial class InventoryReportPage : Page
    {
        //H column definition
        DataGridTextColumn H = new DataGridTextColumn();

        int Pagination = 1;
        string SelectedProductID = "";


        Thumb thumb = new Thumb();
        Thumb thumbTransfer = new Thumb();


        //SET H VISIBILITY
        public void SetH(bool status)
        {            
            H.Visibility = status == true ? Visibility.Visible : Visibility.Collapsed;
            Page foundPage = GeneralFunctions.FindChild<Page>(Application.Current.MainWindow, "invPage");
            if(foundPage != null)
            {
                Grid BkGrid = (Grid)foundPage.FindName("BackgroundGrid");
                BkGrid.Background = status == true ? (SolidColorBrush)Application.Current.TryFindResource("HBackground") : (SolidColorBrush)Application.Current.TryFindResource("PanelBackground");
            }
            BranchCB.IsEnabled = status == true ? false : true;
            TransferStack.IsEnabled = status == true ? false : true;

            Pagination = 1;
            LoadProducts();
        }


        public InventoryReportPage()
        {
            InitializeComponent();

            //Set view model Class 1
            DataContext = new HViewModel(this, 1);

            //Popup thumb drag move
            thumb.Width = 0;
            thumb.Height = 0;

            //transfer popup thumb size
            thumbTransfer.Width = 0;
            thumbTransfer.Height = 0;


            ContentCanvas.Children.Add(thumb);
            InventoryInPopUp.MouseDown += ProductPopUp_MouseDown;
            TransferPopUp.MouseDown += TransferPopUp_MouseDown;
            thumb.DragDelta += Thumb_DragDelta;

            ContentCanvasTransfer.Children.Add(thumbTransfer);
            thumbTransfer.DragDelta += ThumbTransfer_DragDelta;


        }

        private void ThumbTransfer_DragDelta(object sender, DragDeltaEventArgs e)
        {
            TransferPopUp.HorizontalOffset += e.HorizontalChange;
            TransferPopUp.VerticalOffset += e.VerticalChange;
        }

        private void TransferPopUp_MouseDown(object sender, MouseButtonEventArgs e)
        {
            thumbTransfer.RaiseEvent(e);
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            InventoryInPopUp.HorizontalOffset += e.HorizontalChange;
            InventoryInPopUp.VerticalOffset += e.VerticalChange;
        }

        private void ProductPopUp_MouseDown(object sender, MouseButtonEventArgs e)
        {
            thumb.RaiseEvent(e);
        }

        //Page loaded
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
            catch (Exception){}

            //Clear product data
            try
            {
                NovaAPI.APIInventory.products.Clear();
            }
            catch (Exception) {}

            //Request branch information
            bool BranchRequest = await NovaAPI.APIBranch.GetValues("4", DataConfig.LocalAPI, null);

            if (!BranchRequest)
            {
                MessageBox.Show($"Error al cargar datos {NovaAPI.APIBranch.Message}");
            }

            //Create branch data columns
            for (int i = 0; i < NovaAPI.APIBranch.branch.Count; i++)
            {
                foreach (var item in ProductsGrid.Columns)
                {
                    if (item.Header != null)
                    {
                        if (item.Header.ToString() == NovaAPI.APIBranch.branch[i].name)
                        {
                            FilterTX.Focus();
                            LoadProducts();
                            return;
                        }
                    }                    
                }

                DataGridTextColumn column = new DataGridTextColumn();
                column.Header = NovaAPI.APIBranch.branch[i].name;
                column.Binding = new Binding($"branch_data[{i}].count");
                column.CellStyle = (Style)Application.Current.TryFindResource("DataGridCellStyledRight");
                column.MinWidth = 100;
                column.HeaderStyle = (Style)Application.Current.TryFindResource("DataGridHeaderStyledRight");

                ProductsGrid.Columns.Insert(ProductsGrid.Columns.Count - 2, column);

            }

            //H Columns
            H.Header = "H";
            H.Binding = new Binding($"branch_data[{NovaAPI.APIBranch.branch.Count}].count");
            H.CellStyle = (Style)Application.Current.TryFindResource("DataGridCellStyledRight");
            H.MinWidth = 100;
            H.HeaderStyle = (Style)Application.Current.TryFindResource("DataGridHeaderStyledRight");
            H.Visibility = Visibility.Collapsed;

            //Insert H Column
            ProductsGrid.Columns.Insert(ProductsGrid.Columns.Count - 2, H);

            FilterTX.Focus();            
            LoadProducts();          
        }

        //Load inventory data
        private async void LoadProducts(string Page = null, string Filter = null)
        {

            //Set loading grid visibility
            LoadingGrid.Visibility = Visibility.Visible;
            Spinner.Spin = true;

            //Try to clear existent category list
            try
            {
                NovaAPI.APIInventory.products.Clear();
            }
            catch (Exception) { }

            //Send request
            var Data = new NovaAPI.APIInventory.DataClass();
            Data.h = H.Visibility == Visibility.Visible ? "1" : "0"; 
            Data.filter = Filter;
            Data.from = Page;

            string requestData = JsonConvert.SerializeObject(Data);

            bool Response = await NovaAPI.APIInventory.GetValues("4", DataConfig.LocalAPI, requestData);

            if (Response)
            {
                ProductsGrid.ItemsSource = NovaAPI.APIInventory.products;
                ProductsGrid.Items.Refresh();
                TotalProducts.Content = NovaAPI.APIInventory.Count;
                double Pages = (Convert.ToInt32(NovaAPI.APIInventory.Count) / 15);
                double TotalPages = Math.Floor(Pages);

                SetPagination(TotalPages);

            }
            else
            {
                //On load error or data null
                ProductsGrid.Items.Refresh();
                //If user is searching 
                if (FilterGrid.Opacity == 1)
                {
                    FilterTX.Focus();
                }
                else
                {
                    MessageBox.Show($"Se produjo un error al cargar los datos, INFO: {Environment.NewLine}{NovaAPI.APIProdructs.Message}");
                }
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
            LoadProducts(((Pagination - 1) * 15).ToString(), null);
        }

        //On filter textbox key enter pressed
        private void FilterTX_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int parsedValue;
                if (!int.TryParse(FilterTX.Text, out parsedValue))
                {
                    Pagination = 1;
                    LoadProducts(null, FilterTX.Text);
                }
                else
                {
                    FilterTX.Clear();
                    Pagination = 1;
                    LoadProducts(null, FilterTX.Text);
                }
                e.Handled = true;
            }
        }

        //Filter BT Click
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Pagination = 1;

            LoadProducts(null, FilterTX.Text);
        }

        //Refresh product datagrid
        private void RefreshProducts_Click(object sender, RoutedEventArgs e)
        {
            Pagination = 1;           

            LoadProducts();
            FilterTX.Clear();
            FilterTX.Focus();
        }

        //PopUp Exit
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //Enable controls
            InventoryInPopUp.IsOpen = false;
            InventoryDock.IsEnabled = true;
            PaginationGrid.IsEnabled = true;

            await Task.Delay(200);

            //Clear container
            ErrorMessage.Visibility = Visibility.Collapsed;
            InventoryInBT.IsEnabled = false;
            CommentTX.Clear();

            //Clear offset values
            InventoryInPopUp.HorizontalOffset = 0;
            InventoryInPopUp.VerticalOffset = 0;
        }
        
        //Inventory Adjustment
        private void Modify_Click(object sender, RoutedEventArgs e)
        {
            Button control = (Button)sender;
            var ProductData = NovaAPI.APIInventory.products.Find(x => x.id == control.Tag.ToString());

            SelectedProductID = control.Tag.ToString();

            //Set name
            PopUpProductName.Content = ProductData.name;                        

            //Set branch data
            BranchCB.ItemsSource = ProductData.branch_data;
            BranchCB.Items.Refresh();

            //Set selection
            try
            {
                BranchCB.SelectedIndex = 0;
            }
            catch (Exception) { }


            ActualCountTX.Text = H.Visibility == Visibility.Visible ? ProductData.branch_data[NovaAPI.APIBranch.branch.Count].count : ((NovaAPI.APIInventory.BranchClass)BranchCB.SelectedItem).count;
            NewCountTX.Text = ActualCountTX.Text;

            //Disable controls
            InventoryDock.IsEnabled = false;
            PaginationGrid.IsEnabled = false;
            InventoryInPopUp.IsOpen = true;

            //Focus branch selection
            BranchCB.Focus();            
        }

        //Change branch selection on ajustment
        private void BranchCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InventoryInPopUp.IsOpen == true)
            {
                try
                {
                    ActualCountTX.Text = ((NovaAPI.APIInventory.BranchClass)BranchCB.SelectedItem).count;
                    NewCountTX.Text = ActualCountTX.Text;
                }
                catch (Exception) { }
            }
        }

        //Only numbers accept
        private void TX_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        //Observations requiered
        private void CommentTX_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Set inventory ajustment button status
            InventoryInBT.IsEnabled = NewCountTX.Text.Length == 0 || CommentTX.Text.Length < 5 || ActualCountTX.Text == NewCountTX.Text ? false : true;
        }
        
        //Adjustment button
        private async void InventoryInBT_Click(object sender, RoutedEventArgs e)
        {
            InventoryInBT.IsEnabled = false;

            //Get ajustment data
            var Data = new NovaAPI.APIInventory.BranchClass();
            Data.count = NewCountTX.Text;
            Data.id = H.Visibility == Visibility.Visible ? "0" : (NovaAPI.APIBranch.branch.Find(x => x.name == ((NovaAPI.APIInventory.BranchClass)BranchCB.SelectedItem).name)).id;
            Data.product_id = SelectedProductID;
            Data.comment = CommentTX.Text;

            string requestData = JsonConvert.SerializeObject(Data);

            bool Response = await NovaAPI.APIInventory.GetValues("5", DataConfig.LocalAPI, requestData);
            

            if (!Response)
            {
                //On Error
                MessageBox.Show(NovaAPI.APIInventory.Message);
            }
            else
            {
                //H index or branch
                int index = H.Visibility == Visibility.Visible ? NovaAPI.APIBranch.branch.Count : BranchCB.SelectedIndex;

                //Update product list, branch count
                NovaAPI.APIInventory.products.Find(x => x.id == SelectedProductID).branch_data[index].count = Data.count;
                ProductsGrid.Items.Refresh();
            }
            
            //Exit popup event
            Button_Click_1(sender, e);
        }

        //MoveFocus
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

        //Select all text on keyboard focus
        private void NewCountTX_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        //Open transfer popup
        private void Transfer_Click(object sender, RoutedEventArgs e)
        {
            var control = (Button)sender;
            if (NovaAPI.APIBranch.branch.Count <= 1)
            {
                MessageBox.Show("No hay suficientes sucursales para realizar el traslado");
                return;
            }

            SelectedProductID = control.Tag.ToString();

            //Set controls branch transfer data
            FromBranchCB.ItemsSource = NovaAPI.APIBranch.branch;
            ToBranchCB.ItemsSource = NovaAPI.APIBranch.branch;

            //Set index
            FromBranchCB.SelectedIndex = 0;
            ToBranchCB.SelectedIndex = 1;
                       
            //Disable controls
            InventoryDock.IsEnabled = false;
            PaginationGrid.IsEnabled = false;
            TransferPopUp.IsOpen = true;
            
            //Focus control
            FromBranchCB.Focus();
        }

        //Close transfer popup
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

            //Close poppup and enable controls
            TransferPopUp.IsOpen = false;
            InventoryDock.IsEnabled = true;
            PaginationGrid.IsEnabled = true;

            //Clear container
            ErrorMessageTransfer.Visibility = Visibility.Collapsed;
            TransferBT.IsEnabled = false;
            TransferCommentTX.Clear();
            TransferCountTX.Text = "0";

            //Clear offset values
            TransferPopUp.HorizontalOffset = 0;
            TransferPopUp.VerticalOffset = 0;
        }

        //transfer branch selection validation
        private void TransferBranchCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                TransferBT.IsEnabled = TransferCommentTX.Text.Length < 5 || Convert.ToInt32(TransferCountTX.Text) == 0 || FromBranchCB.SelectedIndex == ToBranchCB.SelectedIndex ? false : true;
            }
            catch (Exception) { }
        }

        //Transfer text changed validation
        private void TransferCountTX_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TransferBT.IsEnabled = TransferCommentTX.Text.Length < 5 || Convert.ToInt32(TransferCountTX.Text) == 0 || FromBranchCB.SelectedIndex == ToBranchCB.SelectedIndex ? false : true;
            }
            catch (Exception) { }
        }

        //Transfer button
        private async void TransferBT_Click(object sender, RoutedEventArgs e)
        {
            TransferBT.IsEnabled = false;

            int FromCount = Convert.ToInt32(NovaAPI.APIInventory.products.Find(x => x.id == SelectedProductID).branch_data[FromBranchCB.SelectedIndex].count);
            int ToCount = Convert.ToInt32(NovaAPI.APIInventory.products.Find(x => x.id == SelectedProductID).branch_data[ToBranchCB.SelectedIndex].count);

            //Get transfer data
            var Data = new NovaAPI.APIInventory.ProductTransfer();
            Data.branch_id = DataConfig.WorkPlaceID.ToString();
            Data.product_id = SelectedProductID;
            Data.from_branch = ((NovaAPI.APIBranch.BranchClass)FromBranchCB.SelectedItem).id;
            Data.to_branch = ((NovaAPI.APIBranch.BranchClass)ToBranchCB.SelectedItem).id;
            Data.product_count = TransferCountTX.Text;
            Data.comment = TransferCommentTX.Text;

            int PCount = Convert.ToInt32(Data.product_count);

            //negative count protection 
            if (FromCount == 0 || FromCount - PCount < 0)
            {
                Button_Click_2(sender, e);
                MessageBox.Show("No se puede realizar el traslado, no hay suficientes unidades en la sucursal de origen");
                return;
            }

            //generate json string request
            string requestData = JsonConvert.SerializeObject(Data);

            //send request
            bool Response = await NovaAPI.APIInventory.GetValues("1", DataConfig.LocalAPI, requestData);


            if (!Response)
            {
                //On Error
                //Close PopUp
                Button_Click_2(sender, e);
                MessageBox.Show(NovaAPI.APIInventory.Message);
            }
            else
            {
                //Update product list, branchs count     
                FromCount += -PCount;
                ToCount += PCount;

                NovaAPI.APIInventory.products.Find(x => x.id == SelectedProductID).branch_data[FromBranchCB.SelectedIndex].count = FromCount.ToString();
                NovaAPI.APIInventory.products.Find(x => x.id == SelectedProductID).branch_data[ToBranchCB.SelectedIndex].count = ToCount.ToString();

                ProductsGrid.Items.Refresh();

                //Close PopUp
                Button_Click_2(sender, e);

                //Print ticket
                if (MessageBox.Show("¿Desea imprimir ticket de traslado?","Traslado de inventario",MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    //Set ticket data from product data
                    var TicketData = new PrintFunctions.TransferTicket {
                        
                    };

                    //Call print function
                    PrintFunctions.PrintFunctions.PrintTranslateTicket(TicketData);
                }
            }            
        }
    }
}
