// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MovesService.cs" company="">
//   
// </copyright>
// <summary>
//   The moves service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace neworbitfitbitleague.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using neworbitfitbitleague.Controllers;

    using Newtonsoft.Json;

    using RestSharp;

    /// <summary>
    /// The moves service.
    /// </summary>
    public class MovesService
    {
        /// <summary>
        /// The get steps today.
        /// </summary>
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int GetStepsToday(MovesAccessToken settings)
        {
            try
            {
                var client = new RestClient("https://api.moves-app.com/api/1.1");
                client.AddDefaultHeader("Authorization", string.Format("Bearer {0}", settings.access_token));
                IRestResponse restResponse = client.Get(new RestRequest("/user/summary/daily?pastDays=7"));
                var deserializeObject = JsonConvert.DeserializeObject<MovesClass1[]>(restResponse.Content);
               var movesClass1s = deserializeObject.Where(x => x.summary != null).Last();
                return movesClass1s.summary.Sum(x => x.steps);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get steps week.
        /// </summary>
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int GetStepsWeek(MovesAccessToken settings)
        {
            try
            {
                var client = new RestClient("https://api.moves-app.com/api/1.1");
                client.AddDefaultHeader("Authorization", string.Format("Bearer {0}", settings.access_token));
                IRestResponse restResponse = client.Get(new RestRequest("/user/summary/daily?pastDays=7"));
                var deserializeObject = JsonConvert.DeserializeObject<MovesClass1[]>(restResponse.Content);
                return deserializeObject.Where(x => x.summary != null).SelectMany(x => x.summary).Sum(x => x.steps);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The save user.
        /// </summary>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="movesAccessToken">
        /// The moves access token.
        /// </param>
        public void SaveUser(string email, MovesAccessToken movesAccessToken)
        {
            User user = new TableStorageService().GetAllEmployees().ToList().Single(x => x.EmailAddress == email);
            string json = JsonConvert.SerializeObject(movesAccessToken);
            user.MovesAccessToken = json;
            new TableStorageService().SaveUser(user);
        }
    }

    /// <summary>
    /// The moves class 1.
    /// </summary>
    public class MovesClass1
    {
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public string date { get; set; }

        /// <summary>
        /// Gets or sets the last update.
        /// </summary>
        public string lastUpdate { get; set; }

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        public MovesSummary[] summary { get; set; }
    }

    /// <summary>
    /// The moves summary.
    /// </summary>
    public class MovesSummary
    {
        /// <summary>
        /// Gets or sets the activity.
        /// </summary>
        public string activity { get; set; }

        /// <summary>
        /// Gets or sets the distance.
        /// </summary>
        public float distance { get; set; }

        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        public float duration { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        public string group { get; set; }

        /// <summary>
        /// Gets or sets the steps.
        /// </summary>
        public int steps { get; set; }
    }
}