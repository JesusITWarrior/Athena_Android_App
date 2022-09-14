using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    public static class UserData
    {
        public static string username { get; set; }
        public static string password { get; set; }

        public static void SetCredentials(string user, string pass)
        {
            username = user;
            password = pass;
        }

        public static bool ReadLoginInfo()
        {
            var destination = Path.Combine(Application.Context.GetExternalFilesDir(null).ToString(), "nani.fun");
            if (!File.Exists(destination))
                return false;
            string rawData = File.ReadAllText(destination);
            DataStruct ds = JsonConvert.DeserializeObject<DataStruct>(rawData);
            username = ds.username;
            password = ds.password;
            //Handle any auth here
            //Login();
            return true;
        }

        private static async Task<bool> Login()
        {
            await Task.Delay(1000);
            return true;
        }

        public static void SaveLoginInfo()
        {
            var destination = Path.Combine(Application.Context.GetExternalFilesDir(null).ToString(), "nani.fun");
            if (!File.Exists(destination))
                File.Create(destination);
            DataStruct ds = new DataStruct();
            ds.username = username;
            ds.password = password;
            string raw = JsonConvert.SerializeObject(ds);
            File.WriteAllText(destination, raw);
        }

        public struct DataStruct
        {
            public string username;
            public string password;
        }
    }
    class DatabaseManager
    {
        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = "https://iapyxretrofitapp.documents.azure.com:443/";
        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = "zGVFeZfK22QFtw7H8YMQ3ms6sy0UGpniSwzD2E0SmcCDwK45lSSYwGQideqvwY2PK9VS0qKkBL4myDG5Obqepg==";

        // The Cosmos client instance
        private static CosmosClient client;

        // The database we will create
        private static Database database;

        // The container we will create.
        private static Container container;

        public static bool isOnline = false;

        // The name of the database and container we will create
        private static string databaseId = "Global";
        private static string containerId = "ReportedData";

        public static async Task GetDBInfo()
        {
            client = new CosmosClient(EndpointUri, PrimaryKey);
            database = await client.CreateDatabaseIfNotExistsAsync(databaseId);
            container = await database.CreateContainerIfNotExistsAsync(containerId, partitionKeyPath: "/id", throughput: 400);

            if (container != null)
                isOnline = true;
        }

        public static async Task WriteToDB(List<Item> inventory)
        {
            try
            {
                ItemDB items = new ItemDB();
                items.id = UserData.username +" Inventory";
                items.updatedTime = System.DateTime.Now;
                items.currentInventory = inventory;
                ItemResponse<ItemDB> response;
                try
                {
                    response = await container.ReplaceItemAsync<ItemDB>(items, items.id);
                }
                catch (CosmosException ce)
                {
                    response = await container.CreateItemAsync<ItemDB>(items);
                }
            }
            catch (Exception ce)
            {
                return;
            }
        }

        public static async Task<ItemDB> ReadItemsFromDB()
        {
            try
            {
                string rawQuery = "SELECT * FROM ReportedData r WHERE r.id = \"" + UserData.username + " Inventory\"";
                QueryDefinition query = new QueryDefinition(rawQuery);
                using FeedIterator<ItemDB> queryResult = container.GetItemQueryIterator<ItemDB>(query);
                ItemDB items = new ItemDB();

                while (queryResult.HasMoreResults)
                {
                    FeedResponse<ItemDB> resultSet = await queryResult.ReadNextAsync();
                    foreach (ItemDB item in resultSet)
                    {
                        items = item;
                    }
                }
                return items;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static async Task<StatusDB> ReadStatusFromDB()
        {
            try
            {
                string rawQuery = "SELECT * FROM ReportedData r WHERE r.id = \"" + UserData.username + " Status\"";
                QueryDefinition query = new QueryDefinition(rawQuery);
                using FeedIterator<StatusDB> queryResult = container.GetItemQueryIterator<StatusDB>(query);
                StatusDB status = new StatusDB();

                while (queryResult.HasMoreResults)
                {
                    FeedResponse<StatusDB> resultSet = await queryResult.ReadNextAsync();
                    foreach (StatusDB item in resultSet)
                    {
                        status = item;
                    }
                }
                return status;
            }
            catch(Exception e)
            {
                //No longer connected to the internet!!!
                return null;
            }
        }

        public static async Task DeleteFromDB()
        {

        }
    }
}