using System.Configuration;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace neworbitfitbitleague.Models
{

    public class TableStorageService
    {
        private static readonly string connectionString =
           ConfigurationManager.AppSettings["tableStorageConnectionString"] ?? "DefaultEndpointsProtocol=http;AccountName=abmanleague;AccountKey=Wq4T2CxY1dpXftGEr3Zl6nxs3ZTCzY0ttMQkEhWGUxLcGtRAxwl1uLsS+GvIHqvAJnpG3fGgj9eVDPoNowvwUg==;";

        public CloudTable GetUsersTable()
        {
            CloudTableClient tableClient = GetTable();
            CloudTable table = tableClient.GetTableReference("users");
            table.CreateIfNotExists();
            return table;
        }

        public CloudTableClient GetTable()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            return tableClient;
        }

        public IQueryable<User> GetAllEmployees()
        {
            CloudTableClient tableClient = new TableStorageService().GetTable();
            CloudTable table = tableClient.GetTableReference("users");
            table.CreateIfNotExists();
            var query = from employee in table.CreateQuery<Models.User>()
                        select employee;
            return query;
        }


        public void InsertUser(User newOrbitEmployee)
        {
            GetUsersTable().Execute(TableOperation.Insert(newOrbitEmployee));
        }

        public void SaveUser(User user)
        {
            GetUsersTable().Execute(TableOperation.InsertOrReplace(user));
        }
    }
}