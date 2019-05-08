using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BluetoothPoll.ViewModels;
using CoreLocation;
using Foundation;
using UIKit;

namespace BluetoothPoll.Ios.Services
{
	public class BeaconMonitor
	{
		private CLLocationManager locationManager;

		public List<BeaconItem> Beacons { get; set; }

		public BeaconMonitor()
		{
			locationManager = new CLLocationManager();
			var beaconRegion = new CLBeaconRegion(new NSUuid(PollViewModel.LOOKUP_UUID), "My Beacon")
			{
				NotifyEntryStateOnDisplay = true,
				NotifyOnEntry = true
			};

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
									foreach (var beacon in Beacons)
									{
										if (beacon.Minor.Equals(b.Minor.ToString()))
										{
											beacon.CurrentDistance = Math.Round(b.Accuracy, 2);
											SetProximity(b, beacon);
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
										Beacons.Add(newBeacon);
									}
								}
							}
						}
					};
				}
			};
			locationManager.StartMonitoring(beaconRegion);
		}

		void SetProximity(CLBeacon source, BeaconItem dest)
		{

			CLProximity p = CLProximity.Unknown;

			switch (source.Proximity)
			{
				case CLProximity.Immediate:
					p = CLProximity.Immediate;
					var notification = new UILocalNotification
					{
						FireDate = NSDate.FromTimeIntervalSinceNow(1),
						AlertAction = "Device Proximity Alert",
						AlertBody = "We are close to required bluetooth device!",
						SoundName = UILocalNotification.DefaultSoundName
					};
					//notify main app about beacon proximity
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
	}
}