using System.Threading.Tasks;
using BluetoothPoll.Droid.Services;
using BluetoothPoll.Services;
using Plugin.CurrentActivity;
using Xamarin.Forms;

[assembly: Dependency(typeof(PollBtDevices))]
namespace BluetoothPoll.Droid.Services
{
	public class PollBtDevices:IPollBtDevices
	{
		public async Task<bool> Search()
		{
			var act = ((MainActivity) CrossCurrentActivity.Current.Activity);
			act.TaskCompletionSource = new TaskCompletionSource<bool>();
			act.CheckPermissions();
			return await act.TaskCompletionSource.Task;
		}
	}
}