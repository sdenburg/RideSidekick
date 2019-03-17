using System.Collections.Generic;
using Xamarin.Forms.Maps;

namespace RideSidekick.CustomControls
{
    public class PriceMap : Map
    {
        public List<PricePin> PricePins { get; set; } = new List<PricePin>();
    }
}
