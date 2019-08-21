using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Nova
{
    class PincipalViewModel : BaseViewModel
    {

        #region private members


        #endregion

        #region Public members

        //User information section
        public ImageSource MenuUserLogo { get; set; } = null;
        public string UsernameLabel { get; set; }
        public string userinfoLabel { get; set; }

        //Header information section
        public string WorkplaceNameInfo { get; set; }
        public string ConnectionInfo { get; set; }
        #endregion




        #region Constructor
        public PincipalViewModel()
        {
            //SET IMAGE FROM CACHE!
            MenuUserLogo = new BitmapImage(new Uri("pack://application:,,,/Assets/Logos/UserLogo.png"));

            //Set realname label
            UsernameLabel = NovaAPI.APILoginData.realname;
            userinfoLabel = DataConfig.UserRole;

            //WorkplaceInfo
            try
            {
                WorkplaceNameInfo = $"{NovaAPI.APIStatus.OwnerName} - {NovaAPI.APIStatus.Branch.Find(x => x.BranchID == DataConfig.WorkPlaceID).BranchName} - {NovaAPI.APIStatus.BoxData.Find(x => x.BoxID == DataConfig.WorkPointID).BoxName}";
            }
            catch (Exception)
            {

                WorkplaceNameInfo = $"Error en informacion de sucursal";
            }
            
            //Conection info
            ConnectionInfo = $"Conectado a: {DataConfig.LocalAPI}";

        }
        #endregion

    }
}
