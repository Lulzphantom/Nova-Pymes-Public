using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Nova
{
    enum Status
    {
        Normal,
        Error,
        Pass
    }

    class LoginViewModel : BaseViewModel
    {

        #region Private Members
        private Visibility _LoadingSpinner;

        #endregion

        #region Public Members

        //Login Input visibility
        public Visibility StackInput { get; set; } = Visibility.Collapsed;

        //Login Loading visibility
        public Visibility LoadingSpinner { get { return _LoadingSpinner; }
            set
            {
                StackInput = value == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
                Spin = value == Visibility.Collapsed ? false : true;
                _LoadingSpinner = value;
            }
        }
        public bool Spin { get; set; } = true;

        //WorkPlace Label content
        public string WorkPlaceContent { get; set; } = "Verificando Configuración";

        //Status Info
        public SolidColorBrush StatusColor { get; set; } = (SolidColorBrush)Application.Current.TryFindResource("NormalBrush");
        public string StatusContent { get; set; } = "Cargando datos ...";

        #endregion

        #region Constructor
        /// <summary>
        /// Login public constructor
        /// </summary>
        public LoginViewModel()
        {
            
        }

        #endregion

        #region Public login functions

        /// <summary>
        /// Set status bar content
        /// </summary>
        /// <param name="Content">Text content</param>
        /// <param name="status">Color status</param>
        public void SetStatus(string Content, Status status = Status.Normal)
        {
            StatusContent = Content;
            switch (status)
            {
                case Status.Normal:
                    StatusColor = (SolidColorBrush)Application.Current.TryFindResource("NormalBrush");
                    break;
                case Status.Error:
                    StatusColor = (SolidColorBrush)Application.Current.TryFindResource("ErrorBrush");
                    break;
                case Status.Pass:
                    StatusColor = (SolidColorBrush)Application.Current.TryFindResource("PassBrush");
                    break;
            }
        }

        #endregion
    }
}
