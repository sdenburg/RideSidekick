using System;
using Xamarin.Essentials;

namespace RideSidekick.Models
{
    public class Route : IComparable<Route>
    {
        public Location Start { get; set; }
        public Location Pickup { get; set; }
        public Location Destination { get; set; }
        public Location Dropoff { get; set; }

        public double GetDistanceFromStart()
        {
            return Location.CalculateDistance(this.Start, this.Pickup, DistanceUnits.Miles);
        }

        public double GetDistanceFromDestination()
        {
            return Location.CalculateDistance(this.Destination, this.Dropoff, DistanceUnits.Miles);
        }

        public double GetTotalDistance()
        {
            return this.GetDistanceFromStart() + this.GetDistanceFromDestination();
        }

        public int CompareTo(Route other)
        {
            return this.GetTotalDistance().CompareTo(other.GetTotalDistance());
        }

        public override string ToString()
        {
            var precision = 2;
            var total = Math.Round(this.GetTotalDistance(), precision);
            var start = Math.Round(this.GetDistanceFromStart(), precision);
            var end = Math.Round(this.GetDistanceFromDestination(), precision);

            return $@"{total} total miles walking ({start} then {end})";
        }
    }
}