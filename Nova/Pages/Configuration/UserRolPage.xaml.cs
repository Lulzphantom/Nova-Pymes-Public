using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Nova.Pages.Configuration
{
    /// <summary>
    /// Lógica de interacción para UserRolPage.xaml
    /// </summary>
    public partial class UserRolPage : Page
    {

        string SelectedIndex = "";

        public UserRolPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Start loading data
            LoadData();
            
        }

        private async void LoadData()
        {
            //Set loading grid visibility
            LoadingRolGrid.Visibility = Visibility.Visible;
            RolSpinner.Spin = true;
            IsEnabledControls(false);

            //Refresh ROL list data
            try
            {
                NovaAPI.APIRoles.userrols.Clear();
            }
            catch (Exception) { }

            //Load rol data from API
            bool RolData = await NovaAPI.APIRoles.GetValues("4", DataConfig.LocalAPI);

            if (RolData)
            {
                //Set rol data to DataGrid
                RolGrid.ItemsSource = NovaAPI.APIRoles.userrols;
                RolGrid.Items.Refresh();
            }
            else
            {
                //Set loading grid visibility
                LoadingRolGrid.Visibility = Visibility.Collapsed;
                RolSpinner.Spin = false;
                //On load error
                MessageBox.Show($"Se produjo un error al cargar los datos, INFO: {Environment.NewLine}{NovaAPI.APIRoles.Message}");
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
            EditPermBT.IsEnabled = false;
            RolNameTX.Clear();
            RolDescriptTX.Clear();
            SelectedIndex = "";            
        }

        private void NewRolBT_Click(object sender, RoutedEventArgs e)
        {
            
            //From grid animation
            if (FormGrid.Opacity == 0)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("PopUpGrid"));
                RolNameTX.Focus();
                SaveBT.IsEnabled = true;

            } else if(RolNameTX.Text.Length == 0)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                SaveBT.IsEnabled = false;
            }

            //Clear rol values
            if (RolNameTX.Text.Length > 0)
            {
                ResetForm();
            }
        }

        private void EditRol_Click(object sender, RoutedEventArgs e)
        {    
            //Get button control
            Button Control = (Button)sender;

            string rolid = NovaAPI.APIRoles.userrols.Find(x => x.rolid == Control.Tag.ToString()).rolid;

            if (rolid == "1")
            {
                MessageBox.Show("No se puede editar el rol de administrador del sistema");
                return;
            }

            if (FormGrid.Opacity == 0)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("PopUpGrid"));
            }            

            //Set rol information to controls
            RolNameTX.Text = NovaAPI.APIRoles.userrols.Find(x => x.rolid == Control.Tag.ToString()).rolname;
            RolDescriptTX.Text = NovaAPI.APIRoles.userrols.Find(x => x.rolid == Control.Tag.ToString()).roldescription;

            EditPermBT.IsEnabled = true;

            //Set selected rol id index for edition save
            SelectedIndex = Control.Tag.ToString();

            //Focus editable rol
            RolNameTX.Focus();
            SaveBT.IsEnabled = true;
        }

        private async void DeleteRol_Click(object sender, RoutedEventArgs e)
        {
            if (FormGrid.Opacity != 0)
            {
                ResetForm();
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
            }

            //Get button control
            Button Control = (Button)sender;

            //Get rol information
            string rolname = NovaAPI.APIRoles.userrols.Find(x => x.rolid == Control.Tag.ToString()).rolname;
            string rolid = NovaAPI.APIRoles.userrols.Find(x => x.rolid == Control.Tag.ToString()).rolid;

            if (rolid == "1")
            {
                MessageBox.Show("No se puede eliminar el rol de administrador del sistema");
                return;
            }

            if (MessageBox.Show($"A continuación se eliminara el rol '{rolname}{Environment.NewLine}¿Desea continuar?","Eliminar rol",MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {

                var Data = new rolData();
                Data.id = rolid;

                //Delete rol
                string requestData = JsonConvert.SerializeObject(Data);
                bool response = await NovaAPI.APIRoles.GetValues("3", DataConfig.LocalAPI, requestData);

                if (response)
                {
                    NovaAPI.APIRoles.userrols.Remove(NovaAPI.APIRoles.userrols.Find(x => x.rolid == rolid));
                    RolGrid.Items.Refresh();

                } else
                {
                    MessageBox.Show($"Error al eliminar el rol, INFO: {Environment.NewLine}{NovaAPI.APIRoles.Message}");
                }
            }
        }

        private async void SaveRolBT_Click(object sender, RoutedEventArgs e)
        {
            NewRolBT.Focus();

            if (RolNameTX.Text.Length == 0 || RolNameTX.Text.Length < 5)
            {
                MessageBox.Show("El nombre del rol no puede estar en blanco o ser inferior a 5 caracteres");
                RolNameTX.Focus();
                return;
            }

            //Get rol parameters
            var Data = new rolData();
            Data.id = SelectedIndex;
            Data.name = RolNameTX.Text;
            Data.description = RolDescriptTX.Text;

            //rol json data
            string requestData = JsonConvert.SerializeObject(Data);

            bool response;

            //Modify / Create request
            if (Data.id.Length > 0)
            {
                response = await NovaAPI.APIRoles.GetValues("2", DataConfig.LocalAPI, requestData);

            } else
            {
                response = await NovaAPI.APIRoles.GetValues("1", DataConfig.LocalAPI, requestData);
            }

            //Request response
            if (response)
            {
                if (Data.id.Length > 0)
                {
                    //On role modified
                    NovaAPI.APIRoles.userrols.Find(x => x.rolid == Data.id).rolname = Data.name;
                    NovaAPI.APIRoles.userrols.Find(x => x.rolid == Data.id).roldescription = Data.description;

                    RolGrid.Items.Refresh();
                    FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                    ResetForm();
                    SaveBT.IsEnabled = false;
                }
                else
                {
                    //On new rol created response
                    var newRol = new NovaAPI.APIRoles.Rols();
                    newRol.rolname = Data.name;
                    newRol.roldescription = Data.description;
                    newRol.rolid = NovaAPI.APIRoles.LastID;
                    newRol.usercount = "0";

                    //CREATE ROL DEFAULT PERMISSION DATA
                    
                    FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                    ResetForm();

                    NovaAPI.APIRoles.userrols.Add(newRol);

                    //Reload rol data
                    LoadData();
                    SaveBT.IsEnabled = false;
                }
            }
            else
            {
                MessageBox.Show($"Error al crear el rol, INFO: {Environment.NewLine}{NovaAPI.APIRoles.Message}");
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
            SaveBT.IsEnabled = false;
        }

        private void IsEnabledControls(bool value)
        {
            if (value)
            {
                RefreshBT.IsEnabled = true;
                NewRolBT.IsEnabled = true;

            } else
            {
                RefreshBT.IsEnabled = false;
                NewRolBT.IsEnabled = false;
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

        //ROL Edit permission
        private void EditPermBT_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedIndex != "")
            {
                TabControl.SetIsSelected(PermissionsTab, true);
            }
        }

        //ROL data class
        private class rolData {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
        }


        #region Permissions tab


        string RolID;

        //Permissions Selected
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                if (PermissionsTab.IsSelected)
                {
                    //Get rol index
                    string selection = SelectedIndex == "" ? "1" : SelectedIndex;
                    PermissionsGrid.IsEnabled = selection == "1" ? false : true; //Set disabled permission edit on administrator rol
                    PermissionsSaveBT.IsEnabled = selection == "1" ? false : true;

                    //Reset rol status form
                    PermissionsSaveBT.Background = (SolidColorBrush)Application.Current.TryFindResource("ConfigHeader");
                    FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                    ResetForm();

                    RolID = selection;

                    var Data = NovaAPI.APIRoles.userrols.Find(x => x.rolid == selection).RolData;

                    //Load permissions on selected rol or null rol
                    PermissionsGrid.ItemsSource = Data;
                    PermissionsGrid.Items.Refresh();

                    //Set rol name title
                    LabelTitle.Content = NovaAPI.APIRoles.userrols.Find(x => x.rolid == selection).rolname;

                }
            }
        }


        public class PermissionsModify
        {
            public string id { get; set; }
            public object permissions { get; set; }
        }

        private async void PermissionsSaveBT_Click(object sender, RoutedEventArgs e)
        {
            PermissionsSaveBT.Background = (SolidColorBrush)Application.Current.TryFindResource("NormalBrush");

            //Get permission data
            var PermissionData = NovaAPI.APIRoles.userrols.Find(x => x.rolid == RolID).RolData;

            //rol json data
            PermissionsModify Modifier = new PermissionsModify()
            {
                id = RolID,
                permissions = PermissionData
            };

            string requestData = JsonConvert.SerializeObject(Modifier);

            //API Permission save
            bool Response = await NovaAPI.APIPermissions.GetValues(null,"2", DataConfig.LocalAPI, requestData);

            if (Response)
            {
                PermissionsSaveBT.Background = (SolidColorBrush)Application.Current.TryFindResource("PassBrush");

            } else
            {
                PermissionsSaveBT.Background = (SolidColorBrush)Application.Current.TryFindResource("ErrorBrush");
                MessageBox.Show($"Error al guardar la información de permisos, INFO:{Environment.NewLine}{NovaAPI.APIPermissions.Message}");
            }
        }

        //Checks change status
        private void Consult_Checked(object sender, RoutedEventArgs e)
        {
            var ConsultCheck = (CheckBox)sender;
            var Parent = (StackPanel)VisualTreeHelper.GetParent(ConsultCheck);

            MessageBox.Show(
           NovaAPI.APIRoles.userrols.Find(x => x.rolid == RolID).RolData.Find(x => x.id == Convert.ToInt32(Parent.Tag.ToString())).permissions.create.ToString());      


        }



        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var PermissionData = NovaAPI.APIRoles.userrols.Find(x => x.rolid == RolID).RolData;

            for (int i = 0; i < PermissionData.Count; i++)
            {
                PermissionData[i].permissions.consult = ((Button)sender).Uid == "1" ? 1 : 0;
                PermissionData[i].permissions.create = ((Button)sender).Uid == "1" ? 1 : 0;
                PermissionData[i].permissions.delete = ((Button)sender).Uid == "1" ? 1 : 0;
            }

            PermissionsGrid.Items.Refresh();
        }
    }
}
