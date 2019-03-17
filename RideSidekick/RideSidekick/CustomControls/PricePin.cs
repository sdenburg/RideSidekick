using Xamarin.Forms.Maps;

namespace RideSidekick.CustomControls
{
    public enum PriceColor
    {
        Green,
        Yellow,
        Red
    };

    public class PricePin : Pin
    {
        public PriceColor PriceColor { get; set; }
    }
}
