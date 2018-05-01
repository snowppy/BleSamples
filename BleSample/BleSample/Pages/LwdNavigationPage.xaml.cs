using Prism.Navigation;
using Xamarin.Forms.Xaml;

namespace BleSample.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LwdNavigationPage : INavigationPageOptions
	{
	    public bool ClearNavigationStackOnNavigation => true;
		public LwdNavigationPage()
		{
			InitializeComponent();
		}
	}
}