using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Nova.Cache
{

    public enum ImageType
    {
        UserProfile
    }

    class Cache
    {

        public static string SelectedMovement = "0";
        public static string SelectedSellReport = "0";

        public static int CacheTime = 5;

        public static DateTime ClientsDateTime { get; set; }
        public static DateTime PricesDateTime { get; set; }

        /// <summary>
        /// Returns image if exists or download from host
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static async Task<ImageSource> CachedImage(string image, ImageType type)
        {
            string file = Path.GetFileName(image);
            string path = type == ImageType.UserProfile ? @"\Cache\Alerts\" : @"\Cache\News\";
            //Check file already downloaded
            if (File.Exists($@"{DataConfig.LocalPath}{path}{file}"))
            {
                return new BitmapImage(new Uri($@"{DataConfig.LocalPath}{path}{file}"));
            }
            else
            {
                //Download announcement image
                try
                {
                    return await DownloadImage(image, type, file);
                }
                catch (Exception)
                {
                    //Return base image on exception
                    return new BitmapImage(new Uri("pack://application:,,,/Assets/Logos/NaerLogo.png"));
                }

            }
        }

        //Download image method
        static async Task<ImageSource> DownloadImage(string url, ImageType type, string filename)
        {
            string path = type == ImageType.UserProfile ? @"\Cache\Alerts\" : @"\Cache\News\";

            using (WebClient client = new WebClient())
            {
                await client.DownloadFileTaskAsync(new Uri(url), $@"{DataConfig.LocalPath}{path}{filename}");
                return new BitmapImage(new Uri($@"{DataConfig.LocalPath}{path}{filename}"));
            }
        }



    }
}
