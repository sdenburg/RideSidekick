using System;
using RideSidekick.Models;
using Xamarin.Essentials;
using Xamarin.Forms.Maps;
using Map = Xamarin.Forms.Maps.Map;

namespace RideSidekick.Extensions
{
    public static class MapExtensions
    {
        public static void SetDefaultView(this Map map)
        {
            var defaultMapCenter = new Position(40, -95);
            var defaultMapSpan = MapSpan.FromCenterAndRadius(defaultMapCenter, Distance.FromMiles(500));
            map.MoveToRegion(defaultMapSpan);
        }

        public static async void CenterOnCurrentLocation(this Map map)
        {
            try
            {
                var currentLocation = await Geolocation.GetLastKnownLocationAsync();
                if (currentLocation == null)
                {
                    var locationRequest = new GeolocationRequest(GeolocationAccuracy.Medium);
                    currentLocation = await Geolocation.GetLocationAsync(locationRequest);
                }

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
