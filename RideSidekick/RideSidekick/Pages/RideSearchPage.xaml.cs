using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rideshare.Uber.Sdk;
using Rideshare.Uber.Sdk.Models;
using RideSidekick.Configuration;
using RideSidekick.Extensions;
using RideSidekick.Models;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace RideSidekick.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RideSearchPage : ContentPage
    {
        private const double METERS_PER_DEGREE = 111111;

        public RideSearchPage()
        {
            InitializeComponent();

            this.Map.SetDefaultView();
            this.Map.CenterOnCurrentLocation();
        }

        public async void OnSubmit(object sender, EventArgs e)
        {
            this.SubmitButton.IsEnabled = false;

            var currentLocation = await Geolocation.GetLastKnownLocationAsync();

            if (double.TryParse(this.StartWalkDistanceInput.Text, out double startWalkDistance) &&
                double.TryParse(this.EndWalkDistanceInput.Text, out double endWalkDistance))
            {
                var startWalkDistanceDegrees = startWalkDistance / METERS_PER_DEGREE;
                var endWalkDistanceDegrees = endWalkDistance / METERS_PER_DEGREE;

                var geocoder = new Geocoder();
                var destinationPosition = (await geocoder.GetPositionsForAddressAsync(this.DestinationAddressInput.Text)).FirstOrDefault();
                var endLocation = new Location(destinationPosition.Latitude, destinationPosition.Longitude);
                var rides = await this.GetUberRides(currentLocation, endLocation, startWalkDistanceDegrees, endWalkDistanceDegrees);

                ContentPage resultsPage;
                if (startWalkDistance == 0)
                {
                    resultsPage = new RideResultsMapPage(rides, RideResultsMapPage.SingleLocation.Start);
                }
                else if (endWalkDistance == 0)
                {
                    resultsPage = new RideResultsMapPage(rides, RideResultsMapPage.SingleLocation.End);
                }
                else
                {
                    resultsPage = new RideResultsListPage(rides);
                }

                await Navigation.PushAsync(resultsPage);
            }
            else
            {
                await DisplayAlert("Invalid Input", "Invalid input. ", "OK");
            }

            this.SubmitButton.IsEnabled = true;
        }

        private async Task<IEnumerable<UberRide>> GetUberRides(Location startLocation, Location endLocation, double startWalkDistance, double endWalkDistance)
        {
            var uberClient = new ServerAuthenticatedUberRiderService(UberConfigurationManager.Configuration.ServerToken);

            double queryDensity = 0.01;

            var requests = new List<Task>();
            var rides = new List<UberRide>();
            for (double pickupLatitude = startLocation.Latitude - startWalkDistance; pickupLatitude <= startLocation.Latitude + startWalkDistance; pickupLatitude += queryDensity)
            {
                for (double pickupLongitude = startLocation.Longitude - startWalkDistance; pickupLongitude <= startLocation.Longitude + startWalkDistance; pickupLongitude += queryDensity)
                {
                    for (double dropoffLatitude = endLocation.Latitude - endWalkDistance; dropoffLatitude <= endLocation.Latitude + endWalkDistance; dropoffLatitude += queryDensity)
                    {
                        for (double dropoffLongitude = endLocation.Longitude - endWalkDistance; dropoffLongitude <= endLocation.Longitude + endWalkDistance; dropoffLongitude += queryDensity)
                        {
                            var getPriceEstimateTask = uberClient.GetPriceEstimateAsync((float)pickupLatitude, (float)pickupLongitude, (float)dropoffLatitude, (float)dropoffLongitude);

                            Console.WriteLine($"Uber price estimate request made from {pickupLatitude}, {pickupLongitude} to {dropoffLatitude}, {dropoffLongitude}");

                            var route = new Route
                            {
                                Pickup = new Location(pickupLatitude, pickupLongitude),
                                Dropoff = new Location(dropoffLatitude, dropoffLongitude),
                                Start = startLocation,
                                Destination = endLocation
                            };

                            var handlePriceEstimateTask = getPriceEstimateTask.ContinueWith((response) => this.HandleUberPriceRequestResult(response, route, startLocation, endLocation, rides));

                            requests.Add(getPriceEstimateTask);
                            requests.Add(handlePriceEstimateTask);
                        }
                    }
                }
            }

            await Task.WhenAll(requests);
            return rides;
        }

        private void HandleUberPriceRequestResult(
            Task<UberResponse<PriceEstimateCollection>> response, 
            Route route, 
            Location startLocation, 
            Location endLocation, 
            List<UberRide> rides)
        {
            string uberType = "Pool";

            var priceEstimate = response.Result
                                     .Data
                                     .PriceEstimates
                                     .Where(p => p.DisplayName == uberType)
                                     .SingleOrDefault();

            if (priceEstimate == null)
            {
                Console.WriteLine("Error getting price estimate");
                return;
            }

            var uberRide = new UberRide()
            {
                Route = route,
                PriceEstimate = priceEstimate
            };

            rides.Add(uberRide);
            
            Console.WriteLine("Uber request completed and handled");
        }
    }
}