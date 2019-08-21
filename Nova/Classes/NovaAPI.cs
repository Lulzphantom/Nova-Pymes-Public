using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace Nova
{
    class NovaAPI
    {

        private static readonly HttpClient client = new HttpClient();

        private static string ApiStatusEndPoint = "/status.php";
        private static string ApiLoginEndPoint = "/novalogin.php";
        //Configuration API
        private static string ApiUserPermissionEndPoint = "/modules/users/userpermissions.php";
        private static string ApiUserRolesEndPoint = "/modules/users/userroles.php";
        private static string ApiBranchEndPoint = "/modules/config/branchconfig.php";
        private static string ApiUsersEndPoint = "/modules/users/users.php";
        //Inventory/products API
        private static string ApiSuppliersEndPoint = "/modules/products/suppliers.php";
        private static string ApiCategoriesEndPoint = "/modules/products/categories.php";
        private static string ApiPricesEndPoint = "/modules/products/prices.php";
        private static string ApiProductsEndPoint = "/modules/products/products.php";
        private static string ApiInventoryEndPoint = "/modules/products/inventory.php";
        //Tickets API
        private static string ApiBoxMovementsEndPoint = "/modules/tickets/box_movements.php";
        private static string ApiClientsEndPoint = "/modules/tickets/clients.php";
        private static string ApiTicketsEndPoint = "/modules/tickets/tickets.php";

        //Reports API
        private static string ApiSellReportsEndPoint = "/modules/reports/sellreports.php";

        #region Status API

        public class APIStatus
        {
            [JsonProperty("success")]
            public static int? Success { get; set; }

            [JsonProperty("error_message")]
            public static string Message { get; set; }

            [JsonProperty("name")]
            public static string OwnerName { get; set; }

            //Branchs
            [JsonProperty("Branch")]
            public static List<branch> Branch { get; set; } = new List<branch>();

            //Box points
            [JsonProperty("boxes")]
            public static List<box> BoxData { get; set; } = new List<box>();

            public class branch
            {
                public int BranchID { get; set; }
                public string BranchName { get; set; }
            }

            public class box
            {
                public int BoxID { get; set; }
                public string BoxName { get; set; }
                public string BoxBranch { get; set; }
            }

            /// <summary>
            /// Get Api status values
            /// </summary>
            public static async Task<bool> GetValues(string EndPoint = null)
            {
                string Response;
                try
                {
                    Response = await JsonResponseAsync(EndPoint + ApiStatusEndPoint);
                }
                catch (Exception)
                {
                    return false;
                }
                JsonConvert.DeserializeObject<APIStatus>(Response);
                return Success == 1 ? true : false;
            }
        }

        #endregion

        #region Login API

        public class APILoginData
        {
            [JsonProperty("success")]
            public static int Success { get; set; }

            [JsonProperty("token")]
            public static string TokenHash { get; set; }

            [JsonProperty("userid")]
            public static int userid { get; set; }

            [JsonProperty("userrol")]
            public static int userrol { get; set; }

            [JsonProperty("realname")]
            public static string realname { get; set; }

            [JsonProperty("photo")]
            public static string photo { get; set; }

            [JsonProperty("error_message")]
            public static string Message { get; set; }

            public static async Task<bool> GetValues(string Username, string PasswordHash, string EndPoint = null)
            {
                var Keys = new Dictionary<string, string>
                {
                    { "username", Username },
                    { "password", PasswordHash },
                    { "token", DataConfig.TokenHash }
                };

                string Response;
                try
                {
                    Response = await JsonResponseAsync(EndPoint + ApiLoginEndPoint, Keys);
                }
                catch (Exception)
                {
                    return false;
                }
                JsonConvert.DeserializeObject<APILoginData>(Response);
                DataConfig.TokenHash = Success == 1 ? TokenHash : null;
                return Success == 1 ? true : false;
            }

        }

        #endregion

        #region User Permissions API

        public class APIPermissions
        {
            [JsonProperty("success")]
            public static int? Success { get; set; }

            [JsonProperty("error_message")]
            public static string Message { get; set; }

            [JsonProperty("rolid")]
            public static string RolID { get; set; }

            [JsonProperty("rolname")]
            public static string RolName { get; set; }

            [JsonProperty("roldescription")]
            public static string RolDescription { get; set; }

            //Branchs
            [JsonProperty("roldata")]
            public static List<UserPermissions> RolData { get; set; } = new List<UserPermissions>();

            /// <summary>
            /// Get Api status values
            /// </summary>
            public static async Task<bool> GetValues(string userID = null, string function = null, string EndPoint = null, string data = null)
            {
                var Keys = new Dictionary<string, string>
                {
                    { "username", DataConfig.Username },
                    { "token", DataConfig.TokenHash },
                    { "userid", userID },
                    { "function", function },
                    { "data", data }
                };

                string Response;

                try
                {
                    Response = await JsonResponseAsync(EndPoint + ApiUserPermissionEndPoint, Keys);
                }
                catch (Exception)
                {
                    return false;
                }
                JsonConvert.DeserializeObject<APIPermissions>(Response);
                return Success == 1 ? true : false;
            }
        }

        #endregion

        #region User Rols API

        public class APIRoles
        {
            [JsonProperty("success")]
            public static int? Success { get; set; }

            [JsonProperty("error_message")]
            public static string Message { get; set; }

            [JsonProperty("last_id")]
            public static string LastID { get; set; }

            [JsonProperty("userrols")]
            public static List<Rols> userrols { get; set; } = new List<Rols>();

            public class Rols
            {
                public string rolid { get; set; }
                public string rolname { get; set; }
                public string roldescription { get; set; }
                public string usercount { get; set; }

                public List<UserPermissions> RolData { get; set; } = new List<UserPermissions>();
            }

            /// <summary>
            /// Get Api status values
            /// </summary>
            public static async Task<bool> GetValues(string function, string EndPoint = null, string data = null)
            {
                var Keys = new Dictionary<string, string>
                {
                    { "username", DataConfig.Username },
                    { "token", DataConfig.TokenHash },
                    { "function", function },
                    { "data", data }
                };

                string Response;

                try
                {
                    Response = await JsonResponseAsync(EndPoint + ApiUserRolesEndPoint, Keys);
                }
                catch (Exception ex)
                {
                    return false;
                }
                JsonConvert.DeserializeObject<APIRoles>(Response);
                return Success == 1 ? true : false;
            }
        }

        #endregion

        #region Branch API

        public class APIBranch
        {
            [JsonProperty("success")]
            public static int? Success { get; set; }

            [JsonProperty("error_message")]
            public static string Message { get; set; }

            [JsonProperty("last_id")]
            public static string LastID { get; set; }

            [JsonProperty("branch")]
            public static List<BranchClass> branch { get; set; } = new List<BranchClass>();

            public class BranchClass
            {
                public string id { get; set; }
                public string name { get; set; }
                public string address { get; set; }
                public string phone { get; set; }
                public string count { get; set; }
                public string boxes { get; set; }
                public string enabled { get; set; }

            }

            /// <summary>
            /// Get Api status values
            /// </summary>
            public static async Task<bool> GetValues(string function, string EndPoint = null, string data = null)
            {
                var Keys = new Dictionary<string, string>
                {
                    { "username", DataConfig.Username },
                    { "token", DataConfig.TokenHash },
                    { "function", function },
                    { "data", data }
                };

                string Response;

                try
                {
                    Response = await JsonResponseAsync(EndPoint + ApiBranchEndPoint, Keys);
                    //MessageBox.Show(Response);
                }
                catch (Exception ex)
                {
                    return false;
                }
                JsonConvert.DeserializeObject<APIBranch>(Response);
                return Success == 1 ? true : false;
            }
        }

        public class APIBoxes
        {
            [JsonProperty("success")]
            public static int? Success { get; set; }

            [JsonProperty("error_message")]
            public static string Message { get; set; }

            [JsonProperty("last_id")]
            public static string LastID { get; set; }

            [JsonProperty("boxes")]
            public static List<BoxesClass> boxes { get; set; } = new List<BoxesClass>();

            public class BoxesClass
            {
                public string id { get; set; }
                public string name { get; set; }
                public string branch { get; set; }
                public string status { get; set; }

            }

            public class BoxesData
            {
                public string id { get; set; }
                public string name { get; set; }
                public string branch_id { get; set; }
                public string status { get; set; }
            }

            /// <summary>
            /// Get Api status values
            /// </summary>
            public static async Task<bool> GetValues(string function, string EndPoint = null, string data = null)
            {
                var Keys = new Dictionary<string, string>
                {
                    { "username", DataConfig.Username },
                    { "token", DataConfig.TokenHash },
                    { "function", function },
                    { "data", data }
                };

                string Response;

                try
                {
                    Response = await JsonResponseAsync(EndPoint + ApiBranchEndPoint, Keys);
                }
                catch (Exception ex)
                {
                    return false;
                }
                JsonConvert.DeserializeObject<APIBoxes>(Response);
                return Success == 1 ? true : false;
            }
        }

        #endregion

        #region Users API

        public class APIUsers
        {
            [JsonProperty("success")]
            public static int? Success { get; set; }

            [JsonProperty("error_message")]
            public static string Message { get; set; }

            [JsonProperty("last_id")]
            public static string LastID { get; set; }

            [JsonProperty("users")]
            public static List<UsersClass> users { get; set; } = new List<UsersClass>();

            public class UsersClass
            {
                public string id { get; set; }
                public string name { get; set; }
                public string rolid { get; set; }
                public string rolname { get; set; }
                public string roldescrip { get; set; }
                public string realname { get; set; }
                public string photo { get; set; }
                public string branchid { get; set; }
                public string branchname { get; set; }
                public string status { get; set; }
                //Password
                public string hash { get; set; }
            }

            /// <summary>
            /// Get Api status values
            /// </summary>
            public static async Task<bool> GetValues(string function, string EndPoint = null, string data = null)
            {
                var Keys = new Dictionary<string, string>
                {
                    { "username", DataConfig.Username },
                    { "token", DataConfig.TokenHash },
                    { "function", function },
                    { "data", data }
                };

                string Response;

                try
                {
                    Response = await JsonResponseAsync(EndPoint + ApiUsersEndPoint, Keys);
                }
                catch (Exception ex)
                {
                    return false;
                }
                JsonConvert.DeserializeObject<APIUsers>(Response);
                return Success == 1 ? true : false;
            }
        }

        #endregion

        #region Supplier API

        public class APISupplier
        {
            [JsonProperty("success")]
            public static int? Success { get; set; }

            [JsonProperty("error_message")]
            public static string Message { get; set; }

            [JsonProperty("last_id")]
            public static string LastID { get; set; }

            [JsonProperty("suppliers")]
            public static List<SupplierClass> suppliers { get; set; } = new List<SupplierClass>();

            public class SupplierClass
            {
                public string id { get; set; }
                public string socialname { get; set; }
                public string comercialname { get; set; }
                public string documentid { get; set; }
                public string idtype { get; set; }
                public string phone { get; set; }
                public string celphone { get; set; }
                public string mail { get; set; }
                public string address { get; set; }
                public string contact { get; set; }
                public string observation { get; set; }
                public string status { get; set; }

                public string phones { get { return $"{phone} / {celphone}"; } }
                public string idcomplete { get {

                        string name;
                        switch (idtype)
                        {
                            case "0":
                                name = "NIT";
                                break;
                            case "1":
                                name = "CC";
                                break;
                            case "2":
                                name = "CE";
                                break;
                            case "3":
                                name = "PA";
                                break;
                            case "4":
                                name = "RC";
                                break;
                            case "5":
                                name = "TI";
                                break;
                            default:
                                name = "N/A";
                                break;
                        }
                        return $"{name}-{documentid}";
                    }
                }
            }

            /// <summary>
            /// Get Api suppliers values
            /// </summary>
            public static async Task<bool> GetValues(string function, string EndPoint = null, string data = null)
            {
                var Keys = new Dictionary<string, string>
                {
                    { "username", DataConfig.Username },
                    { "token", DataConfig.TokenHash },
                    { "function", function },
                    { "data", data }
                };

                string Response;

                try
                {
                    Response = await JsonResponseAsync(EndPoint + ApiSuppliersEndPoint, Keys);
                }
                catch (Exception ex)
                {
                    return false;
                }
                JsonConvert.DeserializeObject<APISupplier>(Response);
                return Success == 1 ? true : false;
            }
        }

        #endregion

        #region Category API

        public class APICategory
        {
            [JsonProperty("success")]
            public static int? Success { get; set; }

            [JsonProperty("error_message")]
            public static string Message { get; set; }

            [JsonProperty("last_id")]
            public static string LastID { get; set; }

            [JsonProperty("category")]
            public static List<CategoryClass> category { get; set; } = new List<CategoryClass>();

            public class CategoryClass
            {
                public string id { get; set; }
                public string name { get; set; }
                public string code { get; set; }
                public string products { get; set; }
            }

            /// <summary>
            /// Get Api category values
            /// </summary>
            public static async Task<bool> GetValues(string function, string EndPoint = null, string data = null)
            {
                var Keys = new Dictionary<string, string>
                {
                    { "username", DataConfig.Username },
                    { "token", DataConfig.TokenHash },
                    { "function", function },
                    { "data", data }
                };

                string Response;

                try
                {
                    Response = await JsonResponseAsync(EndPoint + ApiCategoriesEndPoint, Keys);
                }
                catch (Exception ex)
                {
                    return false;
                }
                JsonConvert.DeserializeObject<APICategory>(Response);
                return Success == 1 ? true : false;
            }
        }

        #endregion

        #region Prices API

        public class APIPrice
        {
            [JsonProperty("success")]
            public static int? Success { get; set; }

            [JsonProperty("error_message")]
            public static string Message { get; set; }

            [JsonProperty("last_id")]
            public static string LastID { get; set; }

            [JsonProperty("price")]
            public static List<PriceClass> price { get; set; } = new List<PriceClass>();

            public class PriceClass
            {
                public string id { get; set; }
                public string name { get; set; }
                public string type { get; set; }
                public string value { get; set; }
                public string valuestring { get { return value + "%"; } }
                public string type_name { get
                    {
                        switch (type)
                        {
                            case "0":
                                return "Precio adjunto";
                            case "1":
                                return "Porcentaje general";
                            default:
                                return "";
                        }
                    } }
            }

            /// <summary>
            /// Get Api category values
            /// </summary>
            public static async Task<bool> GetValues(string function, string EndPoint = null, string data = null)
            {
                var Keys = new Dictionary<string, string>
                {
                    { "username", DataConfig.Username },
                    { "token", DataConfig.TokenHash },
                    { "function", function },
                    { "data", data }
                };

                string Response;

                try
                {
                    Response = await JsonResponseAsync(EndPoint + ApiPricesEndPoint, Keys);
                }
                catch (Exception ex)
                {
                    return false;
                }
                JsonConvert.DeserializeObject<APIPrice>(Response);
                return Success == 1 ? true : false;
            }
        }

        #endregion

        #region ProductPrice
        public class APIProdructPrice
        {
            [JsonProperty("success")]
            public static int? Success { get; set; }

            [JsonProperty("error_message")]
            public static string Message { get; set; }

            [JsonProperty("prices")]
            public static List<APIPrice.PriceClass> price { get; set; } = new List<APIPrice.PriceClass>();

            public class PriceModify
            {
                public string price_id { get; set; }
                public string price_value { get; set; }
                public string product_id { get; set; }

            }
            /// <summary>
            /// Get Api prices values
            /// </summary>
            public static async Task<bool> GetValues(string function, string EndPoint = null, string data = null)
            {
                var Keys = new Dictionary<string, string>
                {
                    { "username", DataConfig.Username },
                    { "token", DataConfig.TokenHash },
                    { "function", function },
                    { "data", data }
                };

                string Response;

                try
                {
                    Response = await JsonResponseAsync(EndPoint + ApiProductsEndPoint, Keys);
                }
                catch (Exception ex)
                {
                    return false;
                }
                JsonConvert.DeserializeObject<APIProdructPrice>(Response);
                return Success == 1 ? true : false;
            }
        }
        #endregion

        #region Products API

        public class APIProdructs
        {
            [JsonProperty("success")]
            public static int? Success { get; set; }

            [JsonProperty("error_message")]
            public static string Message { get; set; }

            [JsonProperty("last_id")]
            public static string LastID { get; set; }

            [JsonProperty("count")]
            public static string Count { get; set; }

            [JsonProperty("product")]
            public static List<ProductClass> products { get; set; } = new List<ProductClass>();

            public class ProductClass
            {
                public string id { get; set; }
                public string name { get; set; }
                public string category { get; set; }
                public string category_name { get {
                        return category != null ? APICategory.category.Find(x => x.id == category).name : "";
                    } }
                public string code { get; set; }
                public string costprice { get; set; }
                public string sellprice { get; set; }
                public string minstock { get; set; }
                public string maxstock { get; set; }
                public string description { get; set; }
                public string unity_type { get; set; }

                public string branch_count { get; set; }

                public string iva { get; set; }
                public string iva5 { get; set; }
                public string iac { get; set; }

                public string hproduct { get; set; }

                //Price operations
                public int pCost { get { return Convert.ToInt32(costprice); } }
                public int pSell { get { return Convert.ToInt32(sellprice); } }
                public int pGet { get { return pSell - pCost; } }
                public string utility { get {
                        decimal value = 0;
                        if (pCost != 0 && pSell != 0)
                        {
                            value = ((pSell - pCost) / pCost) * 100;
                        }
                        return $"{value}%";
                    } }

            }

            public class DataClass
            {
                public string h { get; set; }
                public string from { get; set; }
                public string filter { get; set; }
                public string branch { get
                    {
                        return DataConfig.WorkPlaceID.ToString();
                    }
                }
            }

            /// <summary>
            /// Get Api category values
            /// </summary>
            public static async Task<bool> GetValues(string function, string EndPoint = null, string data = null)
            {
                var Keys = new Dictionary<string, string>
                {
                    { "username", DataConfig.Username },
                    { "token", DataConfig.TokenHash },
                    { "function", function },
                    { "data", data }
                };

                string Response;

                try
                {
                    Response = await JsonResponseAsync(EndPoint + ApiProductsEndPoint, Keys);
                    //MessageBox.Show(Response);
                }
                catch (Exception ex)
                {
                    return false;
                }
                JsonConvert.DeserializeObject<APIProdructs>(Response);
                return Success == 1 ? true : false;
            }
        }

        #endregion

        #region Inventory API

        public class APIInventory
        {
            [JsonProperty("success")]
            public static int? Success { get; set; }

            [JsonProperty("error_message")]
            public static string Message { get; set; }

            [JsonProperty("count")]
            public static string Count { get; set; }

            [JsonProperty("product")]
            public static List<ProductClass> products { get; set; } = new List<ProductClass>();

            public class ProductClass
            {
                public string id { get; set; }
                public string name { get; set; }
                public string code { get; set; }
                public string costprice { get; set; }
                public string sellprice { get; set; }
                public string minstock { get; set; }
                public string maxstock { get; set; }
                public string description { get; set; }
                public string unity_type { get; set; }
                public List<BranchClass> branch_data { get; set; } = new List<BranchClass>();

            }

            public class BranchClass
            {
                public string id { get; set; }
                public string count { get; set; }
                public string name { get; set; }
                public string product_id { get; set; }
                public string comment { get; set; }
            }

            public class ProductTransfer
            {
                public string branch_id { get; set; } //local branch id
                public string product_id { get; set; }
                public string product_count { get; set; }
                public string from_branch { get; set; }
                public string to_branch { get; set; }
                public string comment { get; set; }

            }

            public class DataClass
            {
                public string h { get; set; }
                public string from { get; set; }
                public string filter { get; set; }
            }

            public class ProductInClass
            {
                public string product_db_id { get; set; }
                public string product_id { get; set; }
                public string product_code { get; set; }
                public string product_name { get; set; }
                public string product_count { get; set; }
                public decimal product_cost { get; set; }
                public string product_iva { get; set; }
                public string product_iva5 { get; set; }
                public string product_iac { get; set; }
                public string product_taxes { get {
                        return product_iva == "1" ? "19" : product_iva5 == "1" ? "5" : product_iac == "1" ? "8" : "0";

                    } }

                public int product_subtotal { get
                    {
                        return ((Convert.ToInt32(product_count) * Convert.ToInt32(product_cost)));
                    }
                }
                public string product_branch { get; set; }

            }
            
            public class InventoryInData
            {
                public string branch { get; set; }
                public string comment { get; set; }
                public string supplier_id { get; set; }
                public string bill { get; set; }
                public string value { get; set; }
                public string payment { get; set; }
                public string payment_type { get; set; }
                public string payment_method { get; set; }
                public string expiration_date { get; set; }
                public List<ProductInClass> products { get; set; } = new List<ProductInClass>();
            }

            public class LowProductsData
            {
                [JsonProperty("success")]
                public static int? Success { get; set; }

                [JsonProperty("error_message")]
                public static string Message { get; set; }

                [JsonProperty("count")]
                public static string Count { get; set; }

                [JsonProperty("product")]
                public static List<LowProductInfo> products { get; set; } = new List<LowProductInfo>();
            }

            public class LowProductInfo
            {
                public string id { get; set; }
                public string name { get; set; }
                public string code { get; set; }
                public string category { get; set; }
                public string count { get; set; }
                public string product_minstock { get; set; }
            }

            public class LowProductRequestData
            {
                public string branch_id { get; set; }
                public string low_point { get; set; }
                public string from { get; set; }

            }

            /// <summary>
            /// Get Api category values
            /// </summary>
            public static async Task<bool> GetValues(string function, string EndPoint = null, string data = null, bool lowproducts = false)
            {
                var Keys = new Dictionary<string, string>
                {
                    { "username", DataConfig.Username },
                    { "token", DataConfig.TokenHash },
                    { "function", function },
                    { "data", data }
                };

                string Response;
                try
                {
                    Response = await JsonResponseAsync(EndPoint + ApiInventoryEndPoint, Keys);
                    //MessageBox.Show(Response);
                }
                catch (Exception ex)
                {
                    return false;
                }
                if (!lowproducts)
                {
                    JsonConvert.DeserializeObject<APIInventory>(Response);
                    return Success == 1 ? true : false;
                }
                else
                {
                    JsonConvert.DeserializeObject<LowProductsData>(Response);
                    return LowProductsData.Success == 1 ? true : false;
                }
                
            }
        }

        #endregion

        #region Inventory Movements API

        public class APIInventoryMovements
        {
            [JsonProperty("success")]
            public static int? Success { get; set; }

            [JsonProperty("error_message")]
            public static string Message { get; set; }

            [JsonProperty("count")]
            public static string Count { get; set; }

            [JsonProperty("movements")]
            public static List<MovementClass> Movements { get; set; } = new List<MovementClass>();

            public class MovementClass
            {
                public string id { get; set; }
                public string branch { get; set; }
                public string type { get; set; }
                public string user { get; set; }
                public string date { get; set; }
                public string comment { get; set; }
                public string detail_id { get; set; }

            }      
            
            public class RequestData
            {
                public string from { get; set; }
                public string branch { get; set; }
                public string type { get; set; }
                public string date_from { get; set; }
                public string date_to { get; set; }
            }

            /// <summary>
            /// Get Api category values
            /// </summary>
            public static async Task<bool> GetValues(string function, string EndPoint = null, string data = null)
            {
                var Keys = new Dictionary<string, string>
                {
                    { "username", DataConfig.Username },
                    { "token", DataConfig.TokenHash },
                    { "function", function },
                    { "data", data }
                };

                string Response;
                try
                {
                    Response = await JsonResponseAsync(EndPoint + ApiInventoryEndPoint, Keys);                    
                }
                catch (Exception ex)
                {
                    return false;
                }
                JsonConvert.DeserializeObject<APIInventoryMovements>(Response);
                return Success == 1 ? true : false;
            }
        }

        #endregion

        #region BOX Movements API

        public class APIBoxMovements
        {
            [JsonProperty("success")]
            public static int? Success { get; set; }

            [JsonProperty("error_message")]
            public static string Message { get; set; }

            [JsonProperty("box_status")]
            public static string status { get; set; }

            [JsonProperty("movement_id")]
            public static string movement_id { get; set; }

            [JsonProperty("box_data")]
            public static List<BoxData> box_data { get; set; } = new List<BoxData>();


            //BOX DETAILS
            [JsonProperty("comments")]
            public static string comments { get; set; }

            [JsonProperty("opendate")]
            public static string opendate { get; set; }

            [JsonProperty("openuser")]
            public static string openuser { get; set; }

            [JsonProperty("closedate")]
            public static string closedate { get; set; }

            [JsonProperty("closeuser")]
            public static string closeuser { get; set; }

            [JsonProperty("openvalue")]
            public static string openvalue { get; set; }

            [JsonProperty("closevalue")]
            public static string closevalue { get; set; }

            [JsonProperty("totalsell")]
            public static string totalsell { get; set; }

            [JsonProperty("cash")]
            public static string cash { get; set; }

            [JsonProperty("others")]
            public static string others { get; set; }

            //

            public class BoxData
            {
                public string id { get; set; }
                public string opendate { get; set; }
                public string username { get; set; }
                public string openvalue { get; set; }
                public string closevalue { get; set; }
                public string comment { get; set; }
            }


            /// <summary>
            /// Get Api category values
            /// </summary>
            public static async Task<bool> GetValues(string function, string EndPoint = null, string data = null)
            {
                var Keys = new Dictionary<string, string>
                {
                    { "username", DataConfig.Username },
                    { "token", DataConfig.TokenHash },
                    { "function", function },
                    { "data", data }
                };

                string Response;
                try
                {
                    Response = await JsonResponseAsync(EndPoint + ApiBoxMovementsEndPoint, Keys);
                    //MessageBox.Show(Response);
                }
                catch (Exception ex)
                {
                    return false;
                }
                JsonConvert.DeserializeObject<APIBoxMovements>(Response);
                return Success == 1 ? true : false;
            }
        }

        #endregion

        #region Clients API

        public class APIClient
        {
            [JsonProperty("success")]
            public static int? Success { get; set; }

            [JsonProperty("error_message")]
            public static string Message { get; set; }

            [JsonProperty("last_id")]
            public static string LastID { get; set; }

            [JsonProperty("clients")]
            public static List<ClientClass> clients { get; set; } = new List<ClientClass>();

            public class ClientClass
            {
                public string id { get; set; }
                public string name { get; set; }
                public string type { get; set; }
                public string documentid { get; set; }
                public string address { get; set; }
                public string phone { get; set; }
                public string celphone { get; set; }
                public string mail { get; set; }
                public string cancredit { get; set; }

                public string phones { get { return $"{phone} / {celphone}"; } }
                public string idcomplete
                {
                    get
                    {
                        string name;
                        switch (type)
                        {
                            case "0":
                                name = "NIT";
                                break;
                            case "1":
                                name = "CC";
                                break;
                            case "2":
                                name = "CE";
                                break;
                            case "3":
                                name = "PA";
                                break;
                            case "4":
                                name = "RC";
                                break;
                            case "5":
                                name = "TI";
                                break;
                            default:
                                name = "N/A";
                                break;
                        }
                        return $"{name}-{documentid}";
                    }
                }
            }

            /// <summary>
            /// Get Api suppliers values
            /// </summary>
            public static async Task<bool> GetValues(string function, string EndPoint = null, string data = null)
            {
                var Keys = new Dictionary<string, string>
                {
                    { "username", DataConfig.Username },
                    { "token", DataConfig.TokenHash },
                    { "function", function },
                    { "data", data }
                };

                string Response;

                try
                {
                    Response = await JsonResponseAsync(EndPoint + ApiClientsEndPoint, Keys);
                    //MessageBox.Show(Response);
                }
                catch (Exception ex)
                {
                    return false;
                }
                JsonConvert.DeserializeObject<APIClient>(Response);
                return Success == 1 ? true : false;
            }
        }

        #endregion

        #region Tickets API

        public class APITickets
        {
            [JsonProperty("success")]
            public static int? Success { get; set; }

            [JsonProperty("error_message")]
            public static string Message { get; set; }

            [JsonProperty("count")]
            public static string Count { get; set; }

            [JsonProperty("ticketid")]
            public static string TicketID { get; set; }

            [JsonProperty("tickets")]
            public static List<TicketData> tickets { get; set; } = new List<TicketData>();


            public class TicketData
            {
                public string id { get; set; }
                public string date { get; set; }
                //Location data
                public string branch_id { get; set; }
                public string box_id { get; set; }
                public string box_movement_id { get; set; }
                //Ticket data
                public string client_id { get; set; }
                public string payment_type { get; set; }
                public string payment_method { get; set; }
                public string expiration_date { get; set; }
                public string ticket_total { get; set; }
                public int ticket_total_int
                {
                    get
                    {
                        try
                        {
                            return Convert.ToInt32(ticket_total);
                        }
                        catch (Exception)
                        {
                            return 0;
                        } } }

                public string ticket_iva { get; set; }
                public string ticket_iac { get; set; }
                public string ticket_iva5 { get; set; }
                public string ticket_totalpayment { get; set; }
                public string ticket_changepayment { get; set; }
                public string ticket_leftpayment { get; set; }
                public string ticket_comment { get; set; }
                public string ticket_subtotal { get; set; }
                public string ticket_status { get; set; }
                public string user_realname { get; set; }
                public bool status { get { return ticket_status == "1" ? true : false; } }
                public Visibility due { get {

                        int leftpayment = Convert.ToInt32(ticket_leftpayment);
                        return leftpayment > 0 ? Visibility.Visible : Visibility.Collapsed;


                            } }

                //String print data
                public string ticketid { get { return TicketID; } }
                public string client_name { get; set; }
                public string client_documentid { get; set; }
                public string client_address { get; set; }
                public string client_phones { get; set; }                

                //specials
                public string ticket_h { get; set; }
                public string ticket_copy { get; set; }
                public string ticket_coti { get; set; } = "0";

                //Product data
                public List<ProductClass> products { get; set; } = new List<ProductClass>();

                //items data
                public string items_count { get; set; }
                public List<ItemClass> items { get; set; } = new List<ItemClass>();

                //Items tooltip data
                public string ToolTipItems { get
                    {
                        try
                        {
                            string Itemsname = "";
                            for (int i = 0; i < items.Count; i++)
                            {
                                Itemsname += $"({items[i].item_count})\t{items[i].product_name}{Environment.NewLine}";
                            }

                            return Itemsname;
                        }
                        catch (Exception)
                        {

                            return "";
                        }
                       
                    } }
            }

            public class ProductClass
            {
                public string product_id { get; set; }
                public string product_count { get; set; }
                public string product_code { get; set; }
                public string product_name { get; set; }
                public string product_tax { get; set; }
                public string product_pricevalue { get; set; }
                public string product_discountvalue { get; set; }
                public string product_discountpercent { get; set; }
                public string product_total { get; set; }
                public string product_h { get; set; }
                public string product_unity { get; set; }

            }

            public class ItemClass
            {
                public string item_id { get; set; }
                public string item_ticket { get; set; }
                public string item_product { get; set; }
                public string product_name { get; set; }
                public string product_unity { get; set; }
                public string item_count { get; set; }
                public string product_code { get; set; }
                public string item_tax { get; set; }
                public string item_pricevalue { get; set; }
                public string item_discountpercent { get; set; }
                public string item_discountvalue { get; set; }
                public int item_total { get {

                        return Convert.ToInt32(item_count) * Convert.ToInt32(item_pricevalue);
                    } }
            }

            public class DataClass
            {
                public string h { get; set; }
                public string from { get; set; }
                public string filter { get; set; }
                public string client { get; set; }
                public string ticket_box { get; set; }
                public string ticket_branch { get; set; }
                public string ticket_client { get; set; }
                public string from_date { get; set; }
                public string to_date { get; set; }
            }

            public class CancelTicketClass {

                public string branch_id { get; set; }
                public string ticket_id { get; set; }
                public string status { get; set; }
                public string ticket_h { get; set; }
            }
                                   

            /// <summary>
            /// Get Api category values
            /// </summary>
            public static async Task<bool> GetValues(string function, string EndPoint = null, string data = null)
            {
                var Keys = new Dictionary<string, string>
                {
                    { "username", DataConfig.Username },
                    { "token", DataConfig.TokenHash },
                    { "function", function },
                    { "data", data }
                };

                string Response;
                try
                {
                    Response = await JsonResponseAsync(EndPoint + ApiTicketsEndPoint, Keys);
                    //MessageBox.Show(Response);
                }
                catch (Exception ex)
                {
                    return false;
                }
                JsonConvert.DeserializeObject<APITickets>(Response);
                return Success == 1 ? true : false;

            }
        }

        #endregion

        #region Reports API

        public class APIReports
        {
            [JsonProperty("success")]
            public static int? Success { get; set; }

            [JsonProperty("error_message")]
            public static string Message { get; set; }

            [JsonProperty("count")]
            public static string Count { get; set; }

            [JsonProperty("total")]
            public static int Total { get; set; }

            [JsonProperty("credits")]
            public static int Credits { get; set; }

            [JsonProperty("box_items")]
            public static List<BoxitemsClass> Boxitems { get; set; } = new List<BoxitemsClass>();

            [JsonProperty("sell_items")]
            public static List<SellItemsClass> Sellitems { get; set; } = new List<SellItemsClass>();

            public class BoxitemsClass
            {
                public string id { get; set; }
                public string name { get; set; }
                public string opendate { get; set; }
                public string openuser { get; set; }
                public string closedate { get; set; }
                public string closeuser { get; set; }
                public string openvalue { get; set; }
                public string closevalue { get; set; }
                public string comment { get; set; }

                //INT VALUES
                public int openvalueint { get {

                        try
                        {
                            return Convert.ToInt32(openvalue);
                        }
                        catch (Exception)
                        {
                            return 0;
                        }
                    }
                }

                public int closevalueint
                {
                    get
                    {

                        try
                        {
                            return Convert.ToInt32(closevalue);
                        }
                        catch (Exception)
                        {
                            return 0;
                        }
                    }
                }

            }

            public class SellItemsClass
            {
                public string date { get; set; }
                public string ticket_id { get; set; }
                public string product_name { get; set; }
                public int product_count { get; set; }
                public string product_type { get; set; }
                public string unity_string { get { return GeneralFunctions.GetUnityTypeString(product_type); } }
                public int product_price { get; set; }
                public double product_total { get
                    {
                        return product_count * product_price;
                    } }
                public string payment { get; set; }

            }

            public class RequestData
            {
                public string branch_id { get; set; }
                public string box_id { get; set; }
                public string date_from { get; set; }
                public string date_to { get; set; }
                public string from { get; set; }
            }

            /// <summary>
            /// Get Api suppliers values
            /// </summary>
            public static async Task<bool> GetValues(string function, string EndPoint = null, string data = null)
            {
                var Keys = new Dictionary<string, string>
                {
                    { "username", DataConfig.Username },
                    { "token", DataConfig.TokenHash },
                    { "function", function },
                    { "data", data }
                };

                string Response;

                try
                {
                    Response = await JsonResponseAsync(EndPoint + ApiSellReportsEndPoint, Keys);
                    //MessageBox.Show(Response);
                }
                catch (Exception ex)
                {
                    return false;
                }
                JsonConvert.DeserializeObject<APIReports>(Response);
                return Success == 1 ? true : false;
            }
        }

        #endregion

        #region API Constructors

        public static async Task<string> JsonResponseAsync(string Endpoint, Dictionary<string, string> keys = null)
        {
            //POST Content
            var content = keys != null ? new FormUrlEncodedContent(keys) : null;

            //POST Request    
            var response = await client.PostAsync(Endpoint, content);

            //POST Response
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        #endregion

    }
}
