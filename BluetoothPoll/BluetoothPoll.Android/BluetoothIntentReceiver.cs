using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using BluetoothPoll.Models;
using Java.Util;
using Plugin.CurrentActivity;
using Xamarin.Forms;
using Application = Xamarin.Forms.Application;


namespace BluetoothPoll.Droid
{
	[BroadcastReceiver(Enabled = true, Exported = true)]
	[IntentFilter(new[] { BluetoothDevice.ActionFound, BluetoothDevice.ActionUuid, BluetoothAdapter.ActionDiscoveryFinished })]
	public class BluetoothIntentReceiver:BroadcastReceiver
	{
		public BluetoothIntentReceiver()
		{
		}

		/*
		 *We require that the task would run on iOS and on Android using the basic BT api,
		 * without using any BT NuGet. Additionally, I would like to add a local notification
		 * that would show some text like “Hello World” when phones detect a nearby beacon
		 * (can be simulated by any smartphone)
		 * With the following uuid: 3f234454-cf6d-4a0f-adf2-f4911ba9a111
		 *
		 */

		private List<BluetoothDevice> Devices = new List<BluetoothDevice>();

		private static readonly string LOOKUP_UUID = "3f234454-cf6d-4a0f-adf2-f4911ba9a111";
		//private static readonly string LOOKUP_UUID = "00001105-0000-1000-8000-00805f9b34fb";

		public override void OnReceive(Context context, Intent intent)
		{
			var action = intent.Action;

			if (action == BluetoothDevice.ActionUuid)
			{
				var uuids = intent.GetParcelableArrayExtra(BluetoothDevice.ExtraUuid);
				if (uuids != null)
				{
					foreach (ParcelUuid uuid in uuids)
					{
						if (uuid.Uuid.ToString().Equals(LOOKUP_UUID))
						{
							//notify about nearby device
							var act = ((MainActivity)CrossCurrentActivity.Current.Activity);
							act.SendLocalNotification();
						}
					}
				}
			}
			else if (action == BluetoothDevice.ActionFound)
			{
				var device = (BluetoothDevice) intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
				if (device.BondState != Bond.Bonded)
				{
					Devices.Add(device);

					MessagingCenter.Send<App, BtDevice>((App) Application.Current, BtDevice.NewDevice,
						new BtDevice() {Mac = device.Address, Name = device.Name});
				}
			}
			else if (action == BluetoothAdapter.ActionDiscoveryFinished)
			{
				Devices.ForEach(device => device.FetchUuidsWithSdp());
				Devices.RemoveAll(device => device != null);
			}
			
		}
	}
}