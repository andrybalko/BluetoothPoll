using System;
using System.Collections.Generic;
using System.Diagnostics;
using BluetoothPoll.Ios.Services;
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

        // class-level declarations

		private BeaconMonitor _beaconMonitor;
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

			//instantiate beacon tracker
			_beaconMonitor = new BeaconMonitor();
            

           
			return base.FinishedLaunching(application, launchOptions);
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

