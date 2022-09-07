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
        // ADD THIS PART TO YOUR CODE

        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = "https://iapyxretrofitapp.documents.azure.com:443/";
        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = "zGVFeZfK22QFtw7H8YMQ3ms6sy0UGpniSwzD2E0SmcCDwK45lSSYwGQideqvwY2PK9VS0qKkBL4myDG5Obqepg==";

        // The Cosmos client instance
        private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        private Container container;

        // The name of the database and container we will create
        private string databaseId = "Global";
        private string containerId = "ReportedData";

        public static async Task WriteToDB(List<Item> inventory)
        {
            try
            {
                CosmosClient client = new CosmosClient(EndpointUri, PrimaryKey);
                Database database = await client.CreateDatabaseIfNotExistsAsync(id: "Global");
                Container container = await database.CreateContainerIfNotExistsAsync(id: "ReportedData", partitionKeyPath: "/id", throughput: 400);
                ItemDB items = new ItemDB();
                items.id = UserData.username +" Inventory";
                items.currentInventory = inventory;
                ItemResponse<ItemDB> response = await container.CreateItemAsync<ItemDB>(items);
            }
            catch (Exception ce)
            {

            }
        }

        public static async Task ReadFromDB()
        {

        }

        public static async Task DeleteFromDB()
        {

        }
    }
}