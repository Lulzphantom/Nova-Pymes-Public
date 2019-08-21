using System;
using System.IO;
using System.Windows;
using IniParser;
using IniParser.Model;

namespace Nova
{
    class DataConfig
    {

        #region Private Members
        //Config file
        private static string ConfigFile = "NovaConfig.ini";

        //Cloud platform url
        private static string CloudPlatform = "";
        public static string CloudUpdatesURL = "http://novapymes.com/";
        #endregion


        #region Public Members
        public static string LocalPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\NovaSoftware";

        //Configuration file content
        public static string LocalAPI = "";
        public static string TokenHash = "";
        public static string Username = "";
        public static string RealName = "";
        public static int WorkPlaceID = 0;
        public static int WorkPointID = 0;
        public static bool LocalUpdates = true;
        public static bool CloudUpdates = true;
        public static bool PrintBarCode = true;

        //General information
        public static string UserRole = "";


        //Space config
        public static string PrintHeader = $"CALDAS LICORES{Environment.NewLine}NIT: 1.112.458.807-0{Environment.NewLine}REGIMEN SIMPLIFICADO";
        public static string PrintWaterMark = $"Impreso por NovaPymes Software{Environment.NewLine}www.novapymes.com{Environment.NewLine}.";
        public static string PrintFooter = $"GRACIAS POR SU COMPRA";

        public static int wX;
        public static int wY;
        public static int wWidth;
        public static int wHeight;
        //
        public static WindowState wState;

        public static string WorkPlaceLabel = "";

        public static int Initialized = 0;

        //General config
        public static string ivaValue = "19";
        public static string iva5Value = "5";
        public static string iacValue = "8";
        #endregion


        #region Configuration functions

        /// <summary>
        /// Create .ini configuration file in localpath
        /// </summary>
        public static void CreateConfig()
        {
            //Create application directories
            try
            {
                Directory.CreateDirectory(LocalPath + @"\Cache\Images");
                Directory.CreateDirectory(LocalPath + @"\Cache\Exports");

            }
            catch (Exception ex) { MessageBox.Show($"Error al crear el fichero de configuración {Environment.NewLine} Info:{ex.Message}"); }

            //Create default config file
            try
            {
                File.Create(LocalPath + @"\" + ConfigFile).Dispose();
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile(LocalPath + @"\" + ConfigFile);
                data.Sections.AddSection("NovaPymes");
                data["NovaPymes"].AddKey("LocalAPI", "");
                data["NovaPymes"].AddKey("TokenHash", "");
                data["NovaPymes"].AddKey("Username", "");
                data["NovaPymes"].AddKey("WorkPlaceLabel", "");
                data["NovaPymes"].AddKey("WorkPlaceID", "0");
                data["NovaPymes"].AddKey("WorkPointID", "0");
                data["NovaPymes"].AddKey("LocalUpdates", "1");
                data["NovaPymes"].AddKey("CloudConnection", "1");
                parser.WriteFile(LocalPath + @"\" + ConfigFile, data);
            }
            catch (Exception ex) { MessageBox.Show($"Error al crear el archivo de configuración {Environment.NewLine} Info:{ex.Message}"); }
        }


        /// <summary>
        /// Load .ini configuration file
        /// </summary>
        public static bool LoadConfig()
        {
            if (File.Exists(LocalPath + @"\" + ConfigFile))
            {
                try
                {
                    var parser = new FileIniDataParser();
                    IniData data = parser.ReadFile(LocalPath + @"\" + ConfigFile);

                    LocalAPI = data["NovaPymes"]["LocalAPI"];
                    TokenHash = "";
                    Username = data["NovaPymes"]["Username"];
                    WorkPlaceLabel = data["NovaPymes"]["WorkPlaceLabel"];
                    WorkPlaceID = Convert.ToInt32(data["NovaPymes"]["WorkPlaceID"]);
                    WorkPointID = Convert.ToInt32(data["NovaPymes"]["WorkPointID"]);
                    LocalUpdates = data["NovaPymes"]["LocalUpdates"] == "1" ? true : false;
                    CloudUpdates = data["NovaPymes"]["CloudConnection"] == "1" ? true : false;

                    return true;
                }
                catch (Exception ex) { MessageBox.Show($"Error al cargar el archivo de configuración {Environment.NewLine}Info:{ex.Message}"); return false; }
            }
            else
            { CreateConfig(); return true; }
        }


        /// <summary>
        /// Save the actual configuration to the .ini file
        /// </summary>
        public static void SaveConfig()
        {
            try
            {
                var parser = new FileIniDataParser();
                IniData data = new IniData();
                data["NovaPymes"]["LocalAPI"] = LocalAPI;
                data["NovaPymes"]["TokenHash"] = TokenHash;
                data["NovaPymes"]["Username"] = Username;
                data["NovaPymes"]["WorkPlaceLabel"] = WorkPlaceLabel;
                data["NovaPymes"]["WorkPlaceID"] = WorkPlaceID.ToString();
                data["NovaPymes"]["WorkPointID"] = WorkPointID.ToString();
                data["NovaPymes"]["LocalUpdates"] = LocalUpdates == true ? "1" : "0";
                data["NovaPymes"]["CloudConnection"] = CloudUpdates == true ? "1" : "0";
                parser.WriteFile(LocalPath + @"\" + ConfigFile, data);

            }
            catch (Exception ex) { MessageBox.Show($"Error al guardar el archivo de configuración {Environment.NewLine} Info:{ex.Message}"); }
        }
        #endregion

    }
}
