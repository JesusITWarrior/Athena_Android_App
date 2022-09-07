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

        public void InitList()
        {
            var destination = Path.Combine(Application.Context.GetExternalFilesDir(null).ToString(), filename);
            if (File.Exists(destination))
            {
                string rawJson = File.ReadAllText(destination);
                if (rawJson != "")
                {
                    inventory = JsonConvert.DeserializeObject<List<Item>>(rawJson);
                }
            }
            else
            {
                File.Create(destination);
            }
            foreach (Item i in inventory)
            {
                i.ItemChanged += UpdateView;
            }
            DatabaseManager.WriteToDB(inventory);
        }

        public void WriteListToFile()
        {
            var destination = Path.Combine(Application.Context.GetExternalFilesDir(null).ToString(), filename);
            string rawJson = JsonConvert.SerializeObject(inventory);
            File.WriteAllText(destination, rawJson);
        }

        public void UpdateView(Item i)
        {
            if (i.itemAmount == 0)
            {
                inventory.Remove(i);
            }
            WriteListToFile();
            ItemListViewAdapter adapter = new ItemListViewAdapter(this, inventory);
            lv.Adapter = adapter;
        }

        public void AddNewItem(object o, EventArgs e)
        {
            string itemToAdd = FindViewById<TextView>(Resource.Id.inputField1).Text;
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
            }
            WriteListToFile();
            ItemListViewAdapter adapter = new ItemListViewAdapter(this, inventory);
            lv.Adapter = adapter;
        }
    }
}