using System.Configuration;

namespace neworbitfitbitleague
{
    public class Config
    {
        public static string moveClientId = ConfigurationManager.AppSettings["moveClientId"];
        public static string moveSecret = ConfigurationManager.AppSettings["moveSecret"];
        public static string moveRedirect = "http://neworbitfitbitleague.azurewebsites.net/home/MovesCallback";
    }
}