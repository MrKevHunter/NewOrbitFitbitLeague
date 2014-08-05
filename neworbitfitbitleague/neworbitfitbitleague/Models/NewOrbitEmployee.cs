using Microsoft.WindowsAzure.Storage.Table;
using neworbitfitbitleague.Controllers;

namespace neworbitfitbitleague.Models
{
    public class FitbitOAuthSettings
    {
        public string UserId { get; set; }
        public string AuthToken { get; set; }
        public string AuthSecret { get; set; }
    }

    public class User : TableEntity
    {
        public string Organisation { get; set; }
        public string Name { get; set; }
        public string FitbitOAuthSettings { get; set; }
        public string EmailAddress { get; set; }
        public StepCounterApplication StepperMeasurer { get; set; }
        public string MovesAccessToken { get; set; }
    }
}