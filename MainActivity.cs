﻿using System;
using System.IO;
using Java.IO;
using Android.Graphics;
using Android.App;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using Android.Widget;
using AndroidX.DrawerLayout.Widget;
using System.Collections.Generic;
using System.Drawing;

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        List<Status> recordedStatus;
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
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void ToContent()
        {
            SetContentView(Resource.Layout.info_screen);
            Toolbar tb = FindViewById<Toolbar>(Resource.Id.mainToolbar);
            SetActionBar(tb);
            TextView title = FindViewById<TextView>(Resource.Id.ABTitle);
            title.Text = UserData.username + "'s ATHENA";

            /*Toolbar tb = FindViewById<Toolbar>(Resource.Id.mainToolbar);
            SetSupportActionBar(tb);*/
            Button invBtn = FindViewById<Button>(Resource.Id.ToListButton);
            invBtn.Click += (o, e) =>
            {
                StartActivity(new Android.Content.Intent(this, typeof(InventoryActivity)));
            };
            FetchStatusFromDB();
        }

        private async void FetchStatusFromDB()
        {
            await DatabaseManager.GetDBInfo();
            StatusDB databaseList;
            databaseList = await DatabaseManager.ReadStatusFromDB();
            recordedStatus = databaseList.loggedStatus;
            if (recordedStatus == null)
                recordedStatus = new List<Status>();

            UpdateView();
        }

        public void UpdateView()
        {
            TextView temperature = FindViewById<TextView>(Resource.Id.temperature);
            TextView doorStatus = FindViewById<TextView>(Resource.Id.doorStatus);
            ImageView shelf = FindViewById<ImageView>(Resource.Id.shelfPic);
            int? temp=null;
            bool door=false;
            string pic=null;

            for (int i = 0; i < recordedStatus.Count;i++)
            {
                Status analyzingLog = recordedStatus[i];
                switch (analyzingLog.dataName) {
                    case "Temperature":
                        temp = Convert.ToInt32(analyzingLog.value);
                        break;
                    case "Door Status":
                        door = Convert.ToBoolean(analyzingLog.value);
                        break;
                    case "Picture":
                        pic = Convert.ToString(analyzingLog.value);
                        break;
                }
            }

            temperature.Text = "Temperature: "+temp+" F";
            if (door)
                doorStatus.Text = "Door is Open";
            else
                doorStatus.Text = "Door is Closed";
            //Convert pic here
            if (pic != null) {
                byte[] bytes = Convert.FromBase64String(pic);
                Android.Graphics.Bitmap bmp = BitmapFactory.DecodeByteArray(bytes,0,bytes.Length);
                ImageView iv = FindViewById<ImageView>(Resource.Id.shelfPic);
                iv.SetImageBitmap(bmp);
            }
        }
    }
}