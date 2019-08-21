using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Nova.Pages.Configuration
{
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
                    return "Deshabilitado";
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

    /// <summary>
    /// Lógica de interacción para BranchPage.xaml
    /// </summary>
    public partial class BranchPage : Page
    {
        string SelectedIndex = "";
        string SelectedBoxIndex = "";

        public BranchPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Load branch data
            LoadData();
        }

        private async void LoadData()
        {
            //Set loading grid visibility
            LoadingRolGrid.Visibility = Visibility.Visible;
            RolSpinner.Spin = true;
            IsEnabledControls(false);

            //Try to clear existent branch list
            try
            {
                NovaAPI.APIBranch.branch.Clear();
            }
            catch (Exception) { }

            //Load branch 
            bool response = await NovaAPI.APIBranch.GetValues("4", DataConfig.LocalAPI);
            if (response)
            {
                //Set rol data to DataGrid
                BranchGrid.ItemsSource = NovaAPI.APIBranch.branch;
                BranchCB.ItemsSource = NovaAPI.APIBranch.branch;
                BranchGrid.Items.Refresh();
            }
            else
            {
                //Set loading grid visibility
                LoadingRolGrid.Visibility = Visibility.Collapsed;
                RolSpinner.Spin = false;
                //On load error
                MessageBox.Show($"Se produjo un error al cargar los datos, INFO: {Environment.NewLine}{NovaAPI.APIBranch.Message}");
                RefreshBT.IsEnabled = true;
                return;
            }

            await Task.Delay(100);

            //Set loading grid visibility
            LoadingRolGrid.Visibility = Visibility.Collapsed;
            RolSpinner.Spin = false;
            IsEnabledControls(true);

        }

        //Reset form data and visibility
        private void ResetForm()
        {
            //EditPermBT.IsEnabled = false;
            BranchNameTX.Clear();
            BranchAddTX.Clear();
            BranchPhoneTX.Clear();
            SelectedIndex = "";
        }

        private void IsEnabledControls(bool value)
        {
            if (value)
            {
                RefreshBT.IsEnabled = true;
                NewBranchBT.IsEnabled = true;

            }
            else
            {
                RefreshBT.IsEnabled = false;
                NewBranchBT.IsEnabled = false;
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

        private void EditBranch_Click(object sender, RoutedEventArgs e)
        {
            //Get button control
            Button Control = (Button)sender;           

            if (FormGrid.Opacity == 0)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("PopUpGrid"));
            }

            //Set branch information to controls
            BranchNameTX.Text = NovaAPI.APIBranch.branch.Find(x => x.id == Control.Tag.ToString()).name;
            BranchAddTX.Text = NovaAPI.APIBranch.branch.Find(x => x.id == Control.Tag.ToString()).address;
            BranchPhoneTX.Text = NovaAPI.APIBranch.branch.Find(x => x.id == Control.Tag.ToString()).phone;

            //Set selected rol id index for edition save
            SelectedIndex = Control.Tag.ToString();

            //Focus editable rol
            BranchNameTX.Focus();
            SaveBT.IsEnabled = true;
        }

        private async void DeleteBranch_Click(object sender, RoutedEventArgs e)
        {
            if (FormGrid.Opacity != 0)
            {
                ResetForm();
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
            }

            //Get button control
            Button Control = (Button)sender;

            int branchpoints = Convert.ToInt32(NovaAPI.APIBranch.branch.Find(x => x.id == Control.Tag.ToString()).boxes);

            if (branchpoints > 0)
            {
                MessageBox.Show("No se puede eliminar una sucursal con puntos de venta vinculados" + Environment.NewLine + "Por favor desvincule los puntos de venta antes de eliminar la sucursal");
                return;
            }

            //Get Branch information
            string BranchName = NovaAPI.APIBranch.branch.Find(x => x.id == Control.Tag.ToString()).name;

            if (MessageBox.Show($"A continuación se eliminara la sucursal '{BranchName}{Environment.NewLine}¿Desea continuar?", "Eliminar sucursal", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {

                var Data = new NovaAPI.APIBranch.BranchClass();
                Data.id = Control.Tag.ToString();

                //Delete rol
                string requestData = JsonConvert.SerializeObject(Data);
                bool response = await NovaAPI.APIBranch.GetValues("3", DataConfig.LocalAPI, requestData);

                if (response)
                {
                    NovaAPI.APIBranch.branch.Remove(NovaAPI.APIBranch.branch.Find(x => x.id == Control.Tag.ToString()));
                    BranchGrid.Items.Refresh();
                }
                else
                {
                    MessageBox.Show($"Error al eliminar la sucursal, INFO: {Environment.NewLine}{NovaAPI.APIRoles.Message}");
                }
            }
        }


        //Refresh Branch list
        private void RefreshBT_Click(object sender, RoutedEventArgs e)
        {
            FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
            ResetForm();
            LoadData();
            SaveBT.IsEnabled = false;
        }

        //New BT Click , toggle form
        private void NewBranchBT_Click(object sender, RoutedEventArgs e)
        {
            //From grid animation
            if (FormGrid.Opacity == 0)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("PopUpGrid"));
                BranchNameTX.Focus();
                SaveBT.IsEnabled = true;

            }
            else if (BranchNameTX.Text.Length == 0)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                SaveBT.IsEnabled = false;
            }

            //Clear branch values
            if (BranchNameTX.Text.Length > 0)
            {
                ResetForm();

            }
        }

        private async void SaveBT_Click(object sender, RoutedEventArgs e)
        {
            NewBranchBT.Focus();

            if (BranchNameTX.Text.Length == 0 || BranchNameTX.Text.Length < 5)
            {
                MessageBox.Show("El nombre de la sucursal no puede estar en blanco o ser inferior a 5 caracteres");
                BranchNameTX.Focus();
                return;
            }

            //Get rol parameters
            var Data = new NovaAPI.APIBranch.BranchClass();
            Data.id = SelectedIndex;
            Data.name = BranchNameTX.Text;
            Data.address = BranchAddTX.Text;
            Data.phone = BranchPhoneTX.Text;

            //rol json data
            string requestData = JsonConvert.SerializeObject(Data);

            bool response;

            //Modify / Create request
            if (Data.id.Length > 0)
            {
                response = await NovaAPI.APIBranch.GetValues("2", DataConfig.LocalAPI, requestData);

            }
            else
            {
                response = await NovaAPI.APIBranch.GetValues("1", DataConfig.LocalAPI, requestData);
            }

            //Request response
            if (response)
            {
                if (Data.id.Length > 0)
                {
                    //On branch modified
                    NovaAPI.APIBranch.branch.Find(x => x.id == Data.id).name = Data.name;
                    NovaAPI.APIBranch.branch.Find(x => x.id == Data.id).address = Data.address;
                    NovaAPI.APIBranch.branch.Find(x => x.id == Data.id).phone = Data.phone;

                    BranchGrid.Items.Refresh();
                    FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                    ResetForm();
                    SaveBT.IsEnabled = false;
                }
                else
                {
                    //On new branch created response
                    var branch = new NovaAPI.APIBranch.BranchClass();
                    branch.name = Data.name;
                    branch.address = Data.address;
                    branch.phone = Data.phone;
                    branch.id = NovaAPI.APIRoles.LastID;
                    branch.count = "0";

                    FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                    ResetForm();

                    NovaAPI.APIBranch.branch.Add(branch);
                    SaveBT.IsEnabled = false;
                    //Reload rol data
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show($"Error al crear la sucursal, INFO: {Environment.NewLine}{NovaAPI.APIRoles.Message}");
            }
        }

        private async void LoadBoxes(string BranchID)
        {
            //Set loading grid visibility
            LoadingBoxGrid.Visibility = Visibility.Visible;
            BoxSpinner.Spin = true;

            //Try to clear existent branch list
            try
            {
                NovaAPI.APIBoxes.boxes.Clear();
            }
            catch (Exception) { }

            var data = new NovaAPI.APIBoxes.BoxesData{
                branch_id = BranchID
            };

            //branch json data
            string requestData = JsonConvert.SerializeObject(data);

            //Load branch 
            bool response = await NovaAPI.APIBoxes.GetValues("5", DataConfig.LocalAPI, requestData);
            if (response)
            {
                //Set rol data to DataGrid
                BoxDataGrid.ItemsSource = NovaAPI.APIBoxes.boxes;
                BoxDataGrid.Items.Refresh();
            }
            else
            {
                //Set loading grid visibility
                LoadingBoxGrid.Visibility = Visibility.Collapsed;
                BoxSpinner.Spin = false;
                //On load error
                MessageBox.Show($"{NovaAPI.APIBoxes.Message}");
                BoxDataGrid.ItemsSource = null;
                BoxDataGrid.Items.Refresh();
                return;
            }

            await Task.Delay(100);

            //Set loading grid visibility
            LoadingBoxGrid.Visibility = Visibility.Collapsed;
            BoxSpinner.Spin = false;
        }


        //Boxes tab selected
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                if (BoxesTab.IsSelected)
                {
                   BoxFormGrid.Opacity = 0;
                   BoxFormGrid.Height = 0;
                   LoadBoxes(NovaAPI.APIBranch.branch[BranchCB.SelectedIndex].id);
                }
            }
        }

        //New button logic
        private void NewPointBT_Click(object sender, RoutedEventArgs e)
        {
            //From grid animation
            if (BoxFormGrid.Opacity == 0)
            {
                BoxFormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("PopUpGrid"));
                BoxNameTX.Focus();
                SavePointBT.IsEnabled = true;

            }
            else if (BoxNameTX.Text.Length == 0)
            {
                BoxFormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                SavePointBT.IsEnabled = false;
            }

            //Clear box values
            if (BoxNameTX.Text.Length > 0)
            {
                BoxNameTX.Clear();
                SelectedBoxIndex = "";

            }
        }

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

        //Save box point data
        private async void SavePointBT_Click(object sender, RoutedEventArgs e)
        {
            NewPointBT.Focus();

            if (BoxNameTX.Text.Length == 0 || BoxNameTX.Text.Length < 5)
            {
                MessageBox.Show("El nombre del punto de venta no puede ir en blanco o ser inferior a 5 caracteres");
                BoxNameTX.Focus();
                return;
            }

            //Get/Set box parameters
            var Data = new NovaAPI.APIBoxes.BoxesData();
            Data.id = SelectedBoxIndex;
            Data.name = BoxNameTX.Text;
            Data.branch_id = NovaAPI.APIBranch.branch[BranchCB.SelectedIndex].id;
            Data.status = StatusCB.IsChecked == true ? "1" : "0";

            //rol json data
            string requestData = JsonConvert.SerializeObject(Data);

            bool response;

            //Modify / Create request
            if (Data.id.Length > 0)
            {
                response = await NovaAPI.APIBoxes.GetValues("8", DataConfig.LocalAPI, requestData);

            }
            else
            {
                response = await NovaAPI.APIBoxes.GetValues("7", DataConfig.LocalAPI, requestData);
            }

            //Request response
            if (response)
            {
                if (Data.id.Length > 0)
                {
                    //On supplier modified
                    var Boxdata = NovaAPI.APIBoxes.boxes.Find(x => x.id == Data.id);
                    Boxdata.name = Data.name;
                    Boxdata.status = Data.status;
                    BoxDataGrid.Items.Refresh();
                    BoxFormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                    BoxNameTX.Clear();
                    
                }
                else
                {
                    //On new supplier created response
                    var Boxdata = new NovaAPI.APIBoxes.BoxesClass();
                    Boxdata.name = Data.name;
                    Boxdata.status = Data.status;
                    Boxdata.id = NovaAPI.APIBoxes.LastID;

                    BoxFormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                    BoxNameTX.Clear();

                    NovaAPI.APIBoxes.boxes.Add(Boxdata);

                    //Reload box data
                    LoadBoxes(NovaAPI.APIBranch.branch[BranchCB.SelectedIndex].id);
                }
            }
            else
            {
                MessageBox.Show($"Error al crear el punto de venta, INFO: {Environment.NewLine}{NovaAPI.APIBoxes.Message}");
            }
            SavePointBT.IsEnabled = false;
            SelectedBoxIndex = "";
        }

        private void BranchCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BoxesTab.IsSelected)
            {
                SelectedBoxIndex = "";
                if (BoxFormGrid.Opacity != 0)
                {
                    BoxFormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                }
                try
                {
                    LoadBoxes(NovaAPI.APIBranch.branch[BranchCB.SelectedIndex].id);
                }
                catch (Exception) { }
            }
        }

        //Refresh page
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BranchCB.SelectedIndex = 0;
            BoxFormGrid.Opacity = 0;
            BoxFormGrid.Height = 0;
            BoxNameTX.Clear();
            LoadBoxes(NovaAPI.APIBranch.branch[BranchCB.SelectedIndex].id);
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            //Get button control
            Button Control = (Button)sender;

            var Box = NovaAPI.APIBoxes.boxes.Find(x => x.id == Control.Tag.ToString());

            if (BoxFormGrid.Opacity == 0)
            {
                BoxFormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("PopUpGrid"));
            }

            //Set box information to controls
            BoxNameTX.Text = Box.name;
            StatusCB.IsChecked = Box.status == "1" ? true : false;


            //Set selected box id index for edition save
            SelectedBoxIndex = Control.Tag.ToString();

            //Focus editable rol
            BoxNameTX.Focus();
            SavePointBT.IsEnabled = true;
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (BoxFormGrid.Opacity != 0)
            {
                BoxNameTX.Clear();
                BoxFormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
            }

            //Get button control
            Button Control = (Button)sender;

            //Get box information
            string BoxhName = NovaAPI.APIBoxes.boxes.Find(x => x.id == Control.Tag.ToString()).name;

            if (MessageBox.Show($"A continuación se eliminara el punto de venta '{BoxhName}{Environment.NewLine}¿Desea continuar?", "Eliminar punto de venta", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {

                var Data = new NovaAPI.APIBoxes.BoxesClass();
                Data.id = Control.Tag.ToString();

                //Delete box
                string requestData = JsonConvert.SerializeObject(Data);

                bool response = await NovaAPI.APIBoxes.GetValues("6", DataConfig.LocalAPI, requestData);

                if (response)
                {
                    NovaAPI.APIBoxes.boxes.Remove(NovaAPI.APIBoxes.boxes.Find(x => x.id == Control.Tag.ToString()));
                    BoxDataGrid.Items.Refresh();
                }
                else
                {
                    MessageBox.Show($"Error al eliminarel punto de venta, {Environment.NewLine}{NovaAPI.APIBoxes.Message}");
                }
            }
        }
    }
}

