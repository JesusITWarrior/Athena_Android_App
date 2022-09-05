using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using Android.Widget;
using System.Collections.Generic;

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        List<Item> inventory;
        private ListView lv;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            TextView title = FindViewById<TextView>(Resource.Id.ListTitle);
            Button addButton = FindViewById<Button>(Resource.Id.addToList);
            //addButton.Click += (e,o)=>
            //{
            //    StartActivity(new Android.Content.Intent(this, typeof(Test)));
            //};
            addButton.Click += AddNewItem;

            lv = FindViewById<ListView>(Resource.Id.inventory);

            inventory = new List<Item>();
            inventory.Add(new Item() { itemAmount = 5, itemName = "Apple"});
            inventory.Add(new Item() { itemAmount = 2, itemName = "Banana" });

            ItemListViewAdapter adapter = new ItemListViewAdapter(this, inventory);
            lv.Adapter = adapter;
            
            //title.PaintFlags = Android.Graphics.PaintFlags.UnderlineText;
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void AddNewItem(object o, EventArgs e)
        {
            string itemToAdd = FindViewById<TextView>(Resource.Id.inputField1).Text;
            bool foundIt = false;
            for(int i = 0; i < inventory.Count; i++)
            {
                if(inventory[i].itemName == itemToAdd)
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
                inventory.Add(newItem);
            }

            ItemListViewAdapter adapter = new ItemListViewAdapter(this, inventory);
            lv.Adapter = adapter;
        }

        //    public void AddItem(object o, EventArgs e)
        //    {
        //        GroceryListItem item = new GroceryListItem();
        //        //Should add items to this object
        //        item.amount++;
        //        //Make sure to replace item into List
        //    }

        //    public void SubtractItem(object o, EventArgs e)
        //    {
        //        GroceryListItem item = new GroceryListItem();
        //        //get item here from list
        //        if(item.amount == 1)
        //        {
        //            //Warn user they are removing the last of the item
        //        }
        //    }

        //    public void RemoveItem(object o, EventArgs e)
        //    {
        //        GroceryListItem item = new GroceryListItem();
        //        //Removes item from the list after the warning prompt
        //        groceryList.Remove(item);
        //    }
    }
}