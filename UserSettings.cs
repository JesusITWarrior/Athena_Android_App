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
using AndroidX.Core.Graphics.Drawable;
using Android.Graphics;

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    [Activity(Label = "UserSettings", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class UserSettings : Activity
    {
        string picString = UserData.pfpRaw;
        ImageView imageView;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.settings);
            Button hamburger = FindViewById<Button>(Resource.Id.moreOptionsButton);
            hamburger.Visibility = ViewStates.Gone;
            TextView title = FindViewById<TextView>(Resource.Id.ABTitle);
            title.Text = "User Settings";

            Button selectPicture = FindViewById<Button>(Resource.Id.selectPhoto);
            selectPicture.Click += ChangePFP;
            imageView = FindViewById<ImageView>(Resource.Id.pfp);
            RoundedBitmapDrawable rbmpd = RoundedBitmapDrawableFactory.Create(Resources, UserData.pfp);
            rbmpd.Circular = true;
            imageView.SetImageDrawable(rbmpd);

            ToggleButton tb = FindViewById<ToggleButton>(Resource.Id.tempPref);
            tb.Checked = UserPreferences.isF;

            Button apply = FindViewById<Button>(Resource.Id.apply);
            apply.Click += async (o,e) =>
            {
                UserPreferences.isF = tb.Checked;
                if(picString != UserData.pfpRaw)
                {
                    UpdateData log = new UpdateData();
                    log.id = UserData.username;
                    log.password = UserData.password;
                    log.key = UserData.key.ToString();
                    log.picUUID = UserData.picUUID;
                    log.pfp = UserData.pfpRaw;

                    await DatabaseManager.UpdateUser(log);
                }
                Finish();
            };

        }
        public struct UpdateData
        {
            public string id { get; set; }
            public string password { get; set; }
            public string key { get; set; }
            public string picUUID { get; set; }
            public string pfp { get; set; }
        }

        private void ChangePFP(object sender, EventArgs e)
        {
            Intent = new Intent();
            Intent.SetType("image/*");
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), 1000);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (data != null)
            {
                if (requestCode == 1000 && resultCode == Result.Ok)
                {
                    //Gets picture data as a stream
                    System.IO.Stream stream = ContentResolver.OpenInputStream(data.Data);
                    //Creates and converts stream content to bytes
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);

                    //Converts bytes to string for logging
                    picString = Convert.ToBase64String(bytes);

                    //Converts bytes to Bitmap for preview and saving
                    Bitmap bmp = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
                    RoundedBitmapDrawable rbmpd = RoundedBitmapDrawableFactory.Create(Resources, bmp);
                    rbmpd.Circular = true;
                    imageView.SetImageDrawable(rbmpd);
                }
            }
        }
    }
}