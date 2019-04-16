using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluetoothPoll.Ios.Services;
using BluetoothPoll.Models;
using BluetoothPoll.Services;
using CoreBluetooth;
using CoreFoundation;
using ExternalAccessory;
using Foundation;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(PollBtDevices))]
namespace BluetoothPoll.Ios.Services
{
	public class PollBtDevices:IPollBtDevices
	{
		//private static readonly string LOOKUP_UUID = "3f234454-cf6d-4a0f-adf2-f4911ba9a111";
		private static readonly string LOOKUP_UUID = "00001105-0000-1000-8000-00805f9b34fb";

		private readonly CBCentralManager manager = new CBCentralManager(new CbCentralDelegate(), DispatchQueue.DefaultGlobalQueue,
			new CBCentralInitOptions { ShowPowerAlert = true });

		public EventHandler DiscoveredDevice;
		public EventHandler StateChanged;

		public PollBtDevices()
		{
			manager.DiscoveredPeripheral += (sender, args) =>
			{
				var device = $"{args.Peripheral.Name} - {args.Peripheral.Identifier?.Description}";
				Debug.WriteLine($"Discovered {device}");

				MessagingCenter.Send<App, BtDevice>((App)Xamarin.Forms.Application.Current, BtDevice.NewDevice,
					new BtDevice() { Mac = "-", Name = args.Peripheral.Name });
			};
			
		}

		public async Task<bool> Search()
		{
			EAAccessoryManager mgr = EAAccessoryManager.SharedAccessoryManager;
			var accessories = mgr.ConnectedAccessories;
			foreach (var accessory in accessories)
			{
				Console.WriteLine(accessory.ToString());
			}



			if (manager.State == CBCentralManagerState.PoweredOn)
			{
				manager.RetrieveConnectedPeripherals(new[] { CBUUID.FromString(LOOKUP_UUID) });
				Debug.WriteLine("Scanning started");
				var uuids = new[] { CBUUID.FromString(LOOKUP_UUID) };
				manager.ScanForPeripherals(uuids);
				await Task.Delay(15000);
				manager.StopScan();
				return await Task.FromResult(false);

			}





			return await Task.FromResult(true);

		}
	}

	public class CbCentralDelegate : CBCentralManagerDelegate
	{
		public override void UpdatedState(CBCentralManager central)
		{
			if (central.State == CBCentralManagerState.PoweredOn)
			{
				System.Console.WriteLine("Powered On");
			}
			else
			{
				//points user to turn bluetooth on
				Device.BeginInvokeOnMainThread(() =>
				{
					if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
					{
						UIApplication.SharedApplication.OpenUrl(new NSUrl("prefs:root=Bluetooth"));
					}
					else if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
					{
						UIApplication.SharedApplication.OpenUrl(new NSUrl("prefs:root=General&path=Bluetooth"));
					}
					else
					{
						UIApplication.SharedApplication.OpenUrl(new NSUrl("App-Prefs:root=Bluetooth"));
					}
				});

			}
		}
	}
}