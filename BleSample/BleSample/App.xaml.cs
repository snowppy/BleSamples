using BleSample.Pages;
using BleSample.Services.DeviceBluetoothLe.Device;
using BleSample.ViewModels;
using Prism;
using Prism.Ioc;
using Prism.Unity;

namespace BleSample
{
	public partial class App : PrismApplication
	{
	    public App(IPlatformInitializer initializer = null) : base(initializer) { }

		//protected override void OnStart ()
		//{
		//    base.OnStart();
		//	// Handle when your app starts
		//}

	    protected override void RegisterTypes(IContainerRegistry containerRegistry)
	    {
	        containerRegistry.Register<IDeviceInformationPageViewModel, DeviceInformationPageViewModel>();
	        containerRegistry.RegisterForNavigation<LwdNavigationPage, LwdNavigationPageViewModel>();
            containerRegistry.RegisterForNavigation<SamplePage, IDeviceInformationPageViewModel>();
	        containerRegistry.RegisterSingleton<IBleService, BleService>();
	    }

	    protected override void OnInitialized()
	    {
	        InitializeComponent();
	        NavigationService.NavigateAsync("LwdNavigationPage/SamplePage");
	    }
	}
}
