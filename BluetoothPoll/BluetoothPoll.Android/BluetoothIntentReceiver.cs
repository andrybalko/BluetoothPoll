using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BluetoothPoll.Models;
using Xamarin.Forms;
using Application = Xamarin.Forms.Application;


namespace BluetoothPoll.Droid
{
	[BroadcastReceiver(Enabled = true, Exported = true)]
	[IntentFilter(new[] { BluetoothDevice.ActionFound })]
	public class BluetoothIntentReceiver:BroadcastReceiver
	{
		public BluetoothIntentReceiver()
		{
		}

		public override void OnReceive(Context context, Intent intent)
		{
			var action = intent.Action;

			if (action != BluetoothDevice.ActionFound)
			{
				return;
			}

			var device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);

			if (device.BondState != Bond.Bonded)
			{
				MessagingCenter.Send<App, BtDevice>((App) Application.Current, BtDevice.NewDevice,
					new BtDevice() {Mac = device.Address, Name = device.Name});
			}
		}
	}
}