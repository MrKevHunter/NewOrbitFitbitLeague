using System.Linq;
using neworbitfitbitleague.Controllers;
using Newtonsoft.Json;
using RestSharp;

namespace neworbitfitbitleague.Models
{
    public class MovesService
    {
        public void SaveUser(string email, MovesAccessToken movesAccessToken)
        {
            User user = new TableStorageService().GetAllEmployees().ToList().Single(x => x.EmailAddress == email);
            string json = JsonConvert.SerializeObject(movesAccessToken);
            user.MovesAccessToken = json;
            new TableStorageService().SaveUser(user);
        }

        public int GetStepsToday(MovesAccessToken settings)
        {
            var client = new RestClient("https://api.moves-app.com/api/1.1");
            client.AddDefaultHeader("Authorization", string.Format("Bearer {0}", settings.access_token));
            IRestResponse restResponse = client.Get(new RestRequest("/user/summary/daily?pastDays=7"));
            var deserializeObject = JsonConvert.DeserializeObject<MovesClass1[]>(restResponse.Content);
            return deserializeObject.Last().summary.Last().steps;
        }

        public int GetStepsWeek(MovesAccessToken settings)
        {
            var client = new RestClient("https://api.moves-app.com/api/1.1");
            client.AddDefaultHeader("Authorization", string.Format("Bearer {0}", settings.access_token));
            IRestResponse restResponse = client.Get(new RestRequest("/user/summary/daily?pastDays=7"));
            var deserializeObject = JsonConvert.DeserializeObject<MovesClass1[]>(restResponse.Content);
            return deserializeObject.SelectMany(x => x.summary).Select(x => x.steps).Sum();
        }
    }


    public class MovesClass1
    {
        public string date { get; set; }
        public MovesSummary[] summary { get; set; }
        public string lastUpdate { get; set; }
    }

    public class MovesSummary
    {
        public string activity { get; set; }
        public string group { get; set; }
        public float duration { get; set; }
        public float distance { get; set; }
        public int steps { get; set; }
    }
}