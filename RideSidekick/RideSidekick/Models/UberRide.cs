using Rideshare.Uber.Sdk.Models;

namespace RideSidekick.Models
{
    public class UberRide
    {
        public Route Route { get; set; }

        public PriceEstimate PriceEstimate { get; set; }
    }
}