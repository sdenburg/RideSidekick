using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Rideshare.Uber.Sdk.Models;
using RideSidekick.Models;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace RideSidekick.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RideResultsPage : ContentPage
    {
        protected ObservableCollection<UberRow> Items { get; set; }

        public RideResultsPage(IEnumerable<UberRide> rides)
        {
            InitializeComponent();
            
            var orderedRows = rides.OrderBy(p => p.PriceEstimate.HighEstimate)
                                    .ThenBy(p => p.Route)
                                    .Select(p => new UberRow
                                    {
                                        PriceEstimate = p.PriceEstimate.Estimate,
                                        Route = p.Route.ToString()
                                    })
                                    .ToList();

            this.Items = new ObservableCollection<UberRow>(orderedRows);
			RideResultsList.ItemsSource = orderedRows;
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            await DisplayAlert("Item Tapped", "An item was tapped.", "OK");

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        protected class UberRow
        {
            public string PriceEstimate { get; set; }
            public string Route { get; set; }
        }
    }
}
