using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Nova
{
    class POSContentViewModel : BaseViewModel
    {

        #region Public Members

        //H Command
        public bool HOperating = false;
        public ICommand HCommand { get; set; }

        //F1 Add print coti command
        public ICommand PrintCotiCommand { get; set; }

        //F2 Add client command
        public ICommand NewClientCommand { get; set; }

        //F3 Add product command
        public ICommand AddProductCommand { get; set; }

        //F8 End bill command
        public ICommand EndBillCommand { get; set; }

        #endregion





        //Constructor
        public POSContentViewModel(object HostPage)
        {

            //Create H command
            HCommand = new RelayCommand(() => ((Pages.POS.TabedPOSContent)HostPage).SetH());

            //Create AddProductCommand command
            AddProductCommand = new RelayCommand(() => ((Pages.POS.TabedPOSContent)HostPage).InProductBT_Click(null, null));

            //Create newClient command
            NewClientCommand = new RelayCommand(() => ((Pages.POS.TabedPOSContent)HostPage).NewClient_Click(null, null));

            //Create EndBill command
            EndBillCommand = new RelayCommand(() => ((Pages.POS.TabedPOSContent)HostPage).FinalIn_Click(null, null));

            //Print coti command
            PrintCotiCommand = new RelayCommand(() => ((Pages.POS.TabedPOSContent)HostPage).PrintCotiBT_Click(null, null));

        }
    }
}
