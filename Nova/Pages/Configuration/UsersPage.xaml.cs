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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Nova.Pages.Configuration
{
    /// <summary>
    /// Lógica de interacción para UsersPage.xaml
    /// </summary>
    public partial class UsersPage : Page
    {
        string SelectedIndex = "";

        public UsersPage()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();

            try { NovaAPI.APIBranch.branch.Clear(); }
            catch (Exception) { }

            try { NovaAPI.APIRoles.userrols.Clear(); }
            catch (Exception) { }

            bool BranchResponse = await NovaAPI.APIBranch.GetValues("4", DataConfig.LocalAPI);
            bool RolesResponse = await NovaAPI.APIRoles.GetValues("4", DataConfig.LocalAPI);

            try
            {   //Try to remove admin rol
                NovaAPI.APIRoles.userrols.Remove(NovaAPI.APIRoles.userrols.Find(x => x.rolid == "1"));
            }
            catch (Exception) { }

            UserRolCB.ItemsSource = NovaAPI.APIRoles.userrols;
            UserBranchCB.ItemsSource = NovaAPI.APIBranch.branch;
        }

        private async void LoadData()
        {
            //Set loading grid visibility
            LoadingGrid.Visibility = Visibility.Visible;
            Spinner.Spin = true;

            //IsEnabledControls(false);

            //Try to clear existent users list
            try
            {
                NovaAPI.APIUsers.users.Clear();
            }
            catch (Exception) { }

            //Load branch 
            bool response = await NovaAPI.APIUsers.GetValues("4", DataConfig.LocalAPI);
            if (response)
            {
                //Set rol data to DataGrid
                UsersGrid.ItemsSource = NovaAPI.APIUsers.users;
                UsersGrid.Items.Refresh();
            }
            else
            {
                //Set loading grid visibility
                LoadingGrid.Visibility = Visibility.Collapsed;
                Spinner.Spin = false;
                //On load error
                MessageBox.Show($"Se produjo un error al cargar los datos, INFO: {Environment.NewLine}{NovaAPI.APIUsers.Message}");
                RefreshBT.IsEnabled = true;
                return;
            }

            await Task.Delay(100);

            //Set loading grid visibility
            LoadingGrid.Visibility = Visibility.Collapsed;
            Spinner.Spin = false;
            //IsEnabledControls(true);

        }

        //Users action buttons
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            //Get button control
            Button Control = (Button)sender;

            //Set user information to controls
            var UserData = NovaAPI.APIUsers.users.Find(x => x.id == Control.Tag.ToString());
            if (UserData.id == "1")
            {
                MessageBox.Show("No se puede modificar el usuario de administrador del sistema.");
                return;
            }

            if (FormGrid.Opacity == 0)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("PopUpGrid"));
            }
                       

            //User principal data
            UserNameTX.Text = UserData.name;            
            UserRealTX.Text = UserData.realname;
            UserPassword.Password = UserData.name;

            //User rol and branch data
            UserRolCB.SelectedItem = NovaAPI.APIRoles.userrols.Find(x => x.rolid == UserData.rolid);
            UserBranchCB.SelectedItem = NovaAPI.APIBranch.branch.Find(x => x.id == UserData.branchid);

            //user status data
            UserStatusCB.IsChecked = UserData.status == "1" ? true : false;


            //Set photo
            //-------------------------


            //Set selected rol id index for edition save
            SelectedIndex = Control.Tag.ToString();

            UserRolCB.Items.Refresh();
            UserBranchCB.Items.Refresh();

            //Focus editable rol
            UserNameTX.Focus();
            SaveBT.IsEnabled = true;
        }

        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            var UserData = NovaAPI.APIUsers.users.Find(x => x.id == Control.Tag.ToString());

            if (UserData.id == "1")
            {
                MessageBox.Show("No se puede eliminar el usuario administrador del sistema");
                return;
            }

            if (MessageBox.Show($"A continuación se eliminara el usuario '{UserData.name}{Environment.NewLine}¿Desea continuar?", "Eliminar usuario", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var Data = new NovaAPI.APIUsers.UsersClass();
                Data.id = UserData.id;

                //Delete user
                string requestData = JsonConvert.SerializeObject(Data);
                bool response = await NovaAPI.APIUsers.GetValues("3", DataConfig.LocalAPI, requestData);

                if (response)
                {
                    NovaAPI.APIUsers.users.Remove(UserData);
                    UsersGrid.Items.Refresh();
                }
                else
                {
                    MessageBox.Show($"Error al eliminar el usuario, INFO: {Environment.NewLine}{NovaAPI.APIUsers.Message}");
                }
            }

        }

        //New user action
        private void NewUserBT_Click(object sender, RoutedEventArgs e)
        {
            //From grid animation
            if (FormGrid.Opacity == 0)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("PopUpGrid"));
                UserNameTX.Focus();
                SaveBT.IsEnabled = true;

            }
            else if (UserNameTX.Text.Length == 0)
            {
                FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                ResetForm();
            }
            //Clear branch values
            if (UserNameTX.Text.Length > 0)
            {
                ResetForm();
            }

        }

        private void RefreshBT_Click(object sender, RoutedEventArgs e)
        {
            FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
            ResetForm();
            LoadData();
        }

        //Reset form data and visibility
        private void ResetForm()
        {
            //EditPermBT.IsEnabled = false;
            UserNameTX.Clear();
            UserRealTX.Clear();
            UserPassword.Clear();
            FormGrid.MaxHeight = 230;
            CheckPassword.Visibility = Visibility.Collapsed;
            PasswordConfirm.Clear();
            SelectedIndex = "";
            UserRolCB.SelectedIndex = 0;
            UserBranchCB.SelectedIndex = 0;
            SaveBT.IsEnabled = false;
        }

        private void IsEnabledControls(bool value)
        {
            if (value)
            {
                RefreshBT.IsEnabled = true;
                NewUserBT.IsEnabled = true;

            }
            else
            {
                RefreshBT.IsEnabled = false;
                NewUserBT.IsEnabled = false;
            }
        }

        //On password changed
        private void UserPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (UserPassword.IsFocused)
            {
                //Change password confirmation visibility
                CheckPassword.Visibility = Visibility.Visible;
                //Reorganize form control
                FormGrid.MaxHeight = 300;
                FormGrid.Height = 300;
                CheckPassworkFunction();
            }                
        }


        /// <summary>
        /// Password check, security, lenght and match
        /// </summary>
        private void CheckPassworkFunction()
        {
            if (UserPassword.Password != PasswordConfirm.Password)
            {
                PasswordLabel.Content = "Las contraseñas no coinciden";
                PasswordLabel.Foreground = new SolidColorBrush(Colors.Red);
                SaveBT.IsEnabled = false;
            }
            else if (UserPassword.Password.Length < 5)
            {
                PasswordLabel.Content = "La contraseña debe ser superior a 5 caracteres";
                PasswordLabel.Foreground = new SolidColorBrush(Colors.Red);
                SaveBT.IsEnabled = false;
            } else {

                PasswordLabel.Content = "La contraseña es correcta";
                PasswordLabel.Foreground = new SolidColorBrush(Colors.Green);
                SaveBT.IsEnabled = true;
            }
        }

        private void PasswordConfirm_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (PasswordConfirm.IsFocused)
            {
                CheckPassworkFunction();
            }
        }

        /// <summary>
        /// Save user information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SaveBT_Click(object sender, RoutedEventArgs e)
        {
            NewUserBT.Focus();

            if (UserNameTX.Text.Length == 0 || UserNameTX.Text.Length < 5)
            {
                MessageBox.Show("El nombre de usuario no puede estar en blanco o ser inferior a 5 caracteres");
                UserNameTX.Focus();
                return;
            }
            else if (UserPassword.Password.Length == 0 || UserPassword.Password.Length < 5)
            {
                MessageBox.Show("la contraseña no puede estar en blanco o ser inferior a 5 caracteres");
                UserPassword.Focus();
                return;
            }

            //Get/Set user parameters
            var Data = new NovaAPI.APIUsers.UsersClass();
            Data.id = SelectedIndex;
            Data.name = UserNameTX.Text;
            Data.realname = UserRealTX.Text;

            Data.rolid = ((NovaAPI.APIRoles.Rols)UserRolCB.SelectedItem).rolid;
            Data.branchid = ((NovaAPI.APIBranch.BranchClass)UserBranchCB.SelectedItem).id;
            Data.status = UserStatusCB.IsChecked == true ? "1" : "0";

            //Password logic
            if (CheckPassword.Visibility == Visibility.Visible)
            {
                Data.hash = GeneralFunctions.GenerateMD5(UserPassword.Password);
            }
            
            //rol json data
            string requestData = JsonConvert.SerializeObject(Data);

            bool response;

            //Modify / Create request
            if (Data.id.Length > 0)
            {
                response = await NovaAPI.APIUsers.GetValues("2", DataConfig.LocalAPI, requestData);

            }
            else
            {
                response = await NovaAPI.APIUsers.GetValues("1", DataConfig.LocalAPI, requestData);
            }

            //Request response
            if (response)
            {
                if (Data.id.Length > 0)
                {
                    //On user modified
                    var UserData = NovaAPI.APIUsers.users.Find(x => x.id == Data.id);
                    UserData.name = Data.name;
                    UserData.realname = Data.realname;
                    UserData.branchid = Data.branchid;
                    UserData.branchname = NovaAPI.APIBranch.branch.Find(x => x.id == Data.branchid).name;
                    UserData.rolid = Data.rolid;
                    UserData.rolname = NovaAPI.APIRoles.userrols.Find(x => x.rolid == Data.rolid).rolname;                    
                    UserData.status = Data.status;

                    UsersGrid.Items.Refresh();
                    FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                    ResetForm();
                }
                else
                {
                    //On new user created response
                    var UserData = new NovaAPI.APIUsers.UsersClass();
                    UserData.name = Data.name;
                    UserData.realname = Data.realname;
                    UserData.branchid = Data.branchid;
                    UserData.branchname = NovaAPI.APIBranch.branch.Find(x => x.id == Data.branchid).name;
                    UserData.rolid = Data.rolid;
                    UserData.rolname = NovaAPI.APIRoles.userrols.Find(x => x.rolid == Data.rolid).rolname;
                    UserData.status = Data.status;

                    UserData.id = NovaAPI.APIUsers.LastID;

                    FormGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("FadeInGrid"));
                    ResetForm();

                    NovaAPI.APIUsers.users.Add(UserData);

                    //Reload rol data
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show($"Error al crear el usuario, INFO: {Environment.NewLine}{NovaAPI.APIUsers.Message}");
            }
        }

        private void UserNameTX_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UserNameTX.IsFocused)
            {
                if(UserNameTX.Text.Length < 5)
                {
                    UserNameTX.BorderBrush = new SolidColorBrush(Colors.Red);
                } else
                {
                    UserNameTX.BorderBrush = new SolidColorBrush(Colors.Green);
                }
            }
        }

        private void UserStatusCB_Checked(object sender, RoutedEventArgs e)
        {
            if (UserStatusCB.IsChecked == true)
            {
                UserStatusCB.Content = "Habilitado";
                UserStatusCB.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                UserStatusCB.Content = "Deshabilitado";
                UserStatusCB.Foreground = new SolidColorBrush(Colors.Red);

            }
        }
    }


    //Value converter status
    public class YesNoToBooleanConverter : IValueConverter
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
}
