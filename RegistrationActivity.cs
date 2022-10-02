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
using Android.Graphics;

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    [Activity(Label = "RegistrationActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class RegistrationActivity : Activity
    {
        TextView user;
        TextView pass;
        ImageView imageView;
        Bitmap picBitmap;
        string picString;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.registration);
            // Create your application here
            Button register = FindViewById<Button>(Resource.Id.RegisterButton);
            Button selectPicture = FindViewById<Button>(Resource.Id.selectPhoto);
            imageView = FindViewById<ImageView>(Resource.Id.pfp);

            selectPicture.Click += ProfilePictureSetup;

            register.Click += async (o, e) =>
            {
                user = FindViewById<TextView>(Resource.Id.usernameReg);
                pass = FindViewById<TextView>(Resource.Id.passwordReg);
                bool success = await DatabaseManager.Register(user.Text, pass.Text, picString);
                if (success)
                {
                    SetContentView(Resource.Layout.FirstPair);
                    Button accept = FindViewById<Button>(Resource.Id.accept);
                    Button decline = FindViewById<Button>(Resource.Id.decline);

                    accept.Click += (o, e) => {
                        StartActivity(typeof(OnboardingActivity));
                        Finish();
                    };

                    decline.Click += (o, e) =>
                    {
                        Finish();
                    };
                }
                else
                {
                    //Throw some sort of error.
                }
            };
        }

        private void ProfilePictureSetup(object sender, EventArgs e)
        {
            Intent = new Intent();
            Intent.SetType("image/*");
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), 1000);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if(data != null)
            {
                if(requestCode == 1000 && resultCode == Result.Ok)
                {
                    System.IO.Stream stream = ContentResolver.OpenInputStream(data.Data);
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                    picString = Convert.ToBase64String(bytes);
                    imageView.SetImageBitmap(BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length));
                }
            }
        }
    }
}