using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BluetoothPoll.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BluetoothPoll.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PollPage : ContentPage
	{
		public PollPage ()
		{
			InitializeComponent ();
			
			//this approach results in much more slower result
			//this.BackgroundImage = "bg.png";

			var isource = ImageSource.FromResource("BluetoothPoll.bg_image.png");

			Bg.Source = isource;
		}
	}
}