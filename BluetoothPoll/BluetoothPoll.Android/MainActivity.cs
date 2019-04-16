using System;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Widget;
using BluetoothPoll.Models;
using Plugin.CurrentActivity;
using Prism;
using Prism.Ioc;
using Xamarin.Forms;
using TaskStackBuilder = Android.App.TaskStackBuilder;

namespace BluetoothPoll.Droid
{
    [Activity(Label = "BluetoothPoll", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme",
	    MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
	    public const int CheckPermissionsId = 999;
	    public TaskCompletionSource<bool> TaskCompletionSource;

		private BluetoothIntentReceiver _receiver;

	    static readonly int NOTIFICATION_ID = 100500;
	    static readonly string CHANNEL_ID = "local_notification";
	    internal static readonly string CLOSE_DEVICE = "deviceClose";

		protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());

	        CreateNotificationChannel();

			_receiver = new BluetoothIntentReceiver();
	        CrossCurrentActivity.Current.Init(this, bundle);
		}

	    protected override void OnResume()
	    {
		    base.OnResume();
			var f = new IntentFilter();
			f.AddAction(BluetoothDevice.ActionFound);
			f.AddAction(BluetoothDevice.ActionUuid);
			f.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
		    RegisterReceiver(_receiver, f);
		}

	    protected override void OnPause()
	    {
		    base.OnPause();
			UnregisterReceiver(_receiver);
	    }

	    private void RequestDevices()
	    {
		    BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;

		    var task = Task.Run(async () =>
		    {
			    if (adapter.IsEnabled)
			    {
				    if (adapter.IsDiscovering)
				    {
					    adapter.CancelDiscovery();
				    }
				    adapter.StartDiscovery();

				    int timeout = 20000;
				    await Task.Delay(TimeSpan.FromMilliseconds(timeout));

				    adapter.CancelDiscovery();

				    TaskCompletionSource.SetResult(false);
			    }
			    else
			    {
				    TurnBluetoothOn();
				}
		    });
		}

	    private void TurnBluetoothOn()
	    {
		    BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;

			RunOnUiThread(() =>
		    {
			    AlertDialog.Builder alert = new AlertDialog.Builder(this);
			    alert.SetTitle("Confirmation");
			    alert.SetMessage("Do you really want to turn Bluetooth On?");
			    alert.SetPositiveButton("Yes", async (senderAlert, args) =>
			    {
				    adapter.Enable();
				    await Task.Delay(2000);
					if (adapter.IsEnabled)
				    {
					    Toast.MakeText(this, "Bluetooth Ready", ToastLength.Short).Show();
						RequestDevices();
				    }
			    });
			    alert.SetNegativeButton("No", (senderAlert, args) => {
				    Toast.MakeText(this, "Cancelled", ToastLength.Short).Show();
			    });
			    Dialog dialog = alert.Create();
			    dialog.Show();
			});
	    }

	    public void CheckPermissions() 
	    {

		    if (CheckSelfPermission(Manifest.Permission.Bluetooth) == (int)Permission.Granted &&
		        CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) == (int)Permission.Granted)
		    {
				//continue
			    RequestDevices();
		    }
		    else
		    {
			    RequestPermissions(new[] { Manifest.Permission.Bluetooth, Manifest.Permission.AccessCoarseLocation }, CheckPermissionsId);
		    }
	    }

	    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
	    {
		    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		    switch (requestCode)
		    {
			    case CheckPermissionsId:
				    if (CheckSelfPermission(Manifest.Permission.Bluetooth) == Permission.Granted &&
				        CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) == (int)Permission.Granted)
				    {
						//continue
					    RequestDevices();
					}
				    else
				    {
					    BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
						adapter.CancelDiscovery();
					    TaskCompletionSource.SetResult(false);
					    Toast.MakeText(this, "You must to grant Bluetooth and Location permissions to use this app", 
						    ToastLength.Long).Show();
					}
				    break;
			    default:
				    break;
		    }
	    }


	    public void SendLocalNotification()
	    {
		    NotificationCompat.Builder builder = new NotificationCompat.Builder(this, CHANNEL_ID)
			    .SetContentTitle("Device Proximity Alert")
			    .SetContentText("We are close to required bluetooth device!").
			    SetSmallIcon(Resource.Drawable.bt);

		    // Finally, publish the notification:
		    var notificationManager = NotificationManagerCompat.From(this);
		    notificationManager.Notify(NOTIFICATION_ID, builder.Build());
		}

	    private void CreateNotificationChannel()
	    {
		    if (Build.VERSION.SdkInt < BuildVersionCodes.O)
		    {
			    // Notification channels are new in API 26 (and not a part of the
			    // support library). There is no need to create a notification
			    // channel on older versions of Android.
			    return;
		    }

		    var channel = new NotificationChannel(CHANNEL_ID, "Proximity alert", NotificationImportance.Default)
		    {
			    Description = "Notification about close looked up device"
		    };

		    var notificationManager = (NotificationManager)GetSystemService(NotificationService);
		    notificationManager.CreateNotificationChannel(channel);
	    }
	}

}

