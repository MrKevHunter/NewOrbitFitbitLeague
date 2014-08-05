using Microsoft.WindowsAzure.Storage.Table;

namespace neworbitfitbitleague.Models
{
    public class FitbitToken : TableEntity
    {
        public string EmailAddress { get; set; }
    }
}