using Microsoft.WindowsAzure.Storage.Table;

namespace neworbitfitbitleague.Controllers
{
    public class NewOrbitEmployee : TableEntity
    {
        public string UserId { get; set; }
        public string AuthToken { get; set; }
        public string AuthSecret { get; set; }
    }
}