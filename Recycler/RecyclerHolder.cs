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

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP.Recycler
{
    class RecyclerHolder : RecyclerView.ViewHolder
    {
        public TextView NameTxt;
        public ImageView Img;

        public RecyclerHolder(View itemView): base(itemView)
        {
            NameTxt = itemView.FindViewById<TextView>(Resource.Id.nameTxt);
            Img = itemView.FindViewById<ImageView>(Resource.Id.img);
        }
    }

    class GroceryItem
    {
        private string name;
        private int image;

        public GroceryItem()
        {

        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public int Image
        {
            get { return image; }
            set { image = value; }
        }
    }

    class GroceryItemCollection
    {
        public static List<GroceryItem> GetGroceryItems()
        {
            List<GroceryItem> groceries = new List<GroceryItem>();
            GroceryItem item = new GroceryItem();

            item.Name = "Apples";
            item.Image = Resource.Color.red;
            groceries.Add(item);

            item.Name = "Bananas";
            item.Image = Resource.Drawable.yellow;
            groceries.Add(item);

            item.Name = "Cherries";
            item.Image = Resource.Drawable.red;
            groceries.Add(item);

            return groceries;
        }
    }
}