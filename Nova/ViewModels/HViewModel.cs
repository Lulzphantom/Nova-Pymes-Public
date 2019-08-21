using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Nova.Pages;

namespace Nova
{
    class HViewModel : BaseViewModel
    {
        //Public members
        public bool HOperating = false;

        public ICommand HCommand { get; set; }


        //Constructor
        public HViewModel(object HostPage, int ClassIndex)
        {
            //Create command
            HCommand = new RelayCommand(() => SetHOperating(HostPage, ClassIndex));
        }

        private void SetHOperating(object HostPage, int ClassIndex)
        {
            HOperating = HOperating == true ? false : true;

            switch (ClassIndex)
            {
                case 0:
                    break;
                case 1: //Inventory Report
                    ((Pages.Products.InventoryPages.InventoryReportPage)HostPage).SetH(HOperating);
                    break;
                case 5:
                    ((Pages.Products.ProductCategoryPage)HostPage).SetH(HOperating);
                    break;
                case 6:
                    ((Pages.POS.POSListPage)HostPage).SetH(HOperating);
                    break;
                default:
                    break;
            }

            
        }

    }
}
