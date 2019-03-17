using System;
using System.Collections.Generic;
using RideSidekick.CustomControls;
using RideSidekick.Extensions;
using RideSidekick.Models;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace RideSidekick.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RideResultsMapPage : ContentPage
	{
        public enum SingleLocation
        {
            Start,
            End
        }

		public RideResultsMapPage(IEnumerable<UberRide> rides, SingleLocation location)
		{
			InitializeComponent();

            this.Map.SetDefaultView();
            this.Map.CenterOnCurrentLocation();

            foreach(var ride in rides)
            {
                Position position;
                if (location == SingleLocation.Start)
                {
                    position = new Position(ride.Route.Dropoff.Latitude, ride.Route.Dropoff.Longitude);
                }
                else if (location == SingleLocation.End)
                {
                    position = new Position(ride.Route.Pickup.Latitude, ride.Route.Pickup.Longitude);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(location));
                }

                var pin = new PricePin
                {
                    Type = PinType.Place,
                    Position = position,
                    Label = ride.PriceEstimate.Estimate,
                    PriceColor = this.GetColor(ride)
                };

                this.Map.PricePins.Add(pin);
                this.Map.Pins.Add(pin);
            }
        }

        private PriceColor GetColor(UberRide ride)
        {
            var price = ride.PriceEstimate.HighEstimate;
            if (price < 7)
                return PriceColor.Green;
            else if (price < 15)
                return PriceColor.Yellow;
            else
                return PriceColor.Red;
        }
    }
}