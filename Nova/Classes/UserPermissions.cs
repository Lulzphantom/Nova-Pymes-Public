using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova
{
    class UserPermissions
    {

        public int id { get; set; }
        public string name { get; set; }
        public PermissionData permissions { get; set; }

        public class PermissionData
        {
            public int create { get; set; }
            public int delete { get; set; }
            public int consult { get; set; }
        }

    }
}
