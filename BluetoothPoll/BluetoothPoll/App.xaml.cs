using Prism;
using Prism.Ioc;
using BluetoothPoll.ViewModels;
using BluetoothPoll.Views;
using Prism.Navigation;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace BluetoothPoll
{
	public partial class App
	{

		public App() : this(null) { }

		public INavigationService AppNavigationService { get; private set; }

		public App(IPlatformInitializer initializer) : base(initializer) { }

		protected override async void OnInitialized()
		{
			InitializeComponent();
			AppNavigationService = this.NavigationService;
			await NavigationService.NavigateAsync("MainPage");
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{
			containerRegistry.RegisterForNavigation<NavigationPage>();
			containerRegistry.RegisterForNavigation<MainPage>();
			containerRegistry.RegisterForNavigation<PollPage, PollViewModel>();
		}
	}
}
