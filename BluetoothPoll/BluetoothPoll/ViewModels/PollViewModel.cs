using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using BluetoothPoll.Models;
using BluetoothPoll.Services;
using Prism.Navigation;
using Xamarin.Forms;

namespace BluetoothPoll.ViewModels
{
    public class PollViewModel:ViewModelBase
    {
		private bool _isRequesting;
	    private BtDevice _selectedItem;

		public ObservableCollection<BtDevice> Devices { get; set; }

		public bool IsRequesting
	    {
		    get => _isRequesting;
		    set
		    {
			    _isRequesting = value;
			    RaisePropertyChanged(nameof(IsRequesting));
			}
		}


	    public BtDevice SelectedItem
	    {
		    get => _selectedItem;

		    set
		    {
			    _selectedItem = value;
			    if (value != null)
			    {
				    Application.Current.MainPage.DisplayAlert(value.Name + " Selected",
					    "MAC address is: " + value.Mac, "Ok");
			    }
		    }
	    }


	    public ICommand PollBtDevicesCommand => new Command(async () =>
		{
			IsRequesting = true;
			Devices = new ObservableCollection<BtDevice>();
			IsRequesting = await DependencyService.Get<IPollBtDevices>().Search();
		});

	    public PollViewModel(INavigationService navigationService) : base(navigationService)
	    {
			Devices = new ObservableCollection<BtDevice>();
		    MessagingCenter.Subscribe<App, BtDevice>((App)Application.Current, BtDevice.NewDevice, (app, device) =>
		    {
			    var exist = Devices.FirstOrDefault(btDevice =>
				    btDevice.Name == device.Name || btDevice.Mac == device.Mac);

			    if (exist == null)
			    {
				    Debug.Write(device.Name);
				    Devices.Add(device);
				    RaisePropertyChanged(nameof(Devices));
				}

			});
		}

	    public override void Destroy()
	    {
		    MessagingCenter.Unsubscribe<App, BtDevice>((App)Application.Current, BtDevice.NewDevice);
			base.Destroy();
	    }
    }
}
