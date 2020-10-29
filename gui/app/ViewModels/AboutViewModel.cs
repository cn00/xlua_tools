using System;
using System.Windows.Input;
using app.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace app.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://xamarin.com"));
            Back = new Command(async () => await (Application.Current.MainPage as MainPage).NavigateFromMenu((int)Models.MenuItemType.Browse));
        }

        public ICommand OpenWebCommand { get; }
        public ICommand Back { get; }
    }
}