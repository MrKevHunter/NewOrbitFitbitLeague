using Microsoft.WindowsAzure.Storage.Table;

namespace neworbitfitbitleague.Models
{
    public class NewOrbitEmployee : TableEntity
    {
        public string UserId { get; set; }
        public string AuthToken { get; set; }
        public string AuthSecret { get; set; }
    }
}