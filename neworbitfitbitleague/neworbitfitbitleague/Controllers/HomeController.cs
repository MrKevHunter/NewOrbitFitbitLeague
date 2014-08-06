using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Fitbit.Api;
using Fitbit.Models;
using neworbitfitbitleague.Models;
using Newtonsoft.Json;
using RestSharp;
using User = neworbitfitbitleague.Models.User;

namespace neworbitfitbitleague.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            List<StepsModel> data = BuildViewModel();

            return View(data);
        }

        private List<StepsModel> BuildViewModel()
        {
            IQueryable<User> query = new TableStorageService().GetAllEmployees();
            ViewBag.TotalEmployees = query.ToList().Count();
            var data = new List<StepsModel>();
            foreach (User newOrbitEmployee in query)
            {
                if (!string.IsNullOrWhiteSpace(newOrbitEmployee.FitbitOAuthSettings))
                {
                    try
                    {
                        var settings =
                            JsonConvert.DeserializeObject<FitbitOAuthSettings>(
                                newOrbitEmployee.FitbitOAuthSettings);
                        FitbitClient fitbitClient = new FitbitService().GetFitbitClient(settings.AuthToken,
                            settings.AuthSecret);
                        int totalSteps = fitbitClient.GetDayActivity(DateTime.Now).Summary.Steps;
                        data.Add(new StepsModel
                                     {
                                         Name = newOrbitEmployee.Name,
                                         StepsToday = totalSteps,
                                         StepsWeek =
                                             fitbitClient.GetTimeSeries(TimeSeriesResourceType.Steps, DateTime.Now,
                                                 DateRangePeriod.OneWeek).DataList.Select(x => Convert.ToInt32(x.Value)).Sum()
                                     });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                if (!string.IsNullOrWhiteSpace(newOrbitEmployee.MovesAccessToken))
                {
                    var settings =
                        JsonConvert.DeserializeObject<MovesAccessToken>(
                            newOrbitEmployee.MovesAccessToken);

                    int stepsToday = new MovesService().GetStepsToday(settings);
                    int stepsWeek = new MovesService().GetStepsWeek(settings);
                    data.Add(new StepsModel
                    {
                        Name = newOrbitEmployee.Name,
                        StepsToday = stepsToday,
                        StepsWeek = stepsWeek
                    });
                }
            }
            return data.OrderByDescending(x => x.StepsWeek).ToList();
        }


        [HttpGet]
        public ActionResult AddMyDetails()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddMyDetails(AddDetailViewModel addDetailViewModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("AddMyDetails");
            }

            User employee =
                new TableStorageService().GetAllEmployees().ToList()
                    .FirstOrDefault(x => x.EmailAddress == addDetailViewModel.EmailAddress);

            if (employee != null)
            {
                return RedirectToAction("Index");
            }

            var newOrbitEmployee = new User
            {
                PartitionKey = "neworbit",
                RowKey = addDetailViewModel.EmailAddress,
                EmailAddress = addDetailViewModel.EmailAddress,
                StepperMeasurer = addDetailViewModel.StepperMeasurer,
                Name = addDetailViewModel.Name,
            };
            new TableStorageService().InsertUser(newOrbitEmployee);
            if (addDetailViewModel.StepperMeasurer == StepCounterApplication.Fitbit)
            {
                return RedirectToAction("LinkToFitbit", "Home", new {email = addDetailViewModel.EmailAddress});
            }
            if (addDetailViewModel.StepperMeasurer == StepCounterApplication.Moves)
            {
                return RedirectToAction("LinkToMoves", "Home", new {email = addDetailViewModel.EmailAddress});
            }
            return View();
        }

        public ActionResult LinkToFitbit(string email)
        {
            RequestToken token = new FitbitService().GetRequestToken();
            new FitbitService().StoreToken(token, email);
            string authUrl = new FitbitService().GenerateAuthUrlFromRequestToken(token, true);
            return Redirect(authUrl);
        }

        public ActionResult FitbitCallback()
        {
            var token = new RequestToken();
            token.Token = Request.Params["oauth_token"];
            FitbitToken fitbitToken = new FitbitService().GetToken(token.Token);
            token.Secret = fitbitToken.RowKey;
            token.Verifier = Request.Params["oauth_verifier"];

            AuthCredential credential = new FitbitService().ProcessApprovedAuthCallback(token);
            new FitbitService().UpdateEmployeeWithCredentials(credential, fitbitToken);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult LinkToMoves(string email)
        {
            return
                Redirect(
                    string.Format(
                        "https://api.moves-app.com/oauth/v1/authorize?response_type=code&client_id={0}&scope=activity&redirect_uri={1}",
                        Config.moveClientId, Config.moveRedirect + "?email=" + email));
        }

        public ActionResult MovesCallback(string code, string email)
        {
            string uri =
                string.Format(
                    "https://api.moves-app.com/oauth/v1/access_token?grant_type=authorization_code&code={0}&client_id={1}&client_secret={2}&redirect_uri={3}"
                    , code, Config.moveClientId, Config.moveSecret, Config.moveRedirect + "?email=" + email);
            var client = new RestClient();
            IRestResponse restResponse = client.Post(new RestRequest(uri));
            var movesAccessToken = JsonConvert.DeserializeObject<MovesAccessToken>(restResponse.Content);
            new MovesService().SaveUser(email, movesAccessToken);
            return RedirectToAction("Index");
        }
    }
}