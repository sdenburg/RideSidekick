using System;
using System.Collections.Generic;
using System.Linq;
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

            var averagePrice = rides.Average(r => r.PriceEstimate.HighEstimate ?? 0);
            var standardDeviationPrice = this.CalculateStandardDeviation(rides);

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
                    PriceColor = this.GetColor(ride, averagePrice, standardDeviationPrice)
                };

                this.Map.PricePins.Add(pin);
                this.Map.Pins.Add(pin);
            }
        }

        private double CalculateStandardDeviation(IEnumerable<UberRide> rides)
        {
            var prices = rides.Select(r => r.PriceEstimate.HighEstimate ?? 0);
            var average = prices.Average();
            return Math.Sqrt(prices.Average(p => Math.Pow(p - average, 2)));
        }

        private PriceColor GetColor(UberRide ride, double averagePrice, double standardDeviationPrice)
        {
            var price = ride.PriceEstimate.HighEstimate;
            if (price < averagePrice - standardDeviationPrice)
                return PriceColor.Green;
            else if (price < averagePrice + standardDeviationPrice)
                return PriceColor.Yellow;
            else
                return PriceColor.Red;
        }
    }
}