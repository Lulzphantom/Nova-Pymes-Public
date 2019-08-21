using Squirrel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Nova
{

    ///NOVAPYMES - (C)2019

    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        LoginViewModel ViewModel;

        public MainWindow()
        {
            InitializeComponent();

            //Set ViewModel Data context
            DataContext = new LoginViewModel();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Access to LoginViewModel
            ViewModel = (LoginViewModel)DataContext;

            //Load local configuration
            ViewModel.SetStatus("Cargando configuracion local...");
            if (!DataConfig.LoadConfig())
            {
                MessageBox.Show($"Error al cargar la configuración local del programa, {Environment.NewLine}Por favor comunicarse con el soporte. -LOGINLOAD");

            }

            //Verifing configuration status
            if (DataConfig.LocalAPI.Length == 0)
            {
                //Config status
                ViewModel.LoadingSpinner = Visibility.Collapsed;
                ViewModel.SetStatus("Verificar configuración", Status.Normal);
                ConfigButton.IsEnabled = true;
                //Open configuration page
                GoToConfig();
            }
            else
            {

                //CHECK API STATUS
                bool status = await NovaAPI.APIStatus.GetValues(DataConfig.LocalAPI);

                if (!status)
                {
                    //API DISCONNECTED
                    ViewModel.SetStatus("No se pudo establecer conexión al servidor", Status.Error);
                    ConfigButton.IsEnabled = true;
                    ViewModel.LoadingSpinner = Visibility.Collapsed;
                    return;
                }


                //WorkPlace label set content
                ViewModel.WorkPlaceContent = DataConfig.WorkPlaceLabel;
                Title = $"{Title} - {DataConfig.WorkPlaceLabel}";

                string UpdateStatus = "";

                //Check updates      
                if (DataConfig.LocalUpdates == true && DataConfig.Initialized == 0)
                {
                    ViewModel.SetStatus("Verificando nuevas actualizaciones...");

                    string UpdateURL = DataConfig.CloudUpdatesURL;
                    //Check Update Path
                    try
                    {
                        WebRequest request;

                        try
                        {
                            request = WebRequest.Create($"{DataConfig.LocalAPI}/release");
                            UpdateURL = $"{DataConfig.LocalAPI}/release";
                        }
                        catch (Exception)
                        {
                            request = WebRequest.Create($"{DataConfig.CloudUpdatesURL}/release");
                            UpdateURL = $"{DataConfig.CloudUpdatesURL}/release";
                        }

                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    }
                    catch (Exception) { }


                    //Check for update
                    try
                    {
                        using (var mgr = new UpdateManager(UpdateURL))
                        {
                            var releaseEntry = await mgr.UpdateApp();                           
                            //Restart on Update
                            if (releaseEntry != null)
                            {
                                if (MessageBox.Show("Se ha descargado una nueva actualización, ¿deseas reiniciar el programa para aplicarla?", "Actualización", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                {
                                    try
                                    {
                                        UpdateManager.RestartApp();
                                    }
                                    catch (Exception)
                                    {

                                    }
                                    
                                }
                            }
                        }
                    }
                    catch (Exception ex) { UpdateStatus = ex.Message; }

                    //Download Squirrel update
                    //try
                    //{    
                    //    using (var mgr = new UpdateManager(UpdateURL))
                    //    {
                    //        //Check update info
                    //        var updateInfo = mgr.CheckForUpdate().Result;

                    //        //Download release and progress
                    //        await mgr.DownloadReleases(updateInfo.ReleasesToApply, DownloadProgress).ContinueWith((t) =>
                    //        {
                    //            mgr.ApplyReleases(updateInfo, DownloadProgress)
                    //           .ContinueWith((x) =>
                    //           {
                    //               Application.Current.Dispatcher.Invoke(() =>
                    //               {
                    //                   //Restart on Update
                    //                   UpdateManager.RestartApp();
                    //               });
                    //           });
                    //        });

                    //        //DataConfig.CurrVersion = mgr.CurrentlyInstalledVersion().ToString();
                            

                    //    }
                    //} //On update error
                    //catch (Exception ex) { UpdateStatus = ex.Message; }
                }               

                //Pass config/update validation
                ViewModel.SetStatus($"Versión de software: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} {UpdateStatus}", Status.Pass);
                ViewModel.LoadingSpinner = Visibility.Collapsed;                
                //Check user and password for focus
                if (DataConfig.Username.Length > 0)
                {
                    UsernameTX.Text = DataConfig.Username;
                    PasswordTX.Focus();
                }
                else
                {
                    UsernameTX.Focus();
                }                
                ConfigButton.IsEnabled = true;

                DataConfig.Initialized = 1;
            }
        }

        private void DownloadProgress(int a)
        {
            MessageBox.Show(a.ToString());
        }


        #region Configuration logic
        /// <summary>
        /// Configuration page proccess
        /// </summary>
        private void GoToConfig()
        {
            //Show configuration page
            ConfigGrid.Visibility = Visibility.Visible;
            LoginGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("SlideGridOut"));
        }
        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            GoToConfig();
        }
        private void ConfigReturnButton_Click(object sender, RoutedEventArgs e)
        {
            //Return to login page
            LoginGrid.BeginStoryboard((Storyboard)Application.Current.TryFindResource("SlideGridIn"));
            ConfigGrid.Visibility = Visibility.Collapsed;

            if (NovaAPI.APIStatus.Success == 1)
            {
                ViewModel.WorkPlaceContent = DataConfig.WorkPlaceLabel;
                ViewModel.SetStatus("Versión de software 1.0.0", Status.Pass);

            }

        }
        #endregion

        /// <summary>
        /// Login logic
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LoginBT_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadingSpinner = Visibility.Visible;
            ViewModel.SetStatus("Iniciando sesión ...", Status.Normal);

            bool loginstatus = await NovaAPI.APILoginData.GetValues(UsernameTX.Text, GeneralFunctions.GenerateMD5(PasswordTX.Password).ToUpper(), DataConfig.LocalAPI);

            if (loginstatus)
            {
                //LOGIN SUCCESS!
                DataConfig.RealName = NovaAPI.APILoginData.realname; //Set user realname                
                DataConfig.Username = UsernameTX.Text; //Set username
                DataConfig.SaveConfig(); //Save parameters

                try
                {
                    NovaAPI.APIPermissions.RolData.Clear();
                }
                catch (Exception)
                {

                }

                //GET USER PERMISSIONS
                bool upermissions = await (NovaAPI.APIPermissions.GetValues(NovaAPI.APILoginData.userid.ToString(), "1", DataConfig.LocalAPI));
                if (upermissions)
                {
                    //Load permissions OK!
                    DataConfig.UserRole = NovaAPI.APIPermissions.RolName;
                }
                else
                {
                    //Load permissions Fail!
                    ViewModel.SetStatus($"Error al cargar los permisos: {NovaAPI.APIPermissions.Message}", Status.Error);
                    ViewModel.LoadingSpinner = Visibility.Collapsed;
                }

                //Show principal work window
                var newWindow = new PrincipalWindow();
                newWindow.Show();
                //Close login window
                Close();
            }
            else
            {
                //LOGIN FAIL
                ViewModel.SetStatus("No se pudo iniciar sesión " + NovaAPI.APILoginData.Message, Status.Error);
                ViewModel.LoadingSpinner = Visibility.Collapsed;
                UsernameTX.Focus();
            }            
        }
        /// <summary>
        /// Input move focus on ENTER key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveFocus(object sender, KeyEventArgs e)
        {
            var item = (Control)sender;
            if (e.Key == Key.Enter && item.Name == "PasswordTX")
            {
                LoginBT_Click(sender, e);

            } else if (e.Key == Key.Enter)
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

        private void UsernameTX_GotFocus(object sender, dynamic e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void PasswordTX_GotFocus(object sender, dynamic e)
        {
            ((PasswordBox)sender).SelectAll();
        }
    }
       
}
