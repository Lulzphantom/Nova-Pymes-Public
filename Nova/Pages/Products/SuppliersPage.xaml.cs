using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Data.Linq.SqlClient;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;

namespace Nova.Pages.Products
{
    /// <summary>
    /// Lógica de interacción para SuppliersPage.xaml
    /// </summary>
    public partial class SuppliersPage : Page
    {
        string SelectedIndex = "";
        public SuppliersPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private async void LoadData()
        {
            //Set loading grid visibility
            LoadingGrid.Visibility = Visibility.Visible;
            Spinner.Spin = true;

            //Try to clear existent supplier list
            try
            {
                NovaAPI.APISupplier.suppliers.Clear();
            }
            catch (Exception) { }

            //Send request
            bool Response = await NovaAPI.APISupplier.GetValues("4", DataConfig.LocalAPI);

            if (Response)
            {
                SuppliersGrid.ItemsSource = NovaAPI.APISupplier.suppliers;
                SuppliersGrid.Items.Refresh();
                TotalSuppliers.Content = NovaAPI.APISupplier.suppliers.Count.ToString();
            }
            else
            {
                //Set loading grid visibility
                LoadingGrid.Visibility = Visibility.Collapsed;
                Spinner.Spin = false;
                //On load error
                MessageBox.Show($"Se produjo un error al cargar los datos, INFO: {Environment.NewLine}{NovaAPI.APISupplier.Message}");
                RefreshBT.IsEnabled = true;
                return;
            }

            await Task.Delay(100);
            //Set loading grid visibility
            LoadingGrid.Visibility = Visibility.Collapsed;
            Spinner.Spin = false;
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            //Hide filter
            if (FilterGrid.Opacity != 0)
            {
                FilterGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));

            }

            //Get button control
            Button Control = (Button)sender;

            var Supplier = NovaAPI.APISupplier.suppliers.Find(x => x.id == Control.Tag.ToString());

            if (FormGrid.Opacity == 0)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("PopUpGrid"));
            }

            //Set supplier information to controls
            SuppSocialNameTX.Text = Supplier.socialname;
            SuppComercialNameTX.Text = Supplier.comercialname;
            SuppidTypeCB.SelectedIndex = Convert.ToInt32(Supplier.idtype);
            SuppIDTX.Text = Supplier.documentid;
            SuppAddressTX.Text = Supplier.address;
            SuppPhoneTX.Text = Supplier.phone;
            SuppCelphoneTX.Text = Supplier.celphone;
            SuppEmailTX.Text = Supplier.mail;
            SuppContactTX.Text = Supplier.contact;
            SuppObvsTX.Text = Supplier.observation;
            StatusCB.IsChecked = Supplier.status == "1" ? true : false;


            //Set selected supplier id index for edition save
            SelectedIndex = Control.Tag.ToString();

            //Focus editable rol
            SuppSocialNameTX.Focus();
            SaveBT.IsEnabled = true;
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
            var Supplier = NovaAPI.APISupplier.suppliers.Find(x => x.id == Control.Tag.ToString());


            if (MessageBox.Show($"A continuación se eliminara el proveedor '{Supplier.comercialname}'{Environment.NewLine}¿Desea continuar?", "Eliminar proveedor", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var Data = new NovaAPI.APISupplier.SupplierClass();
                Data.id = Supplier.id;

                //Delete user
                string requestData = JsonConvert.SerializeObject(Data);
                bool response = await NovaAPI.APISupplier.GetValues("3", DataConfig.LocalAPI, requestData);

                if (response)
                {
                    NovaAPI.APISupplier.suppliers.Remove(Supplier);
                    SuppliersGrid.Items.Refresh();
                }
                else
                {
                    MessageBox.Show($"Error al eliminar el proveedor, INFO: {Environment.NewLine}{NovaAPI.APISupplier.Message}");
                }
            }
        }

        /// <summary>
        /// See datagrid button, toggle row details visibility
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void See_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < SuppliersGrid.Items.Count; i++)
            {
                DataGridRow row = (DataGridRow)SuppliersGrid.ItemContainerGenerator.ContainerFromIndex(i);
                if (SuppliersGrid.SelectedIndex == i)
                {
                    //Toggle row details
                    row.DetailsVisibility =  row.DetailsVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
                }
                else if (row != null)
                {
                    row.DetailsVisibility = Visibility.Collapsed;
                }
            }
        }

        private void RefreshBT_Click(object sender, RoutedEventArgs e)
        {
            if (FormGrid.Opacity == 1)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
            }           
            ResetForm();
            LoadData();
        }

        //Reset form data
        private void ResetForm()
        {
            FilterTX.Clear();
            SuppSocialNameTX.Clear();
            SuppComercialNameTX.Clear();
            SuppIDTX.Clear();
            SuppidTypeCB.SelectedIndex = 0;
            SuppAddressTX.Clear();
            SuppPhoneTX.Clear();
            StatusCB.IsChecked = true;
            SuppCelphoneTX.Clear();
            SuppEmailTX.Clear();
            SuppContactTX.Clear();
            SuppObvsTX.Clear();
            SaveBT.IsEnabled = false;
        }

        private async void NewSupplierBT_Click(object sender, RoutedEventArgs e)
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
                SuppSocialNameTX.Focus();
                SaveBT.IsEnabled = true;

            }
            else if (SuppSocialNameTX.Text.Length == 0)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                ResetForm();
            }
            //Clear supplier values
            if (SuppSocialNameTX.Text.Length > 0)
            {
                ResetForm();
            }
        }

        //Supplier create / modify functions
        private async void SaveBT_Click(object sender, RoutedEventArgs e)
        {
            NewSupplierBT.Focus();

            if (SuppSocialNameTX.Text.Length == 0 || SuppSocialNameTX.Text.Length < 5)
            {
                MessageBox.Show("La razón social no puede estar en blanco o ser inferior a 5 caracteres");
                SuppSocialNameTX.Focus();
                return;
            }
            else if (SuppIDTX.Text.Length == 0 || SuppIDTX.Text.Length < 5)
            {
                MessageBox.Show("El numero de identificacion no puede estar en blanco o ser inferior a 5 caracteres");
                SuppIDTX.Focus();
                return;
            }

            //Get/Set Supplier parameters
            var Data = new NovaAPI.APISupplier.SupplierClass();
            Data.id = SelectedIndex;
            Data.socialname = SuppSocialNameTX.Text;
            Data.comercialname = SuppComercialNameTX.Text.Length == 0 ? SuppSocialNameTX.Text : SuppComercialNameTX.Text;
            Data.idtype = SuppidTypeCB.SelectedIndex.ToString();
            Data.documentid = SuppIDTX.Text;
            Data.address = SuppAddressTX.Text;
            Data.mail = SuppEmailTX.Text;
            Data.phone = SuppPhoneTX.Text;
            Data.celphone = SuppCelphoneTX.Text;
            Data.contact = SuppContactTX.Text;
            Data.observation = SuppObvsTX.Text;
            Data.status = StatusCB.IsChecked == true ? "1" : "0";


            //rol json data
            string requestData = JsonConvert.SerializeObject(Data);

            bool response;

            //Modify / Create request
            if (Data.id.Length > 0)
            {
                response = await NovaAPI.APISupplier.GetValues("2", DataConfig.LocalAPI, requestData);

            }
            else
            {
                response = await NovaAPI.APISupplier.GetValues("1", DataConfig.LocalAPI, requestData);
            }

            //Request response
            if (response)
            {
                if (Data.id.Length > 0)
                {
                    //On supplier modified
                    var SuppData = NovaAPI.APISupplier.suppliers.Find(x => x.id == Data.id);
                    SuppData.socialname = Data.socialname;
                    SuppData.comercialname = Data.comercialname;
                    SuppData.idtype = Data.idtype;
                    SuppData.documentid = Data.documentid;
                    SuppData.address = Data.address;
                    SuppData.mail = Data.mail;
                    SuppData.phone = Data.phone;
                    SuppData.celphone = Data.celphone;
                    SuppData.contact = Data.contact;
                    SuppData.observation = Data.observation;
                    SuppData.status = Data.status;
                    SuppliersGrid.Items.Refresh();
                    FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                    ResetForm();
                }
                else
                {
                    //On new supplier created response
                    var SuppData = new NovaAPI.APISupplier.SupplierClass();
                    SuppData.socialname = Data.socialname;
                    SuppData.comercialname = Data.comercialname;
                    SuppData.idtype = Data.idtype;
                    SuppData.documentid = Data.documentid;
                    SuppData.address = Data.address;
                    SuppData.mail = Data.mail;
                    SuppData.phone = Data.phone;
                    SuppData.celphone = Data.celphone;
                    SuppData.contact = Data.contact;
                    SuppData.status = Data.status;
                    SuppData.observation = Data.observation;
                    SuppData.id = NovaAPI.APISupplier.LastID;

                    FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                    ResetForm();

                    NovaAPI.APISupplier.suppliers.Add(SuppData);

                    //Reload rol data
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show($"Error al crear el proveedor, INFO: {Environment.NewLine}{NovaAPI.APISupplier.Message}");
            }
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

        private async void FilterBT_Click(object sender, RoutedEventArgs e)
        {
            if (FormGrid.Opacity != 0)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                ResetForm();
                await Task.Delay(100);
            }

            FilterTX.Clear();

            if (FilterGrid.Opacity == 0)
            {
                FilterGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("PopUpGrid"));
                FilterTX.Focus();
            } else {
                FilterGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
            }
        }


        /// <summary>
        /// Filter function on button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            //Get function
            string Function = ((Button)sender).Uid;

            //FIND supplier parameters
            Filter(Function);                                
            
        }


        //Filter function
        private void Filter(string Function)
        {
            if (Function == "2")
            {
                var Data = NovaAPI.APISupplier.suppliers.Where(x => x.socialname.ToLower().Contains(FilterTX.Text.ToLower()) ||
                                                            x.comercialname.ToLower().Contains(FilterTX.Text.ToLower()) ||
                                                            x.documentid.ToLower().Contains(FilterTX.Text.ToLower()) ||
                                                            x.contact.ToLower().Contains(FilterTX.Text.ToLower()));

                SuppliersGrid.ItemsSource = Data;
                SuppliersGrid.Items.Refresh();
            }
            else
            {
                var Data = NovaAPI.APISupplier.suppliers.Where(x => (x.socialname.ToLower().Contains(FilterTX.Text.ToLower()) ||
                                                            x.comercialname.ToLower().Contains(FilterTX.Text.ToLower()) ||
                                                            x.documentid.ToLower().Contains(FilterTX.Text.ToLower()) ||
                                                            x.contact.ToLower().Contains(FilterTX.Text.ToLower())) && x.status == Function);

                SuppliersGrid.ItemsSource = Data;
                SuppliersGrid.Items.Refresh();
            }
        }


        //Status checkbox logic
        private void StatusCB_Checked(object sender, RoutedEventArgs e)
        {
            if (StatusCB.IsChecked == true)
            {
                StatusCB.Content = "Habilitado";
                StatusCB.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                StatusCB.Content = "Deshabilitado";
                StatusCB.Foreground = new SolidColorBrush(Colors.Red);

            }
        }

        //Filter textbox on enter
        private void FilterTX_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Filter("2");
                e.Handled = true;
            }            
        }
    }

    //Value converter status
    public class StatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch (value.ToString())
            {
                case "1":
                    return ((SolidColorBrush)Application.Current.TryFindResource("PassBrush"));
                case "0":
                    return ((SolidColorBrush)Application.Current.TryFindResource("DarkBackground"));
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch (value.ToString())
            {
                case "1":
                    return "Habilitado";
                case "0":
                    return "Deshabilitado" ;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.ToString() == "Habilitado")
                return "1";
            else
                return "0";
        }
    }
}
