using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Fitbit.Api;
using Fitbit.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace neworbitfitbitleague.Controllers
{
    public class HomeController : Controller
    {
        private static readonly string connectionString =
            ConfigurationManager.AppSettings["tableStorageConnectionString"];

        private readonly Authenticator authenticator;

        public HomeController()
        {
            authenticator = new Authenticator(ConfigurationManager.AppSettings["consumerKey"],
                ConfigurationManager.AppSettings["consumerSecret"],
                "http://api.fitbit.com/oauth/request_token",
                "http://api.fitbit.com/oauth/access_token",
                "http://api.fitbit.com/oauth/authorize");
        }

        public ActionResult Index()
        {
            List<FitBitModel> data = BuildViewModel();

            return View(data);
        }

        private List<FitBitModel> BuildViewModel()
        {
            CloudTableClient tableClient = GetTable();
            CloudTable table = tableClient.GetTableReference("users");
            table.CreateIfNotExists();
            IQueryable<NewOrbitEmployee> query = from employee in table.CreateQuery<NewOrbitEmployee>()
                select employee;
            ViewBag.TotalEmployees = query.ToList().Count();
            var data = new List<FitBitModel>();
            foreach (NewOrbitEmployee newOrbitEmployee in query)
            {
                FitbitClient fitbitClient = GetFitbitClient(newOrbitEmployee.AuthToken, newOrbitEmployee.AuthSecret);
                string name = fitbitClient.GetUserProfile().FullName;
                string totalSteps = fitbitClient.GetDayActivity(DateTime.Now).Summary.Steps.ToString();
                data.Add(new FitBitModel {Name = name, StepsToday = totalSteps});
            }
            return data.OrderByDescending(x => x.StepsToday).ToList();
        }

        public ActionResult AddMyDetails()
        {
            RequestToken token = authenticator.GetRequestToken();
            StoreToken(token.Secret);
            string authUrl = authenticator.GenerateAuthUrlFromRequestToken(token, true);


            return Redirect(authUrl);
        }

        private void StoreToken(string input)
        {
            CloudTableClient tableClient = GetTable();
            CloudTable table = tableClient.GetTableReference("temp");
            table.CreateIfNotExists();
            table.Execute(
                TableOperation.Insert(new MyTempTable {TempKey = input, PartitionKey = "x", RowKey = "y"}));
        }

        private string GetToken()
        {
            CloudTableClient tableClient = GetTable();
            CloudTable table = tableClient.GetTableReference("temp");
            MyTempTable item = (from t in table.CreateQuery<MyTempTable>()
                select t).First();
            string tempKey = item.TempKey;
            table.DeleteIfExists();
            return tempKey;
        }

        private FitbitClient GetFitbitClient(string token, string secret)
        {
            var client = new FitbitClient(ConfigurationManager.AppSettings["consumerKey"],
                ConfigurationManager.AppSettings["consumerSecret"],
                token,
                secret);

            return client;
        }

        //Final step. Take this authorization information and use it in the app
        public ActionResult Callback()
        {
            var token = new RequestToken();
            token.Token = Request.Params["oauth_token"];
            token.Secret = GetToken();
            token.Verifier = Request.Params["oauth_verifier"];

            //execute the Authenticator request to Fitbit
            AuthCredential credential = authenticator.ProcessApprovedAuthCallback(token);

            AddEmployeeIfNotPresent(credential);

            return RedirectToAction("Index", "Home");
        }

        private void AddEmployeeIfNotPresent(AuthCredential credential)
        {
            CloudTable table = GetUsersTable();

            NewOrbitEmployee employee = (from e in table.CreateQuery<NewOrbitEmployee>()
                select e).ToList().FirstOrDefault(x => x.UserId == credential.UserId);

            if (employee == null)
            {
                var newOrbitEmployee = new NewOrbitEmployee
                {
                    AuthSecret = credential.AuthTokenSecret,
                    UserId = credential.UserId,
                    AuthToken = credential.AuthToken,
                    PartitionKey = "ts",
                    RowKey = credential.UserId,
                };
                table.Execute(
                    TableOperation.Insert(newOrbitEmployee));
            }
        }

        private static CloudTable GetUsersTable()
        {
            CloudTableClient tableClient = GetTable();
            CloudTable table = tableClient.GetTableReference("users");
            table.CreateIfNotExists();
            return table;
        }

        private static CloudTableClient GetTable()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            return tableClient;
        }
    }
}