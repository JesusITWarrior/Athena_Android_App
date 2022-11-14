using System;
using System.IO;
using Java.IO;
using Android.Graphics;
using Android.App;
using Android.OS;
using Android.Content;
using Android.Runtime;
using AndroidX.AppCompat.App;
using Android.Widget;
using AndroidX.Core.Graphics.Drawable;
using AndroidX.DrawerLayout.Widget;
using System.Collections.Generic;

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    /// <summary>
    /// First Activity of the app. Handles login and fridge status UI stuff
    /// </summary>
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        StatusDB recordedStatus;
        ProgressBar nonInvasiveLoadingIcon;
        private BackgroundFetchingService fetchService;
        private Intent fetchIntent;
        /// <summary>
        /// Acts as "Main" function. Is run the moment this activity is started (app is booted)
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            //If "remember me" button is checked a file is written. If that file exists, it automatically logs in the user
            /*if (UserData.ReadLoginInfo())
            {
                Login();
            }
            //otherwise, it takes the user through the login/registration process.
            else
            {*/
            UserPreferences.SetViewSize(this);
            if (UserData.ReadLoginInfo())
            {
                Login(false);
            }
            else
            {
                if (UserPreferences.widthWindowSize == UserPreferences.WindowSize.COMPACT)
                    SetContentView(Resource.Layout.activity_main);
                else
                    SetContentView(Resource.Layout.activity_main);
                //Log In Button variable
                Button loginButton = FindViewById<Button>(Resource.Id.LoginButton);
                loginButton.Click += (o, e) =>
                {
                //When login button is clicked, it gets the username and password components, sets the credentials, checks the remember me box, and then attempts a login with the input credentials.
                    string user = FindViewById<TextView>(Resource.Id.username).Text;
                    string pass = FindViewById<TextView>(Resource.Id.password).Text;
                    UserData.SetCredentials(user, pass);
                    Login(true);
                };
                //Registration Button variable
                Button registrationButton = FindViewById<Button>(Resource.Id.registration);
                registrationButton.Click += (o, e) =>
                {
                //Opens new Registration Activity
                    StartActivityForResult(typeof(RegistrationActivity), 1);
                };
                /*Button testButton = FindViewById<Button>(Resource.Id.testButton);
                testButton.Click += (o,e) =>
                {
                    StartActivity(typeof(Test));
                };*/
                //}
            }
        }

        /// <summary>
        /// Function is called back when a permission request is run. At the moment, it just grants results
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="permissions"></param>
        /// <param name="grantResults"></param>
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        /// <summary>
        /// Attempts a login, a server error prints Connection Error, while incorrect login prints "Wrong info"
        /// </summary>
        private async void Login(bool credsVisible)
        {
            Dialog loading = new Dialog(this);
            loading.SetContentView(Resource.Layout.whole_screen_loading_symbol);
            if(credsVisible)
                loading.Show();

            //Check database
            bool canLogin = true;
            DatabaseManager.GetAuthDBInfo();
            if (!DatabaseManager.isOnline)
            {
                if (!credsVisible)
                {
                    SetContentView(Resource.Layout.activity_main);
                }
                //Throw login connection error
                TextView loginError = FindViewById<TextView>(Resource.Id.loginError);
                loginError.Text = "Connection Error";
                loginError.Visibility = Android.Views.ViewStates.Visible;
                canLogin = false;
            }
            else
                canLogin = await DatabaseManager.CheckLogin();

            //If login is successful, then it fetches the log container information and proceeds past login screen to status screen
            if (canLogin)
            {
                UserData.SaveLoginInfo();
                await DatabaseManager.GetLogDBInfo();
                ToContent();
            }
            //If it's unsuccessful and online, it resets all UserData instance values and informs user of incorrect issue.
            else if (DatabaseManager.isOnline)
            {
                UserData.username = "";
                UserData.password = "";
                //Throw login error
                TextView loginError = FindViewById<TextView>(Resource.Id.loginError);
                loginError.Text = "Username or Password is Incorrect";
                loginError.Visibility = Android.Views.ViewStates.Visible;
                
            }
            if (credsVisible)
            {
                loading.Dismiss();
                loading.Hide();
            }
        }

        /// <summary>
        /// Sets the XML from login page to status page
        /// </summary>
        private void ToContent()
        {
            if (BackgroundFetchingService.instance != null)
                StopService(new Intent(this, BackgroundFetchingService.instance.Class));

            fetchService = new BackgroundFetchingService();
            fetchIntent = new Intent(this, fetchService.Class);

            //Handle service start here:
            if (!serviceIsRunning(fetchService.Class))
            {
                StartService(fetchIntent);
            }

            SetContentView(Resource.Layout.info_screen);
            Toolbar tb = FindViewById<Toolbar>(Resource.Id.mainToolbar);
            SetActionBar(tb);

            //Sets user's pfp to the ImageButton
            ImageButton pfp = FindViewById<ImageButton>(Resource.Id.pfp);
            if (UserData.pfp != null)
            {
                RoundedBitmapDrawable rbmpd = RoundedBitmapDrawableFactory.Create(Resources, UserData.pfp);
                rbmpd.Circular = true;
                pfp.SetImageDrawable(rbmpd);
            }
            else
            {
                Bitmap bmp = BitmapFactory.DecodeResource(Resources ,Resource.Drawable.blank_profile_picture);
                RoundedBitmapDrawable rbmpd = RoundedBitmapDrawableFactory.Create(Resources, bmp);
                rbmpd.Circular = true;
                pfp.SetImageDrawable(rbmpd);
            }

            //Sets up inventoryButton event to start inventory list activity
            Button invBtn = FindViewById<Button>(Resource.Id.ToListButton);
            invBtn.Click += (o, e) =>
            {
                //When clicked, it starts new Inventory Activity
                StartActivity(new Intent(this, typeof(InventoryActivity)));
            };

            //Sets up Bluetooth button setup button for onboarding process
            Button bluetoothSetup = FindViewById<Button>(Resource.Id.bluetooth);
            bluetoothSetup.Click += (o, e) =>
            {
                StartActivity(typeof(OnboardingActivity));
            };

            Button test = FindViewById<Button>(Resource.Id.testButton);
            test.Click += async (o,e) =>
            {
                await System.Threading.Tasks.Task.Delay(3000);
                StartActivity(typeof(GraphingActivity));
                /*NotificationChannel chan = new NotificationChannel(channelName, channelName, NotificationImportance.Max);
                NotificationManager manager = (NotificationManager)GetSystemService(Context.NotificationService);
                manager.CreateNotificationChannel(chan);

                Notification.Builder notificationBuilder = new Notification.Builder(this, channelName);

                Notification notification = notificationBuilder.SetContentTitle(channelName)
                                                               .SetSmallIcon(Resource.Mipmap.ic_launcher_round)
                                                               .SetContentText("You clicked the notification button.")
                                                               .SetOngoing(false)
                                                               .SetChannelId(channelName)
                                                               .SetAutoCancel(true)
                                                               .Build();


                manager.Notify(1, notification);*/
            };

            //Gets the current status values for the fridge
            FetchStatusFromDB();
        }

        /// <summary>
        /// Gets Status values for Fridge from Database
        /// </summary>
        private async void FetchStatusFromDB()
        {
            //Fetches items from database
            recordedStatus = await DatabaseManager.ReadCurrentStatusFromDB(true);
            //Sets items into the global variable for use
            //recordedStatus = databaseList.loggedStatus;

            UpdateView();
        }
        
        private bool serviceIsRunning(Java.Lang.Class serviceClass)
        {
            ActivityManager manager = (ActivityManager)GetSystemService(Context.ActivityService);
#pragma warning disable CS0618 // Type or member is obsolete
            foreach (ActivityManager.RunningServiceInfo service in manager.GetRunningServices(int.MaxValue))
            {
                if (serviceClass.Name.Equals(service.Service.ClassName))
                {
                    return true;
                }
            }
            return false;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Makes sure all items are correctly up-to-date when called
        /// </summary>
        public void UpdateView()
        {
            TextView temperature = FindViewById<TextView>(Resource.Id.temperature);
            TextView doorStatus = FindViewById<TextView>(Resource.Id.doorStatus);
            ImageView shelf = FindViewById<ImageView>(Resource.Id.shelfPic);
            nonInvasiveLoadingIcon = FindViewById<ProgressBar>(Resource.Id.nonInvasiveLoading);
            nonInvasiveLoadingIcon.Visibility = Android.Views.ViewStates.Visible;
            //int temp= (UserPreferences.isF) ? recordedStatus.Temperature[1] : recordedStatus.Temperature[0];
            int temp = recordedStatus.Temperature;
            bool door=recordedStatus.DoorOpenStatus;
            string pic=recordedStatus.Picture;

            //Formats the Views to a more readable format with the new values
            if (UserPreferences.isF)
            {
                temperature.Text = temp + " F";
                if (temp > 30 && temp < 40)
                    temperature.SetTextColor(Color.Green);
                else
                    temperature.SetTextColor(Color.Red);
            }
            else
            {
                temp = (int)((float)(temp) - 32) * (5 / 9);
                temperature.Text = temp + " C";
                if(temp > -1 && temp < 4)
                    temperature.SetTextColor(Color.Green);
                else
                    temperature.SetTextColor(Color.Red);
            }
            if (door)
            {
                doorStatus.Text = "Open";
                doorStatus.SetTextColor(Color.Red);
            }
            else
            {
                doorStatus.Text = "Closed";
                doorStatus.SetTextColor(Color.Green);
            }

            //Converts pic string to Bitmap for ImageView
            if (pic != null) {
                byte[] bytes = Convert.FromBase64String(pic);
                Bitmap bmp = BitmapFactory.DecodeByteArray(bytes,0,bytes.Length);
                ImageView iv = FindViewById<ImageView>(Resource.Id.shelfPic);
                iv.SetImageBitmap(bmp);
            }

            nonInvasiveLoadingIcon.Visibility = Android.Views.ViewStates.Invisible;
        }
        /*protected override void OnDestroy()
        {
            Intent broadcastIntent = new Intent();
            broadcastIntent.SetAction("restartservice");
            broadcastIntent.SetClass(this, typeof(Restarter));
            this.SendBroadcast(broadcastIntent);
            base.OnDestroy();
        }*/

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 1) {
                if (resultCode == Result.Ok)
                {
                    ToContent();
                }
                else
                {
                    Toast.MakeText(this, "Registration Failed", ToastLength.Short).Show();
                }
            }
        }
    }
}