using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace RideSidekick.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RideResultsMapPage : ContentPage
	{
		public RideResultsMapPage ()
		{
			InitializeComponent ();

            var defaultMapCenter = new Position(40, -95);
            var defaultMapSpan = MapSpan.FromCenterAndRadius(defaultMapCenter, Distance.FromMiles(100));
            this.Map.MoveToRegion(defaultMapSpan);
        }
	}
}