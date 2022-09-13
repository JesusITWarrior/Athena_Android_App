using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using Android.Widget;
using System.Collections.Generic;
using System.Drawing;

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            if (UserData.ReadLoginInfo())
            {
                ToContent();
            }
            else
            {
                SetContentView(Resource.Layout.activity_main);
                Button loginButton = FindViewById<Button>(Resource.Id.LoginButton);
                loginButton.Click += (o, e) =>
                {
                    string user = FindViewById<TextView>(Resource.Id.username).Text;
                    string pass = FindViewById<TextView>(Resource.Id.password).Text;
                    UserData.SetCredentials(user, pass);
                    //Check credentials
                    CheckBox rm = FindViewById<CheckBox>(Resource.Id.RememberMe);
                    if (rm.Checked)
                    {
                        UserData.SaveLoginInfo();
                    }
                    ToContent();
                };
            }
            // Set our view from the "main" layout resource

            
            //title.PaintFlags = Android.Graphics.PaintFlags.UnderlineText;
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void ToContent()
        {
            SetContentView(Resource.Layout.info_screen);
            Toolbar tb = FindViewById<Toolbar>(Resource.Id.mainToolbar);
            SetActionBar(tb);
            TextView title = FindViewById<TextView>(Resource.Id.ABTitle);
            title.Text = UserData.username + "'s ATHENA";
            /*Toolbar tb = FindViewById<Toolbar>(Resource.Id.mainToolbar);
            SetSupportActionBar(tb);*/
            /*Button invBtn = FindViewById<Button>(Resource.Id.ToListButton);
            invBtn.Click += (o, e) =>
            {
                StartActivity(new Android.Content.Intent(this, typeof(InventoryActivity)));
            };*/
        }
    }
}