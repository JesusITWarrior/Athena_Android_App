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

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    [Activity(Label = "Test")]
    public class Test : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.test);

            Button button = FindViewById<Button>(Resource.Id.dialog_button);
            button.Click += (o,e) => {
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetCancelable(true);
                builder.SetTitle("Testing Alert");
                builder.SetMessage("This is the message portion");
                
                builder.Show();
            };
        }
    }
}