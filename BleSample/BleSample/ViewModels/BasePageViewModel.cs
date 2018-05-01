using Prism.Mvvm;
using Prism.Navigation;

namespace BleSample.ViewModels
{
    public abstract class BasePageViewModel : BindableBase
    {
        private string title;
        private string subTitle;
        private string icon;

        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        public string SubTitle
        {
            get { return subTitle; }
            set { SetProperty(ref subTitle, value); }
        }

        public string Icon
        {
            get { return icon; }
            set { SetProperty(ref icon, value); }
        }

        public virtual void OnNavigatedFrom(NavigationParameters parameters)
        {

        }

        public virtual void OnNavigatedTo(NavigationParameters parameters)
        {

        }

        public virtual void OnNavigatingTo(NavigationParameters parameters)
        {

        }
    }
}