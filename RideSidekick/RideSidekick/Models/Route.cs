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

        public int CompareTo(Route other)
        {
            var thisDistanceToPickup = Location.CalculateDistance(this.Start, this.Pickup, DistanceUnits.Miles);
            var otherDistanceToPickup = Location.CalculateDistance(other.Start, other.Pickup, DistanceUnits.Miles);

            var thisDistanceToDropoff = Location.CalculateDistance(this.Start, this.Pickup, DistanceUnits.Miles);
            var otherDistanceToDropoff = Location.CalculateDistance(other.Start, other.Pickup, DistanceUnits.Miles);

            var thisTotalWalking = thisDistanceToPickup + thisDistanceToDropoff;
            var otherTotalWalking = otherDistanceToPickup + otherDistanceToDropoff;

            return thisTotalWalking.CompareTo(otherTotalWalking);
        }

        public override string ToString()
        {
            return $"({Math.Round(this.Pickup.Latitude, 3)}, {Math.Round(this.Pickup.Longitude, 3)}) to ({Math.Round(this.Dropoff.Latitude, 3)}, {Math.Round(this.Dropoff.Longitude, 3)})";
        }
    }
}