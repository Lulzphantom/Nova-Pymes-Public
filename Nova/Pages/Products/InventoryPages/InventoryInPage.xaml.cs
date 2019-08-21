using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFTextBoxAutoComplete;

namespace Nova.Pages.Products.InventoryPages
{
    /// <summary>
    /// Lógica de interacción para InventoryInPage.xaml
    /// </summary>
    public partial class InventoryInPage : Page
    {
        string SelectedSupplierID = "";
        int PId = 0;
        Thumb thumb = new Thumb();
        Thumb thumb2 = new Thumb();

        int Total = 0;

        List<NovaAPI.APIInventory.ProductInClass> ProductInGrid;

        public InventoryInPage()
        {
            InitializeComponent();
            //Popup thumb drag move
            thumb.Width = 0;
            thumb.Height = 0;

            thumb2.Width = 0;
            thumb2.Height = 0;

            ContentCanvas.Children.Add(thumb);
            ContentCanvasFinish.Children.Add(thumb2);
            InventoryInPopUp.MouseDown += ProductPopUp_MouseDown;
            InventoryFinishPopUp.MouseDown += InventoryFinishPopUp_MouseDown;
            thumb.DragDelta += Thumb_DragDelta;
            thumb2.DragDelta += Thumb2_DragDelta;
        }

        private void Thumb2_DragDelta(object sender, DragDeltaEventArgs e)
        {
            InventoryFinishPopUp.HorizontalOffset += e.HorizontalChange;
            InventoryFinishPopUp.VerticalOffset += e.VerticalChange;
        }

        private void InventoryFinishPopUp_MouseDown(object sender, MouseButtonEventArgs e)
        {
            thumb2.RaiseEvent(e);
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ProductInGrid = new List<NovaAPI.APIInventory.ProductInClass>();
            ProductsGrid.ItemsSource = ProductInGrid;


            //Set loading grid visibility
            LoadingGrid.Visibility = Visibility.Visible;
            Spinner.Spin = true;

            GetData();
        }

        private async void GetData()
        {
            //Set loading grid visibility
            LoadingGrid.Visibility = Visibility.Visible;
            Spinner.Spin = true;

            //Get Branch List
            try
            {
                //Clear de actual branch list
                NovaAPI.APIBranch.branch.Clear();
            }
            catch (Exception) { }

            bool Response = await NovaAPI.APIBranch.GetValues("4", DataConfig.LocalAPI);
            if (Response)
            {
                BranchCB.ItemsSource = NovaAPI.APIBranch.branch;
                BranchCB.SelectedIndex = BranchCB.Items.IndexOf(NovaAPI.APIBranch.branch.Find(x => x.id == DataConfig.WorkPlaceID.ToString()));
            }


            //Get Suppliers list

            try
            {
                //Clear de actual supplier list
                NovaAPI.APISupplier.suppliers.Clear();
            }
            catch (Exception) { }

            await NovaAPI.APISupplier.GetValues("4", DataConfig.LocalAPI);

            //Set IEnumerable LIST
            IEnumerable<string> BranchList = Enumerable.Empty<string>();

            for (int i = 0; i < NovaAPI.APISupplier.suppliers.Count; i++)
            {
                if (NovaAPI.APISupplier.suppliers[i].status == "1")
                {
                    BranchList = BranchList.Concat(new[] { NovaAPI.APISupplier.suppliers[i].comercialname });
                }
            }

            //Set list to textbox
            FilterTX.SetValue(AutoCompleteBehavior.AutoCompleteItemsSource, BranchList);

            FilterTX.Focus();

            await Task.Delay(100);

            //Set loading grid visibility
            LoadingGrid.Visibility = Visibility.Collapsed;
            Spinner.Spin = false;
        }

        //Supplier Search
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Clear data
            ClearForm();

            //Search supplier value
            var Data = NovaAPI.APISupplier.suppliers.Find(x => x.comercialname.ToLower().Contains(FilterTX.Text.ToLower()) ||
                                                            x.documentid.ToLower().Contains(FilterTX.Text.ToLower()));
            //Set supplier data
            if (Data != null && Data.status == "1")
            {
                //Supplier found
                FilterTX.Text = Data.comercialname;
                SelectedSupplierID = Data.id;
                SelectSupplier(Data);
                InProductBT.IsEnabled = true;
                SuppBillTX.IsEnabled = true;
                SuppBillTX.Focus();
            }
            else
            {
                //Supplier not found
                ClearForm();
                FilterTX.Focus();
                FilterTX.SelectAll();
            }
        }

        //Filter textbox enter press
        private void FilterTX_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //Clear data
                ClearForm();

                //Search supplier value
                var Data = NovaAPI.APISupplier.suppliers.Find(x => x.comercialname.ToLower().Contains(FilterTX.Text.ToLower()));

                if (Data != null && Data.status == "1")
                {
                    SelectedSupplierID = Data.id;
                    SelectSupplier(Data);
                    InProductBT.IsEnabled = true;
                    SuppBillTX.IsEnabled = true;
                    SuppBillTX.Focus();
                }
                e.Handled = true;
            }
        }

        //Refresh all data and clear form data
        private void RefreshBT_Click(object sender, RoutedEventArgs e)
        {
            GetData();
            ClearForm();

            FilterTX.Focus();
            FilterTX.SelectAll();
        }

        //Clear supplier information, payment and products
        private void ClearForm()
        {
            //Set controls
            PrintIn.Visibility = Visibility.Collapsed;
            SuppBillTX.Clear();

            InProductBT.Visibility = Visibility.Visible;
            FinalIn.Visibility = Visibility.Visible;

            ProductInGrid.Clear();
            ProductsGrid.Items.Refresh();

            ProductsGrid.IsEnabled = true;

            //Clear Content
            SocialnameLB.Content = "";
            ComercialNameLB.Content = "";
            IdLB.Content = "";
            PhonesLB.Content = "";
            AddressLB.Content = "";
            ContactLB.Content = "";
            
            //Reset form 
            InProductBT.IsEnabled = false;
            SuppBillTX.IsEnabled = false;
            SuppBillTX.Clear();
            TotalProductsLB.Content = "0";
            TotalCostLB.Content = string.Format("{0:C0}", 0);
        }

        //Set Supplier data
        private void SelectSupplier(NovaAPI.APISupplier.SupplierClass Data)
        {            
            SocialnameLB.Content = Data.socialname;
            ComercialNameLB.Content = Data.comercialname;
            IdLB.Content = Data.idcomplete;
            PhonesLB.Content = Data.phones;
            AddressLB.Content = Data.address;
            ContactLB.Content = Data.contact;
        }


        private void ResetPopUP()
        {
            ProductSearchTX.Clear();
            ProductCB.ItemsSource = null;
            BranchCB.SelectedIndex = BranchCB.Items.IndexOf(NovaAPI.APIBranch.branch.Find(x => x.id == DataConfig.WorkPlaceID.ToString()));
            ProcutCountTX.Text = "1";
            ProductCostTX.Number = 0;
            InventoryInBT.IsEnabled = false;
        }

        //PopUp Exit
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            InventoryInPopUp.IsOpen = false;
            InventoryDock.IsEnabled = true;

            await Task.Delay(200);

            //Clear container
            ErrorMessage.Visibility = Visibility.Collapsed;

            //Clear offset values
            InventoryInPopUp.HorizontalOffset = 0;
            InventoryInPopUp.VerticalOffset = 0;

            //resume focus
            InProductBT.Focus();
            ResetPopUP();
        }

        //Open PopUp on add products
        private void In_Click(object sender, RoutedEventArgs e)
        {          

            InventoryDock.IsEnabled = false;
            InventoryInPopUp.IsOpen = true;

            //Focus search TX
            ProductSearchTX.Focus();
        }


        //Bill key down
        private void SuppBillTX_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                InProductBT.Focus();
                e.Handled = true;
            }
        }

        //Search TX on Enter key
        private void ProductSearchTX_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Button_Click_1(null, null);
                e.Handled = true;
            }
            if (e.Key == Key.Enter)
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
                MoveFocus(sender,e);
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
            Data.h = "0";

            string requestData = JsonConvert.SerializeObject(Data);

            bool Response = await NovaAPI.APIProdructs.GetValues("4", DataConfig.LocalAPI, requestData);

            if (Response)
            {
                ProductCB.ItemsSource = NovaAPI.APIProdructs.products;
                ProductCB.Items.Refresh();
                ProductCB.SelectedIndex = 0;
               

                //Enabled save button
                InventoryInBT.IsEnabled = true;
            }
        }


        private void ProductCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InventoryInPopUp.IsOpen == true)
            {
                //Set product info
                var ProductData = (NovaAPI.APIProdructs.ProductClass)ProductCB.SelectedItem;

                ProductCostTX.Number = Convert.ToInt32(ProductData.costprice);
                //Set taxes
                if (ProductData.iva == "0" && ProductData.iac == "0" && ProductData.iva5 == "0")
                {
                    TaxNoneRB.IsChecked = true;
                }
                else
                {
                    TaxIvaRB.IsChecked = ProductData.iva == "1" ? true : false;
                    TaxIva5RB.IsChecked = ProductData.iva5 == "1" ? true : false;
                    TaxIacRB.IsChecked = ProductData.iac == "1" ? true : false;
                }
            }
            
        }

        //Select all text in textbox on focus
        private void TX_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).Focus();
            ((TextBox)sender).SelectAll();
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

        //Add product record on list
        private void InventoryInBT_Click(object sender, RoutedEventArgs e)
        {

            var Data = new NovaAPI.APIInventory.ProductInClass();
            var PData = (NovaAPI.APIProdructs.ProductClass)ProductCB.SelectedItem;

            Data.product_code = PData.code;
            Data.product_name = PData.name;
            Data.product_count = ProcutCountTX.Text;
            Data.product_cost = ProductCostTX.Number;
            Data.product_iva = TaxIvaRB.IsChecked == true ? "1" : "0";
            Data.product_iac = TaxIacRB.IsChecked == true ? "1" : "0";
            Data.product_iva5 = TaxIva5RB.IsChecked == true ? "1" : "0";
            Data.product_id = PId.ToString();
            Data.product_db_id = PData.id;
            Data.product_branch = ((NovaAPI.APIBranch.BranchClass)BranchCB.SelectedItem).id;

            //ID Step
            PId += 1;

            if (PData.costprice != ProductCostTX.Number.ToString())
            {
                InventoryInPopUp.IsOpen = false;
                //Update product price
                if (MessageBox.Show($"Se ha modificado el valor de costo de producto '{Data.product_name}'{Environment.NewLine}¿desea actualizarlo?", "Actualizar precio costo de producto", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {

                    //Update request

                }
            }

            ProductInGrid.Add(Data);
            ProductsGrid.Items.Refresh();
            Button_Click_1(sender, e);

            ValueUpdate();            
        }

        //Set counter values
        private void ValueUpdate()
        {
            Total = 0;

            foreach (var item in ProductsGrid.Items)
            {
                Total += ((NovaAPI.APIInventory.ProductInClass)item).product_subtotal;
            }

            TotalCostLB.Content = string.Format("{0:C0}", Total);
            TotalProductsLB.Content = ProductsGrid.Items.Count.ToString();

            FinalIn.IsEnabled = ProductsGrid.Items.Count > 0 ? true : false;
        }


        //DataGrid Item Delete
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            //Get button control
            Button Control = (Button)sender;

            //Get product information
            var Product = ProductInGrid.Find(x => x.product_id == Control.Tag.ToString());


            if (MessageBox.Show($"A continuación se eliminara el producto '{Product.product_name}'{Environment.NewLine}¿Desea continuar?", "Eliminar producto", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                ProductInGrid.Remove(Product);
                ProductsGrid.Items.Refresh();
                ValueUpdate();
            }            
        }


        //Finish inventory in PopUP
        private void FinalIn_Click(object sender, RoutedEventArgs e)
        {
            InventoryDock.IsEnabled = false;
            InventoryFinishPopUp.IsOpen = true;

            PaymentType.Focus();
        }


        //Inventory Close
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            InventoryDock.IsEnabled = true;
            InventoryFinishPopUp.IsOpen = false;

            InProductBT.Focus();
        }

        private void PaymentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                BillDateGrid.Visibility = PaymentType.SelectedIndex == 0 ? Visibility.Collapsed : Visibility.Visible;
                BillDatePicker.SelectedDate = DateTime.Now.AddDays(30);
            }
            catch (Exception) { }            
        }




        //Inventory Finish BT
        private async void Button_Click_3(object sender, RoutedEventArgs e)
        {            
            //Create inventory data
            var Data = new NovaAPI.APIInventory.InventoryInData
            {
                branch = DataConfig.WorkPlaceID.ToString(),
                comment = CommentTX.Text,
                supplier_id = SelectedSupplierID,
                bill = SuppBillTX.Text,
                value = Total.ToString(),
                payment = PaymentTX.Number.ToString(),
                payment_type = PaymentType.SelectedIndex.ToString(),
                payment_method = PaymentMethodCB.SelectedIndex.ToString(),
                expiration_date = PaymentType.SelectedIndex == 0 ? "" : BillDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
                products = ProductInGrid
            };

            string requestData = JsonConvert.SerializeObject(Data);

            //Send inventory IN request
            bool Response = await NovaAPI.APIInventory.GetValues("3", DataConfig.LocalAPI, requestData);
                       
            //Set controls
            PrintIn.Visibility = Visibility.Visible;
            SuppBillTX.IsEnabled = false;

            InProductBT.Visibility = Visibility.Collapsed;
            FinalIn.Visibility = Visibility.Collapsed;

            //On Error
            if (!Response)
            {
                MessageBox.Show($"Error al guardar: {NovaAPI.APIInventory.Message}");
            }

            //Reset controls

            ProductsGrid.IsEnabled = false;
            InventoryDock.IsEnabled = true;
            InventoryFinishPopUp.IsOpen = false;
            ClearExpendPopUp();

            PrintIn.Focus();

        }

        //Update payment left
        private void PaymentTX_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                PaymentLeft.Content = string.Format("{0:C0}", Total - PaymentTX.Number);
            }
            catch (Exception) { }
           
        }

        private void ClearExpendPopUp()
        {
            PaymentType.SelectedIndex = 0;
            PaymentMethodCB.SelectedIndex = 0;
            PaymentTX.Number = 0;
            CommentTX.Clear();
        }


        //Print Ticket - PENDENT!!
        private void PrintIn_Click(object sender, RoutedEventArgs e)
        {
            PrintFunctions.TransferTicket Test = new PrintFunctions.TransferTicket();
            PrintFunctions.PrintFunctions.PrintTranslateTicket(Test);
            ClearForm();
        }

        private void ProductIVATX_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

    }


}

