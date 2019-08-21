using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Classes
{
    class Enums
    {
        public enum ProductValues
        {
            Unidad,
            Paquete,
            Gramos,
            Libras,
            Kilos
            
        }

        public enum MovementTypes
        {
            In,
            Out,
            Adjust,
            Move,
            Return
        }

        public enum PrintPages
        {
            FinalTicket,
            CotiTicket,
            LeftPaymentTicket,
            TransferTicket,
            TestTicket,
            BoxDetail
        }
    }
}
