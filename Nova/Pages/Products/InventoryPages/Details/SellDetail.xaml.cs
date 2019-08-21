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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Nova.Pages.Products.InventoryPages.Details
{
    /// <summary>
    /// Lógica de interacción para SellDetail.xaml
    /// </summary>
    public partial class SellDetail : Page
    {
        public SellDetail()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Cache.Cache.SelectedMovement == "0" || Cache.Cache.SelectedMovement == "")
            {
                BillNumberLB.Content = "";
                BillDateLB.Content = "";
                BillMethodLB.Content = "";
                BillExpirationLB.Content = "";
                BillClientLB.Content = "";
                BillUserLB.Content = "";
                BillTotalLB.Content = "";
                BillTotalPayment.Content = "";
                BillExchangeLB.Content = "";
                BillLeftLB.Content = "";

                ProductsDataGrid.ItemsSource = null;
                ProductsDataGrid.Items.Refresh();

                return;
            }

            LoadTickets(Cache.Cache.SelectedMovement);
        }

        public async void LoadTickets(string Filter)
        {
            //Try to clear existent branch list
            try
            {
                NovaAPI.APITickets.tickets.Clear();
            }
            catch (Exception) { }

            //Send request
            var Data = new NovaAPI.APITickets.DataClass();            

            Data.filter = Filter;

            string requestData = JsonConvert.SerializeObject(Data);

            //Load branch 
            bool response = await NovaAPI.APITickets.GetValues("2", DataConfig.LocalAPI, requestData);
            if (response)
            {
                SetData(Filter);
            }
            else
            {
                //On load error
                MessageBox.Show($"Se produjo un error al cargar los datos, INFO: {Environment.NewLine}{NovaAPI.APITickets.Message}");
                await Task.Delay(100);
            }
        }

        private void SetData(string ID)
        {
            var BillData = NovaAPI.APITickets.tickets.Find(x => x.id == ID);

            BillNumberLB.Content = BillData.id;
            BillDateLB.Content = BillData.date;
            BillMethodLB.Content = BillData.payment_type == "0" ? "CONTADO" : "CREDITO";
            BillExpirationLB.Content = BillData.expiration_date;
            BillClientLB.Content = BillData.client_name;
            BillUserLB.Content = BillData.user_realname;
            BillTotalLB.Content = string.Format("{0:C0}", BillData.ticket_total_int);
            BillTotalPayment.Content = string.Format("{0:C0}", Convert.ToInt32(BillData.ticket_totalpayment));
            BillExchangeLB.Content = string.Format("{0:C0}", Convert.ToInt32(BillData.ticket_changepayment));
            BillLeftLB.Content = string.Format("{0:C0}", Convert.ToInt32(BillData.ticket_leftpayment));

            ProductsDataGrid.ItemsSource = BillData.items;
            ProductsDataGrid.Items.Refresh();
        }
    }
}
