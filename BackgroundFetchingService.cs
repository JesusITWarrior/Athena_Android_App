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
    public class BackgroundFetchingService : Service
    {
        public override void OnCreate()
        {
            base.OnCreate();
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                StartForegroundService();
            }
            else
            {
                StartForeground(1, new Notification());
            }
        }

        private void StartForegroundService()
        {
            string notificationChannelID = "example.permanence";
            string channelName = "Background Service";
            NotificationChannel chan = new NotificationChannel(notificationChannelID, channelName, NotificationImportance.None);
            NotificationManager manager = (NotificationManager) GetSystemService(Context.NotificationService);
            manager.CreateNotificationChannel(chan);

            Notification.Builder notificationBuilder = new Notification.Builder(this, notificationChannelID);
            Notification notification = notificationBuilder.SetOngoing(true).SetContentTitle("App is running in background").SetPriority((int)NotificationImportance.Min).SetCategory(Notification.CategoryService).Build();
            StartForeground(2, notification);
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
    }

    public class Restarter : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Toast.MakeText(context, "Service restarted", ToastLength.Short).Show();
            if(Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                context.StartForegroundService(new Intent(context, typeof(BackgroundFetchingService)));
            }
            else
            {
                context.StartService(new Intent(context, typeof(BackgroundFetchingService)));
            }
            return;
        }
    }
}