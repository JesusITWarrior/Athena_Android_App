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
        List<GroceryListItem> groceryList = new List<GroceryListItem>();
        public struct GroceryListItem {
            public string name { get; set; }
            public int amount { get; set; }
        }

        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            TextView title = FindViewById<TextView>(Resource.Id.ListTitle);
            Button addButton = FindViewById<Button>(Resource.Id.addItemButton);
            addButton.Click += (e,o)=>
            {
                StartActivity(new Android.Content.Intent(this, typeof(Test)));
            };
            //addButton.Click += AddNewItem;
            //title.PaintFlags = Android.Graphics.PaintFlags.UnderlineText;
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void AddNewItem(object o, EventArgs e)
        {
            string itemName = FindViewById<TextView>(Resource.Id.inputField1).Text;
            GroceryListItem newItem = new GroceryListItem();
            newItem.name = itemName;
            newItem.amount = 1;
            groceryList.Add(newItem);
            //Add new entry in the list
            groceryList.Sort();         //MAKE SURE TO ADJUST THIS ACCORDINGLY
        }

        public void AddItem(object o, EventArgs e)
        {
            GroceryListItem item = new GroceryListItem();
            //Should add items to this object
            item.amount++;
            //Make sure to replace item into List
        }

        public void SubtractItem(object o, EventArgs e)
        {
            GroceryListItem item = new GroceryListItem();
            //get item here from list
            if(item.amount == 1)
            {
                //Warn user they are removing the last of the item
            }
        }

        public void RemoveItem(object o, EventArgs e)
        {
            GroceryListItem item = new GroceryListItem();
            //Removes item from the list after the warning prompt
            groceryList.Remove(item);
        }
    }
}