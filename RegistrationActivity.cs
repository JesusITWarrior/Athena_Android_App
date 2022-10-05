using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using AndroidX.Core.Graphics.Drawable;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Graphics;

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    /// <summary>
    /// Handles Registration Activity
    /// </summary>
    [Activity(Label = "RegistrationActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class RegistrationActivity : Activity
    {
        //Important Views to be read from multiple functions
        TextView user;
        TextView pass;
        ImageView imageView;
        string picString=null;

        /// <summary>
        /// Run when Activity is started
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.registration);
            
            //Sets up registration button, picture selection button, and the image view
            Button register = FindViewById<Button>(Resource.Id.RegisterButton);
            Button selectPicture = FindViewById<Button>(Resource.Id.selectPhoto);
            imageView = FindViewById<ImageView>(Resource.Id.pfp);

            //Setting the default picture to be circular
            Bitmap bmp = BitmapFactory.DecodeResource(Resources, Resource.Drawable.blank_profile_picture);
            RoundedBitmapDrawable rbmpd = RoundedBitmapDrawableFactory.Create(Resources, bmp);
            rbmpd.Circular = true;
            imageView.SetImageDrawable(rbmpd);

            //Subscribes selectPicture click event to the ProfilePictureSetup Function
            selectPicture.Click += ProfilePictureSetup;

            
            register.Click += async (o, e) =>
            {
                Dialog loading = new Dialog(this);
                loading.SetContentView(Resource.Layout.whole_screen_loading_symbol);
                loading.Show();
                //Attempts to register the user with the username, password, and profile picture
                user = FindViewById<TextView>(Resource.Id.usernameReg);
                pass = FindViewById<TextView>(Resource.Id.passwordReg);
                bool success = await DatabaseManager.Register(user.Text, pass.Text, picString);
                //If the registration was successful it should prompt to start onboarding with Athena device
                if (success)
                {
                    SetContentView(Resource.Layout.FirstPair);
                    Button accept = FindViewById<Button>(Resource.Id.accept);
                    Button decline = FindViewById<Button>(Resource.Id.decline);

                    accept.Click += (o, e) => {
                        //User accepted onboarding, which will start onboarding activity, and close the registration
                        StartActivity(typeof(OnboardingActivity));
                        Finish();
                    };

                    decline.Click += (o, e) =>
                    {
                        //User declined onboarding, which will close registration
                        Finish();
                    };
                }
                //Registration failed for some reason
                else
                {
                    //Throw some sort of error.
                }
                loading.Dismiss();
                loading.Hide();
            };
        }

        /// <summary>
        /// Sets up Request to access photos to upload profile picture
        /// </summary>
        /// <param name="sender">Button that sent this</param>
        /// <param name="e">Empty</param>
        private void ProfilePictureSetup(object sender, EventArgs e)
        {
            Intent = new Intent();
            Intent.SetType("image/*");
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), 1000);
        }

        /// <summary>
        /// Run when child activity is finished. In this case, it will be for "Profile pictures"
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="resultCode"></param>
        /// <param name="data">Should possess the picture file reference</param>
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if(data != null)
            {
                if(requestCode == 1000 && resultCode == Result.Ok)
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