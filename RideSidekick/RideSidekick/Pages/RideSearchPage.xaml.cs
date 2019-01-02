using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rideshare.Uber.Sdk;
using Rideshare.Uber.Sdk.Models;
using RideSidekick.Configuration;
using RideSidekick.Models;
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
        public RideSearchPage()
        {
            InitializeComponent();

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
                var rides = await this.GetUberRides(currentLocation, endLocation, startWalkDistance, endWalkDistance);

                await Navigation.PushAsync(new RideResultsPage(rides));
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
            string uberType = "Pool";

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

                                var uberRide = new UberRide()
                                {
                                    Route = new Route
                                    {
                                        Pickup = new Location(pickupLatitude, pickupLongitude),
                                        Dropoff = new Location(dropoffLatitude, dropoffLongitude),
                                        Start = startLocation,
                                        Destination = endLocation
                                    },
                                    PriceEstimate = priceEstimate
                                };

                                rides.Add(uberRide);

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
            return rides;
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
    }
}