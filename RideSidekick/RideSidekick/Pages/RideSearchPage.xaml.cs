using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rideshare.Uber.Sdk;
using Rideshare.Uber.Sdk.Models;
using RideSidekick.Configuration;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;
using Map = Xamarin.Forms.Maps.Map;

namespace RideSidekick.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RideSearchPage : ContentPage
    {
        public string UberServerToken { get; set;
        }
        public RideSearchPage()
        {
            InitializeComponent();

            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(RideResultsPage)).Assembly;
            Stream stream = assembly.GetManifestResourceStream("RideSidekick.Configuration.UberConfiguration.json");
            using (var reader = new StreamReader(stream))
            {
                string text = reader.ReadToEnd();
                var uberConfiguration = JsonConvert.DeserializeObject<UberConfiguration>(text);
                this.UberServerToken = uberConfiguration.ServerToken;
            }

            var defaultMapCenter = new Position(40, -95);
            var defaultMapSpan = MapSpan.FromCenterAndRadius(defaultMapCenter, Distance.FromMiles(100));
            this.Map.MoveToRegion(defaultMapSpan);
            this.CenterMapOnCurrentLocation(this.Map);
        }

        public async void OnSubmit(object sender, EventArgs e)
        {
            this.SubmitButton.IsEnabled = false;

            var currentLocation = await Geolocation.GetLastKnownLocationAsync();

            if (double.TryParse(this.StartWalkDistanceInput.Text, out double startWalkDistance) &&
                double.TryParse(this.EndWalkDistanceInput.Text, out double endWalkDistance))
            {
                var geocoder = new Geocoder();
                var destinationPosition = (await geocoder.GetPositionsForAddressAsync(this.DestinationAddressInput.Text)).FirstOrDefault();
                var endLocation = new Location(destinationPosition.Latitude, destinationPosition.Longitude);
                var prices = await this.GetUberPrices(currentLocation, endLocation, startWalkDistance, endWalkDistance);

                await Navigation.PushAsync(new RideResultsPage(prices));
            }
            else
            {
                await DisplayAlert("Invalid Input", "Invalid input. ", "OK");
            }

            this.SubmitButton.IsEnabled = true;
        }

        private async Task<Dictionary<Route, PriceEstimate>> GetUberPrices(Location startLocation, Location endLocation, double startWalkDistance, double endWalkDistance)
        {
            var uberClient = new ServerAuthenticatedUberRiderService(this.UberServerToken);

            double queryDensity = 0.01;
            string uberType = "Pool";

            var requests = new List<Task>();
            var prices = new Dictionary<Route, PriceEstimate>();
            for (double tempStartLatitude = startLocation.Latitude - startWalkDistance; tempStartLatitude <= startLocation.Latitude + startWalkDistance; tempStartLatitude += queryDensity)
            {
                for (double tempStartLongitude = startLocation.Longitude - startWalkDistance; tempStartLongitude <= startLocation.Longitude + startWalkDistance; tempStartLongitude += queryDensity)
                {
                    for (double tempEndLatitude = endLocation.Latitude - endWalkDistance; tempEndLatitude <= endLocation.Latitude + endWalkDistance; tempEndLatitude += queryDensity)
                    {
                        for (double tempEndLongitude = endLocation.Longitude - endWalkDistance; tempEndLongitude <= endLocation.Longitude + endWalkDistance; tempEndLongitude += queryDensity)
                        {
                            var getPriceEstimateTask = uberClient.GetPriceEstimateAsync((float)tempStartLatitude, (float)tempStartLongitude, (float)tempEndLatitude, (float)tempEndLongitude);

                            Console.WriteLine($"Uber price estimate request made from {tempStartLatitude}, {tempStartLongitude} to {tempEndLatitude}, {tempEndLongitude}");
                            var handlePriceEstimateTask = getPriceEstimateTask.ContinueWith((response) =>
                            {
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

                                var tempRoute = new Route
                                {
                                    Pickup = new Location(tempStartLatitude, tempStartLongitude),
                                    Dropoff = new Location(tempEndLatitude, tempEndLongitude),
                                    Current = startLocation,
                                    Destination = endLocation
                                };

                                prices.Add(tempRoute, priceEstimate);

                                // this.DrawRouteOnMap(this.Map, tempRoute);
                                Console.WriteLine("Uber request completed and handled");
                            });

                            requests.Add(getPriceEstimateTask);
                            requests.Add(handlePriceEstimateTask);
                        }
                    }
                }
            }

            await Task.WhenAll(requests);
            return prices;
        }

        private void DrawRouteOnMap(Map map, Route route)
        {
            var pin = new Pin
            {
                Type = PinType.Place,
                Position = new Position(route.Dropoff.Latitude, route.Dropoff.Longitude),
                Label = "Price",
                Address = "Details"
            };
            // this.Map.Pins.Add(pin);
        }

        private async void CenterMapOnCurrentLocation(Map map)
        {
            try
            {
                var currentLocation = await Geolocation.GetLastKnownLocationAsync();
                var currentPosition = new Position(currentLocation.Latitude, currentLocation.Longitude);
                var mapSpan = MapSpan.FromCenterAndRadius(currentPosition, Distance.FromMiles(1));
                map.MoveToRegion(mapSpan);
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
                return;
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
                return;
            }
            catch (Exception ex)
            {
                // Unable to get location
                return;
            }
        }

        public class Route : IComparable<Route>
        {
            public Location Current { get; set; }
            public Location Pickup { get; set; }
            public Location Destination{ get; set; }
            public Location Dropoff { get; set; }

            public int CompareTo(Route other)
            {
                var thisDistanceToPickup = Location.CalculateDistance(this.Current, this.Pickup, DistanceUnits.Miles);
                var otherDistanceToPickup = Location.CalculateDistance(other.Current, other.Pickup, DistanceUnits.Miles);

                var thisDistanceToDropoff = Location.CalculateDistance(this.Current, this.Pickup, DistanceUnits.Miles);
                var otherDistanceToDropoff = Location.CalculateDistance(other.Current, other.Pickup, DistanceUnits.Miles);

                var thisTotalWalking = thisDistanceToPickup + thisDistanceToDropoff;
                var otherTotalWalking = otherDistanceToPickup + otherDistanceToDropoff;

                return thisTotalWalking.CompareTo(otherTotalWalking);
            }
        }
    }
}