using System;
using System.Collections.Generic;
using System.Diagnostics;
using BluetoothPoll.ViewModels;
using CoreBluetooth;
using CoreLocation;
using Foundation;
using Prism;
using Prism.Ioc;
using UIKit;
using UserNotifications;
using Xamarin.Forms;

namespace BluetoothPoll.Ios
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
        private CLLocationManager locationManager;

        // class-level declarations

        List<BeaconItem> beacons;

        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App(new iOSInitializer()));

            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                UNUserNotificationCenter center = UNUserNotificationCenter.Current;
                center.RequestAuthorization(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound, (bool arg1, NSError arg2) =>{});
                center.Delegate = new NotificationDelegate();
            }
            else if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                var settings = UIUserNotificationSettings.GetSettingsForTypes(UIUserNotificationType.Alert | UIUserNotificationType.Sound, new NSSet());
                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            }


            locationManager = new CLLocationManager();
            var beaconId = new NSUuid(PollViewModel.LOOKUP_UUID);
            var beaconRegion = new CLBeaconRegion(beaconId, "My Beacon");
            beaconRegion.NotifyEntryStateOnDisplay = true;
            beaconRegion.NotifyOnEntry = true;

            locationManager.RegionEntered += (s, e) => {

                if (e.Region.Identifier == "My Beacon")
                {
                    Console.WriteLine("Found My Beacon");

                    locationManager.StartRangingBeacons(beaconRegion);
                    locationManager.DidRangeBeacons += (lm, e2) => {
                        if (e2.Beacons.Length > 0)
                        {
                            foreach (var b in e2.Beacons)
                            {

                                if (b.Proximity != CLProximity.Unknown)
                                {
                                    Console.WriteLine("UUID: {0} | Major: {1} | Minor: {2} | Accuracy: {3} | Proximity: {4} | RSSI: {5}", b.ProximityUuid, b.Major, b.Minor, b.Accuracy, b.Proximity, b.Rssi);
                                    var exists = false;
                                    for (int i = 0; i < beacons.Count; i++)
                                    {
                                        if (beacons[i].Minor.Equals(b.Minor.ToString()))
                                        {
                                            beacons[i].CurrentDistance = Math.Round(b.Accuracy, 2);
                                            SetProximity(b, beacons[i]);
                                            exists = true;
                                        }
                                    }

                                    if (!exists)
                                    {
                                        var newBeacon = new BeaconItem
                                        {
                                            Minor = b.Minor.ToString(),
                                            Name = "",
                                            CurrentDistance = Math.Round(b.Accuracy, 2)
                                        };
                                        SetProximity(b, newBeacon);
                                        beacons.Add(newBeacon);
                                    }
                                }
                            }
                        }
                    };
                }
            };
            locationManager.StartMonitoring(beaconRegion);

           
			return base.FinishedLaunching(application, launchOptions);
		}

        void SetProximity(CLBeacon source, BeaconItem dest)
        {

            CLProximity p = CLProximity.Unknown;

            switch (source.Proximity)
            {
                case CLProximity.Immediate:
                    p = CLProximity.Immediate;
                    var notification = new UILocalNotification();
                    notification.FireDate = NSDate.FromTimeIntervalSinceNow(1);
                    notification.AlertAction = "Device Proximity Alert";
                    notification.AlertBody = "We are close to required bluetooth device!";
                    notification.SoundName = UILocalNotification.DefaultSoundName;

                    UIApplication.SharedApplication.ScheduleLocalNotification(notification);
                    break;
                case CLProximity.Near:
                    p = CLProximity.Near;
                    break;
                case CLProximity.Far:
                    p = CLProximity.Far;
                    break;
            }


            dest.Proximity = p;
        }

        public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
            // show an alert
            UIAlertController okayAlertController = UIAlertController.Create(notification.AlertAction, notification.AlertBody, UIAlertControllerStyle.Alert);
            okayAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(okayAlertController, true, null);
        }
    }

	public class iOSInitializer : IPlatformInitializer
	{
		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}
	}

    public class NotificationDelegate : UNUserNotificationCenterDelegate
    {
        public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
        {
            completionHandler(UNNotificationPresentationOptions.Alert | UNNotificationPresentationOptions.Sound);
        }

    }

    public class BeaconItem
    {
        public string Minor { get; set; }
        public string Name { get; set; }
        public double CurrentDistance { get; set; }
        public CLProximity Proximity { get; set; }
    }
}

