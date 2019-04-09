using System;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using BluetoothPoll.Models;
using Plugin.CurrentActivity;
using Prism;
using Prism.Ioc;
using Xamarin.Forms;

namespace BluetoothPoll.Droid
{
    [Activity(Label = "BluetoothPoll", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme",
	    MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
	    public const int CheckPermissionsId = 999;
	    public TaskCompletionSource<bool> TaskCompletionSource;

		private BluetoothIntentReceiver _receiver;

	    protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());

	        _receiver = new BluetoothIntentReceiver();
	        CrossCurrentActivity.Current.Init(this, bundle);
		}

	    protected override void OnResume()
	    {
		    base.OnResume();
		    RegisterReceiver(_receiver, new IntentFilter(BluetoothDevice.ActionFound));
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
	}

}

