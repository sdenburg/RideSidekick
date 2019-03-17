using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using RideSidekick.CustomControls;
using RideSidekick.Droid.CustomRenderers;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Android;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(PriceMap), typeof(CustomMapRenderer))]
namespace RideSidekick.Droid.CustomRenderers
{
    public class CustomMapRenderer : MapRenderer, GoogleMap.IInfoWindowAdapter
    {
        List<PricePin> PricePins { get; set; }

        public CustomMapRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Map> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                var formsMap = (PriceMap)e.NewElement;
                this.PricePins = formsMap.PricePins;
                Control.GetMapAsync(this);
            }
        }

        protected override MarkerOptions CreateMarker(Pin pin)
        {
            var marker = new MarkerOptions();
            marker.SetPosition(new LatLng(pin.Position.Latitude, pin.Position.Longitude));
            marker.SetTitle(pin.Label);
            marker.SetSnippet(pin.Address);

            var pricePin = this.GetPricePin(pin);
            float hue;
            if (pricePin.PriceColor == PriceColor.Green)
                hue = BitmapDescriptorFactory.HueGreen;
            else if (pricePin.PriceColor == PriceColor.Yellow)
                hue = BitmapDescriptorFactory.HueYellow;
            else if (pricePin.PriceColor == PriceColor.Red)
                hue = BitmapDescriptorFactory.HueRed;
            else
                throw new ArgumentOutOfRangeException(nameof(pin));

            marker.SetIcon(BitmapDescriptorFactory.DefaultMarker(hue));

            return marker;
        }

        protected PricePin GetPricePin(Pin pin)
        {
            return this.PricePins.Where(p => p.Position == pin.Position).Single();
        }

        public Android.Views.View GetInfoContents(Marker marker)
        {
            return null;
        }

        public Android.Views.View GetInfoWindow(Marker marker)
        {
            return null;
        }
    }
}