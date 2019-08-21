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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Nova.Pages.Clients
{
    //Value converter status
    public class StatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch (value.ToString())
            {
                case "1":
                    return Visibility.Visible;
                case "0":
                    return Visibility.Hidden;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
    }


    /// <summary>
    /// Lógica de interacción para ClientsPage.xaml
    /// </summary>
    public partial class ClientsPage : Page
    {
        string SelectedIndex = "";

        public ClientsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadClients();
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

        private async void LoadClients()
        {
            //Set loading grid visibility
            LoadingGrid.Visibility = Visibility.Visible;
            Spinner.Spin = true;

            //Try to clear existent clients list
            try
            {
                NovaAPI.APIClient.clients.Clear();
            }
            catch (Exception) { }

            //Send request
            bool Response = await NovaAPI.APIClient.GetValues("4", DataConfig.LocalAPI);

            if (Response)
            {
                ClientGrid.ItemsSource = NovaAPI.APIClient.clients;
                ClientGrid.Items.Refresh();
            }
            else
            {
                //Set loading grid visibility
                LoadingGrid.Visibility = Visibility.Collapsed;
                Spinner.Spin = false;
                //On load error
                MessageBox.Show($"Se produjo un error al cargar los datos, INFO: {Environment.NewLine}{NovaAPI.APIClient.Message}");
                RefreshBT.IsEnabled = true;
                return;
            }

            await Task.Delay(100);
            //Set loading grid visibility
            LoadingGrid.Visibility = Visibility.Collapsed;
            Spinner.Spin = false;
        }

        //Clear all form data and selections
        private void ClearFormData()
        {
            if (FormGrid.Opacity != 0)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
            }
            if (FilterGrid.Opacity != 0)
            {
                FilterGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
            }
            FilterTX.Clear();
            ClientNameTX.Clear();
            ClientIDTX.Clear();
            ClientPhoneTX.Clear();
            ClientAddressTX.Clear();
            ClientMailTX.Clear();
            ClientCelphoneTX.Clear();
            ClientTypeCB.SelectedIndex = 0;
            ClientCreditCB.IsChecked = true;
            SaveBT.IsEnabled = false;
        }

        //Refresh data
        private void RefreshBT_Click(object sender, RoutedEventArgs e)
        {
            ClearFormData();
            LoadClients();
        }

        //Filter form toggle
        private async void FilterBT_Click(object sender, RoutedEventArgs e)
        {

            if (FormGrid.Opacity != 0)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                ClearFormData();
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

        //Open new client form
        private async void NewClientBT_Click(object sender, RoutedEventArgs e)
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
                ClientNameTX.Focus();
                SaveBT.IsEnabled = true;

            }
            else if (ClientNameTX.Text.Length == 0)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                ClearFormData();
            }
            //Clear supplier values
            if (ClientNameTX.Text.Length > 0)
            {
                ClearFormData();
            }
        }

        //Save client
        private async void SaveBT_Click(object sender, RoutedEventArgs e)
        {
            NewClientBT.Focus();

            if (ClientNameTX.Text.Length == 0)
            {
                MessageBox.Show("El nombre del cliente no puede estar en blanco");
                ClientNameTX.Focus();
                return;
            }

            //Get/Set client parameters
            var Data = new NovaAPI.APIClient.ClientClass();
            Data.id = SelectedIndex;
            Data.name = ClientNameTX.Text;
            Data.type = ClientTypeCB.SelectedIndex.ToString();
            Data.documentid = ClientIDTX.Text;
            Data.phone = ClientPhoneTX.Text;
            Data.celphone = ClientCelphoneTX.Text;
            Data.mail = ClientMailTX.Text;
            Data.address = ClientAddressTX.Text;
            Data.cancredit = ClientCreditCB.IsChecked == true ? "1" : "0";


            //rol json data
            string requestData = JsonConvert.SerializeObject(Data);

            bool response;

            //Modify / Create request
            if (Data.id.Length > 0)
            {
                response = await NovaAPI.APIClient.GetValues("2", DataConfig.LocalAPI, requestData);

            }
            else
            {
                response = await NovaAPI.APIClient.GetValues("1", DataConfig.LocalAPI, requestData);
            }

            //Request response
            if (response)
            {
                if (Data.id.Length > 0)
                {
                    //On client modified
                    var ClientData = NovaAPI.APIClient.clients.Find(x => x.id == Data.id);
                    ClientData.name = Data.name;
                    ClientData.documentid = Data.documentid;
                    ClientData.type = Data.type;
                    ClientData.address = Data.address;
                    ClientData.phone = Data.phone;
                    ClientData.mail = Data.mail;
                    ClientData.celphone = Data.celphone;
                    ClientData.cancredit = Data.cancredit;
                    ClientGrid.Items.Refresh();
                    FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                    ClearFormData();
                }
                else
                {
                    //On new supplier created response
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

                    FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                    ClearFormData();

                    NovaAPI.APIClient.clients.Add(ClientData);

                    //Reload rol data
                    LoadClients();
                }
            }
            else
            {
                MessageBox.Show($"Error al crear el cliente, INFO: {Environment.NewLine}{NovaAPI.APIClient.Message}");
            }
        }

        //Open client form for edit
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            //Hide filter
            if (FilterGrid.Opacity != 0)
            {
                FilterGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
            }

            //Get button control
            Button Control = (Button)sender;

            var Client = NovaAPI.APIClient.clients.Find(x => x.id == Control.Tag.ToString());

            if (FormGrid.Opacity == 0)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("PopUpGrid"));
            }

            //Set supplier information to controls
            ClientNameTX.Text = Client.name;
            ClientIDTX.Text = Client.documentid;
            ClientTypeCB.SelectedIndex = Convert.ToInt32(Client.type);
            ClientPhoneTX.Text = Client.phone;
            ClientCelphoneTX.Text = Client.celphone;
            ClientAddressTX.Text = Client.address;
            ClientMailTX.Text = Client.mail;
            ClientCreditCB.IsChecked = Client.cancredit == "0" ? false : true;

            //Set selected supplier id index for edition save
            SelectedIndex = Control.Tag.ToString();

            //Focus editable rol
            ClientNameTX.Focus();
            SaveBT.IsEnabled = true;
        }

        //Delete client
        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (FormGrid.Opacity != 0)
            {
                ClearFormData();
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
            }

            //Get button control
            Button Control = (Button)sender;


            //Get User information
            var Client = NovaAPI.APIClient.clients.Find(x => x.id == Control.Tag.ToString());


            if (MessageBox.Show($"A continuación se eliminara el cliente '{Client.name}'{Environment.NewLine}¿Desea continuar?", "Eliminar cliente", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var Data = new NovaAPI.APIClient.ClientClass();
                Data.id = Client.id;

                //Delete user
                string requestData = JsonConvert.SerializeObject(Data);
                bool response = await NovaAPI.APIClient.GetValues("3", DataConfig.LocalAPI, requestData);

                if (response)
                {
                    NovaAPI.APIClient.clients.Remove(Client);
                    ClientGrid.Items.Refresh();
                }
                else
                {
                    MessageBox.Show($"Error al eliminar el cliente, INFO: {Environment.NewLine}{NovaAPI.APIClient.Message}");
                }
            }
        }

        //Credits operations
        private void CreditBT_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Get function
            string Function = ((Button)sender).Uid;

            //FIND supplier parameters
            Filter(Function);
        }

        //Filter function
        private void Filter(string Function)
        {

            switch (Function)
            {
                case "2":
                    var Data = NovaAPI.APIClient.clients.Where(x => x.name.ToLower().Contains(FilterTX.Text.ToLower()) ||
                                                            x.id.ToLower().Contains(FilterTX.Text.ToLower()) ||
                                                            x.documentid.ToLower().Contains(FilterTX.Text.ToLower()));

                    ClientGrid.ItemsSource = Data;
                    ClientGrid.Items.Refresh();
                    break;
                case "1":

                    break;
                case "0":
                    break;
                default:
                    break;
            }
        }

        private void FilterTX_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Filter("2");
                e.Handled = true;
            }
        }
    }
}
