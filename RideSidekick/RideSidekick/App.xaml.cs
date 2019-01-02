using RideSidekick.Configuration;
using RideSidekick.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace RideSidekick
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            UberConfigurationManager.LoadConfiguration();
            MainPage = new NavigationPage(new RideSearchPage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
