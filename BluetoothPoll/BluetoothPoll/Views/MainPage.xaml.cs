using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BluetoothPoll.Views
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
			var isource = ImageSource.FromResource("BluetoothPoll.logo.PNG");
			logo.Source = isource;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			GoInside();
		}

		private async void GoInside()
		{
			await Task.Delay(5000);
			await this.FadeTo(0, 500, Easing.CubicIn);
			await ((App)(App.Current)).AppNavigationService.NavigateAsync("NavigationPage/PollPage");
		}
	}
}