using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CurrencyTextBoxControl;
using System.Windows.Controls.Primitives;

namespace Nova.Pages.Products
{
    /// <summary>
    /// Lógica de interacción para ProductCategoryPage.xaml
    /// </summary>
    public partial class ProductCategoryPage : Page
    {
        string H = "0";

        string CategorySelectIndex = "";
        string PriceSelectIndex = "";
        string ProductSelectIndex = "";

        int Pagination = 1;
        double TotalPages = 0;

        Thumb thumb = new Thumb();

        //SET H VISIBILITY
        public void SetH(bool status)
        {
            H = status == true ? "1" : "0";
            BackgroundGrid.Background = status == true ? (SolidColorBrush)Application.Current.TryFindResource("HBackground") : (SolidColorBrush)Application.Current.TryFindResource("PanelBackground");
            HLabel.Visibility = status == true ? Visibility.Visible : Visibility.Collapsed;
            TotalPages = 0;
            Pagination = 1;

            LoadProducts();
        }

        public ProductCategoryPage()
        {
            InitializeComponent();
            DataContext = new HViewModel(this, 5);

            //Set loading grid visibility
            LoadingProductGrid.Visibility = Visibility.Visible;
            ProductSpinner.Spin = true;

            //Popup thumb drag move
            thumb.Width = 0;
            thumb.Height = 0;

            ContentCanvas.Children.Add(thumb);
            ProductPopUp.MouseDown += ProductPopUp_MouseDown;
            thumb.DragDelta += Thumb_DragDelta;
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ProductPopUp.HorizontalOffset += e.HorizontalChange;
            ProductPopUp.VerticalOffset += e.VerticalChange;
        }

        private void ProductPopUp_MouseDown(object sender, MouseButtonEventArgs e)
        {
            thumb.RaiseEvent(e);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Load data category and products
            await LoadCategory();
            await LoadPrices();
            LoadProducts();

            ProductCatCB.ItemsSource = NovaAPI.APICategory.category;

            NewProductBT.Focus();
        }

        //MoveFocus Function
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

        #region Category

        private async Task LoadCategory()
        {
            //Set loading grid visibility
            LoadingGrid.Visibility = Visibility.Visible;
            Spinner.Spin = true;

            //Try to clear existent category list
            try
            {
                NovaAPI.APICategory.category.Clear();
            }
            catch (Exception) { }

            //Send request
            bool Response = await NovaAPI.APICategory.GetValues("4", DataConfig.LocalAPI);

            if (Response)
            {
                CategoryGrid.ItemsSource = NovaAPI.APICategory.category;
                CategoryGrid.Items.Refresh();
            }
            else
            {

                //Set loading grid visibility
                LoadingGrid.Visibility = Visibility.Collapsed;
                Spinner.Spin = false;
                //On load error
                MessageBox.Show($"Se produjo un error al cargar los datos, INFO: {Environment.NewLine}{NovaAPI.APICategory.Message}");
                RefreshCatBT.IsEnabled = true;
                return;
            }

            await Task.Delay(100);


            //Set loading grid visibility
            LoadingGrid.Visibility = Visibility.Collapsed;
            Spinner.Spin = false;
        }

        //Clear category form data
        private void ResetCategoryForm()
        {
            if (CategoryFormGrid.Opacity == 1)
            {
                CategoryFormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
            }
            CategoryNameTX.Clear();
            CategoryCodeTX.Clear();
            SaveCategoryBT.IsEnabled = false;
        }

        private void RefreshCatBT_Click(object sender, RoutedEventArgs e)
        {
            //Reset data
            ResetCategoryForm();

            //Load Category data
            LoadCategory();
        }

        //Category form visibility logic
        private void NewCategoryBT_Click(object sender, RoutedEventArgs e)
        {
            //From grid animation
            if (CategoryFormGrid.Opacity == 0)
            {
                CategoryFormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("PopUpGrid"));
                CategoryNameTX.Focus();
                SaveCategoryBT.IsEnabled = true;

            }
            else if (CategoryNameTX.Text.Length == 0)
            {
                CategoryFormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                ResetCategoryForm();
            }
            //Clear category form values
            if (CategoryNameTX.Text.Length > 0)
            {
                ResetCategoryForm();
            }
        }

        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            //Get button control
            Button Control = (Button)sender;

            var CategoryData = NovaAPI.APICategory.category.Find(x => x.id == Control.Tag.ToString());

            if (CategoryFormGrid.Opacity == 0)
            {
                CategoryFormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("PopUpGrid"));
            }

            //Set category information to controls
            CategoryNameTX.Text = CategoryData.name;
            CategoryCodeTX.Text = CategoryData.code;


            //Set selected category id index for edition save
            CategorySelectIndex = Control.Tag.ToString();

            //Focus editable category
            CategoryNameTX.Focus();
            SaveCategoryBT.IsEnabled = true;
        }

        private async void SaveCategoryBT_Click(object sender, RoutedEventArgs e)
        {
            NewCategoryBT.Focus();

            if (CategoryNameTX.Text.Length == 0 || CategoryNameTX.Text.Length < 4)
            {
                MessageBox.Show("El nombre de la categoria no puede estar en blanco o ser inferior a 4 caracteres");
                CategoryNameTX.Focus();
                return;
            }

            var Category = NovaAPI.APICategory.category.Find(x => x.code == CategoryCodeTX.Text || x.name == CategoryNameTX.Text );

            if (Category != null && CategorySelectIndex != Category.id)
            {
                MessageBox.Show("El nombre o codigo de la categoria ya esta en uso");
                CategoryNameTX.Focus();
                return;
            }

            if (CategoryCodeTX.Text.Length == 0)
            {
                MessageBox.Show("El codigo de la categoria no puede estar en blanco");
                CategoryNameTX.Focus();
                return;
            }

            //Get/Set Category parameters
            var Data = new NovaAPI.APICategory.CategoryClass();
            Data.id = CategorySelectIndex;
            Data.name = CategoryNameTX.Text;
            Data.code = CategoryCodeTX.Text;


            //rol json data
            string requestData = JsonConvert.SerializeObject(Data);

            bool response;

            //Modify / Create request
            if (Data.id.Length > 0)
            {
                response = await NovaAPI.APICategory.GetValues("2", DataConfig.LocalAPI, requestData);

            }
            else
            {
                response = await NovaAPI.APICategory.GetValues("1", DataConfig.LocalAPI, requestData);
            }

            //Request response
            if (response)
            {
                if (Data.id.Length > 0)
                {
                    //On Category modified
                    var CategoryData = NovaAPI.APICategory.category.Find(x => x.id == Data.id);
                    CategoryData.name = Data.name;
                    CategoryData.code = Data.code;
                    CategoryData.products = Data.products;
                    CategoryGrid.Items.Refresh();
                    CategoryFormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                    ResetCategoryForm();
                }
                else
                {
                    //On new category created response
                    var CategoryData = new NovaAPI.APICategory.CategoryClass();
                    CategoryData.name = Data.name;
                    CategoryData.code = Data.code;
                    CategoryData.products = "0";
                    CategoryData.id = NovaAPI.APISupplier.LastID;

                    CategoryFormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                    ResetCategoryForm();

                    NovaAPI.APICategory.category.Add(CategoryData);

                    //Reload rol data
                    LoadCategory();
                }
            }
            else
            {
                MessageBox.Show($"Error al crear la categoria, INFO: {Environment.NewLine}{NovaAPI.APICategory.Message}");
            }
        }

        private async void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryFormGrid.Opacity != 0)
            {
                ResetCategoryForm();
                CategoryFormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
            }

            //Get button control
            Button Control = (Button)sender;

            //Get category information
            var CategoryData = NovaAPI.APICategory.category.Find(x => x.id == Control.Tag.ToString());

            if (CategoryData.products != "0")
            {
                MessageBox.Show("No se puede eliminar, la categoria cuenta con productos asignados");
                return;
            }

            if (MessageBox.Show($"A continuación se eliminara la categoria '{CategoryData.name}'{Environment.NewLine}¿Desea continuar?", "Eliminar categoria", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var Data = new NovaAPI.APICategory.CategoryClass();
                Data.id = CategoryData.id;

                //Delete category
                string requestData = JsonConvert.SerializeObject(Data);
                bool response = await NovaAPI.APICategory.GetValues("3", DataConfig.LocalAPI, requestData);

                if (response)
                {
                    NovaAPI.APICategory.category.Remove(CategoryData);
                    CategoryGrid.Items.Refresh();
                }
                else
                {
                    MessageBox.Show($"Error al eliminar la categoria, INFO: {Environment.NewLine}{NovaAPI.APICategory.Message}");
                }
            }
        }
        #endregion
        
        #region Products

       

        private async void LoadProducts(string Page = null, string Filter = null)
        {

            //Set loading grid visibility
            LoadingProductGrid.Visibility = Visibility.Visible;
            ProductSpinner.Spin = true;

            //Try to clear existent category list
            try
            {
                NovaAPI.APIProdructs.products.Clear();
            }
            catch (Exception) { }

            //Send request
            var Data = new NovaAPI.APIProdructs.DataClass();
            Data.h = H;
            Data.filter = Filter;
            Data.from = Page;

            string requestData = JsonConvert.SerializeObject(Data);

            bool Response = await NovaAPI.APIProdructs.GetValues("4", DataConfig.LocalAPI, requestData);
            
            if (Response)
            {
                ProductsGrid.ItemsSource = NovaAPI.APIProdructs.products;
                ProductsGrid.Items.Refresh();
                TotalProducts.Content = NovaAPI.APIProdructs.Count;
                double Pages = (Convert.ToInt32(NovaAPI.APIProdructs.Count) / 15);
                TotalPages = Math.Floor(Pages);

                SetPagination(TotalPages);
             
            }
            else
            {
                //On load error or data null
                ProductsGrid.Items.Refresh();               
                RefreshProducts.IsEnabled = true;
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
                LoadingProductGrid.Visibility = Visibility.Collapsed;
                ProductSpinner.Spin = false;
                return;
            }

            await Task.Delay(100);

            //Set loading grid visibility
            LoadingProductGrid.Visibility = Visibility.Collapsed;
            ProductSpinner.Spin = false;
        }

        //Search BT
        private void FilterBT_Click(object sender, RoutedEventArgs e)
        {
            TotalPages = 0;
            Pagination = 1;

            LoadProducts(null, FilterTX.Text);            
        }

        //Toggle filter
        private async void FilterBT_Click_Form(object sender, RoutedEventArgs e)
        {
            if (FormGrid.Opacity != 0)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                //ResetForm();
                await Task.Delay(100);
            }

            FilterTX.Clear();

            if (FilterGrid.Opacity == 0)
            {
                FilterGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("PopUpGrid"));
                FilterTX.Focus();
            }
            else
            {
                FilterGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
            }
        }

        //Filter on search textbox
        private void FilterTX_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int parsedValue;
                if (!int.TryParse(FilterTX.Text, out parsedValue))
                {                    
                    LoadProducts(null, FilterTX.Text);
                }
                else
                {                    
                    LoadProducts(null, FilterTX.Text);
                    FilterTX.Clear();
                }                
                e.Handled = true;
            }
        }


        //Refresh products data
        private void RefreshProducts_Click(object sender, RoutedEventArgs e)
        {
            if (FormGrid.Opacity == 1)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
            }

            if (FilterGrid.Opacity == 1)
            {
                FilterGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
            }

            TotalPages = 0;
            Pagination = 1;

            ResetForm();

            LoadProducts();
            NewProductBT.Focus();
        }


        private void ResetForm()
        {
            ProductCodeTX.Clear();
            ProductNameTX.Clear();
            ProductCatCB.SelectedIndex = 0;
            ProductSelectIndex = "";
            ProductCostTX.Number = 0;
            ProductPriceTX.Number = 0;
            MinStockTX.Text = "0";
            MaxStockTX.Text = "0";
            PricesBT.IsEnabled = false;
            ProductCatCB.SelectedIndex = 0;
            SaveProductBT.IsEnabled = false;
        }

        //Toggle product form
        private async void NewProductBT_Click(object sender, RoutedEventArgs e)
        {           

            if (FilterGrid.Opacity != 0)
            {
                FilterGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                await Task.Delay(100);
            }

            //From grid animation
            if (FormGrid.Opacity == 0)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("PopUpGrid"));
                ProductCodeTX.Focus();
                SaveProductBT.IsEnabled = true;

            }
            else if (ProductCodeTX.Text.Length == 0)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                ResetForm();                
            }
            //Clear products values
            if (ProductCodeTX.Text.Length > 0)
            {
                ResetForm();
                SaveProductBT.IsEnabled = true;
            }
        }

        //Only numbers
        private void ProductCodeTX_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }


        //Open popup and disable product modifications
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ProductPopUp.IsOpen = true;
            ProductTab.IsEnabled = false;
        }


        //Edit product button toggle
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            //Get button control
            Button Control = (Button)sender;

            if (FilterGrid.Opacity != 0)
            {
                FilterGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
            }

            var ProductData = NovaAPI.APIProdructs.products.Find(x => x.id == Control.Tag.ToString());

            if (FormGrid.Opacity == 0)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("PopUpGrid"));
            }

            //Set popup name
            PopUpProductName.Content = ProductData.name;
            PopUpProductValue.Content = string.Format("Precio venta general: {0:C0}", Convert.ToInt32(ProductData.sellprice));

            //Set product information to controls
            ProductCodeTX.Text = ProductData.code;
            ProductNameTX.Text = ProductData.name;
            ProductCatCB.SelectedItem = NovaAPI.APICategory.category.Find(x => x.id == ProductData.category);
            ProductCostTX.Number = Convert.ToInt32(ProductData.costprice);
            ProductPriceTX.Number = Convert.ToInt32(ProductData.sellprice);
            ProductTypeCB.SelectedIndex = Convert.ToInt32(ProductData.unity_type);
            MinStockTX.Text = ProductData.minstock;
            MaxStockTX.Text = ProductData.maxstock;


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

            //Set selected product id index for edition save
            ProductSelectIndex = Control.Tag.ToString();

            //Focus editable product
            ProductCodeTX.Focus();
            SaveProductBT.IsEnabled = true;
            PricesBT.IsEnabled = true;
        }

        /// <summary>
        /// See datagrid button, toggle row details visibility
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void See_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < ProductsGrid.Items.Count; i++)
            {
                DataGridRow row = (DataGridRow)ProductsGrid.ItemContainerGenerator.ContainerFromIndex(i);
                if (ProductsGrid.SelectedIndex == i)
                {
                    //Toggle row details
                    row.DetailsVisibility = row.DetailsVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
                }
                else if (row != null)
                {
                    row.DetailsVisibility = Visibility.Collapsed;
                }
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (FormGrid.Opacity != 0)
            {
                ResetForm();
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
            }

            //Get button control
            Button Control = (Button)sender;

            //Get User information
            var Product = NovaAPI.APIProdructs.products.Find(x => x.id == Control.Tag.ToString());


            if (MessageBox.Show($"A continuación se eliminara el producto '{Product.name}'{Environment.NewLine}¿Desea continuar?", "Eliminar producto", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var Data = new NovaAPI.APIProdructs.ProductClass();
                Data.id = Product.id;

                //Delete user
                string requestData = JsonConvert.SerializeObject(Data);
                bool response = await NovaAPI.APIProdructs.GetValues("3", DataConfig.LocalAPI, requestData);

                if (response)
                {
                    NovaAPI.APIProdructs.products.Remove(Product);
                    ProductsGrid.Items.Refresh();
                }
                else
                {
                    MessageBox.Show($"Error al eliminar el producto, INFO: {Environment.NewLine}{NovaAPI.APIProdructs.Message}");
                }
            }
        }

        private async void SaveProductBT_Click(object sender, RoutedEventArgs e)
        {
            NewProductBT.Focus();

            if (ProductCodeTX.Text.Length == 0)
            {
                MessageBox.Show("El codigo del producto no puede estar en blanco");
                ProductCodeTX.Focus();
                return;
            }
            else if (ProductNameTX.Text.Length == 0 || ProductNameTX.Text.Length < 5)
            {
                MessageBox.Show("El nombre del producto no puede estar en blanco o ser inferior a 5 caracteres");
                ProductNameTX.Focus();
                return;
            }

            //Get/Set product parameters
            var Data = new NovaAPI.APIProdructs.ProductClass();
            Data.id = ProductSelectIndex;
            Data.code = ProductCodeTX.Text;
            Data.name = ProductNameTX.Text;
            Data.category = ((NovaAPI.APICategory.CategoryClass)ProductCatCB.SelectedItem).id;
            Data.costprice = ProductCostTX.Number.ToString();
            Data.sellprice = ProductPriceTX.Number.ToString();
            Data.minstock = MinStockTX.Text;
            Data.maxstock = MaxStockTX.Text;
            Data.unity_type = ProductTypeCB.SelectedIndex.ToString();
            Data.hproduct = H;
            Data.iva = TaxIvaRB.IsChecked == true ? "1" : "0";
            Data.iac = TaxIacRB.IsChecked == true ? "1" : "0";
            Data.iva5 = TaxIva5RB.IsChecked == true ? "1" : "0";


            //rol json data
            string requestData = JsonConvert.SerializeObject(Data);

            bool response;

            //Modify / Create request
            if (Data.id.Length > 0)
            {
                response = await NovaAPI.APIProdructs.GetValues("2", DataConfig.LocalAPI, requestData);

            }
            else
            {
                response = await NovaAPI.APIProdructs.GetValues("1", DataConfig.LocalAPI, requestData);
            }

            //Request response
            if (response)
            {
                if (Data.id.Length > 0)
                {
                    //On Product modified
                    var ProductData = NovaAPI.APIProdructs.products.Find(x => x.id == Data.id);
                    ProductData.code = Data.code;
                    ProductData.name = Data.name;
                    ProductData.category = Data.category;
                    ProductData.costprice = Data.costprice;
                    ProductData.sellprice = Data.sellprice;
                    ProductData.minstock = Data.minstock;
                    ProductData.maxstock = Data.maxstock;
                    ProductData.unity_type = Data.unity_type;
                    ProductData.iac = Data.iac;
                    ProductData.iva5 = Data.iva5;
                    ProductData.iva = Data.iva;
                    ProductsGrid.Items.Refresh();
                    FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                    ResetForm();
                }
                else
                {
                    //On new produict created response
                    var ProductData = new NovaAPI.APIProdructs.ProductClass();
                    ProductData.code = Data.code;
                    ProductData.name = Data.name;
                    ProductData.category = Data.category;
                    ProductData.costprice = Data.costprice;
                    ProductData.sellprice = Data.sellprice;
                    ProductData.minstock = Data.minstock;
                    ProductData.maxstock = Data.maxstock;
                    ProductData.iac = Data.iac;
                    ProductData.iva5 = Data.iva5;
                    ProductData.iva = Data.iva;
                    ProductData.unity_type = Data.unity_type;
                    ProductData.id = NovaAPI.APIProdructs.LastID;

                    FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                    ResetForm();

                    NovaAPI.APIProdructs.products.Add(ProductData);

                    //reset pagination
                    TotalPages = 0;
                    Pagination = 1;

                    //Reload product data
                    LoadProducts(null, ProductData.code);
                }
            }
            else
            {
                MessageBox.Show($"Error al crear el producto, INFO: {Environment.NewLine}{NovaAPI.APIProdructs.Message}");
            }
        }

        #endregion

        #region Product Prices

        //Set popup content
        private async void ProductPopUp_Opened(object sender, EventArgs e)
        {
            //Get/Set product parameters
            var Data = new NovaAPI.APIPrice.PriceClass();
            Data.id = ProductSelectIndex;
            //rol json data
            string requestData = JsonConvert.SerializeObject(Data);

            try
            {
                NovaAPI.APIProdructPrice.price.Clear();
            }
            catch (Exception) { }
            //Load prices data
            bool Response = await NovaAPI.APIProdructPrice.GetValues("5", DataConfig.LocalAPI, requestData);
            if (Response)
            {
                SolidColorBrush ContainerColor =  (SolidColorBrush)Application.Current.TryFindResource("PanelLightBackground");
                //Create prices controls
                for (int i = 0; i < NovaAPI.APIProdructPrice.price.Count; i++)
                {
                    ContainerColor = ContainerColor == (SolidColorBrush)Application.Current.TryFindResource("PanelLightBackground") ? new SolidColorBrush(Colors.White) : (SolidColorBrush)Application.Current.TryFindResource("PanelLightBackground");

                    Grid Container = new Grid
                    {
                        Height = 30,
                        Background = ContainerColor
                    };

                    //Price Name
                    Label Name = new Label
                    {
                        Content = NovaAPI.APIProdructPrice.price[i].type == "0" ? $"{NovaAPI.APIProdructPrice.price[i].name}:" : $"{NovaAPI.APIProdructPrice.price[i].name} ({NovaAPI.APIProdructPrice.price[i].value}%):",
                        FontSize = 14,
                        Opacity = .7,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Margin = new Thickness(0,0,(PriceStack.ActualWidth / 2) + 50 ,0)
                        
                    };

                    Label PercentValue = new Label();
                    CurrencyTextBox GeneralValue = new CurrencyTextBox();

                    //If price type 0
                    if (NovaAPI.APIProdructPrice.price[i].type == "0")
                    {
                        GeneralValue.Name = $"value{NovaAPI.APIProdructPrice.price[i].id}";
                        GeneralValue.Number = Convert.ToInt32(NovaAPI.APIProdructPrice.price[i].value);
                        GeneralValue.StringFormat = "C0";
                        GeneralValue.FontSize = 14;
                        GeneralValue.HorizontalContentAlignment = HorizontalAlignment.Left;
                        GeneralValue.Width = 100;
                        GeneralValue.HorizontalAlignment = HorizontalAlignment.Left;
                        GeneralValue.Margin = new Thickness(PriceStack.ActualWidth / 2, 3, 0, 3);

                        Container.Children.Add(GeneralValue);

                    }//If price type 1
                    else
                    {
                        double ProductValue = Convert.ToInt32(NovaAPI.APIProdructs.products.Find(x => x.id == Data.id).pSell);
                        double DiscountValue = Convert.ToInt32(NovaAPI.APIProdructPrice.price[i].value);

                        double TotalDiscount = ProductValue != 0 && DiscountValue != 0 ? Math.Round((ProductValue * DiscountValue) / 100) : 0;


                        string price =  string.Format("{0:C0}", ProductValue - TotalDiscount);

                        PercentValue.Content = $"{price} \t \t (Descuento: {string.Format("{0:C0}",TotalDiscount)})";
                        PercentValue.FontSize = 14;
                        PercentValue.Opacity = .7;
                        PercentValue.Margin = new Thickness(PriceStack.ActualWidth / 2, 0, 0, 0);

                        Container.Children.Add(PercentValue);
                    }
                    
                    Container.Children.Add(Name);
                    PriceStack.Children.Add(Container);
                }

                PopUpSaveBT.IsEnabled = true;
            }
            else
            {
                ErrorMessage.Content = NovaAPI.APIProdructPrice.Message;
                ErrorMessage.Visibility = Visibility.Visible;
            }
        }

        //Exit PopUp
        private async void ExitClick(object sender, RoutedEventArgs e)
        {
            ProductPopUp.IsOpen = false;
            ProductTab.IsEnabled = true;

            await Task.Delay(200);

            //Clear container
            PriceStack.Children.Clear();
            ErrorMessage.Visibility = Visibility.Collapsed;
            PopUpSaveBT.IsEnabled = false;

            //Clear offset values
            ProductPopUp.HorizontalOffset = 0;
            ProductPopUp.VerticalOffset = 0;
        }

        //Popup save button
        private async void PopUpSaveBT_Click(object sender, RoutedEventArgs e)
        {
            //List controls
            foreach (var item in PriceStack.Children)
            {
                Grid container = (Grid)item;
                
                foreach (var Grid_Childs in container.Children)
                {
                    var control = (Control)Grid_Childs;
                    if (control.Name.Contains("value"))
                    {
                        //Save price content
                        CurrencyTextBox PriceBox = (CurrencyTextBox)control;

                        string Price_id = PriceBox.Name.Replace("value", "");

                        var Data = new NovaAPI.APIProdructPrice.PriceModify
                        {
                            price_id = Price_id,
                            price_value = PriceBox.Number.ToString(),
                            product_id = ProductSelectIndex
                        };

                        string requestData = JsonConvert.SerializeObject(Data);

                        //Send modify request
                        await NovaAPI.APIProdructs.GetValues("6", DataConfig.LocalAPI, requestData);                       
                    }
                }
            }

            //Exit
            ProductPopUp.IsOpen = false;
            ProductTab.IsEnabled = true;
            //Clear container
            PriceStack.Children.Clear();
            ErrorMessage.Visibility = Visibility.Collapsed;
            PopUpSaveBT.IsEnabled = false;
        }


        

        #endregion

        #region Product Operations

        #endregion

        #region PricesCategory

        //Prices logic
        private async Task LoadPrices()
        {
            //Set loading grid visibility
            LoadingPricesGrid.Visibility = Visibility.Visible;
            PriceSpinner.Spin = true;

            //Try to clear existent category list
            try
            {
                NovaAPI.APIPrice.price.Clear();
            }
            catch (Exception) { }

            //Send request
            bool Response = await NovaAPI.APIPrice.GetValues("4", DataConfig.LocalAPI);

            if (Response)
            {
                PricesGrid.ItemsSource = NovaAPI.APIPrice.price;
                PricesGrid.Items.Refresh();
            }

            await Task.Delay(100);


            //Set loading grid visibility
            LoadingPricesGrid.Visibility = Visibility.Collapsed;
            PriceSpinner.Spin = false;
        }

        private void NewPriceBT_Click(object sender, RoutedEventArgs e)
        {
            //From grid animation
            if (PriceFormGrid.Opacity == 0)
            {
                PriceFormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("PopUpGrid"));
                PriceNameTX.Focus();
                SavePriceBT.IsEnabled = true;

            }
            else if (PriceNameTX.Text.Length == 0)
            {
                PriceFormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                ResetPriceForm();
                SavePriceBT.IsEnabled = false;
            }
            //Clear price form values
            if (PriceNameTX.Text.Length > 0)
            {
                ResetPriceForm();
            }
        }

        private void ResetPriceForm()
        {
            PriceSelectIndex = "";
            PriceNameTX.Clear();
            PriceTypeCB.SelectedIndex = 0;
            PriceValueTX.Text = "0";
            PriceValueTX.IsEnabled = false;
            PriceTypeCB.IsEnabled = true;
            SavePriceBT.IsEnabled = false;
        }

        private void RefreshPriceBT_Click(object sender, RoutedEventArgs e)
        {
            ResetPriceForm();
            LoadPrices();
        }
        

        private async void DeletePrice_Click(object sender, RoutedEventArgs e)
        {
            if (PriceFormGrid.Opacity != 0)
            {
                ResetPriceForm();
                PriceFormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
            }

            //Get button control
            Button Control = (Button)sender;

            //Get category information
            var PriceData = NovaAPI.APIPrice.price.Find(x => x.id == Control.Tag.ToString());


            if (MessageBox.Show($"A continuación se eliminara la categoria '{PriceData.name}'{Environment.NewLine}¿Desea continuar?", "Eliminar categoria", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var Data = new NovaAPI.APIPrice.PriceClass();
                Data.id = PriceData.id;

                //Delete category
                string requestData = JsonConvert.SerializeObject(Data);
                bool response = await NovaAPI.APIPrice.GetValues("3", DataConfig.LocalAPI, requestData);

                if (response)
                {
                    NovaAPI.APIPrice.price.Remove(PriceData);
                    PricesGrid.Items.Refresh();
                }
                else
                {
                    MessageBox.Show($"Error al eliminar la categoria, INFO: {Environment.NewLine}{NovaAPI.APIPrice.Message}");
                }
            }
        }

        private void EditPrice_Click(object sender, RoutedEventArgs e)
        {
            //Get button control
            Button Control = (Button)sender;

            var PriceData = NovaAPI.APIPrice.price.Find(x => x.id == Control.Tag.ToString());

            if (PriceFormGrid.Opacity == 0)
            {
                PriceFormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("PopUpGrid"));
            }

            //Set category information to controls
            PriceNameTX.Text = PriceData.name;
            PriceTypeCB.SelectedIndex = Convert.ToInt32(PriceData.type);
            PriceTypeCB.IsEnabled = false;
            PriceValueTX.Text = PriceData.value;


            //Set selected category id index for edition save
            PriceSelectIndex = Control.Tag.ToString();

            //Focus editable category
            PriceNameTX.Focus();
            SavePriceBT.IsEnabled = true;
        }

        private async void SavePriceBT_Click(object sender, RoutedEventArgs e)
        {
            NewPriceBT.Focus();

            if (PriceNameTX.Text.Length == 0 || PriceNameTX.Text.Length < 4)
            {
                MessageBox.Show("El nombre de la categoria no puede estar en blanco o ser inferior a 4 caracteres");
                PriceNameTX.Focus();
                return;
            }

            int value = Convert.ToInt32(PriceValueTX.Text);
            if (value > 100)
            {
                MessageBox.Show("El rango del valor debe estar entre 0 - 100 %");
                PriceValueTX.Focus();
                return;
            }

            //Get/Set Category parameters
            var Data = new NovaAPI.APIPrice.PriceClass();
            Data.id = PriceSelectIndex;
            Data.name = PriceNameTX.Text;
            Data.type = PriceTypeCB.SelectedIndex.ToString();
            Data.value = PriceValueTX.Text;


            //rol json data
            string requestData = JsonConvert.SerializeObject(Data);

            bool response;

            //Modify / Create request
            if (Data.id.Length > 0)
            {
                response = await NovaAPI.APIPrice.GetValues("2", DataConfig.LocalAPI, requestData);

            }
            else
            {
                response = await NovaAPI.APIPrice.GetValues("1", DataConfig.LocalAPI, requestData);
            }

            //Request response
            if (response)
            {
                if (Data.id.Length > 0)
                {
                    ////On Category modified
                    var PriceData = NovaAPI.APIPrice.price.Find(x => x.id == Data.id);
                    PriceData.name = Data.name;
                    PriceData.value = Data.value;
                    PricesGrid.Items.Refresh();
                    PriceFormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                    ResetPriceForm();
                }
                else
                {
                    //On new category created response
                    var PriceData = new NovaAPI.APIPrice.PriceClass();
                    PriceData.name = Data.name;
                    PriceData.type = Data.type;
                    PriceData.value = Data.value;
                    PriceData.id = NovaAPI.APISupplier.LastID;

                    PriceFormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                    ResetPriceForm();

                    NovaAPI.APIPrice.price.Add(PriceData);

                    //Reload rol data
                    LoadPrices();
                }
            }
            else
            {
                MessageBox.Show($"Error al crear la categoria, INFO: {Environment.NewLine}{NovaAPI.APIPrice.Message}");
            }
        }

        private void PriceTypeCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                switch (PriceTypeCB.SelectedIndex)
                {
                    case 0: // Adj price selected
                        PriceValueTX.IsEnabled = false;
                        PriceValueTX.Text = "0";
                        break;
                    case 1: //Value price selected
                        PriceValueTX.IsEnabled = true;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception) { }
            
           
        }
        #endregion

        #region pagination


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
            catch (Exception) {
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

            var LastBT = (Button)PaginationStack.Children[PaginationStack.Children.Count -1];
            LastBT.Style = (Style)Application.Current.TryFindResource("PaginationRightButton");
        }

        private void Page_Click(object sender, RoutedEventArgs e)
        {
            Button page = (Button)sender;
            Pagination = Convert.ToInt32(page.Tag.ToString());
            LoadProducts(((Pagination - 1) * 15).ToString(), null);
        }

        #endregion

        private void MinStockTX_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            MinStockTX.SelectAll();
        }

        private void MaxStockTX_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            MaxStockTX.SelectAll();
        }
    }
}
