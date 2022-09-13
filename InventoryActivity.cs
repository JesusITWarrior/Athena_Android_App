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
using Newtonsoft.Json;
using System.IO;

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    [Activity(Label = "InventoryActivity")]
    public class InventoryActivity : Activity
    {
        bool isOnline = true;
        string filename = "inv-list.txt";
        List<Item> inventory;
        private ListView lv;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.inventory);
            TextView title = FindViewById<TextView>(Resource.Id.ListTitle);
            Button addButton = FindViewById<Button>(Resource.Id.addToList);
            //addButton.Click += (e,o)=>
            //{
            //    StartActivity(new Android.Content.Intent(this, typeof(Test)));
            //};
            addButton.Click += AddNewItem;

            lv = FindViewById<ListView>(Resource.Id.inventory);
            inventory = new List<Item>();

            InitList();
            if (inventory.Count != 0)
            {
                ItemListViewAdapter adapter = new ItemListViewAdapter(this, inventory);
                lv.Adapter = adapter;
            }
        }

        public async void InitList()
        {
            DateTime fileTime=System.DateTime.MinValue, dbTime=System.DateTime.MinValue;
            ItemFile itemFile = new ItemFile();
            ItemDB databaseList = new ItemDB();
            var destination = Path.Combine(Application.Context.GetExternalFilesDir(null).ToString(), filename);
            if (File.Exists(destination))
            {
                string rawJson = File.ReadAllText(destination);
                if (rawJson != "")
                {
                    itemFile = JsonConvert.DeserializeObject<ItemFile>(rawJson);
                    //inventory = itemFile.currentInventory;
                    fileTime = itemFile.updatedTime;
                }
            }
            else
            {
                File.Create(destination);
            }
            try
            {
                //Try to fetch list from database
                await DatabaseManager.GetDBInfo();
                databaseList = await DatabaseManager.ReadFromDB();
                //Show loading
                //inventory = databaseList.currentInventory;
                dbTime = databaseList.updatedTime;
            }
            catch (Exception e)
            {
                isOnline = false;
            }
            if (isOnline)
                inventory = (fileTime >= dbTime) ? itemFile.currentInventory : databaseList.currentInventory;
            else
                inventory = itemFile.currentInventory;
            
            if (inventory == null)
                inventory = new List<Item>();
            AddItemEvents();
            UpdateView();
        }

        public void WriteListToFile()
        {
            var destination = Path.Combine(Application.Context.GetExternalFilesDir(null).ToString(), filename);
            if (!File.Exists(destination))
                File.Create(destination);
            ItemFile itemFile = new ItemFile();
            itemFile.updatedTime = System.DateTime.Now;
            itemFile.currentInventory = inventory;
            string rawJson = JsonConvert.SerializeObject(itemFile);
            File.WriteAllText(destination, rawJson);
        }

        public void UpdateView(Item i=null)
        {
            if (i != null)
            {
                if (i.itemAmount == 0)
                {
                    inventory.Remove(i);
                }
            }
            WriteListToFile();
            DatabaseManager.WriteToDB(inventory);
            //await DatabaseManager.WriteToDB(inventory);
            ItemListViewAdapter adapter = new ItemListViewAdapter(this, inventory);
            lv.Adapter = adapter;
        }

        public void AddItemEvents()
        {
            foreach (Item i in inventory)
            {
                i.ItemChanged += UpdateView;
            }
        }

        public void AddNewItem(object o, EventArgs e)
        {
            string itemToAdd = FindViewById<TextView>(Resource.Id.itemInput).Text;
            bool foundIt = false;
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i].itemName == itemToAdd)
                {
                    inventory[i].itemAmount++;
                    foundIt = true;
                    break;
                }
            }
            if (!foundIt)
            {
                Item newItem = new Item();
                newItem.itemName = itemToAdd;
                newItem.itemAmount = 1;
                newItem.ItemChanged += UpdateView;
                inventory.Add(newItem);
                inventory.Sort((x,y) => string.Compare(x.itemName, y.itemName));
            }
            WriteListToFile();
            DatabaseManager.WriteToDB(inventory);
            ItemListViewAdapter adapter = new ItemListViewAdapter(this, inventory);
            lv.Adapter = adapter;
        }
    }
}