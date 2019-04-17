using System;
using System.Diagnostics;
using CoreBluetooth;
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

        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
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

            global::Xamarin.Forms.Forms.Init();
	        LoadApplication(new App(new iOSInitializer()));
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
}

