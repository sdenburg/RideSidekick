using System;
using Rideshare.Uber.Sdk;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;
using Map = Xamarin.Forms.Maps.Map;

namespace RideSidekick.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class MapPage : ContentPage
    {
        public MapPage()
        {
            var defaultMapCenter = new Position(40, -95);
            var defaultMapSpan = MapSpan.FromCenterAndRadius(defaultMapCenter, Distance.FromMiles(100));
            var map = new Map(defaultMapSpan)
            {
                IsShowingUser = true,
                HeightRequest = 100,
                WidthRequest = 960,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            var latitude = new Entry()
            {

            };

            var pin = new Pin
            {
                Type = PinType.Place,
                Position = defaultMapCenter,
                Label = "custom pin",
                Address = "custom detail info"
            };
            map.Pins.Add(pin);

            var slider = new Slider(1, 18, 1);
            slider.ValueChanged += (sender, e) => {
                var zoomLevel = e.NewValue; // between 1 and 18
                var latlongdegrees = 360 / (Math.Pow(2, zoomLevel));
                map.MoveToRegion(new MapSpan(map.VisibleRegion.Center, latlongdegrees, latlongdegrees));
            };

            var stack = new StackLayout { Spacing = 0 };
            stack.Children.Add(latitude);
            stack.Children.Add(map);
            stack.Children.Add(slider);
            Content = stack;
            


            var uberClient = new ServerAuthenticatedUberRiderService(serverToken);

            var priceRequest =  uberClient.GetPriceEstimateAsync(startLat, startLng, endLat, endLng);
            var p = priceRequest.Result.Data.PriceEstimates;

            this.CenterMapOnCurrentLocation(map);
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