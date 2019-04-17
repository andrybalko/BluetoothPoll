using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluetoothPoll.Ios.Services;
using BluetoothPoll.Models;
using BluetoothPoll.Services;
using BluetoothPoll.ViewModels;
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
		private readonly CBCentralManager manager = new CBCentralManager(new CbCentralDelegate(), DispatchQueue.DefaultGlobalQueue,
                                                                         new CBCentralInitOptions { ShowPowerAlert = true });

		
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
                await ScanPeripherals();
				return await Task.FromResult(false);
			}
			return await Task.FromResult(true);
		}

        private async Task ScanPeripherals()
        {
            if (manager.State == CBCentralManagerState.PoweredOn)
            {
                manager.ScanForPeripherals(new CBUUID[0]);
                await Task.Delay(15000);
                manager.StopScan();
            }
        }
    }

	public class CbCentralDelegate : CBCentralManagerDelegate
	{
		public override void UpdatedState(CBCentralManager central)
		{
			if (central.State == CBCentralManagerState.PoweredOn)
			{
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

        
        public override void DiscoveredPeripheral(CBCentralManager central, CBPeripheral peripheral, NSDictionary advertisementData, NSNumber RSSI)
        {
            if (peripheral.Identifier.AsString() == PollViewModel.LOOKUP_UUID)
            {
                //show local notification
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
                    {

                    }
                    else if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
                    {
                        var notification = new UILocalNotification();
                        notification.FireDate = NSDate.FromTimeIntervalSinceNow(1);
                        notification.AlertAction = "Device Proximity Alert";
                        notification.AlertBody = "We are close to required bluetooth device!";
                        notification.SoundName = UILocalNotification.DefaultSoundName;

                        UIApplication.SharedApplication.ScheduleLocalNotification(notification);
                    }
                });
            }
            MessagingCenter.Send<App, BtDevice>((App)Xamarin.Forms.Application.Current, BtDevice.NewDevice,
                new BtDevice() { Mac = "-", Name = peripheral.Name });
        }
	}
}