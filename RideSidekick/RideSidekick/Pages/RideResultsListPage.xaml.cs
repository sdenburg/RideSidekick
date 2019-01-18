using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using RideSidekick.Models;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace RideSidekick.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RideResultsListPage : ContentPage
    {
        protected ObservableCollection<UberRow> Items { get; set; }

        public RideResultsListPage(IEnumerable<UberRide> rides)
        {
            InitializeComponent();
            
            var orderedRows = rides.OrderBy(p => p.PriceEstimate.HighEstimate)
                                    .ThenBy(p => p.Route)
                                    .Select(p => new UberRow
                                    {
                                        PriceEstimate = p.PriceEstimate.Estimate,
                                        Route = p.Route.ToString(),
                                        Pickup = p.Route.Pickup,
                                        Dropoff = p.Route.Dropoff
                                    })
                                    .ToList();

            this.Items = new ObservableCollection<UberRow>(orderedRows);
			RideResultsList.ItemsSource = orderedRows;
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var uberRow = e.Item as UberRow;
            if (uberRow == null)
                return;

            var geocoder = new Geocoder();
            var pickupAddressTask = geocoder.GetAddressesForPositionAsync(new Position(uberRow.Pickup.Latitude, uberRow.Pickup.Longitude));
            var dropoffAddressTask = geocoder.GetAddressesForPositionAsync(new Position(uberRow.Dropoff.Latitude, uberRow.Dropoff.Longitude));

            await Task.WhenAll(pickupAddressTask, dropoffAddressTask);

            string alertMessage = $"Pickup from {pickupAddressTask.Result.FirstOrDefault()} dropoff at {dropoffAddressTask.Result.FirstOrDefault()} for {uberRow.PriceEstimate}.";
            await DisplayAlert("Ride Selected", alertMessage, "OK");

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        protected class UberRow
        {
            public string PriceEstimate { get; set; }
            public string Route { get; set; }
            public Location Pickup { get; set; }
            public Location Dropoff { get; set; }
        }
    }
}
