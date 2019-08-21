using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Nova.Pages.Reports.SellReportsPages
{
    /// <summary>
    /// Lógica de interacción para BoxMovements.xaml
    /// </summary>
    public partial class BoxMovements : Page
    {
        int Pagination = 1;

        public BoxMovements()
        {
            InitializeComponent();


            if (Cache.Cache.SelectedSellReport == "0")
            {
                //Set columns

                DataGridTextColumn column = new DataGridTextColumn
                {
                    Header = "CAJA",
                    Binding = new Binding($"name"),
                    MinWidth = 100,
                    MaxWidth = 120                    
                };

                DataGridTextColumn column1 = new DataGridTextColumn
                {
                    Header = "F. APERTURA",
                    Binding = new Binding($"opendate"),
                    MinWidth = 100,
                    MaxWidth = 150                    
                };

                DataGridTextColumn column2 = new DataGridTextColumn
                {
                    Header = "REALIZÓ",
                    Binding = new Binding($"openuser"),
                    MinWidth = 100,
                    MaxWidth = 150                    
                };

                Binding OpenValue = new Binding($"openvalueint");
                OpenValue.StringFormat = "{0:C0}";

                DataGridTextColumn column3 = new DataGridTextColumn
                {
                    Header = "MONTO",
                    Binding = OpenValue,
                    MinWidth = 100,
                    MaxWidth = 150,
                    HeaderStyle = (Style)Application.Current.TryFindResource("DataGridHeaderStyledRight"),                    
                    CellStyle = (Style)Application.Current.TryFindResource("DataGridCellStyledRight")
                };

                DataGridTextColumn column4 = new DataGridTextColumn
                {
                    Header = "F. CIERRE",
                    Binding = new Binding($"closedate"),
                    MinWidth = 100,
                    MaxWidth = 150
                };

                DataGridTextColumn column5 = new DataGridTextColumn
                {
                    Header = "REALIZÓ",
                    Binding = new Binding($"closeuser"),
                    MinWidth = 100,
                    MaxWidth = 150
                };


                Binding CloseValue = new Binding($"closevalueint");
                CloseValue.StringFormat = "{0:C0}";

                DataGridTextColumn column6 = new DataGridTextColumn
                {
                    Header = "MONTO",
                    Binding = CloseValue,
                    MinWidth = 100,
                    MaxWidth = 150,
                    HeaderStyle = (Style)Application.Current.TryFindResource("DataGridHeaderStyledRight"),
                    CellStyle = (Style)Application.Current.TryFindResource("DataGridCellStyledRight")
                };

                DataGridTextColumn column7 = new DataGridTextColumn
                {
                   Width = new DataGridLength(1,DataGridLengthUnitType.Star)
                };

                BoxAction.Visibility = Visibility.Visible;

                ContentDataGrid.Columns.Insert(ContentDataGrid.Columns.Count - 1, column);
                ContentDataGrid.Columns.Insert(ContentDataGrid.Columns.Count - 1, column1);
                ContentDataGrid.Columns.Insert(ContentDataGrid.Columns.Count - 1, column2);
                ContentDataGrid.Columns.Insert(ContentDataGrid.Columns.Count - 1, column3);
                ContentDataGrid.Columns.Insert(ContentDataGrid.Columns.Count - 1, column4);
                ContentDataGrid.Columns.Insert(ContentDataGrid.Columns.Count - 1, column5);
                ContentDataGrid.Columns.Insert(ContentDataGrid.Columns.Count - 1, column6);
                ContentDataGrid.Columns.Insert(ContentDataGrid.Columns.Count - 1, column7);                
                
            } //Box movements
            else if (Cache.Cache.SelectedSellReport == "1")
            {
                DataGridTextColumn columndate = new DataGridTextColumn
                {
                    Header = "FECHA",
                    Binding = new Binding($"date")
                };

                DataGridTextColumn column = new DataGridTextColumn
                {
                    Header = "No. FACTURA",
                    Binding = new Binding($"ticket_id"),
                    HeaderStyle = (Style)Application.Current.TryFindResource("DataGridHeaderStyledRight"),
                    CellStyle = (Style)Application.Current.TryFindResource("DataGridCellStyledRight")
                };

                DataGridTextColumn column1 = new DataGridTextColumn
                {
                    Header = "PRODUCTO",
                    Binding = new Binding($"product_name"),
                    MinWidth = 120,
                    MaxWidth = 250
                };

                DataGridTextColumn column2 = new DataGridTextColumn
                {
                    Header = "CANT.",
                    Binding = new Binding($"product_count"),
                    HeaderStyle = (Style)Application.Current.TryFindResource("DataGridHeaderStyledRight"),
                    CellStyle = (Style)Application.Current.TryFindResource("DataGridCellStyledRight")
                };

                DataGridTextColumn column3 = new DataGridTextColumn
                {
                    Header = "TIPO",
                    Binding = new Binding($"unity_string"),
                };

                Binding uValue = new Binding($"product_price");
                uValue.StringFormat = "{0:C0}";

                DataGridTextColumn column4 = new DataGridTextColumn
                {
                    Header = "V. UNITARIO",
                    Binding = uValue,
                    MinWidth = 120,
                    MaxWidth = 150,
                    HeaderStyle = (Style)Application.Current.TryFindResource("DataGridHeaderStyledRight"),
                    CellStyle = (Style)Application.Current.TryFindResource("DataGridCellStyledRight")
                };

                Binding sTotal = new Binding($"product_total");
                sTotal.StringFormat = "{0:C0}";

                DataGridTextColumn column5 = new DataGridTextColumn
                {
                    Header = "SUBTOTAL",
                    Binding = sTotal,
                    MinWidth = 120,
                    MaxWidth = 150,
                    HeaderStyle = (Style)Application.Current.TryFindResource("DataGridHeaderStyledRight"),
                    CellStyle = (Style)Application.Current.TryFindResource("DataGridCellStyledRight")
                };

                DataGridTextColumn column6 = new DataGridTextColumn
                {
                    Header = "PAGO",
                    Binding = new Binding($"payment")
                };

                ContentDataGrid.Columns.Insert(ContentDataGrid.Columns.Count - 1, columndate);
                ContentDataGrid.Columns.Insert(ContentDataGrid.Columns.Count - 1, column);
                ContentDataGrid.Columns.Insert(ContentDataGrid.Columns.Count - 1, column1);
                ContentDataGrid.Columns.Insert(ContentDataGrid.Columns.Count - 1, column2);
                ContentDataGrid.Columns.Insert(ContentDataGrid.Columns.Count - 1, column3);
                ContentDataGrid.Columns.Insert(ContentDataGrid.Columns.Count - 1, column4);
                ContentDataGrid.Columns.Insert(ContentDataGrid.Columns.Count - 1, column5);
                ContentDataGrid.Columns.Insert(ContentDataGrid.Columns.Count - 1, column6);
            } //Sell reports
        } //SET COLUMNS

        private void SellPage_Loaded(object sender, RoutedEventArgs e)
        {
            //-----
            LoadFilterData(); //Load and set filter values      

            FilterBT_Click(null, null);
        }

        private async void LoadFilterData()
        {
            //Set date controls days
            FromDateDT.SelectedDate = DateTime.Now.AddDays(-31);
            ToDateDT.SelectedDate = DateTime.Now.AddDays(1);

            try
            {
                NovaAPI.APIBranch.branch.Clear();
            }
            catch (Exception) { }

            //Request branch information
            bool BranchRequest = await NovaAPI.APIBranch.GetValues("4", DataConfig.LocalAPI, null);

            if (!BranchRequest)
            {
                MessageBox.Show($"Error al cargar datos {NovaAPI.APIBranch.Message}");
                return;
            }
            //Set branch combobox
            BranchCB.ItemsSource = NovaAPI.APIBranch.branch;
            BranchCB.Items.Refresh();

            BranchCB.SelectedIndex = 0;

            SetFilterHint(); //Set filter label info
        }

        private void SetFilterHint()
        {
            FilterLB.Text = $"FILTRANDO POR SUCURSAL: {((NovaAPI.APIBranch.BranchClass)BranchCB.SelectedItem).name} , PUNTO DE VENTA: {((NovaAPI.APIStatus.box)BoxCB.SelectedItem).BoxName}, " +
                $"DESDE: {FromDateDT.SelectedDate.Value.ToShortDateString()}, HASTA: {ToDateDT.SelectedDate.Value.ToShortDateString()}";
        }

        private async void LoadData(string Page = null)
        {
            //Set loading grid visibility
            LoadingGrid.Visibility = Visibility.Visible;
            Spinner.Spin = true;

            //Try to clear existent list
            try
            {
                NovaAPI.APIReports.Boxitems.Clear();
                NovaAPI.APIReports.Sellitems.Clear();
            }
            catch (Exception) { }

            //Set data for request
            var Data = new NovaAPI.APIReports.RequestData
            {
                branch_id = ((NovaAPI.APIBranch.BranchClass)BranchCB.SelectedItem).id,
                box_id = ((NovaAPI.APIStatus.box)BoxCB.SelectedItem).BoxID.ToString(),
                date_from = FromDateDT.SelectedDate.Value.ToString("yyyy-MM-dd"),
                date_to = ToDateDT.SelectedDate.Value.ToString("yyyy-MM-dd"),
                from = Page
            };

            //Create string request
            string requestData = JsonConvert.SerializeObject(Data);

            //Send request
            string requestType = Cache.Cache.SelectedSellReport == "0" ? "1" : "2";
            bool Response = await NovaAPI.APIReports.GetValues(requestType, DataConfig.LocalAPI, requestData);

            if (Response)
            {
                //Set data to datagrid
                if (requestType == "1")
                {
                    ContentDataGrid.ItemsSource = NovaAPI.APIReports.Boxitems;                    
                }
                else
                {
                    ContentDataGrid.ItemsSource = NovaAPI.APIReports.Sellitems;                    
                }
                TotalValueLB.Text = requestType == "1" ? "" : string.Format("Venta total: {0:C0} \t Creditos: {1:C0}", NovaAPI.APIReports.Total, NovaAPI.APIReports.Credits);
                TotalValueLB.Visibility = requestType == "1" ? Visibility.Collapsed : Visibility.Visible;
                ContentDataGrid.Items.Refresh();

                TotalProducts.Content = NovaAPI.APIReports.Count;

                //Set pagination info
                double Pages = (Convert.ToInt32(NovaAPI.APIReports.Count) / 15);
                double TotalPages = Math.Floor(Pages);

                SetPagination(TotalPages);
            }
            else
            {
                //Reset pagination data
                ContentDataGrid.Items.Refresh();
                TotalProducts.Content = "0";
                TotalValueLB.Text = "";
                Pagination = 1;
                SetPagination(0);

                MessageBox.Show(NovaAPI.APIReports.Message);

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

            LPage.Content = (((Pagination - 1) * 15) + ContentDataGrid.Items.Count).ToString();


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
                command.Background = (SolidColorBrush)Application.Current.TryFindResource("ReportsHeader");
                command.Click += Page_Click;
                command.Content = "«";
                PaginationStack.Children.Add(command);

                Button command2 = new Button();
                command2.Tag = Pagination - 1;
                command2.Background = (SolidColorBrush)Application.Current.TryFindResource("ReportsHeader");
                command2.Click += Page_Click;
                command2.Style = (Style)Application.Current.TryFindResource("PaginationCenterButton");
                command2.Content = "‹";
                PaginationStack.Children.Add(command2);
            }

            for (int i = 1; i <= TotalPages + 1; i++)
            {
                //Define button controls            
                Button page = new Button();
                page.Background = (SolidColorBrush)Application.Current.TryFindResource("ReportsHeader");
                page.Style = (Style)Application.Current.TryFindResource("PaginationCenterButton");

                if (i == Pagination)
                {
                    //Print current page number

                    page.Tag = (i).ToString();
                    page.Content = page.Tag;
                    page.BorderBrush = (SolidColorBrush)Application.Current.TryFindResource("ReportsHeader");
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
                command2.Background = (SolidColorBrush)Application.Current.TryFindResource("ReportsHeader");
                command2.Click += Page_Click;
                command2.Style = (Style)Application.Current.TryFindResource("PaginationCenterButton");
                command2.Content = "›";
                PaginationStack.Children.Add(command2);

                //Define button controls            
                Button command = new Button();
                command.Tag = TotalPages + 1;
                command.Background = (SolidColorBrush)Application.Current.TryFindResource("ReportsHeader");
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
            LoadData(((Pagination - 1) * 15).ToString());
        }

        //Popup open
        private void FilterBT_Click(object sender, RoutedEventArgs e)
        {
            ReportDock.IsEnabled = false;
            PaginationGrid.IsEnabled = false;

            FilterPopUp.IsOpen = true;
        }

        //Exit popup click
        private void ExitPopUp_Click(object sender, RoutedEventArgs e)
        {
            ReportDock.IsEnabled = true;
            PaginationGrid.IsEnabled = true;

            FilterPopUp.IsOpen = false;
        }

        //Selection change, set BOX data
        private void BranchCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && BranchCB.SelectedItem != null)
            {
                //Search for boxes on branch
                var Boxes = NovaAPI.APIStatus.BoxData.FindAll(x => x.BoxBranch == ((NovaAPI.APIBranch.BranchClass)BranchCB.SelectedItem).id);

                if (Boxes.Count > 0)
                {

                    BoxCB.Items.Clear(); //clear boxCB item data

                    for (int i = 0; i < Boxes.Count + 1; i++)
                    {
                        if (i == 0)
                        {
                            var AllItems = new NovaAPI.APIStatus.box
                            {
                                BoxName = " TODOS ",
                                BoxBranch = "0",
                                BoxID = 0
                            };

                            BoxCB.Items.Add(AllItems);
                        }
                        else
                        {
                            BoxCB.Items.Add(Boxes[i - 1]);
                        }
                    }

                    BoxCB.SelectedIndex = 0;
                }
            }            
        }

        private void ApplyFilterBT_Click(object sender, RoutedEventArgs e)
        {
            //Set label values
            SetFilterHint();

            //Close popup
            ExitPopUp_Click(null, null);

            Pagination = 1;
            //Load data
            LoadData();
        }

        //Print box movement
        private async void Print_Click(object sender, RoutedEventArgs e)
        {
            var data = new NovaAPI.APIBoxMovements.BoxData
            {
                id = ((Button)sender).Tag.ToString()
            };

            string requestData = JsonConvert.SerializeObject(data);

            bool Detailresponse = await NovaAPI.APIBoxMovements.GetValues("4", DataConfig.LocalAPI, requestData);

            if (Detailresponse)
            {
                PrintFunctions.PrintFunctions.Print(Classes.Enums.PrintPages.BoxDetail, null);
            }
            else
            {
                MessageBox.Show("No se pudo realizar la impresion, error:" + NovaAPI.APIBoxMovements.Message);
            }
        }

        private void RefreshPage_Click(object sender, RoutedEventArgs e)
        {
            ContentDataGrid.ItemsSource = null;
            ContentDataGrid.Items.Refresh();

            LoadFilterData();
            TotalValueLB.Text = "";
            ExportBT.IsEnabled = false;
            Pagination = 1;
            TotalProducts.Content = "0";
            SetPagination(0);            
        }

        private void ExportBT_Click(object sender, RoutedEventArgs e)
        {
            //Loading status
            ReportDock.IsEnabled = false;
            PaginationGrid.IsEnabled = false;
            Spinner.Spin = true;
            LoadingLabel.Content = "Generando informe de datos ...";
            LoadingGrid.Visibility = Visibility.Visible;
            
            ExportFunction();

            

        }

        private async Task ExportFunction()
        {
            var initInstance = NovaAPI.APIReports.Boxitems;

            //Create a new List<T> 
            var TestList = new List<NovaAPI.APIReports.BoxitemsClass>();

            double Pages = (Convert.ToInt32(NovaAPI.APIReports.Count) / 15);
            double TotalPages = Math.Floor(Pages);


            //Get list of all items
            for (int i = 0; i < TotalPages + 1; i++)
            {
                LoadingLabel.Content = $"Obteniendo datos {i + 1} de {TotalPages + 1} paginas...";
                //Try to clear existent list
                try
                {
                    NovaAPI.APIReports.Boxitems.Clear();
                    NovaAPI.APIReports.Sellitems.Clear();
                }
                catch (Exception) { }

                var Data = new NovaAPI.APIReports.RequestData
                {
                    branch_id = ((NovaAPI.APIBranch.BranchClass)BranchCB.SelectedItem).id,
                    box_id = ((NovaAPI.APIStatus.box)BoxCB.SelectedItem).BoxID.ToString(),
                    date_from = FromDateDT.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    date_to = ToDateDT.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    from = (i * 15).ToString()
                };

                //Create string request
                string requestData = JsonConvert.SerializeObject(Data);

                //Send request
                string requestType = Cache.Cache.SelectedSellReport == "0" ? "1" : "2";
                bool Response = await NovaAPI.APIReports.GetValues(requestType, DataConfig.LocalAPI, requestData);
                if (Response)
                {
                    TestList.AddRange(NovaAPI.APIReports.Boxitems);
                }
                else
                {
                    MessageBox.Show(NovaAPI.APIReports.Message);
                    return;
                }

                await Task.Delay(100);
            }

            LoadingLabel.Content = "Generando informe de datos ...";

            //Create datatable 
            DataTable dt = new DataTable();

            //Asing datatable colummns
            dt = await ExportModule.ConvertToDataTable(TestList.Select(l => new {
                NOMBRE = l.name,
                APERTURA = l.opendate,
                USUARIO_APERTURA = l.openuser,
                VALOR_APERTURA = l.openvalue,
                CIERRE = l.closedate,
                USUARIO_CIERRE = l.closeuser,
                VALOR_CIERRE = l.closevalue,
                COMMENTARIOS = l.comment
            }).ToList());
            

            //Openfile dialog file path
            await ExportModule.GenerateExcel(dt, @"C:\Users\nicot\OneDrive\Documentos 1\CPY_SAVES\TOTAL.xls", "TEst");

            NovaAPI.APIReports.Boxitems = initInstance;

            //Restore loading status
            ReportDock.IsEnabled = true;
            PaginationGrid.IsEnabled = true;
            LoadingGrid.Visibility = Visibility.Collapsed;
            Spinner.Spin = false;
            LoadingLabel.Content = "Cargando datos";
        }
    }
}
