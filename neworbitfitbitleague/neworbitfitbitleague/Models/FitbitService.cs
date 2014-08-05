using System.Configuration;
using System.Linq;
using Fitbit.Api;
using Fitbit.Models;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace neworbitfitbitleague.Models
{
    public class FitbitService
    {
        private Authenticator authenticator;
        private string consumerKey;
        private string consumerSecret;

        public FitbitService()
        {
            consumerKey = ConfigurationManager.AppSettings["fitbitConsumerKey"];
            consumerSecret = ConfigurationManager.AppSettings["fitbitConsumerSecret"];
            authenticator = new Authenticator(consumerKey,
                consumerSecret,
                "http://api.fitbit.com/oauth/request_token",
                "http://api.fitbit.com/oauth/access_token",
                "http://api.fitbit.com/oauth/authorize");
        }

        public void StoreToken(RequestToken token, string email)
        {
            CloudTableClient tableClient = new TableStorageService().GetTable();
            CloudTable table = tableClient.GetTableReference("FitbitTokens");
            table.CreateIfNotExists();
            table.Execute(
                TableOperation.Insert(new FitbitToken() { PartitionKey = token.Token, RowKey = token.Secret,EmailAddress = email}));
        }

        public FitbitToken GetToken(string token)
        {
            CloudTableClient tableClient = new TableStorageService().GetTable();
            CloudTable table = tableClient.GetTableReference("FitbitTokens");
            return (from t in table.CreateQuery<FitbitToken>()
                        where t.PartitionKey == token
                        select t).Single();

        }

        public RequestToken GetRequestToken()
        {
            return authenticator.GetRequestToken();
        }

        public string GenerateAuthUrlFromRequestToken(RequestToken token, bool forceLogoutBeforeAuth)
        {
            return authenticator.GenerateAuthUrlFromRequestToken(token, forceLogoutBeforeAuth);
        }

        public AuthCredential ProcessApprovedAuthCallback(RequestToken token)
        {
            return authenticator.ProcessApprovedAuthCallback(token);

        }

        public void UpdateEmployeeWithCredentials(AuthCredential credential, FitbitToken fitbitToken)
        {
            User user = new TableStorageService().GetAllEmployees().ToList().Single(x => x.EmailAddress == fitbitToken.EmailAddress);
            var fitbitOAuthSettings = new FitbitOAuthSettings(){AuthSecret = credential.AuthTokenSecret,AuthToken = credential.AuthToken,UserId = credential.UserId};
            string json = JsonConvert.SerializeObject(fitbitOAuthSettings);
            user.FitbitOAuthSettings = json;
            new TableStorageService().SaveUser(user);
        }

        public FitbitClient GetFitbitClient(string token, string secret)
        {
            var client = new FitbitClient(consumerKey,
                consumerSecret,
                token,
                secret);

            return client;
        }
    }
}