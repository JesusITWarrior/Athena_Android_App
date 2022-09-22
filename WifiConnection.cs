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
    [Activity(Label = "WifiConnection")]
    public class WifiConnection : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.WifiCredentials);
            TextView title = FindViewById<TextView>(Resource.Id.wifiTitle);
            TextView user = FindViewById<TextView>(Resource.Id.usernameInput);
            TextView password = FindViewById<TextView>(Resource.Id.passwordInput);
            Button connect = FindViewById<Button>(Resource.Id.connectButton);

            Bundle bundle = Intent.Extras;
            string name = bundle.GetString("WifiTitle");

            title.Text = name;

            connect.Click += (o,e) =>
            {

            };
        }
    }
}