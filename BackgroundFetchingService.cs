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
using System.Threading.Tasks;

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    [Service]
    public class BackgroundFetchingService : Service
    {
        private bool isStillRunning = true;
        public static BackgroundFetchingService instance = null;
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
            instance = this;
        }

        private void StartForegroundService()
        {
            string channelName = "Background Service";
            NotificationChannel chan = new NotificationChannel(channelName, channelName, NotificationImportance.Low);
            NotificationManager manager = (NotificationManager) GetSystemService(Context.NotificationService);
            manager.CreateNotificationChannel(chan);

            Notification.Builder notificationBuilder = new Notification.Builder(this, channelName);
#pragma warning disable CS0618 // Type or member is obsolete
            Notification notification = notificationBuilder.SetContentTitle("Keeping Fridge Status Updated")
                                                           .SetSmallIcon(Resource.Mipmap.ic_launcher_round)
                                                           .SetContentText("Keeping all of your data accurate for you!")
                                                           .SetPriority((int)NotificationImportance.Low)
                                                           .SetOngoing(true)
                                                           .SetChannelId(channelName)
                                                           .SetAutoCancel(true)
                                                           .Build();
#pragma warning restore CS0618 // Type or member is obsolete
            StartForeground(1001, notification);
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            base.OnStartCommand(intent, flags, startId);
            //Function to run here
            Task.Run(async () =>
            {
                float timer = 0f;
                //bool tempAlarm = false;
                bool doorAlarm = false;
                while (isStillRunning)
                {
                    StatusDB dbStatus = await DatabaseManager.ReadStatusFromDB();
                    List<Status> status = dbStatus.loggedStatus;
                    int temp;
                    bool door=false;
                    for(int i = 0; i < status.Count; i++)
                    {
                        switch (status[i].dataName)
                        {
                            case "Temperature":
                                temp = Convert.ToInt32(status[i].value);
                                break;
                            case "Door Open Status":
                                door = Convert.ToBoolean(status[i].value);
                                break;
                        }
                    }
                    if (door)
                    {
                        if(timer >= 60)
                        {
                            doorAlarm = true;
                            timer = 0;
                        }
                        //Door is open
                        if (doorAlarm)
                        {
                            DoorAlarm();
                            doorAlarm = false;
                        }
                        await Task.Delay(1000);
                        timer += 1;
                    }
                    else
                    {
                        //Door is closed
                        timer = 0;
                        doorAlarm = false;
                        await Task.Delay(300000);
                    }
                }
            });
            return StartCommandResult.Sticky;
        }

        private void DoorAlarm()
        {
            string channelName = "Fridge Door Alarm!";
            NotificationChannel chan = new NotificationChannel(channelName, channelName, NotificationImportance.Max);
            NotificationManager manager = (NotificationManager)GetSystemService(Context.NotificationService);
            manager.CreateNotificationChannel(chan);

            Notification.Builder notificationBuilder = new Notification.Builder(this, channelName);
#pragma warning disable CS0618 // Type or member is obsolete
            Notification notification = notificationBuilder.SetContentTitle(channelName)
                                                           .SetSmallIcon(Resource.Mipmap.ic_launcher_round)
                                                           .SetContentText("Your Fridge Door has been open for longer than a minute!")
                                                           .SetPriority((int)NotificationImportance.Max)
                                                           .SetOngoing(false)
                                                           .SetChannelId(channelName)
                                                           .SetAutoCancel(true)
                                                           .Build();
#pragma warning restore CS0618 // Type or member is obsolete

            manager.Notify(1, notification);
        }

        public override void OnTaskRemoved(Intent rootIntent)
        {
            base.OnTaskRemoved(rootIntent);

            StopSelf();
            isStillRunning = false;

            Intent broadcastIntent = new Intent();
            broadcastIntent.SetAction("restartservice");
            broadcastIntent.SetClass(this, typeof(Restarter));
            this.SendBroadcast(broadcastIntent);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            //Function to run when stopped
            
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
    }

    [BroadcastReceiver]
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
                if (BackgroundFetchingService.instance != null)
                    context.StopService(new Intent(context, BackgroundFetchingService.instance.Class));
                context.StartService(new Intent(context, typeof(BackgroundFetchingService)));
            }
            return;
        }
    }
}